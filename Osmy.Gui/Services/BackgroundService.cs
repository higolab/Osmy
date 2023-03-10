using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Osmy.Services
{
    public abstract class BackgroundService : IDisposable
    {
        private CancellationTokenSource? _cancellationTokenSorce;
        private Task? _executeTask;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSorce = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _executeTask = ExecuteAsync(_cancellationTokenSorce.Token);

            if (_executeTask.IsCompleted)
            {
                return _executeTask;
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executeTask is null)
            {
                return;
            }

            try
            {
                _cancellationTokenSorce?.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executeTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
        }

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
