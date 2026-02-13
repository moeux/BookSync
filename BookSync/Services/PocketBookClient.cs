using System.Text.Json;
using System.Web;
using BookSync.Models;
using Microsoft.Extensions.Logging;

namespace BookSync.Services;

public class PocketBookClient(ILogger<PocketBookClient> logger, IHttpClientFactory httpClientFactory)
{
    public async Task<bool> UploadMedia(string path, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(path);
        using var client = httpClientFactory.CreateClient($"{nameof(PocketBookClient)}-WriteClient");
        await using var fileStream = File.OpenRead(path);
        using var streamContent = new StreamContent(fileStream);
        var uploadResponse = await client.PutAsync(
            $"files/{HttpUtility.UrlEncode(fileName)}",
            streamContent,
            cancellationToken);

        if (uploadResponse.IsSuccessStatusCode)
            logger.LogInformation("Successfully uploaded media '{File}'.", fileName);
        else
            logger.LogError("Failed to upload media '{File}'.", fileName);

        return uploadResponse.IsSuccessStatusCode;
    }

    public async Task DeleteAllReadMedia(CancellationToken cancellationToken = default)
    {
        var books = await GetAllBooks(cancellationToken);
        using var client = httpClientFactory.CreateClient($"{nameof(PocketBookClient)}-WriteClient");

        foreach (var book in books.Where(book => book.ReadStatus == "read" || book.ReadPercent == 100))
        {
            using var deleteResponse = await client.PostAsync(
                $"fileops/delete/?fast_hash={HttpUtility.UrlEncode(book.FastHash)}",
                null,
                cancellationToken);

            if (deleteResponse.IsSuccessStatusCode)
                logger.LogInformation("Deleted read cloud media '{Book}'.", book.Name);
            else
                logger.LogError("Failed to delete read cloud media '{Book}'.", book.Name);
        }
    }

    private async Task<IEnumerable<Book>> GetAllBooks(CancellationToken cancellationToken = default)
    {
        using var client = httpClientFactory.CreateClient($"{nameof(PocketBookClient)}-ReadClient");
        using var statsResponse = await client.GetAsync("stats/books-info", cancellationToken);
        var statsContent = await statsResponse.Content.ReadAsStringAsync(cancellationToken);
        var limit = JsonDocument.Parse(statsContent).RootElement.GetProperty("total").Deserialize<string>();
        using var booksResponse = await client.GetAsync(
            $"books?limit={HttpUtility.UrlEncode(limit)}", cancellationToken);
        var booksContent = await booksResponse.Content.ReadAsStringAsync(cancellationToken);
        var books =
            JsonDocument.Parse(booksContent).RootElement
                .GetProperty("items")
                .Deserialize<List<Book>>()
            ?? [];

        return books;
    }
}