using ModernAudioTagger.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernAudioTagger.Provider
{
    public class ViewModelLocator
    {
        #region Constructor

        private ViewModelLocator()
        {
            MainVM = new MainViewModel();
            MusicDbVM = new MusicDbViewModel();
            TrackTagVM = new TrackTagViewModel();
            NetworkVM = new NetworkViewModel();
            MediaVM = new MediaViewModel();
            LocalSystemVM = new LocalSystemViewModel();
            RenameVM = new RenameViewModel();
            SettingsAppVM = new SettingsAppearanceViewModel();
            BatchHelperVM = new BatchHelperViewModel();            
        }
        
        #endregion

        #region Singleton Instance

        private static ViewModelLocator instance;

        public static ViewModelLocator Instance
        {
            get
            {
                return instance ?? (instance = new ViewModelLocator());
            }
        }
        
        #endregion

        #region Properties

        public MainViewModel MainVM
        {
            get;
            private set;
        }

        public BatchHelperViewModel BatchHelperVM
        {
            get;
            private set;
        }

        public SettingsAppearanceViewModel SettingsAppVM
        {
            get;
            private set;
        }

        public MusicDbViewModel MusicDbVM
        {
            get;
            private set;
        }

        public TrackTagViewModel TrackTagVM 
        { 
            get; 
            private set; 
        }

        public NetworkViewModel NetworkVM
        {
            get;
            private set;
        }

        public LocalSystemViewModel LocalSystemVM
        {
            get;
            private set;
        }

        public RenameViewModel RenameVM
        {
            get;
            private set;
        }

        public MediaViewModel MediaVM
        {
            get;
            private set;
        }
        
        #endregion
    }
}
