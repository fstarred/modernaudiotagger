using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace UltimateMusicTagger.Business
{
    class WebServiceDelegate
    {
        /*
        private LastfmClient lastfmClient;
        private MusicBrainzClient musicBrainzClient;

        public WebServiceDelegate(string lastfmApikey)
        {
            LastfmClient lastfmClient = new LastfmClient(lastfmApikey, null);
            lastfmClient.UserAgent = String.Format("{0} [ / {1}]", Globals.APP_NAME, MTUtility.GetVersion());

            MusicBrainzClient musicBrainzClient = new MusicBrainzClient();
            musicBrainzClient.UserAgent = String.Format("{0} [ / {1}]", Globals.APP_NAME, MTUtility.GetVersion());
            
            this.lastfmClient = lastfmClient;
            this.musicBrainzClient = musicBrainzClient;
        }

        public void SetProxy(WebProxy proxy)
        {
            lastfmClient.Proxy = proxy;
            musicBrainzClient.Proxy = proxy;
        }


        public string GetAlbumInfoLastfm(string album, string artist)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("artist", artist);
            param.Add("album", album);
            param.Add("autocorrect", "1");

            try
            {
                ret = lastfmClient.CallMethod("album.getInfo", param);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }

            return ret;

        }

        public string GetTopTracksLastfm(string artist)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("artist", artist);
            param.Add("autocorrect", "1");

            try
            {
                ret = lastfmClient.CallMethod("artist.getTopTracks", param);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }

            return ret;
        }


        public string GetTrackInfoLastfm(string mbid)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("mbid", mbid);
            param.Add("autocorrect", "1");

            try
            {
                ret = lastfmClient.CallMethod("track.getInfo", param);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }

            return ret;

        }


        public string GetAlbumInfoLastfm(string mbid)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("mbid", mbid);
            param.Add("autocorrect", "1");

            try
            {
                ret = lastfmClient.CallMethod("album.getInfo", param);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }

            return ret;

        }

        public string GetTrackInfoLastfm(string track, string artist)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("artist", artist);
            param.Add("track", track);
            param.Add("autocorrect", "1");

            try
            {
                ret = lastfmClient.CallMethod("track.getInfo", param);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }

            return ret;

        }

        public string GetRecordingsMusicBrainz(string artist)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("artist", artist);
            param.Add("status", "official");

            try
            {
                ret = musicBrainzClient.SearchMethod("recording", param);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }

            return ret;
        }

        public string GetReleasesGroupMusicBrainz(string artist)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("artist", artist);
            param.Add("status", "Official");
            param.Add("primarytype", "Album");

            try
            {
                ret = musicBrainzClient.SearchMethod("release-group", param);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }

            return ret;
        }
         * */
    }
}
