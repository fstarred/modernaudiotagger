using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UltimateMusicTagger.Model;

namespace UltimateMusicTagger.Business
{
    internal class LastfmParser 
    {
        #region Constructor

        #endregion

        #region Fields

        #endregion

        #region Methods


        //[Obsolete()]
        //public List<OutputInfo> GetTopAlbums(XmlDocument xmlDoc)
        //{
        //    List<OutputInfo> albums = new List<OutputInfo>();

        //    XmlNodeList nodeList = xmlDoc.SelectNodes("//album");

        //    foreach (XmlNode node in nodeList)
        //    {
        //        OutputInfo output = new OutputInfo();

        //        XmlNode subNode = null;

        //        subNode = node.SelectSingleNode("name");

        //        string album = null;
        //        string image = null;
        //        string mbid = null;

        //        if (subNode != null)
        //            album = subNode.InnerText;

        //        subNode = node.SelectSingleNode("image[@size='small']");

        //        if (subNode != null)
        //            image = subNode.InnerText;

        //        subNode = node.SelectSingleNode("mbid");

        //        if (subNode != null)
        //            mbid = subNode.InnerText;

        //        output.Album = album;
        //        output.ImagePath = image;
        //        output.ReleaseMbid = mbid;

        //        albums.Add(output);
        //    }

        //    return albums;
        //}

        public List<TrackInfo> GetTrackInfosFromAlbum(XmlDocument xmlDoc)
        {
            List<TrackInfo> trackInfos = new List<TrackInfo>();

            XmlNodeList nodeList = xmlDoc.SelectNodes("//album/tracks/track");

            foreach (XmlNode node in nodeList)
            {
                XmlNode subNode = node.Attributes["rank"];

                string pos = null;
                string title = null;
                string mbid = null;
                ArtistInfo[] artists = null;
                uint trackpos = 0;

                if (subNode != null)
                    pos = subNode.InnerText;

                UInt32.TryParse(pos, out trackpos);

                subNode = node.SelectSingleNode("name");

                if (subNode != null)
                    title = subNode.InnerText;

                subNode = node.SelectSingleNode("mbid");

                if (subNode != null)
                    mbid = subNode.InnerText;

                XmlNodeList subNodeList = node.SelectNodes("artist");

                artists = GetArtistFromTrack(subNodeList);

                TrackInfo track = new TrackInfo { Mbid = mbid, Title = title, Track = trackpos, Artists = artists };

                trackInfos.Add(track);
            }


            return trackInfos;
        }

        public ArtistInfo[] GetArtistFromTrack(XmlNodeList node)
        {
            ArtistInfo[] output = null;

            if (node != null)
            {
                int idx = 0;

                output = new ArtistInfo[node.Count];

                foreach (XmlNode n in node)
                {
                    string mbid = null;
                    string name = null;

                    XmlNode subnode = null;

                    subnode = n.SelectSingleNode("mbid");

                    if (subnode != null)
                        mbid = subnode.InnerText;

                    subnode = n.SelectSingleNode("name");

                    if (subnode != null)
                        name = subnode.InnerText;

                    ArtistInfo artistInfo = new ArtistInfo
                    {
                        Mbid = mbid,
                        Name = name
                    };

                    output[idx++] = artistInfo;
                }
            }

            return output;
        }

        public uint GetTrackPosition(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//album");

            uint position = 0;

            if (node != null)
            {
                XmlAttribute attr = node.Attributes["position"];
                if (attr != null)
                {
                    UInt32.TryParse(attr.InnerText, out position);
                }
            }

            return position;
        }

        public string GetReleaseMbid(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//album/mbid");

            string mbid = null;

            if (node != null)
                mbid = node.InnerText;

            return mbid;
        }

        public string GetTrackMbid(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//track/mbid");

            string mbid = null;

            if (node != null)
                mbid = node.InnerText;

            return mbid;
        }

        public string GetArtistMbid(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//artist/mbid");

            string mbid = null;

            if (node != null)
                mbid = node.InnerText;

            return mbid;
        }


        public string GetReleaseName(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//album/name");

            string name = null;

            if (node != null)
                name = node.InnerText;

            return name;
        }

        public string GetReleaseTitle(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//album/title");

            string name = null;

            if (node != null)
                name = node.InnerText;

            return name;
        }

        public string GetTrackName(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//track/name");

            string name = null;

            if (node != null)
                name = node.InnerText;

            return name;
        }

