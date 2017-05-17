using MicroMvvm;
using ModernAudioTagger.BusinessLogic;
using ModernAudioTagger.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModernAudioTagger.ViewModel
{
    public class NetworkViewModel : BaseViewModel
    {
        #region EventHandler

        //public event EventHandler SettingsSaved;
        //public event EventHandler SettingsReloaded;

        public event EventHandler SoftwareOutOfDateEvent;
        public event EventHandler SoftwareUpToDateEvent;
        public event EventHandler NetworkErrorEvent;

        public event EventHandler ProxyChanged;

        #endregion

        #region Fields

        //private ServiceUpdater updater;

        #endregion

        #region Properties

        public string AppUri { get; set; }

        private bool enableProxy;

        public bool EnableProxy
        {
            get { return enableProxy; }
            set
            {
                enableProxy = value;
                RaisePropertyChanged(() => EnableProxy);
            }
        }

        private string host;

        public string Host
        {
            get { return host; }
            set
            {
                host = value;
                RaisePropertyChanged(() => EnableProxy);
            }
        }

        private int port;

        public int Port
        {
            get { return port; }
            set
            {
                port = value;
                RaisePropertyChanged(() => Port);
            }
        }

        private string user;

        public string User
        {
            get { return user; }
            set
            {
                user = value;
                RaisePropertyChanged(() => User);
            }
        }


        private string password;

        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        private string domain;

        public string Domain
        {
            get { return domain; }
            set
            {
                domain = value;
                RaisePropertyChanged(() => Domain);
            }
        }

        #endregion

        #region Methods

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                if (this.GetType().GetProperty(columnName).Name.Equals(columnName))
                {
                }

                return null;
            }
        }

        void CheckAppUpdatesExecute()
        {
            //if (ModernUIHelper.IsInDesignMode == false)
            //{
            WebProxy proxy = null;

            if (enableProxy)
            {
                proxy = new WebProxy(host, port);
                proxy.Credentials = new NetworkCredential(user, password, domain);
            }

            ServiceUpdater updater = new ServiceUpdater(proxy);

            Task.Factory.StartNew(() =>
                {
                    ServiceUpdater.VersionInfo version = null;

                    try
                    {
                        version = updater.CheckForUpdates(AppUri);
                    }
                    catch (Exception)
                    {

                    }

                    return version;

                }).ContinueWith((task) =>
                {
                    ServiceUpdater.VersionInfo latestVersionInfo = task.Result;

                    if (latestVersionInfo != null)
                    {
                        Version productVersion = Utility.GetProductVersion();

                        bool isVersionUpToDate = latestVersionInfo.LatestVersion <= productVersion;

                        RaiseEventInvoker(isVersionUpToDate ? SoftwareUpToDateEvent : SoftwareOutOfDateEvent, new Model.DefaultEventArgs { ObjArg = latestVersionInfo });
                    }
                    else
                    {
                        RaiseEventInvoker(NetworkErrorEvent);
                    }
                });

        }

        void OnProxyChanged()
        {
            if (enableProxy)
            {
                WebProxy proxy = new WebProxy(host, port);
                UltimateMusicTaggerLocator.Instance.UMTagger.SetProxy(proxy);
                proxy.Credentials = new NetworkCredential(user, password, domain);
                Utility.SetDefaultProxy(proxy);
            }
            else
            {
                Utility.SetDefaultProxy(null);
                UltimateMusicTaggerLocator.Instance.UMTagger.SetProxy(null);
            }

            RaiseEventInvoker(ProxyChanged);
        }

        #endregion

        #region Commands

        //public ICommand ReloadNetworkSettings { get { return new RelayCommand(ReloadNetworkSettingsExecuted); } }
        //public ICommand SaveNetworkSettings { get { return new RelayCommand(SaveNetworkSettingsExecuted); } }

        public ICommand CheckAppUpdatesCommand
        { 
            get { return checkAppUpdatesCommand ?? (checkAppUpdatesCommand = new RelayCommand(CheckAppUpdatesExecute)); } 
        }

        RelayCommand proxyChangedCommand;
        RelayCommand checkAppUpdatesCommand;

        public ICommand ProxyChangedCommand
        {
            get { return proxyChangedCommand ?? (proxyChangedCommand = new RelayCommand(OnProxyChanged)); }
        }

        #endregion
    }
}
