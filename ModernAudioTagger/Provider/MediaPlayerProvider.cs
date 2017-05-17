using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ModernAudioTagger.Provider
{
    class MediaPlayerProvider
    {

        #region Constructor

        private MediaPlayerProvider()
        {
            Player = new MediaPlayer();
        }
        
        #endregion

        #region Singleton Instance

        private static MediaPlayerProvider instance;

        public static MediaPlayerProvider Instance
        {
            get
            {
                return instance ?? (instance = new MediaPlayerProvider());
            }
        }

        #endregion

        #region Properties

        public MediaPlayer Player { get; private set; }

        #endregion


    }
}
