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
        private readonly Dictionary<Type, BackgroundService> _services = new();

        public void Register<T>(T backgroundService) where T : BackgroundService
        {
            _services.Add(typeof(T), backgroundService);
        }

        public T Resolve<T>() where T : BackgroundService
        {
            return (T)_services[typeof(T)];
        }

        public async Task StartAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var tasks = _services.Values.Select(service => service.StartAsync(cancellationTokenSource.Token));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            foreach (BackgroundService service in _services.Values)
            {
                await service.StopAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }
    }
}
