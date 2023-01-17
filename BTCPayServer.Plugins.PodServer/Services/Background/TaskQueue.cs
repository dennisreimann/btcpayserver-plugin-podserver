using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BTCPayServer.Plugins.PodServer.Services.Background;

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio#queued-background-tasks
public class TaskQueue : ITaskQueue
{
    private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

    public TaskQueue(int capacity)
    {
        // Capacity should be set based on the expected application load and
        // number of concurrent threads accessing the queue.
        // BoundedChannelFullMode.Wait will cause calls to WriteAsync() to return a task,
        // which completes only when space became available. This leads to backpressure,
        // in case too many publishers/calls start accumulating.
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
    }

    public async ValueTask QueueAsync(Func<CancellationToken, ValueTask> task)
    {
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        await _queue.Writer.WriteAsync(task);
    }

    public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken) =>
        await _queue.Reader.ReadAsync(cancellationToken);
}
