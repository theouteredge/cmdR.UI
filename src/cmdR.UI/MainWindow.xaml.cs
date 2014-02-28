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
using System.Windows.Threading;
using cmdR.UI.ViewModels;

namespace cmdR.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _model;

        public MainWindow()
        {
            InitializeComponent();

            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, this.OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, this.OnMaximizeWindow, this.OnCanResizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, this.OnMinimizeWindow, this.OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, this.OnRestoreWindow, this.OnCanResizeWindow));

            FocusManager.SetFocusedElement(this, _command);


            _model = new MainWindowViewModel(Dispatcher, this);
            DataContext = _model;

            //var timer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 2)};
            //timer.Tick += ((sender, e) =>
            //    {
            //        if (_scrollViewer.VerticalOffset != _scrollViewer.ScrollableHeight)
                        
            //    });
            //timer.Start();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _model.SaveSettings();

            base.OnClosing(e);
        }

        private void OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return:
                    HandleReturnKeyPress();
                    break;

                case Key.Up:
                    _model.HandleUpKeyPress();
                    break;

                case Key.Down:
                    _model.HandleDownKeyPress();
                    break;

                case Key.Tab:
                    _model.HandleTabKeyPress();
                    break;
            }
        }

        private void HandleUpKeyPress()
        {
            throw new NotImplementedException();
        }

        private void HandleReturnKeyPress()
        {
            // the binding to the ViewModel only updates when focus is lost. this bit of code forces the binding to be updated
            object focusObj = FocusManager.GetFocusedElement(this);
            if (focusObj != null && focusObj is TextBox)
            {
                var binding = (focusObj as TextBox).GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();
            }

            if (!string.IsNullOrEmpty(_model.Command))
            {
                _model.HandleReturnKeyPressed();
                _scrollViewer.ScrollToEnd();
            }
        }


        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode != ResizeMode.NoResize;
        }

        private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }
    }
}
