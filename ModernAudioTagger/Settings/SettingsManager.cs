using MicroMvvm;
using ModernAudioTagger.Provider;
using ModernAudioTagger.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;

namespace ModernAudioTagger.Settings
{
    public class SettingsManager
    {
        #region EventHandler

        public static event EventHandler SettingsSaved;
        public static event EventHandler SettingsReloaded;
        
        #endregion

        #region Singleton

        public static SettingsManager Instance
        {
            get
            {
                return instance ?? (instance = new SettingsManager());
            }
        }

        private static SettingsManager instance;
        
        #endregion

        #region Commands

        static RelayCommand saveNetworkCommand;
        static RelayCommand loadNetworkCommand;
        static RelayCommand saveMainCommand;
        static RelayCommand loadMainCommand;

        public ICommand SaveNetworkCommand
        {
            get { return saveNetworkCommand ?? (saveNetworkCommand = new RelayCommand(SaveNetwork)); }
        }

        public ICommand LoadNetworkCommand
        {
            get { return loadNetworkCommand ?? (loadNetworkCommand = new RelayCommand(LoadNetwork)); }
        }

        public ICommand SaveMainCommand
        {
            get { return saveMainCommand ?? (saveMainCommand = new RelayCommand(SaveMain)); }
        }

        public ICommand LoadMainCommand
        {
            get { return loadMainCommand ?? (loadMainCommand = new RelayCommand(LoadMain)); }
        }
        
        #endregion

        #region Methods

        void SaveMain()
        {
            AppSettings settings = (AppSettings)SettingsLocator.Instance.App;
            MusicDbViewModel musicDbVM = ViewModelLocator.Instance.MusicDbVM;
            SettingsAppearanceViewModel settingsAppearanceVM = ViewModelLocator.Instance.SettingsAppVM;
            LocalSystemViewModel localSystemVM = ViewModelLocator.Instance.LocalSystemVM;
            MediaViewModel mediaVM = ViewModelLocator.Instance.MediaVM;

            settings.DbSite = musicDbVM.SelectedDb;
            settings.SelectedPath = localSystemVM.SelectedPath;

            settings.SelectedAccentColor = settingsAppearanceVM.SelectedAccentColor;
            settings.SelectedThemeIndex = settingsAppearanceVM.Themes.IndexOf(settingsAppearanceVM.SelectedTheme);
            settings.SelectedFontSize = settingsAppearanceVM.SelectedFontSize;

            settings.ExternalApplicationPath = mediaVM.ExternalApplicationPath;
            settings.CommandArguments = mediaVM.CommandArguments;
            settings.OpenMediaMode = mediaVM.OpenMediaMode.ToString();

            settings.Save();
        }

        void LoadMain()
        {
            AppSettings settings = (AppSettings)SettingsLocator.Instance.App;
            MusicDbViewModel musicDbVM = ViewModelLocator.Instance.MusicDbVM;
            LocalSystemViewModel localSystemVM = ViewModelLocator.Instance.LocalSystemVM;
            MediaViewModel mediaVM = ViewModelLocator.Instance.MediaVM;

            SettingsAppearanceViewModel settingsAppearanceVM = ViewModelLocator.Instance.SettingsAppVM;

            musicDbVM.SelectedDb = settings.DbSite;
            localSystemVM.SelectedPath = settings.SelectedPath;
            mediaVM.ExternalApplicationPath = settings.ExternalApplicationPath;
            mediaVM.CommandArguments = settings.CommandArguments;
            mediaVM.OpenMediaMode = (MediaViewModel.OPEN_MEDIA_MODE)Enum.Parse(typeof(MediaViewModel.OPEN_MEDIA_MODE), settings.OpenMediaMode);

            settingsAppearanceVM.SelectedAccentColor = settings.SelectedAccentColor;
            settingsAppearanceVM.SelectedTheme = settingsAppearanceVM.Themes[settings.SelectedThemeIndex];
            settingsAppearanceVM.SelectedFontSize = settings.SelectedFontSize;
        }

        void SaveNetwork()
        {
            NetworkSettings settings = (NetworkSettings)SettingsLocator.Instance.Network;
            NetworkViewModel networkVM = ViewModelLocator.Instance.NetworkVM;

            settings.EnableProxy = networkVM.EnableProxy;
            settings.Host = networkVM.Host;
            settings.Port = networkVM.Port;
            settings.ProxyDomain = networkVM.Domain;
            settings.ProxyPassword = networkVM.Password;
            settings.ProxyUser = networkVM.User;

            settings.Save();

            if (SettingsSaved != null)
                SettingsSaved(settings, EventArgs.Empty);
        }

        void LoadNetwork()
        {
            NetworkSettings settings = (NetworkSettings)SettingsLocator.Instance.Network;
            NetworkViewModel networkVM = ViewModelLocator.Instance.NetworkVM;

            networkVM.EnableProxy = settings.EnableProxy;
            networkVM.Host = settings.Host;
            networkVM.Port = settings.Port;
            networkVM.Domain = settings.ProxyDomain;
            networkVM.Password = settings.ProxyPassword;
            networkVM.User = settings.ProxyUser;

            if (SettingsReloaded != null)
                SettingsReloaded(settings, EventArgs.Empty);
        }

        
        #endregion
    }
}
