using System.Windows;
using MahApps.Metro.Controls;
using Prism.Regions;
using Prism.Ioc;
using System;

namespace Osmy.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string ContentRegionName = "ContentRegion";

        public MainWindow()
        {
            InitializeComponent();

            RegisterRegions();
        }

        private void MetroWindow_SourceInitialized(object sender, EventArgs e)
        {
            if (HamburgerMenuControl.SelectedItem is HamburgerMenuItem menuItem)
            {
                menuItem.Command.Execute(menuItem.CommandParameter);
            }
        }

        private void HamburgerMenuControl_OnItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs args)
        {
            if (!args.IsItemOptions && HamburgerMenuControl.IsPaneOpen)
            {
                // You can close the menu if an item was selected
                HamburgerMenuControl.SetCurrentValue(HamburgerMenu.IsPaneOpenProperty, false);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            UnregisterRegions();
        }

        private void RegisterRegions()
        {
            RegionManager.SetRegionName(_content, ContentRegionName);
            var regionManager = ContainerLocator.Current.Resolve<IRegionManager>();
            RegionManager.SetRegionManager(_content, regionManager);
        }

        private void UnregisterRegions()
        {
            var regionManager = ContainerLocator.Current.Resolve<IRegionManager>();
            regionManager.Regions.Remove(ContentRegionName);
        }
    }
}
