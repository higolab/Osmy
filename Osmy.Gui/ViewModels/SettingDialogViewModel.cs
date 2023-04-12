using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Osmy.Gui.ViewModels
{
    public class SettingDialogViewModel : ViewModelBase
    {
        public ReactiveUI.ReactiveCommand<string, bool> CloseDialogCommand { get; }

        public SettingDialogViewModel()
        {
            CloseDialogCommand = ReactiveUI.ReactiveCommand.Create<string, bool>(CloseDialog/*, TODO */);
        }

        public bool CloseDialog(string parameter)
        {
            if (parameter.Equals("ok", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
