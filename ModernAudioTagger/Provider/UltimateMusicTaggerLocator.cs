using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernAudioTagger.Provider
{
    class UltimateMusicTaggerLocator
    {
        
        #region Constructor

        private UltimateMusicTaggerLocator()
        {
            this.UMTagger = new UltimateMusicTagger.UltiMp3Tagger();
        }
        
        #endregion

        #region Singleton Instance

        private static UltimateMusicTaggerLocator instance;

        public static UltimateMusicTaggerLocator Instance
        {
            get
            {
                return instance ?? (instance = new UltimateMusicTaggerLocator());
            }
        }

        #endregion

        #region Properties

        public UltimateMusicTagger.UltiMp3Tagger UMTagger
        {
            get;
            private set;
        }
        
        #endregion
    }
}
