using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Osmy.Gui.ViewModels;
using System;

namespace Osmy.Gui
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            var name = param?.GetType().FullName?.Replace("ViewModel", "View");
            if(name == null)
            {
                return new TextBlock { Text = "Not Found" };
            }

            var type = Type.GetType(name);
            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
