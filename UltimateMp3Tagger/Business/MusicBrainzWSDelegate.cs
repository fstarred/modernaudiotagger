using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace UltimateMusicTagger.Business
{
    internal class MusicBrainzWSDelegate : WServiceBase
    {
        private MusicBrainzClient musicBrainzClient;

        public MusicBrainzWSDelegate()
        {
            MusicBrainzClient musicBrainzClient = new MusicBrainzClient();
            musicBrainzClient.UserAgent = String.Format("{0} [ / {1}]", Globals.APP_NAME, MTUtility.GetVersion());

            this.musicBrainzClient = musicBrainzClient;
        }

        public void SetProxy(IWebProxy proxy)
        {
            musicBrainzClient.Proxy = proxy;
        }

        public string GetRecording(string title, string artist)
        {
            string ret = null;

            IDictionary<string, string> paramQuery = new Dictionary<string, string>();
            IDictionary<string, string> param = new Dictionary<string, string>();


            paramQuery.Add("artist", artist);
            paramQuery.Add("recording", title);
            paramQuery.Add("status", "official");
            param.Add("limit", "50");

            ret = musicBrainzClient.SearchMethod("recording", paramQuery, param);
            
            return ret;
        }

        public string GetRecordingList(string release, string artist)
        {
            string ret = null;

            IDictionary<string, string> paramQuery = new Dictionary<string, string>();
            IDictionary<string, string> param = new Dictionary<string, string>();

            paramQuery.Add("artist", artist);
            paramQuery.Add("release", release);
            paramQuery.Add("status", "official");
            param.Add("limit", "50");

            ret = musicBrainzClient.SearchMethod("recording", paramQuery, param);
            
            return ret;
        }

        public string GetRecordingList(string artist)
        {
            string ret = null;

            IDictionary<string, string> paramQuery = new Dictionary<string, string>();
            IDictionary<string, string> param = new Dictionary<string, string>();

            paramQuery.Add("artist", artist);
            paramQuery.Add("status", "official");
            param.Add("limit", "50");

            ret = musicBrainzClient.SearchMethod("recording", paramQuery, param);
            
            return ret;
        }

        public string GetReleaseGroupList(string artist)
        {
            string ret = null;

            IDictionary<string, string> paramQuery = new Dictionary<string, string>();
            IDictionary<string, string> param = new Dictionary<string, string>();

            paramQuery.Add("artist", artist);
            paramQuery.Add("status", "Official");
            param.Add("limit", "50");
            //param.Add("primarytype", "Album");

            ret = musicBrainzClient.SearchMethod("release-group", paramQuery, param);
            
            return ret;
        }

        public string GetReleaseGroup(string album, string artist)
        {
            string ret = null;

            IDictionary<string, string> paramQuery = new Dictionary<string, string>();
            IDictionary<string, string> param = new Dictionary<string, string>();

            paramQuery.Add("artist", artist);
            paramQuery.Add("release", album);
            paramQuery.Add("status", "official");
            param.Add("limit", "50");

            ret = musicBrainzClient.SearchMethod("release-group", paramQuery, param);
            
            return ret;
        }

        public string GetReleaseList(string album, string artist)
        {
            string ret = null;

            IDictionary<string, string> paramQuery = new Dictionary<string, string>();
            IDictionary<string, string> param = new Dictionary<string, string>();

            paramQuery.Add("artist", artist);
            paramQuery.Add("release", album);
            paramQuery.Add("status", "official");
            param.Add("limit", "50");

            ret = musicBrainzClient.SearchMethod("release", paramQuery, param);
            
            return ret;
        }


        public string GetReleaseArtist(string artist)
        {
            string ret = null;

            IDictionary<string, string> paramQuery = new Dictionary<string, string>();
            IDictionary<string, string> param = new Dictionary<string, string>();

            paramQuery.Add("artist", artist);
            paramQuery.Add("status", "official");
            param.Add("limit", "50");

            ret = musicBrainzClient.SearchMethod("release", paramQuery, param);
            
            return ret;
        }

        public string GetRelease(string mbid)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("inc", "recordings+artists+release-groups");

            ret = musicBrainzClient.LookupMethod("release", mbid, param);
            
            return ret;
        }

        public string GetRecording(string mbid)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("inc", "releases+artists+release-groups");

            ret = musicBrainzClient.LookupMethod("recording", mbid, param);
            
            return ret;
        }

    }
}
