using System;
using System.Threading;
using System.Threading.Tasks;

namespace BTCPayServer.Plugins.PodServer.Services.Background;

public interface ITaskQueue
{
    ValueTask QueueAsync(Func<CancellationToken, ValueTask> task);

    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}
