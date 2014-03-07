using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cmdR.UI.ViewModels;

namespace cmdR.UI.CmdRModules
{
    public class SystemModule : ICmdRModule
    {
        private CmdR _cmdR;

        public SystemModule(CmdR cmdR)
        {
            _cmdR = cmdR;
            _cmdR.RegisterRoute("cls", Cls, "Clears the content of the screen");
        }

        private void Cls(IDictionary<string, string> param, CmdR cmdR)
        {
            var wpfconsole = (WpfConsole)_cmdR.Console;
            if (wpfconsole != null)
            {
                wpfconsole.Clear();
            }
            else _cmdR.Console.WriteLine("Unable to clear the screen, the current CmdR Console is not a WPF Console object... O_o");
        }
    }
}
