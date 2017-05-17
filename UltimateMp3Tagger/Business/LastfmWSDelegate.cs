using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace UltimateMusicTagger.Business
{
    internal class LastfmWSDelegate : WServiceBase
    {
        private LastfmClient lastfmClient;

        public LastfmWSDelegate(string apikey)
        {
            LastfmClient lastfmClient = new LastfmClient(apikey, null);
            lastfmClient.UserAgent = String.Format("{0} [ / {1}]", Globals.APP_NAME, MTUtility.GetVersion());

            this.lastfmClient = lastfmClient;
        }

        public void SetProxy(IWebProxy proxy)
        {
            lastfmClient.Proxy = proxy;
        }

        public string GetAlbumInfo(string album, string artist)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("artist", artist);
            param.Add("album", album);
            param.Add("autocorrect", "1");

            ret = lastfmClient.CallMethod("album.getInfo", param);
            
            return ret;

        }

        public string GetTopTracks(string artist)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("artist", artist);
            param.Add("autocorrect", "1");

            ret = lastfmClient.CallMethod("artist.getTopTracks", param);
            
            return ret;
        }


        public string GetTrackInfo(string mbid)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("mbid", mbid);
            param.Add("autocorrect", "1");

            ret = lastfmClient.CallMethod("track.getInfo", param);
            
            return ret;

        }


        public string GetAlbumInfo(string mbid)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("mbid", mbid);
            param.Add("autocorrect", "1");

            ret = lastfmClient.CallMethod("album.getInfo", param);
            
            return ret;

        }

        public string GetTrackInfo(string track, string artist)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("artist", artist);
            param.Add("track", track);
            param.Add("autocorrect", "1");

            ret = lastfmClient.CallMethod("track.getInfo", param);
            
            return ret;

        }

        public string GetTopAlbums(string artist)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("artist", artist);
            param.Add("autocorrect", "1");

            ret = lastfmClient.CallMethod("artist.getTopAlbums", param);
            
            return ret;
        }


    }
}
