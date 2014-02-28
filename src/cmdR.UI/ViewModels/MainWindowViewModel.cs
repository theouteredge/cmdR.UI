using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ServiceStack.Text;
using cmdR.CommandParsing;
using cmdR.UI.Properties;

namespace cmdR.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IWpfViewModel
    {
        private readonly MainWindow _window;
        private readonly CmdR _cmdR;

        public string Command { get; set; }
        public string Output { get; set; }
        public string Prompt { get { return _cmdR == null ? "c:\\users\\andy\\" : _cmdR.State.CmdPrompt; } set { } }

        public IList<string> CommandHistory { get; set; }
        public int? CommandHistoryPointer { get; set; }

        public AppSettings Settings { get; set; }


        public MainWindowViewModel()
        {
            Command = "";
            Output = "";
            CommandHistory = new List<string>();
        }

        public MainWindowViewModel(Dispatcher dispatcher, MainWindow window) : base(dispatcher)
        {
            _window = window;
            Command = "";
            Output = "";
            CommandHistory = new List<string>();
            CommandHistoryPointer = null;

            LoadSettings();

            _cmdR = new CmdR(console: new WpfConsole(this));
            _cmdR.Console.WriteLine("<Run FontWeight=\"Bold\" Foreground=\"#FFE671B8\">Welcome to CmdR :D</Run>");

            _cmdR.State.CmdPrompt = Settings.Path ?? GetUserDirectory();
            _cmdR.State.Variables.Add("path", Settings.Path ?? GetUserDirectory());

            CommandHistory = Settings.Last10Commands ?? new List<string>();
            
            InvokeOnBackgroundThread(() => {
                    _cmdR.Console.WriteLine("Discovering commands, please wait...");
                    _cmdR.AutoRegisterCommands();
                    _cmdR.Console.WriteLine("{0} commands registered", _cmdR.State.Routes.Count);
                    
                    NotifyPropertyChanged("Output");
                    NotifyPropertyChanged("Prompt");
                });
        }

        public void SaveSettings()
        {
            Settings.Path = (string)_cmdR.State.Variables["path"];
            Settings.Last10Commands = CommandHistory.Skip(CommandHistory.Count - 10)
                                                    .Take(10)
                                                    .ToList();

            File.WriteAllText("settings.json", Settings.ToJson());
        }

        private void LoadSettings()
        {
            var settings = "";

            if (File.Exists("settings.json"))
                settings = File.ReadAllText("settings.json");

            Settings = string.IsNullOrEmpty(settings) ? new AppSettings() : settings.FromJson<AppSettings>();
        }

        private string GetUserDirectory()
        {
            return Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)).FullName;
        }


        public void RaiseOutputChanged()
        {
            NotifyPropertyChanged("Output");
        }


        public void HandleReturnKeyPressed()
        {
            if (string.IsNullOrEmpty(Command))
            {
                _cmdR.Console.WriteLine("");
                return;
            }

            CommandHistoryPointer = null;

            InvokeOnBackgroundThread(() =>
                {
                    var command = Command;

                    try
                    {
                        CommandHistory.Add(Command);

                        if (Command.StartsWith("?") || Command.StartsWith("help"))
                            _cmdR.Console.WriteLine("<Run FontWeight=\"Bold\">\n{0}\n</Run>", Command.XmlEscape());
                        else
                            _cmdR.Console.WriteLine("<Run FontWeight=\"Bold\">\n{0}</Run>", Command.XmlEscape());

                        
                        Command = string.Empty;

                        NotifyPropertyChanged("Output");
                        NotifyPropertyChanged("Command");
                        
                        InvokeOnUIThread(() => _window._output.IsReadOnly = true);
                        _cmdR.ExecuteCommand(command);
                        InvokeOnUIThread(() => _window._output.IsReadOnly = false);
                        
                        Command = string.Empty;
                    }
                    catch (Exception e)
                    {
                        _cmdR.Console.WriteLine("An exception was thrown while running your command");
                        _cmdR.Console.WriteLine(" {0}\n", e.Message);

                        Command = command;
                    }

                    NotifyPropertyChanged("Output");
                    NotifyPropertyChanged("Command");
                    NotifyPropertyChanged("Prompt");
                });
        }

        public void HandleUpKeyPress()
        {
            // check to see if we have any history to cycle through before we do anything
            if (CommandHistory.Count() == 0)
                return;

            // if we haven't cycled yet and we are pressing Down we probably want to see the last command
            if (CommandHistoryPointer == null)
                CommandHistoryPointer = CommandHistory.Count()-1;
            else
                CommandHistoryPointer -= 1;

            if (CommandHistoryPointer < 0)
                CommandHistoryPointer = CommandHistory.Count() - 1;

            if (CommandHistoryPointer < CommandHistory.Count())
            {
                Command = CommandHistory[CommandHistoryPointer.Value];
                NotifyPropertyChanged("Command");
            }
        }

        public void HandleDownKeyPress()
        {
            // check to see if we have any history to cycle through before we do anything
            if (CommandHistory.Count() == 0)
                return;

            // if we haven't cycled yet and we are pressing Up we probably want to start at the begining
            if (CommandHistoryPointer == null)
                CommandHistoryPointer = 0;
            else
                CommandHistoryPointer += 1;

            if (CommandHistoryPointer >= CommandHistory.Count())
                CommandHistoryPointer = 0;

            if (CommandHistoryPointer < CommandHistory.Count())
            {
                Command = CommandHistory[CommandHistoryPointer.Value];
                NotifyPropertyChanged("Command");
            }
        }

        public void HandleTabKeyPress()
        {
            // todo: show a dropdown or autocomplete the current bit of text being types into the command prompt
        }
    }
}
