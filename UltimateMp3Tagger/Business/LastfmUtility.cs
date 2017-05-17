using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using UltimateMusicTagger.Model;

namespace UltimateMusicTagger.Business
{
    public class LastfmUtility
    {

        #region Fields

        LastfmWSDelegate lastfmWSDelegate = null;
        LastfmParser lastfmParser = null;

        private const string Apikey = "f3765d1a55ebee4682e7fe24a91e0e88";
        
        #endregion

        #region Constructor

        public LastfmUtility()
            : this(null, null)
        {
        }

        public LastfmUtility(string apikey)
            : this(apikey, null)
        {
        }

        public LastfmUtility(string apikey, IWebProxy proxy)
        {
            string finalApikey = apikey ?? Apikey;

            lastfmWSDelegate = new LastfmWSDelegate(finalApikey);
            lastfmWSDelegate.SetProxy(proxy);

            lastfmParser = new LastfmParser();
        }
        
        #endregion

        #region Methods

        public void SetProxy(WebProxy proxy)
        {
            lastfmWSDelegate.SetProxy(proxy);
        }

        public TrackInfo GetTrack(string mbid)
        {
            string xml = null;

            xml = lastfmWSDelegate.GetTrackInfo(mbid);            

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            return lastfmParser.GetTrack(xmlDoc);
        }
        
        public TrackInfo GetTrack(string title, string artist)
        {
            string xml = lastfmWSDelegate.GetTrackInfo(title, artist);

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            return lastfmParser.GetTrack(xmlDoc);
        }

        public List<TrackInfo> GetTrackList(string artist)
        {
            string xml = lastfmWSDelegate.GetTopTracks(artist);

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            return lastfmParser.GetTopTracks(xmlDoc);
        }

        public List<ReleaseInfo> GetReleaseList(string artist)
        {
            string xml = lastfmWSDelegate.GetTopAlbums(artist);

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            return lastfmParser.GetTopAlbums(xmlDoc);
        }

        public ReleaseInfo GetRelease(string album, string artist)
        {
            string xml = lastfmWSDelegate.GetAlbumInfo(album, artist);

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            return lastfmParser.GetAlbum(xmlDoc);
        }

        public ReleaseInfo GetRelease(string mbid)
        {
            string xml = lastfmWSDelegate.GetAlbumInfo(mbid);

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            return lastfmParser.GetAlbum(xmlDoc);
        }
        
        #endregion

    }
}
