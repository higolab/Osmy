﻿using Prism.Commands;
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

        public DelegateCommand<string> PageChangeCommand => _pageChangeCommand ??= new DelegateCommand<string>(PageChanged);
        private DelegateCommand<string>? _pageChangeCommand;

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void PageChanged(string pageName)
        {
            switch (pageName)
            {
                case "softwares":
                    _regionManager.RequestNavigate("ContentRegion", "SbomListView", OnNavigated);
                    break;
            }
        }

        public void OnNavigated(NavigationResult result)
        {
            System.Diagnostics.Debug.WriteLine(result.Result);
            System.Diagnostics.Debug.WriteLine(result.Error);
        }
    }
}