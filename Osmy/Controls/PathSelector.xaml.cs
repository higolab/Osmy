using Microsoft.WindowsAPICodePack.Dialogs;
using Osmy.Models.Sbom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Osmy.Controls
{
    /// <summary>
    /// PathSelector.xaml の相互作用ロジック
    /// </summary>
    public partial class PathSelector : UserControl
    {
        public PathSelector()
        {
            InitializeComponent();
        }

        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }
        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register("SelectedPath", typeof(string), typeof(PathSelector),
                new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public PathSelectionMode PathSelectionMode
        {
            get { return (PathSelectionMode)GetValue(PathSelectionModeProperty); }
            set { SetValue(PathSelectionModeProperty, value); }
        }
        public static readonly DependencyProperty PathSelectionModeProperty =
            DependencyProperty.Register("PathSelectionMode", typeof(PathSelectionMode), typeof(PathSelector), new PropertyMetadata(PathSelectionMode.File));

        public ICommand? PathSelectedCommand
        {
            get { return (ICommand?)GetValue(PathSelectedCommandProperty); }
            set { SetValue(PathSelectedCommandProperty, value); }
        }
        public static readonly DependencyProperty PathSelectedCommandProperty =
            DependencyProperty.Register("PathSelectedCommand", typeof(ICommand), typeof(PathSelector), new PropertyMetadata(default));

        public object? PathSelectedCommandParameter
        {
            get { return GetValue(PathSelectedCommandParameterProperty); }
            set { SetValue(PathSelectedCommandParameterProperty, value); }
        }
        public static readonly DependencyProperty PathSelectedCommandParameterProperty =
            DependencyProperty.Register("PathSelectedCommandParameter", typeof(object), typeof(PathSelector), new PropertyMetadata(default));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (PathSelectionMode)
            {
                case PathSelectionMode.File:
                    SelectPath(false);
                    break;
                case PathSelectionMode.Directory:
                    SelectPath(true);
                    break;
            }
        }

        private void SelectPath(bool isDirectory)
        {
            var dialog = new CommonOpenFileDialog
            {
                Multiselect = false,
                IsFolderPicker = isDirectory,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SelectedPath = dialog.FileName;
                PathSelectedCommand?.Execute(PathSelectedCommandParameter);
            }
        }
    }

    public enum PathSelectionMode
    {
        File,
        Directory,
    }
}
