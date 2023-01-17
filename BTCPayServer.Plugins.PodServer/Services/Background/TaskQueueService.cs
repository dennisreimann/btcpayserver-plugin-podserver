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

public class TaskQueueService : BackgroundService
{
    private readonly ImportRepository _importRepository;
    private readonly ILogger<TaskQueueService> _logger;
    private readonly ITaskQueue _taskQueue;
    private readonly FeedImporter _feedImporter;

    public TaskQueueService(
        ImportRepository importRepository,
        FeedImporter feedImporter,
        ITaskQueue taskQueue,
        ILogger<TaskQueueService> logger)
    {
        _importRepository = importRepository;
        _feedImporter = feedImporter;
        _taskQueue = taskQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var iss = await _importRepository.GetUnfinishedImports();

        IEnumerable<Import> enumerable = iss.ToList();
        _logger.LogInformation("Starting with {Count} remaining imports", enumerable.Count());

        foreach (var import in enumerable)
        {
            await _taskQueue.QueueAsync(cancelToken => _feedImporter.Import(import.ImportId, cancellationToken));
        }

        await BackgroundProcessing(cancellationToken);
    }

    private async Task BackgroundProcessing(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var task = await _taskQueue.DequeueAsync(cancellationToken);

            try
            {
                await task(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Processing {Task} failed: {Message}", nameof(task), exception.Message);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping");

        await base.StopAsync(cancellationToken);
    }
}
