using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using BookSync.Models;
using Microsoft.Extensions.Logging;

namespace BookSync.Services;

public class AuthorizationHandler(ILogger<AuthorizationHandler> logger, IHttpClientFactory httpClientFactory)
    : DelegatingHandler
{
    private Login _login = new();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_login.IsExpired)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _login.AccessToken);
            return await base.SendAsync(request, cancellationToken);
        }

        using var client = httpClientFactory.CreateClient($"{nameof(PocketBookClient)}-AuthorizationClient");
        var pocketBookProvider = await GetProvider(client, cancellationToken);

        if (pocketBookProvider is null)
        {
            logger.LogError("PocketBook provider not found.");
            return await base.SendAsync(request, cancellationToken);
        }

        var login = await Login(pocketBookProvider, client, cancellationToken);

        if (login is null)
        {
            logger.LogError("Login failed.");
            return await base.SendAsync(request, cancellationToken);
        }

        logger.LogInformation("Login succeeded.");

        _login = login;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }

    private static async Task<Login?> Login(Provider provider, HttpClient client, CancellationToken cancellationToken)
    {
        var uri = $"login/{HttpUtility.UrlEncode(provider.Alias)}";
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "shop_id", provider.ShopId },
            { "username", BookSync.PluginConfiguration.Username },
            { "client_id", BookSync.PluginConfiguration.ClientId },
            { "client_secret", BookSync.PluginConfiguration.ClientSecret },
            { "grant_type", provider.LoggedBy },
            { "password", BookSync.PluginConfiguration.Password },
            { "language", "en" }
        });
        using var loginResponse = await client.PostAsync(uri, content, cancellationToken);

        return await loginResponse.Content.ReadFromJsonAsync<Login>(cancellationToken);
    }

    private static async Task<Provider?> GetProvider(HttpClient client, CancellationToken cancellationToken)
    {
        var uri = $"login?username={BookSync.PluginConfiguration.Username}&" +
                  $"client_id={BookSync.PluginConfiguration.ClientId}&" +
                  $"client_secret={BookSync.PluginConfiguration.ClientSecret}&" +
                  $"language=en";
        using var providerResponse = await client.GetAsync(uri, cancellationToken);
        var providerContent = await providerResponse.Content.ReadAsStringAsync(cancellationToken);
        var providers = JsonDocument.Parse(providerContent).RootElement
                            .GetProperty("providers")
                            .Deserialize<List<Provider>>()?.ToArray()
                        ?? [];
        var pocketBookProvider = providers.FirstOrDefault(p => p.Alias.StartsWith("pocketbook_"));

        return pocketBookProvider;
    }
}