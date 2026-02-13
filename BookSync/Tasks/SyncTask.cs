using BookSync.Services;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace BookSync.Tasks;

public class SyncTask(
    ILogger<SyncTask> logger,
    ILibraryManager libraryManager,
    IUserManager userManager,
    PocketBookClient client)
    : IScheduledTask
{
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting synchronization task.");
        progress.Report(0);

        if (!cancellationToken.IsCancellationRequested && BookSync.PluginConfiguration.DeleteReadCloudMedia)
        {
            logger.LogInformation("Deleting read cloud media.");
            await client.DeleteAllReadMedia(cancellationToken);
        }

        progress.Report(33);

        if (!cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Synchronizing media.");
            await SyncMedia(BookSync.PluginConfiguration.DeleteAfterSync, cancellationToken);
        }

        progress.Report(66);

        if (!cancellationToken.IsCancellationRequested && BookSync.PluginConfiguration.DeleteReadLocalMedia)
        {
            logger.LogInformation("Deleting local read media.");
            DeleteReadLocalMedia(cancellationToken);
        }

        logger.LogInformation("Synchronization complete.");
        progress.Report(100);
    }

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return
        [
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfoType.IntervalTrigger,
                IntervalTicks = TimeSpan.FromDays(BookSync.PluginConfiguration.Interval).Ticks
            }
        ];
    }

    public string Name => Resources.SyncTaskName;
    public string Key => Resources.SyncTaskKey;
    public string Description => Resources.SyncTaskDescription;
    public string Category => Resources.PluginName;

    private async Task SyncMedia(bool deleteAfterSync, CancellationToken cancellationToken = default)
    {
        var media = userManager.Users.SelectMany(user =>
                libraryManager.GetItemList(new InternalItemsQuery(user)
                {
                    IncludeItemTypes = [BaseItemKind.Book],
                    ExcludeTags = BookSync.PluginConfiguration.GetExcludedTags(),
                    IsVirtualItem = false,
                    OrderBy = [(ItemSortBy.DateCreated, SortOrder.Descending)]
                }))
            .DistinctBy(item => item.Id)
            .ToArray();

        foreach (var item in media)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Synchronization cancellation requested.");
                return;
            }

            logger.LogInformation("Uploading item '{Name}' to cloud.", item.Name);
            var uploadSuccess = await client.UploadMedia(item.Path, cancellationToken);

            if (!deleteAfterSync || !uploadSuccess) continue;

            logger.LogInformation("Deleting item from local library '{Name}'.", item.Name);
            libraryManager.DeleteItem(item, new DeleteOptions { DeleteFileLocation = true }, true);
        }
    }

    private void DeleteReadLocalMedia(CancellationToken cancellationToken)
    {
        var media = userManager.Users.SelectMany(user =>
                libraryManager.GetItemList(new InternalItemsQuery(user)
                {
                    IncludeItemTypes = [BaseItemKind.Book],
                    ExcludeTags = BookSync.PluginConfiguration.GetExcludedTags(),
                    IsVirtualItem = false,
                    IsPlayed = true,
                    OrderBy = [(ItemSortBy.DateCreated, SortOrder.Descending)]
                }))
            .DistinctBy(item => item.Id)
            .ToArray();

        foreach (var item in media)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Synchronization cancellation requested.");
                return;
            }

            logger.LogInformation("Deleting item from local library '{Name}'.", item.Name);
            libraryManager.DeleteItem(item, new DeleteOptions { DeleteFileLocation = true }, true);
        }
    }
}