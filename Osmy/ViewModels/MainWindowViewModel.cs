using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Osmy.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        //private readonly CompositeDisposable _disposables = new();
        private readonly IRegionManager _regionManager;

        public ReactivePropertySlim<string> Title { get; } = new("Osmy");

        public ReactiveCommand<string> PageChangeCommand { get; }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            PageChangeCommand = new ReactiveCommand<string>().WithSubscribe(PageChanged);
        }

        public void PageChanged(string pageName)
        {
            _regionManager.RequestNavigate("ContentRegion", "SbomListView", OnNavigated);
        }

        public void OnNavigated(NavigationResult result)
        {
            System.Diagnostics.Debug.WriteLine(result.Result);
            System.Diagnostics.Debug.WriteLine(result.Error);
        }
    }
}
