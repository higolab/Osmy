using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using System.Windows.Input;

namespace Osmy.Gui.Controls
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

        public string? SelectedPath
        {
            get { return GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }
        public static readonly StyledProperty<string?> SelectedPathProperty =
            AvaloniaProperty.Register<PathSelector, string?>("SelectedPath", defaultBindingMode: BindingMode.TwoWay);

        public PathSelectionMode PathSelectionMode
        {
            get { return GetValue(PathSelectionModeProperty); }
            set { SetValue(PathSelectionModeProperty, value); }
        }
        public static readonly StyledProperty<PathSelectionMode> PathSelectionModeProperty =
            AvaloniaProperty.Register<PathSelector, PathSelectionMode>("PathSelectionMode", PathSelectionMode.File);

        public ICommand? PathSelectedCommand
        {
            get { return GetValue(PathSelectedCommandProperty); }
            set { SetValue(PathSelectedCommandProperty, value); }
        }
        public static readonly StyledProperty<ICommand?> PathSelectedCommandProperty =
            AvaloniaProperty.Register<PathSelector, ICommand?>("PathSelectedCommand");

        public object? PathSelectedCommandParameter
        {
            get { return GetValue(PathSelectedCommandParameterProperty); }
            set { SetValue(PathSelectedCommandParameterProperty, value); }
        }
        public static readonly StyledProperty<object?> PathSelectedCommandParameterProperty =
            AvaloniaProperty.Register<PathSelector, object?>("PathSelectedCommandParameter");

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
            // TODO
            //var dialog = new CommonOpenFileDialog
            //{
            //    Multiselect = false,
            //    IsFolderPicker = isDirectory,
            //};

            //if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            //{
            //    SelectedPath = dialog.FileName;
            //    PathSelectedCommand?.Execute(PathSelectedCommandParameter);
            //}
        }
    }

    public enum PathSelectionMode
    {
        File,
        Directory,
    }
}
