using Osmy.Models;
using Osmy.Properties;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Osmy.ViewModels
{
    public class SettingViewViewModel : BindableBase
    {
        public SettingItem<bool> StartWithWindows { get; }
        public SettingItem<TimeSpan> VulnsScanInterval { get; }
        public SettingItem<TimeSpan> HashValidationInterval { get; }

        public ReactiveCommand SaveCommand { get; }
        public ReactiveCommand ResetCommand { get; }

        public SettingViewViewModel()
        {
            StartWithWindows = new SettingItem<bool>(Settings.Default.StartWithWindows,
                startWithWindows =>
                {
                    if (startWithWindows)
                    {
                        StartupRegister.Register();
                    }
                    else
                    {
                        StartupRegister.Unregister();
                    }

                    Settings.Default.StartWithWindows = startWithWindows;
                });

            VulnsScanInterval = new SettingItem<TimeSpan>(Settings.Default.VulnsScanInterval,
                vulnsScanInterval =>
                {
                    Settings.Default.VulnsScanInterval = vulnsScanInterval;
                });

            HashValidationInterval = new SettingItem<TimeSpan>(
                Settings.Default.HashValidationInterval,
                hashValidationInterval =>
                {
                    Settings.Default.HashValidationInterval = hashValidationInterval;
                });

            StartWithWindows.ValueChanged.Subscribe(x => System.Diagnostics.Debug.WriteLine($"StartWithWindows value changed {x}"));
            SaveCommand = YieldSettingItems().CombineLatest(xs => xs.Any(x => x)).ToReactiveCommand(false).WithSubscribe(Save);
            ResetCommand = YieldSettingItems().CombineLatest(xs => xs.Any(x => x)).ToReactiveCommand(false).WithSubscribe(Reset);
        }

        public void Save()
        {
            StartWithWindows.Save();
            VulnsScanInterval.Save();
            HashValidationInterval.Save();
            Settings.Default.Save();
        }

        public void Reset()
        {
            StartWithWindows.Reset();
            VulnsScanInterval.Reset();
            HashValidationInterval.Reset();
        }

        private IEnumerable<IObservable<bool>> YieldSettingItems()
        {
            yield return StartWithWindows.ValueChanged;
            yield return VulnsScanInterval.ValueChanged;
            yield return HashValidationInterval.ValueChanged;
        }
    }

    public class SettingItem<T> : ReactiveProperty<T>
    {
        private readonly Action<T> _saveAction;
        private readonly ReactivePropertySlim<T> _initialValue;

        public IObservable<bool> ValueChanged { get; }

        public SettingItem(T initialValue, Action<T> saveAction) : base(initialValue)
        {
            _initialValue = new ReactivePropertySlim<T>(initialValue);
            ValueChanged = this.CombineLatest(_initialValue, IsValueChanged);
            _saveAction = saveAction;
        }

        public void Save()
        {
            _saveAction(Value);
            _initialValue.Value = Value;
        }

        public void Reset()
        {
            Value = _initialValue.Value;
        }

        private bool IsValueChanged(T a, T b)
        {
            return b is null ? a is not null : !b.Equals(a);
        }
    }
}
