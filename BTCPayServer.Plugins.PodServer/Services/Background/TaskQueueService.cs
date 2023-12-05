using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Imports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BTCPayServer.Plugins.PodServer.Services.Background;

public class TaskQueueService(
    ImportRepository importRepository,
    FeedImporter feedImporter,
    ITaskQueue taskQueue,
    ILogger<TaskQueueService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var iss = await importRepository.GetUnfinishedImports();

        IEnumerable<Import> enumerable = iss.ToList();
        logger.LogInformation("Starting with {Count} remaining imports", enumerable.Count());

        foreach (var import in enumerable)
        {
            await taskQueue.QueueAsync(cancelToken => feedImporter.Import(import.ImportId, cancellationToken));
        }

        await BackgroundProcessing(cancellationToken);
    }

    private async Task BackgroundProcessing(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var task = await taskQueue.DequeueAsync(cancellationToken);

            try
            {
                await task(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Processing {Task} failed: {Message}", nameof(task), exception.Message);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping");

        await base.StopAsync(cancellationToken);
    }
}
