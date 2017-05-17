using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using UltimateMusicTagger.Model;

namespace UltimateMusicTagger.Business
{
    public class MusicBrainzUtility
    {
        #region Fields

        CoverArtClient coverArtClient = null;
        MusicBrainzWSDelegate musicBrainzWSDelegate = null;
        MusicBrainzParser musicBrainzParser = null;
        
        #endregion

        #region Constructor

        public MusicBrainzUtility()
            : this(null)
        {
        }

        public MusicBrainzUtility(IWebProxy proxy)
        {
            musicBrainzWSDelegate = new MusicBrainzWSDelegate();
            musicBrainzWSDelegate.SetProxy(proxy);

            musicBrainzParser = new MusicBrainzParser();

            coverArtClient = new CoverArtClient();
            coverArtClient.Proxy = proxy;
        }
        
        #endregion

        #region Methods

        public void SetProxy(WebProxy proxy)
        {
            musicBrainzWSDelegate.SetProxy(proxy);
            coverArtClient.Proxy = proxy;
        }

        public ReleaseInfo GetRelease(string mbid)
        {
            string xml = musicBrainzWSDelegate.GetRelease(mbid);

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            musicBrainzParser.InitXmlNamespaceManager(xmlDoc);

            return musicBrainzParser.GetRelease(xmlDoc);
        }

        public List<ReleaseInfo> GetReleaseList(string title, string artist)
        {
            string xml = musicBrainzWSDelegate.GetReleaseList(title, artist);

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            musicBrainzParser.InitXmlNamespaceManager(xmlDoc);

            return musicBrainzParser.GetReleaseList(xmlDoc);
        }

        public List<ReleaseInfo> GetReleaseList(string artist)
        {
            string xml = musicBrainzWSDelegate.GetReleaseArtist(artist);

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            musicBrainzParser.InitXmlNamespaceManager(xmlDoc);

            return musicBrainzParser.GetReleaseList(xmlDoc);
        }


        public TrackInfo GetRecording(string mbid)
        {
            string xml = musicBrainzWSDelegate.GetRecording(mbid);

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            musicBrainzParser.InitXmlNamespaceManager(xmlDoc);

            return musicBrainzParser.GetRecording(xmlDoc);
        }

        public List<TrackInfo> GetRecordingList(string artist)
        {
            string xml = musicBrainzWSDelegate.GetRecordingList(artist);

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            musicBrainzParser.InitXmlNamespaceManager(xmlDoc);

            return musicBrainzParser.GetRecordingList(xmlDoc);
        }

        public List<TrackInfo> GetRecordingList(string title, string artist)
        {
            string xml = musicBrainzWSDelegate.GetRecordingList(title, artist);

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            musicBrainzParser.InitXmlNamespaceManager(xmlDoc);

            return musicBrainzParser.GetRecordingList(xmlDoc);
        }
        
        #endregion
    }
}
