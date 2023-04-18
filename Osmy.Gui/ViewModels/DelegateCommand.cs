using System;
using System.Windows.Input;

namespace Osmy.Gui.ViewModels
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _action;

        public DelegateCommand(Action action)
        {
            _action = action;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _action.Invoke();
        }
    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T?> _action;

        public DelegateCommand(Action<T?> action)
        {
            _action = action;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _action.Invoke((T?)parameter);
        }
    }
}
