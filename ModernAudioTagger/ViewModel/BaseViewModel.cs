using MicroMvvm;
using System;
using System.Linq.Expressions;
using System.Windows.Threading;

namespace ModernAudioTagger.ViewModel
{
    public class BaseViewModel : ObservableObject
    {
        public Func<bool, string[]> ShowOpenFilesDlg
        {
            protected get;
            set;
        }

        public Func<string, string> OpenSaveFileDlg
        {
            protected get;
            set;
        }

        private Dispatcher _rootDispatcher = null;

        public Dispatcher RootDispatcher
        {
            get
            {
                _rootDispatcher = _rootDispatcher ??
                    (App.Current != null
                        ? App.Current.Dispatcher
                        : Dispatcher.CurrentDispatcher);
                return _rootDispatcher;
            }
            // unit tests can get access to this via InternalsVisibleTo
            internal set
            {
                _rootDispatcher = value;
            }
        }

        protected void RaiseObjectChangedSafeInvoker<T>(Expression<Func<T>> propertyExpresssion)
        {
            if (RootDispatcher.CheckAccess())
            {
                // do work on UI thread
                RaisePropertyChanged(propertyExpresssion);
            }
            else
            {
                // or BeginInvoke()
                RootDispatcher.Invoke((Action)delegate()
                {
                    RaisePropertyChanged(propertyExpresssion);
                });
            }
        }

        protected void RaiseEventInvoker(EventHandler eve)
        {
            RaiseEventInvoker(eve, EventArgs.Empty);
        }

        protected void RaiseEventInvoker(EventHandler eve, EventArgs args)
        {
            if (RootDispatcher.CheckAccess())
            {
                if (eve != null)
                    eve.Invoke(this, args);
            }
            else
            {
                RootDispatcher.Invoke((Action)delegate()
                {
                    if (eve != null)
                        eve.Invoke(this, args);
                });
            }
        }

        protected void RaiseEventBeginInvoker(EventHandler eve)
        {
            if (RootDispatcher.CheckAccess())
            {
                if (eve != null)
                    eve.BeginInvoke(this, EventArgs.Empty, null, null);
            }
            else
            {
                RootDispatcher.BeginInvoke((Action)delegate()
                {
                    if (eve != null)
                        eve.Invoke(this, EventArgs.Empty);
                });
            }
        }
    }
}
