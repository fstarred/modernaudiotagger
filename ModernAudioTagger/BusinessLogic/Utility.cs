using ModernAudioTagger.ViewModelElement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UltimateMusicTagger.Model;

namespace ModernAudioTagger.BusinessLogic
{
    public static class Utility
    {
        public static Version GetProductVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            return new Version(fileVersionInfo.ProductVersion);
        }

        public static void SetDefaultProxy(WebProxy proxy)
        {            
            //IWebProxy proxy = new WebProxy(host, port);
            //proxy.Credentials = new NetworkCredential(user, pwd, domain);

            WebRequest.DefaultWebProxy = proxy;
        }

        public static uint GetYearFromDate(string date)
        {
            string pattern = "\\d{4}";

            Match match = Regex.Match(date ?? String.Empty, pattern);

            uint ret = 0;

            uint.TryParse(match.Value, out ret);

            return ret;
        }

        public static void CheckSelectedItems(IEnumerable<ISelectable> list)
        {
            bool isSelected = false;

            if (list.Count((o) => { return o.IsSelected; }) != list.Count())
            {
                isSelected = true;
            }

            list.Select((o) => { o.IsSelected = isSelected; return o; }).ToList();
        }


        public static void ExclusiveSelectItem(ISelectable selectable, IEnumerable<ISelectable> list)
        {
            int levelSelectedCount = list.Where(x => x.IsSelected == true).Count();
            int levelCount = list.Count();
            bool isLevelSelected = selectable.IsSelected;

            bool result = (isLevelSelected && levelSelectedCount == 1);

            list.All(x => { x.IsSelected = result; return true; });

            if (result == false)
                selectable.IsSelected = true;
        }

        public static ModelTag GetModelTag(ReleaseInfo release)
        {
            string[] releaseArtists = release.Artists.Select((o) => { return o.Name; }).ToArray<string>();

            ModelTag output = new ModelTag
            {
                Album = release.Title,
                AlbumArtists = releaseArtists,
                Year = Utility.GetYearFromDate(release.Year),
                ReleaseMbid = release.Mbid,
            };

            //ImagePath = release.ImagePath;

            return output;
        }

        public static ModelTag GetModelTag(TrackInfoVM trackVM)
        {
            string[] trackArtists = null;
            string[] releaseArtists = null;
            string artistMbid = null;
            ReleaseInfo release = null;
            string releaseTitle = null;
            uint year = 0;
            string releaseMbid = null;
            string imagePath = null;
            //string trackMbid = null;

            if (trackVM.Artists != null)
            {
                trackArtists = trackVM.Artists.Select((o) => { return o.Name; }).ToArray<string>();
                artistMbid = trackVM.Artists[0].Mbid;
            }

            if (trackVM.Releases.Length > 0)
            {
                release = trackVM.Releases[0];

                releaseTitle = release.Title;
                releaseMbid = release.Mbid;
                year = Utility.GetYearFromDate(release.Year);
                imagePath = release.ImagePath;

                if (release.Artists != null)
                {
                    releaseArtists = release.Artists.Select((o) => { return o.Name; }).ToArray<string>();
                }
            }

            ModelTag output = new ModelTag
            {
                Title = trackVM.Title,
                Position = trackVM.Track,
                TrackArtists = trackArtists,
                Album = releaseTitle,
                AlbumArtists = releaseArtists,
                Year = year,
                ReleaseMbid = releaseMbid,
                ArtistMbid = artistMbid,
                TrackMbid = trackVM.Mbid,
            };

            //ImagePath = imagePath;

            return output;
        }

        public static string GetImagePathFromReleases(ReleaseInfo[] releases)
        {
            string output = null;

            if (releases != null && releases.Length > 0)
                output = releases[0].ImagePath;

            return output;
        }

        public static ModelTag GetModelTag(TrackInfo trackInfo)
        {
            ReleaseInfo release = null;

            string[] trackArtists = null;
            string[] albumArtists = null;
            string album = null;
            uint year = 0;
            string releaseMbid = null;
            string artistMbid = null;
            System.Drawing.Image picture = null;

            if (trackInfo.Releases != null && trackInfo.Releases.Length > 0)
            {
                release = trackInfo.Releases.First();
            }

            if (trackInfo.Artists != null)
            {
                trackArtists = trackInfo.Artists.Select((o) => { return o.Name; }).ToArray<string>();
                if (trackArtists.Length > 0)
                {
                    artistMbid = trackInfo.Artists[0].Mbid;
                }
            }
            if (release != null)
            {
                album = release.Title;
                albumArtists = release.Artists != null && release.Artists.Length > 0 ? release.Artists.Select((o) => { return o.Name; }).ToArray<string>() : null;
                year = Utility.GetYearFromDate(release.Year);
                releaseMbid = release.Mbid;
                picture = release.Picture;
            }


            ModelTag output = new ModelTag
            {
                Title = trackInfo.Title,
                Position = trackInfo.Track,
                TrackArtists = trackArtists,
                Album = album,
                AlbumArtists = albumArtists,
                Year = year,
                ReleaseMbid = releaseMbid,
                ArtistMbid = artistMbid,
                TrackMbid = trackInfo.Mbid,
                Genres = trackInfo.Genres,
                Picture = picture,
            };

            return output;
        }


        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            string[] searchPatterns = searchPattern.Split('|');
            List<string> files = new List<string>();
            foreach (string sp in searchPatterns)
                files.AddRange(System.IO.Directory.GetFiles(path, sp, searchOption));
            files.Sort();
            return files.ToArray();
        }

        public static void CheckAllItems(IEnumerable<ISelectable> list, int maxItemsSelectable)
        {
            int selectedItemsCount = list.Count((o) => o.IsSelected == true);

            bool select = selectedItemsCount != maxItemsSelectable;
            
            list.All((o) => { o.IsSelected = select; return true; });
        }

        public static string GetTimeFormattedFromTrackLength(uint duration)
        {
            string output = null;

            // if value is > 6000 is probably expressed in ms
            const int ESTABILISHED_SECONDS_LIMIT = 6000;

            if (duration > ESTABILISHED_SECONDS_LIMIT)
            {
                duration /= 1000;
            }

            TimeSpan ts = new TimeSpan(0, 0, (int)duration);

            if (ts.Hours == 0)
                output = String.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
            else
                output = String.Format("{0:D3}:{1:D2}", ts.Minutes, ts.Seconds);

            return output;
        }

    }
}