        public string[] GetTrackArtist(XmlDocument xmlDoc)
        {
            string[] result = null;

            result = xmlDoc.SelectNodes("//track/artist").OfType<XmlNode>().Select(x => x.InnerText).ToArray();

            return result;
        }


        public string[] GetReleaseArtist(XmlDocument xmlDoc)
        {
            XmlNodeList node = null;

            //object result = xmlDoc.SelectNodes("//album/artist").OfType<XmlNode>().Aggregate<object>(
            //    (workingSentence, next) => next + " " + workingSentence);

            string[] result = null;

            node = xmlDoc.SelectNodes("//album/artist");

            if (node != null)
            {
                result = new string[node.Count];

                int idx = 0;

                foreach (XmlNode n in node)
                    result[idx++] = n.InnerText;
            }
            else
                result = new string[0];

            return result;
        }

        private static string GetDescriptionFromSize(IMAGE_RELEASE_SIZE value)
        {
            string ret = null;

            switch (value)
            {
                case IMAGE_RELEASE_SIZE.LARGE:
                    ret = "large";
                    break;
                case IMAGE_RELEASE_SIZE.MEDIUM:
                    ret = "medium";
                    break;
                case IMAGE_RELEASE_SIZE.SMALL:
                    ret = "small";
                    break;
            }

            return ret;
        }

        private uint GetYearFromDate(string date)
        {
            string pattern = "\\d{4}";

            Match match = Regex.Match(date, pattern);

            uint ret = 0;

            uint.TryParse(match.Value, out ret);

            return ret;
        }


        public List<TrackInfo> GetTrackList(XmlNode nodeRoot)
        {
            List<TrackInfo> output = new List<TrackInfo>();

            XmlNodeList nodeList = nodeRoot.SelectNodes("//track");

            foreach (XmlNode node in nodeList)
            {
                TrackInfo trackInfo = new TrackInfo
                {
                    Track = node.Attributes["rank"] != null ? UInt32.Parse( node.Attributes["rank"].InnerText ) : 0,
                    Mbid = node["mbid"] != null ? node["mbid"].InnerText : null,
                    Title = node["name"].InnerText,
                    Artists = new ArtistInfo[]{ GetArtist(node["artist"]) },
                    Length = node["duration"] != null ? UInt32.Parse(node["duration"].InnerText) : 0
                };

                output.Add(trackInfo);
            }

            return output;
        }

        //public ArtistInfo GetArtist(XmlNode nodeRoot)
        //{
        //    return new ArtistInfo
        //    {
        //        Mbid = nodeRoot["mbid"] != null ? nodeRoot["mbid"].InnerText : null,
        //        Name = nodeRoot["name"].InnerText
        //    };
        //}

        public string GetAlbumImage(XmlDocument xmlDoc, IMAGE_RELEASE_SIZE releaseSize)
        {

            string imageUri = null;

            try
            {
                XmlNode node = null;

                string size = GetDescriptionFromSize(releaseSize);

                node = xmlDoc.SelectSingleNode(String.Format("//album/image[@size='{0}']", size));


                if (node != null)
                {
                    imageUri = node.InnerText;
                    //retImg = MTUtility.ImageFromUri(imageUri, this.proxy);
                }

            }
            catch (Exception e)
            {

            }

            return imageUri;
        }

        public uint GetYear(XmlDocument xmlDoc)
        {
            uint ret = 0;

            try
            {
                XmlNode node = null;

                node = xmlDoc.SelectSingleNode("//album/releasedate");

                string date = null;

                if (node != null)
                    date = node.InnerText;

                string pattern = "\\d{4}";

                Match match = Regex.Match(date, pattern);

                ret = UInt16.Parse(match.Value);

            }
            catch (Exception e)
            {

            }

            return ret;

        }

        public TrackInfo GetTrack(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.SelectSingleNode("//track");

            return GetTrack(node);
        }

        private TrackInfo GetTrack(XmlNode node)
        {
            string mbid = node["mbid"] != null ? node["mbid"].InnerText : null;
            string title = node["name"].InnerText;            
            uint position = 0;

            XmlNode nodeArtist = node["artist"];
            XmlNode nodeRelease = node["album"];

            if (nodeRelease != null && nodeRelease.Attributes["position"] != null)
                position = uint.Parse(nodeRelease.Attributes["position"].InnerText);

            TrackInfo track = new TrackInfo
            {
                Mbid = mbid,
                Title = title,
                Track = position,
                Artists = new ArtistInfo[] { GetArtist(nodeArtist) },
                Releases = nodeRelease != null ? new ReleaseInfo[] { GetAlbumFromTrack(nodeRelease) } : new ReleaseInfo[0]
            };

            return track;
        }

