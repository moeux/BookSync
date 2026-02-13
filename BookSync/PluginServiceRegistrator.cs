using BookSync.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace BookSync;

public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        const string readClientName = $"{nameof(PocketBookClient)}-ReadClient";
        const string writeClientName = $"{nameof(PocketBookClient)}-WriteClient";
        const string authorizationClientName = $"{nameof(PocketBookClient)}-AuthorizationClient";

        serviceCollection.AddHttpClient(
            authorizationClientName,
            client => { client.BaseAddress = new Uri("https://cloud.pocketbook.digital/api/v1.0/auth/login"); });

        serviceCollection.AddTransient<AuthorizationHandler>();

        serviceCollection
            .AddHttpClient(
                readClientName,
                client => { client.BaseAddress = new Uri("https://cloud.pocketbook.digital/api/v1.0/"); })
            .AddHttpMessageHandler<AuthorizationHandler>();
        serviceCollection
            .AddHttpClient(
                writeClientName,
                client => { client.BaseAddress = new Uri("https://cloud.pocketbook.digital/api/v1.1/"); })
            .AddHttpMessageHandler<AuthorizationHandler>();

        serviceCollection.AddSingleton<PocketBookClient>();
    }
}