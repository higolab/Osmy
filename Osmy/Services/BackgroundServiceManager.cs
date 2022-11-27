using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Osmy.Services
{
    internal class BackgroundServiceManager
    {
        private readonly HashSet<BackgroundService> _services = new();

        public void Register(BackgroundService backgroundService)
        {
            _services.Add(backgroundService);
        }

        public async Task StartAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            foreach (BackgroundService service in _services)
            {
                await service.StartAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        public async Task StopAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            foreach (BackgroundService service in _services)
            {
                await service.StopAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }
    }
}
