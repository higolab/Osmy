using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Osmy.Gui.Controls
{
    [TemplatePart(SelectButtonName, typeof(Button))]
    public partial class PathSelector : TemplatedControl
    {
        private const string SelectButtonName = "PART_SelectButton";

        private Button? _selectButton;

        public PathSelector()
        {
            InitializeComponent();

        }

        public string? SelectedPath
        {
            get { return _selectedPath; }
            set { SetAndRaise(SelectedPathProperty, ref _selectedPath, value); }
        }
        private string? _selectedPath;
        public static readonly DirectProperty<PathSelector, string?> SelectedPathProperty =
            AvaloniaProperty.RegisterDirect<PathSelector, string?>(
                nameof(SelectedPath),
                o => o.SelectedPath,
                (o, v) => o.SelectedPath = v,
                defaultBindingMode: BindingMode.TwoWay,
                enableDataValidation: true);

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

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (_selectButton is not null)
            {
                _selectButton.Click -= OnSelectButtonClicked;
            }

            _selectButton = e.NameScope.Find(SelectButtonName) as Button;

            if (_selectButton is not null)
            {
                _selectButton.Click += OnSelectButtonClicked;
            }
        }

        protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
        {
            base.UpdateDataValidation(property, state, error);

            if (property == SelectedPathProperty)
            {
                DataValidationErrors.SetError(this, error);
            }
        }

        private void OnSelectButtonClicked(object? sender, RoutedEventArgs e) => SelectPath();

        private async void SelectPath()
        {
            switch (PathSelectionMode)
            {
                case PathSelectionMode.File:
                    await SelectFile();
                    break;
                case PathSelectionMode.Directory:
                    await SelectFolder();
                    break;
            }
        }

        private async Task SelectFile()
        {
            var storageProvider = GetStorageProvider();

            var options = new FilePickerOpenOptions
            {
                AllowMultiple = false,
                SuggestedStartLocation = await FetchFolderOfSelectedPath()
            };

            var files = await storageProvider.OpenFilePickerAsync(options);
            if (files.Count != 0)
            {
                SelectedPath = files[0].Path.LocalPath;
                ExecutePathSelectedCommand();
            }
        }

        private async Task SelectFolder()
        {
            var storageProvider = GetStorageProvider();

            var options = new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                SuggestedStartLocation = await FetchFolderOfSelectedPath()
            };

            var folders = await storageProvider.OpenFolderPickerAsync(options);
            if (folders.Count != 0)
            {
                SelectedPath = folders[0].Path.LocalPath;
                ExecutePathSelectedCommand();
            }
        }

        private async Task<IStorageFolder?> FetchFolderOfSelectedPath()
        {
            var storageProvider = GetStorageProvider();
            var dir = Directory.Exists(SelectedPath) ? SelectedPath : Path.GetDirectoryName(SelectedPath);

            return dir is null ? null : await storageProvider.TryGetFolderFromPathAsync(dir);
        }

        private IStorageProvider GetStorageProvider()
        {
            var topLevel = TopLevel.GetTopLevel(this)!;
            return topLevel.StorageProvider;
        }

        private void ExecutePathSelectedCommand()
        {
            PathSelectedCommand?.Execute(PathSelectedCommandParameter);
        }
    }

    public enum PathSelectionMode
    {
        File,
        Directory,
    }
}
