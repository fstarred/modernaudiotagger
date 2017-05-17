using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltimateMusicTagger
{
    public class Globals
    {
        private Globals()
        {
        }

        private static Globals instance;

        public static Globals GetInstance()
        {
            if (instance == null)
                instance = new Globals();

            return instance;
        }

        public const string UPDATE_URL = "https://dl.dropboxusercontent.com/u/55285635/ultimatemusictagger.xml";

        public const string APP_NAME = "Ultimate Music Tagger";

        //public const string HOME_PAGE = "http://umtagger.codeplex.com/";
    }
}