        private ArtistInfo GetArtist(XmlNode node)
        {
            string mbid = node["mbid"] != null ? node["mbid"].InnerText : null;
            string title = node["name"].InnerText;

            ArtistInfo artist = new ArtistInfo
            {
                Mbid = mbid,
                Name = title
            };

            return artist;
        }

        private ReleaseInfo GetAlbumFromTrack(XmlNode node)
        {
            string mbid = node["mbid"] != null ? node["mbid"].InnerText : null;
            string title = node["title"].InnerText;
            string artist = node["artist"].InnerText;
            string path = node["image"] != null ? node.SelectSingleNode("image[@size='large']").InnerText : null;

            ReleaseInfo release = new ReleaseInfo
            {
                Mbid = mbid,
                Title = title,       
                ImagePath = path,
                Artists = new ArtistInfo[]{new ArtistInfo{ Name = artist }}                
            };

            return release;
        }


        public List<TrackInfo> GetTopTracks(XmlDocument xmlDoc)
        {
            List<TrackInfo> tracks = new List<TrackInfo>();

            XmlNodeList nodeList = xmlDoc.SelectNodes("//toptracks/track");

            foreach (XmlNode node in nodeList)
            {
                TrackInfo output = new TrackInfo();

                XmlNode subNode = null;

                subNode = node.SelectSingleNode("name");

                string mbid = null;
                string title = null;

                if (subNode != null)
                    title = subNode.InnerText;

                subNode = node.SelectSingleNode("mbid");

                if (subNode != null)
                    mbid = subNode.InnerText;

                output.Title = title;
                output.Mbid = mbid;

                tracks.Add(output);
            }

            return tracks;
        }

        public ReleaseInfo GetAlbum(XmlDocument xmlDoc)
        {
            ReleaseInfo output = null;

            XmlNode nodeAlbum = xmlDoc.SelectSingleNode("//album");

            output = GetAlbum(nodeAlbum);

            return output;

        }


        private ReleaseInfo GetAlbum(XmlNode nodeAlbum)
        {
            ReleaseInfo output = null;

            string name = nodeAlbum["name"].InnerText;
            string mbid = null;
            string image = nodeAlbum["image"] != null ? nodeAlbum.SelectSingleNode("image[@size='large']").InnerText : null;

            if (nodeAlbum["mbid"] != null)
                mbid = nodeAlbum["mbid"].InnerText;

            XmlNode nodeTracklist = nodeAlbum.SelectSingleNode("//tracks");

            string year = null;
            string artist = null;

            if (nodeAlbum["releasedate"] != null)
                year = nodeAlbum["releasedate"].InnerText;
                //year = GetYearFromDate(nodeAlbum["releasedate"].InnerText);

            if (nodeAlbum["artist"] != null)
            {
                if (nodeAlbum["artist"].SelectSingleNode("name") != null)
                    artist = nodeAlbum["artist"].SelectSingleNode("name").InnerText;
                else
                    artist = nodeAlbum["artist"].InnerText;
            }

            output = new ReleaseInfo
            {
                Mbid = mbid,
                Title = name,
                Year = year,
                ImagePath = image,
                Artists = new ArtistInfo[]
                { 
                    new ArtistInfo{ Name = artist }
                },
                TrackInfos = nodeTracklist != null ? 
                    GetTrackList(nodeTracklist).ToArray() : new TrackInfo[0]
            };

            return output;

        }


        public List<ReleaseInfo> GetTopAlbums(XmlDocument xmlDoc)
        {
            List<ReleaseInfo> albums = new List<ReleaseInfo>();

            XmlNode nodeRoot = xmlDoc.SelectSingleNode("//topalbums");

            return GetAlbumList(nodeRoot);
        }


        private List<ReleaseInfo> GetAlbumList(XmlNode nodeRoot)
        {
            List<ReleaseInfo> output = new List<ReleaseInfo>();

            XmlNodeList nodelist = nodeRoot.SelectNodes("//album");

            foreach (XmlNode node in nodelist)
            {
                output.Add(GetAlbum(node));
            }

            return output;

        }

        

        #endregion
    }
}
