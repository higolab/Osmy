using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Osmy.Gui.Services
{
    internal abstract class QueueingBackgroundService<TItem, TResult> : BackgroundService
    {
        private readonly ConcurrentQueue<Request<TItem, TResult>> _manualQueue = new();
        private readonly ConcurrentQueue<Request<TItem, TResult>> _autoQueue = new();

        public TimeSpan ProcessInterval { get; set; } = TimeSpan.FromMilliseconds(500);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartQueueProcessing(stoppingToken), stoppingToken);
        }

        protected async Task<TResult> EnqueueAuto(TItem item, CancellationToken cancellationToken = default)
        {
            var request = _manualQueue.Concat(_autoQueue).FirstOrDefault(x => x?.Value?.Equals(item) ?? false);
            if (request is null)
            {
                request = new Request<TItem, TResult>(item);
                _autoQueue.Enqueue(request);
            }

            using (cancellationToken.Register(() => request.Tcs.TrySetCanceled()))
            {
                return await request.Tcs.Task.ConfigureAwait(false);
            }
        }

        protected async Task<TResult> EnqueueManual(TItem item, CancellationToken cancellationToken = default)
        {
            var request = _manualQueue.FirstOrDefault(x => x?.Value?.Equals(item) ?? false);
            if (request is null)
            {
                request = new Request<TItem, TResult>(item);
                _manualQueue.Enqueue(request);
            }

            using (cancellationToken.Register(() => request.Tcs.TrySetCanceled()))
            {
                return await request.Tcs.Task.ConfigureAwait(false);
            }
        }

        protected abstract Task<TResult> ProcessAsync(TItem item, CancellationToken cancellationToken);

        private async Task StartQueueProcessing(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Request<TItem, TResult>? request = null;
                try
                {
                    if (_manualQueue.TryDequeue(out request) || _autoQueue.TryDequeue(out request))
                    {
                        if (request.Tcs.Task.IsCanceled) { continue; }
                        request.Tcs.TrySetResult(await ProcessAsync(request.Value, cancellationToken).ConfigureAwait(false));
                    }
                    else
                    {
                        await Task.Delay(ProcessInterval, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        request?.Tcs.TrySetException(ex);
                    }
                    catch { }   // TODO logging
                }
            }
        }

        class Request<TIn, TOut>
        {
            public TIn Value { get; }
            public TaskCompletionSource<TOut> Tcs { get; }

            public Request(TIn value)
            {
                Value = value;
                Tcs = new TaskCompletionSource<TOut>();
            }
        }
    }
}
