using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UltimateMusicTagger.Model;

namespace UltimateMusicTagger.Business
{
    internal class MusicBrainzParser
    {
        #region Fields

        public const string NAMESPACE = "http://musicbrainz.org/ns/mmd-2.0#";

        XmlNamespaceManager nsMgr = null;
        CoverArtClient coverArtClient = null;

        #endregion

        #region Constructor

        public MusicBrainzParser()
        {
            coverArtClient = new CoverArtClient();
        }

        #endregion

        #region Methods

        public void InitXmlNamespaceManager(XmlDocument xmlDoc)
        {

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsMgr.AddNamespace("m", MusicBrainzParser.NAMESPACE);

            this.nsMgr = nsMgr;
        }


        private uint GetYearFromDate(string date)
        {
            string pattern = "\\d{4}";

            Match match = Regex.Match(date, pattern);

            uint ret = UInt16.Parse(match.Value);

            return ret;
        }

        public List<TrackInfo> GetTrackList(XmlNode rootNode)
        {
            List<TrackInfo> output = new List<TrackInfo>();

            if (rootNode != null)
            {
                XmlNodeList nodeList = rootNode.SelectNodes("./m:track", nsMgr);

                foreach (XmlNode node in nodeList)
                {
                    uint position = 0;

                    if (node["position"] != null)
                        position = uint.Parse(node["position"].InnerText);

                    XmlNode nodeRecording = node["recording"];

                    string title = null;
                    string mbid = null;
                    uint length = 0;

                    if (nodeRecording != null)
                    {
                        title = nodeRecording["title"].InnerText;
                        mbid = nodeRecording.Attributes["id"].InnerText;
                        if (nodeRecording["length"] != null)
                            length = UInt32.Parse(nodeRecording["length"].InnerText);
                    }
                    else
                    {
                        title = node["title"].InnerText;
                    }

                    TrackInfo track = new TrackInfo
                    { 
                        Mbid = mbid,
                        Title = title,
                        Track = position,
                        Length = length
                    };

                    output.Add(track);
                }

            }

            return output;
        }


        public ReleaseInfo GetRelease(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.SelectSingleNode("//m:release", nsMgr);

            return GetRelease(node);
        }

        public ReleaseInfo GetRelease(XmlNode node)
        {
            string mbid = node.Attributes["id"].InnerText;
            string title = node["title"].InnerText;
            string image = null;
            string year = null;

            if (node["release-group"] != null)
                image = coverArtClient.GetFirstUrl(CoverArtClient.ENTITY.RELEASE_GROUP,
                    node["release-group"].Attributes["id"].InnerText, CoverArtClient.IMAGE_TYPE.FRONT, 
                    CoverArtClient.IMAGE_SIZE.SMALL);


            if (node["date"] != null)
                year = node["date"].InnerText;
                //year = GetYearFromDate(node["date"].InnerText);
            

            XmlNode nodeTracklist = node.SelectSingleNode("//m:track-list", nsMgr);
            XmlNode nodeArtists = node.SelectSingleNode("//m:artist-credit", nsMgr);

            ReleaseInfo release = new ReleaseInfo
            {
                Mbid = mbid,
                Title = title,
                ImagePath = image,
                Year = year,                
                Artists = GetArtistList(nodeArtists).ToArray(),
                TrackInfos = GetTrackList(nodeTracklist).ToArray()
            };

            return release;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeList"></param>
        /// <returns></returns>
        public List<ReleaseInfo> GetReleaseList(XmlNode rootNode)
        {
            List<ReleaseInfo> output = new List<ReleaseInfo>();

            if (rootNode != null)
            {
                XmlNodeList nodeList = rootNode.SelectNodes("//m:release-list/m:release", nsMgr);

                foreach (XmlNode node in nodeList)
                {
                    output.Add(GetRelease(node));
                }

            }

            return output;
        }

        /// <summary>
        /// GetArtistList
        /// </summary>
        /// <param name="nodeList"></param>
        /// <returns></returns>
        public List<ArtistInfo> GetArtistList(XmlNode rootNode)
        {
            List<ArtistInfo> output = new List<ArtistInfo>();

            if (rootNode != null)
            {
                XmlNodeList nodeList = rootNode.SelectNodes("./m:name-credit/m:artist", nsMgr);

                foreach (XmlNode node in nodeList)
                {
                    string mbid = node.Attributes["id"].InnerText;
                    string name = node["name"].InnerText;

                    ArtistInfo artist = new ArtistInfo
                    {
                        Mbid = mbid,
                        Name = name
                    };

                    output.Add(artist);
                }

            }

            return output;
        }


        public TrackInfo GetRecording(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.SelectSingleNode("//m:recording", nsMgr);

            return GetRecording(node);
        }

        public TrackInfo GetRecording(XmlNode node)
        {
            string mbid = node.Attributes["id"].InnerText;
            string title = node["title"].InnerText;

            XmlNode nodeArtist = node["artist-credit"];
            XmlNode nodeRelease = node["release-list"];

            TrackInfo track = new TrackInfo
            {
                Mbid = mbid,
                Title = title,
                Artists = GetArtistList(nodeArtist).ToArray(),
                Releases = GetReleaseList(nodeRelease).ToArray()
            };

            return track;

        }

        /// <summary>
        /// GetRecordings
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public List<TrackInfo> GetRecordingList(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList = null;

            nodeList = xmlDoc.SelectNodes("//m:recording-list/m:recording", nsMgr);

            List<TrackInfo> tracklist = new List<TrackInfo>();

            if (nodeList != null)
            {
                foreach (XmlNode node in nodeList)
                {                    
                    tracklist.Add(GetRecording(node));
                }

            }

            return tracklist;
        }

        #endregion
    }
}
