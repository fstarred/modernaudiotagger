using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TagLib;
using UltimateMusicTagger.Business;
using UltimateMusicTagger.Model;

namespace UltimateMusicTagger
{
    public class MTUtility
    {
        public static bool IsWindowsOS()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    return true;
                default:
                    return false;
            }
        }

        public static Version GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();            
            return new Version( FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion);
        }

        internal static Image ImageFromPicture(IPicture picture)
        {
            TagLib.ByteVector bv = picture.Data;

            // NOTE: closing MemoryStream would generate a GDI+ error if later image is saved to a file
            MemoryStream ms = new MemoryStream(bv.Count);
            ms.Write(bv.Data, 0, bv.Count);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);

            return img;
        }

        internal static Picture PictureFromImage(Image source)
        {
            TagLib.ByteVector imageData;

            using (var memoryStream = new MemoryStream())
            {
                ImageFormat format = ImageFormat.Png.Equals(source.RawFormat) ? ImageFormat.Png : ImageFormat.Jpeg;

                source.Save(memoryStream, format);
                imageData = new TagLib.ByteVector(memoryStream.ToArray());
            }

            Picture picture = new TagLib.Picture(imageData);

            return picture;
        }

        public static Image ImageFromUri(string uri, IWebProxy proxy)
        {
            Image retImg = null;

            using (WebClient wc = new WebClient())
            {
                wc.Proxy = proxy;
                try
                {
                    retImg = Image.FromStream(wc.OpenRead(uri));
                }
                catch (Exception) { }
            }

            return retImg;
        }

        public static string GetFilenameByLevenshteinDistance(string[] files, TrackInfo trackInfo)
        {
            string output = null;

            int minDistance = 0;

            bool isFirstRecord = true;

            string title = trackInfo.Title;

            foreach (string filename in files)
            {
                string file = Path.GetFileNameWithoutExtension(filename);

                int distance = Levenshtein.Distance(file, title);
                if (distance < minDistance || isFirstRecord)
                {
                    minDistance = distance;
                    output = filename;
                    isFirstRecord = false;
                }

                if (minDistance == 0)
                    break;
            }

            return output;
        }

        public static TrackInfo GetTrackInfoByLevenshteinDistance(string filename, IEnumerable<TrackInfo> trackInfos)
        {
            TrackInfo output = null;

            int minDistance = 0;

            bool isFirstRecord = true;

            foreach (TrackInfo trackInfo in trackInfos)
            {
                int distance = Levenshtein.Distance(filename, trackInfo.Title);
                if (distance < minDistance || isFirstRecord)
                {
                    minDistance = distance;
                    output = trackInfo;
                    isFirstRecord = false;
                }
                
                if (minDistance == 0)
                    break;
            }

            return output;
        }

        public static bool IsLocalPath(string p)
        {
            return new Uri(p).IsFile;
        }


        public static TrackInfo GetTrackInfoByJaroWinklerDistance(string filename, IEnumerable<TrackInfo> trackInfos)
        {
            TrackInfo output = null;

            double minDistance = 0;

            bool isFirstRecord = true;

            JaroWinklerDistance jaro = new JaroWinklerDistance();

            foreach (TrackInfo trackInfo in trackInfos)
            {
                double distance = jaro.Distance(filename, trackInfo.Title);
                if (distance < minDistance || isFirstRecord)
                {
                    minDistance = distance;
                    output = trackInfo;
                    isFirstRecord = false;
                }

                if (minDistance == 0)
                    break;
            }

            return output;
        }

        public static string GetFilenameByPosition(string[] files, TrackInfo trackInfo)
        {
            const string patternLeft = "^[0-9]*";
            const string patternRight = "[0-9]*$";

            string[] patterns = new string[] { patternLeft, patternRight };

            string position = null;

            string output = null;

            foreach (string filename in files)
            {
                string file = Path.GetFileNameWithoutExtension(filename);

                foreach (string pattern in patterns)
                {
                    Match match = Regex.Match(file, pattern);

                    if (match.Length > 0)
                    {
                        position = match.Value;
                        break;
                    }
                }

                if (position != null)
                {
                    uint pos = 0;

                    UInt32.TryParse(position, out pos);

                    if (pos == trackInfo.Track)
                        output = filename;
                }
            }

            return output;
        }

        public static TrackInfo GetTrackInfoByPosition(string filename, IEnumerable<TrackInfo> trackInfos)
        {
            TrackInfo trackInfo = null;

            const string patternLeft = "^[0-9]*";
            const string patternRight = "[0-9]*$";

            string[] patterns = new string[] { patternLeft, patternRight };

            string position = null;

            foreach (string pattern in patterns)
            {
                Match match = Regex.Match(filename, pattern);

                if (match.Length > 0)
                {
                    position = match.Value;
                    break;
                }
            }

            if (position != null)
            {
                uint pos = 0;

                UInt32.TryParse(position, out pos);

                var query = from track in trackInfos
                            where track.Track == pos
                            select track;

                if (query.Count() > 0)
                    trackInfo = query.First();
            }

            return trackInfo;
        }

    }
}
