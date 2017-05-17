using ModernAudioTagger.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernAudioTagger.Provider
{
    public class SettingsLocator
    {
        #region Constructor

        public SettingsLocator()
        {
            Network = new NetworkSettings();
            App = new AppSettings();
        }
        
        #endregion

        #region Singleton Instance

        private static SettingsLocator instance;

        public static SettingsLocator Instance
        {
            get
            {
                return instance ?? (instance = new SettingsLocator());
            }
        }

        #endregion

        #region Properties

        public NetworkSettings Network
        {
            get;
            private set;
        }

        public AppSettings App
        {
            get;
            private set;
        }
        
        #endregion

    }
}
