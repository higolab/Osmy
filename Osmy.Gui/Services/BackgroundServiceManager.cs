using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Osmy.Gui.Services
{
    internal class BackgroundServiceManager
    {
        private readonly Dictionary<Type, BackgroundService> _services = new();

        public static BackgroundServiceManager Instance => _instance ??= new BackgroundServiceManager();
        private static BackgroundServiceManager? _instance;

        private BackgroundServiceManager() { }

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
