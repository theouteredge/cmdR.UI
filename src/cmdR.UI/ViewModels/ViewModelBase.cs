using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace cmdR.UI.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        private readonly Dispatcher _dispatcher;

        public event EventHandler OnLoadStarting;
        public event EventHandler OnComplete;
        public event EventHandler OnError;
        public event EventHandler OnUserCancelled;


        public Dispatcher Dispatcher { get { return _dispatcher; } }


        protected ViewModelBase() {}
        protected ViewModelBase(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }


        public bool IsOnLoadStartingEventSubscribedTo()
        {
            return (OnLoadStarting != null);
        }

        public bool IsOnCompleteEventSubscribedTo()
        {
            return (OnComplete != null);
        }

        public bool IsOnErrorEventSubscribedTo()
        {
            return (OnError != null);
        }

        public bool IsOnUserCancelledEventSubscribedTo()
        {
            return (OnUserCancelled != null);
        }




        public void InvokeOnUIThread(Action action)
        {
            _dispatcher.BeginInvoke((ThreadStart)(() => action.Invoke()));
        }

        public void InvokeOnUIThread<T>(T invokeOn, Action<T> action)
        {
            _dispatcher.BeginInvoke((ThreadStart)(() => action.Invoke(invokeOn)));
        }


        public void InvokeOnBackgroundThread(Action action)
        {
            var bw = new BackgroundWorker();
            bw.DoWork += (sender, args) => action.Invoke();
            bw.RunWorkerAsync();
        }

        public void InvokeOnBackgroundThread<T>(T invokeOn, Action<T> action)
        {
            var bw = new BackgroundWorker();
            bw.DoWork += (sender, args) => action.Invoke(invokeOn);
            bw.RunWorkerAsync();
        }




        protected bool RaiseOnLoadStarting()
        {
            if (OnLoadStarting != null)
            {
                OnLoadStarting(this, null);
                return true;
            }

            return false;
        }

        protected bool RaiseOnCompleteEvent()
        {
            if (OnComplete != null)
            {
                OnComplete(this, null);
                return true;
            }

            return false;
        }

        protected bool RaiseOnErrorEvent(string msg)
        {
            if (OnError != null)
            {
                OnError(this, new ErrorEventArgs(new Exception(msg)));
                return true;
            }

            return false;
        }

        protected bool RaiseOnErrorEvent(Exception ex)
        {
            if (OnError != null)
            {
                OnError(this, new ErrorEventArgs(ex));
                return true;
            }

            return false;
        }

        protected bool RaiseOnUserCancelled()
        {
            if (OnUserCancelled != null)
            {
                OnUserCancelled(this, null);
                return true;
            }

            return false;
        }





        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyChanged)
        {
            if (String.IsNullOrEmpty(propertyChanged) || PropertyChanged == null)
                return;

            InvokeOnUIThread(() => PropertyChanged(this, new PropertyChangedEventArgs(propertyChanged)));
        }
    }

    public class ErrorEventArgs : EventArgs
    {
        private readonly Exception _exception;

        public ErrorEventArgs(Exception exception)
        {
            _exception = exception;
        }

        public Exception GetException()
        {
            return _exception;
        }
    }
}
