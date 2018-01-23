using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using TagLib;
using UltimateMusicTagger.Business;
using UltimateMusicTagger.Model;

namespace UltimateMusicTagger
{

    #region Enums

    [Flags]
    public enum TAG_FIELDS
    {
        NONE = 0x0,
        TITLE = 0x1,
        ALBUM_ARTIST = 0x2,
        TRACK_ARTIST = 0x4,
        ALBUM = 0x8,
        YEAR = 0x10,
        TRACK_POS = 0x20,
        IMAGE = 0x40,
        GENRES = 0x80,
        MUSICBRAINZ_ID = 0x100
    };

    public enum FILENAME_MATCH
    {
        TRACK_POSITION,
        LEVENSHTEIN_DISTANCE,
        JAROWINKLER_DISTANCE
    }


    public enum IMAGE_ARTIST
    {
        SMALL,
        MEDIUM,
        LARGE,
        EXTRALARGE,
        MEGA
    }

    public enum IMAGE_RELEASE_SIZE
    {
        SMALL,
        MEDIUM,
        LARGE
    }

    public enum LOG_TYPE
    {
        INFO,
        WARNING,
        ERROR
    }

    #endregion

    public class UltiMp3Tagger
    {
        #region Constructor

        public UltiMp3Tagger()
            : this(null)
        {

        }

        public UltiMp3Tagger(IWebProxy proxy)
        {
            this.queueMessage = new Queue<UMTMessage>();

            this.proxy = proxy;

            //string finalApikey = apikey ?? Apikey;

            //lastfmWSDelegate = new LastfmWSDelegate(finalApikey);
            //lastfmWSDelegate.SetProxy(proxy);

            //musicBrainzWSDelegate = new MusicBrainzWSDelegate();
            //musicBrainzWSDelegate.SetProxy(proxy);

            //coverArtClient = new CoverArtClient();
            //coverArtClient.Proxy = proxy;

            lastfmParser = new LastfmParser();
            musicBrainzParser = new MusicBrainzParser();
        }

        #endregion

        #region Fields

        //CoverArtClient coverArtClient = null;
        //LastfmWSDelegate lastfmWSDelegate = null;
        //MusicBrainzWSDelegate musicBrainzWSDelegate = null;

        LastfmParser lastfmParser = null;
        MusicBrainzParser musicBrainzParser = null;

        IWebProxy proxy = null;

        public const string PatternTitle = @"%\bt\b|%\btitle\b";
        public const string PatternAlbumArtist = @"%\baa\b|%\baartist\b";
        public const string PatternTrackArtist = @"%\bta\b|%\btartist\b";
        public const string PatternAlbum = @"%\br\b|%\balbum\b";
        public const string PatternYear = @"%\bd\b|%\by\b|%\byear\b";
        public const string PatternPos = @"%\bp\b|%\bpos\b";

        //private const string Apikey = "dfc449a2ea1f558b5f2e825bf4b878d6";

        #endregion

        #region Methods General

        public UMTMessage[] UnqueueMessages()
        {
            Queue<UMTMessage> queue = this.queueMessage;

            UMTMessage[] msg = queue.ToArray();
            queue.Clear();

            return msg;
        }


        public void SetProxy(WebProxy proxy)
        {
            //lastfmWSDelegate.SetProxy(proxy);
            //musicBrainzWSDelegate.SetProxy(proxy);
            //coverArtClient.Proxy = proxy;
        }


        public static bool IsExtensionSupported(string file)
        {
            string ext = String.Empty;

            string mimetype = null;

            TagLib.File.IFileAbstraction abstraction = new TagLib.File.LocalFileAbstraction(file);

            int index = abstraction.Name.LastIndexOf(".") + 1;

            if (index >= 1 && index < abstraction.Name.Length)
                ext = abstraction.Name.Substring(index,
                    abstraction.Name.Length - index);

            mimetype = "taglib/" + ext.ToLower(CultureInfo.InvariantCulture);

            return FileTypes.AvailableTypes.ContainsKey(mimetype);

        }


        #endregion

        #region Methods Rename

        public string ShowNameByTag(string file, string pattern)
        {
            string filename = null;

            if (IsExtensionSupported(file))
            {
                TagLib.File tagFile = TagLib.File.Create(file);

                filename = GenerateFilenameWithTag(tagFile, pattern);
            }

            return filename;
        }

        public bool RenameFilesByTag(string[] files, string pattern)
        {
            bool ret = true;

            foreach (string file in files)
            {
                bool result = RenameFileByTag(file, pattern);
                if (result == false)
                    ret = false;
            }

            return ret;
        }

        public bool RenameFolderByTag(string path, string file, string pattern, out string newfoldername)
        {
            bool result = false;

            string lastFoldernameRenamed = GenerateFolderNameWithTag(path, file, pattern);

            if (String.IsNullOrEmpty(lastFoldernameRenamed) == false)
            {
                try
                {
                    string oldpath = path;
                    string newpath = Path.Combine(Directory.GetParent(path).FullName, lastFoldernameRenamed);
                    if (oldpath.Equals(newpath) == false)
                    {
                        Directory.Move(oldpath, newpath);

                        queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("new folder name: {0}", lastFoldernameRenamed)));
                    }
                    else
                        queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.WARNING, String.Format("folder name: \"{0}\" already exists", lastFoldernameRenamed)));

                    lastFoldernameRenamed = newpath;

                    newfoldername = newpath;

                    result = true;

                }
                catch (Exception e)
                {
                    queueMessage.Enqueue(
                        new UMTMessage(
                            UMTMessage.M_TYPE.WARNING,
                            e.Message
                            ));

                    throw;
                }
            }
            else
                throw new ApplicationException("Can't generate folder name. selected files must have same album, year and artist tag");

            //return foldernameReplaced;

            return result;

        }

        public bool RenameFolderByTag(string path, string[] files, string pattern, out string newfoldername)
        {
            bool result = false;

            string lastFoldernameRenamed = GenerateFolderNameWithTag(files, path, pattern);

            if (String.IsNullOrEmpty(lastFoldernameRenamed) == false)
            {
                try
                {
                    string oldpath = path;
                    string newpath = Path.Combine(Directory.GetParent(path).FullName, lastFoldernameRenamed);
                    if (oldpath.Equals(newpath) == false)
                    {
                        Directory.Move(oldpath, newpath);

                        queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("new folder name: {0}", lastFoldernameRenamed)));
                    }
                    else
                        queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.WARNING, String.Format("folder name: \"{0}\" already exists", lastFoldernameRenamed)));

                    lastFoldernameRenamed = newpath;

                    newfoldername = newpath;

                    result = true;

                }
                catch (Exception e)
                {
                    queueMessage.Enqueue(
                        new UMTMessage(                            
                            UMTMessage.M_TYPE.ERROR,
                            String.Format("failed to rename folder {0}: {1}", path, e.Message)
                            ));

                    throw;
                }
            }
            else
                throw new ApplicationException("Can't generate folder name. selected files must have same album, year and artist tag");

            //return foldernameReplaced;

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="path"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private string GenerateFolderNameWithTag(string path, string file, string pattern)
        {
            string album = null;
            uint year = 0;
            string artist = null;

            //bool isSameAlbum = true;

            TagLib.File tagFile = null;

            //foreach (string file in files)
            //{
            tagFile = TagLib.File.Create(file);

            //if (isTagInitialized)
            //{
            //    if (album.Equals(tagFile.Tag.Album) == false) isSameAlbum = false;
            //    if (year.Equals(tagFile.Tag.Year) == false) isSameAlbum = false;
            //    if (tagFile.Tag.Performers.Length > 0)
            //        if (artist.Equals(tagFile.Tag.Performers[0]) == false) isSameAlbum = false;
            //}
            //else
            //{
            album = tagFile.Tag.Album;
            year = tagFile.Tag.Year;
            if (tagFile.Tag.Performers.Length > 0)
                artist = tagFile.Tag.Performers[0];
            //}

            //    isTagInitialized = true;
            //}

            string folderName = pattern;

            Regex rgx = null;

            // album artist
            rgx = new Regex(PatternAlbumArtist, RegexOptions.IgnoreCase);

            folderName = rgx.Replace(folderName, String.Join(",", tagFile.Tag.AlbumArtists));

            // track artist
            rgx = new Regex(PatternTrackArtist, RegexOptions.IgnoreCase);

            folderName = rgx.Replace(folderName, String.Join(",", tagFile.Tag.Performers));

            // album
            rgx = new Regex(PatternAlbum, RegexOptions.IgnoreCase);

            folderName = rgx.Replace(folderName, tagFile.Tag.Album ?? String.Empty);

            // year
            rgx = new Regex(PatternYear, RegexOptions.IgnoreCase);

            folderName = rgx.Replace(folderName, tagFile.Tag.Year.ToString());

            // replace invalid folder characters
            folderName = Path.GetInvalidFileNameChars().Aggregate(folderName, (current, c) => current.Replace(c.ToString(), string.Empty));


            return folderName;
        }

        /// <summary>
        /// Generate folder naming based to all files tag. 
        /// If one file's tag differ from another one, return a blank string 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="path"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private string GenerateFolderNameWithTag(string[] files, string path, string pattern)
        {
            string album = null;
            uint year = 0;
            string artist = null;

            bool isTagInitialized = false;
            bool isSameAlbum = true;

            TagLib.File tagFile = null;

            foreach (string file in files)
            {
                tagFile = TagLib.File.Create(file);

                if (isTagInitialized)
                {
                    if (album.Equals(tagFile.Tag.Album) == false) isSameAlbum = false;
                    if (year.Equals(tagFile.Tag.Year) == false) isSameAlbum = false;
                    if (tagFile.Tag.Performers.Length > 0)
                        if (artist.Equals(tagFile.Tag.Performers[0]) == false) isSameAlbum = false;
                }
                else
                {
                    album = tagFile.Tag.Album;
                    year = tagFile.Tag.Year;
                    if (tagFile.Tag.Performers.Length > 0)
                        artist = tagFile.Tag.Performers[0];
                }

                isTagInitialized = true;
            }

            string folderName = pattern;

            if (isSameAlbum)
            {

                Regex rgx = null;

                // album artist
                rgx = new Regex(PatternAlbumArtist, RegexOptions.IgnoreCase);

                folderName = rgx.Replace(folderName, String.Join(",", tagFile.Tag.AlbumArtists));

                // track artist
                rgx = new Regex(PatternTrackArtist, RegexOptions.IgnoreCase);

                folderName = rgx.Replace(folderName, String.Join(",", tagFile.Tag.Performers));

                // album
                rgx = new Regex(PatternAlbum, RegexOptions.IgnoreCase);

                folderName = rgx.Replace(folderName, tagFile.Tag.Album ?? String.Empty);

                // year
                rgx = new Regex(PatternYear, RegexOptions.IgnoreCase);

                folderName = rgx.Replace(folderName, tagFile.Tag.Year.ToString());

                // replace invalid folder characters
                folderName = Path.GetInvalidFileNameChars().Aggregate(folderName, (current, c) => current.Replace(c.ToString(), string.Empty));
            }
            else
                queueMessage.Enqueue(new UMTMessage(
                    UMTMessage.M_TYPE.WARNING,
                    "selected files must have same album, year and artist tag"
                ));

            return folderName;
        }

        [Obsolete("Do not use this")]
        private string GenerateFolderNameWithTagOld(string[] files, string path, string pattern)
        {
            string album = null;
            uint year = 0;
            string artist = null;

            bool isTagInitialized = false;
            bool isSameAlbum = true;

            string folderName = null;

            TagLib.File tagFile = null;

            foreach (string file in files)
            {
                tagFile = TagLib.File.Create(file);

                if (isTagInitialized)
                {
                    if (album.Equals(tagFile.Tag.Album) == false) isSameAlbum = false;
                    if (year.Equals(tagFile.Tag.Year) == false) isSameAlbum = false;
                    if (tagFile.Tag.Performers.Length > 0)
                        if (artist.Equals(tagFile.Tag.Performers[0]) == false) isSameAlbum = false;
                }
                else
                {
                    album = tagFile.Tag.Album;
                    year = tagFile.Tag.Year;
                    if (tagFile.Tag.Performers.Length > 0)
                        artist = tagFile.Tag.Performers[0];
                }

                isTagInitialized = true;
            }

            if (isSameAlbum)
            {
                StringBuilder sb = new StringBuilder(pattern);

                Regex rgx = null;

                MatchCollection matches = null;

                // album artist
                rgx = new Regex(PatternAlbumArtist, RegexOptions.IgnoreCase);

                //matches = rgx.Matches(sb.ToString());

                //foreach (Match match in matches)
                //    sb = sb.Replace(match.Value, String.Join(",", tagFile.Tag.AlbumArtists));

                // track artist
                rgx = new Regex(PatternTrackArtist, RegexOptions.IgnoreCase);

                matches = rgx.Matches(sb.ToString());

                foreach (Match match in matches)
                    sb = sb.Replace(match.Value, String.Join(",", tagFile.Tag.Performers));

                // album
                rgx = new Regex(PatternAlbum, RegexOptions.IgnoreCase);

                matches = rgx.Matches(sb.ToString());

                foreach (Match match in matches)
                    sb = sb.Replace(match.Value, tagFile.Tag.Album);

                // year
                rgx = new Regex(PatternYear, RegexOptions.IgnoreCase);

                matches = rgx.Matches(sb.ToString());

                foreach (Match match in matches)
                    sb = sb.Replace(match.Value, tagFile.Tag.Year.ToString());

                folderName = sb.ToString();

                // replace invalid folder characters
                folderName = Path.GetInvalidFileNameChars().Aggregate(folderName, (current, c) => current.Replace(c.ToString(), string.Empty));
            }
            else
                queueMessage.Enqueue(new UMTMessage(
                    UMTMessage.M_TYPE.WARNING,
                    "selected files must have same album, year and artist tag"
                ));

            return folderName;
        }

        private bool RenameFileByTag(string file, string pattern)
        {
            bool ret = false;

            if (IsExtensionSupported(file))
            {
                TagLib.File tagFile = TagLib.File.Create(file);

                string filenameNew = GenerateFilenameWithTag(tagFile, pattern);

                if (String.IsNullOrEmpty(filenameNew) == false)
                {
                    string filenameOld = Path.GetFileNameWithoutExtension(file);
                    string path = Path.GetDirectoryName(file);
                    string ext = Path.GetExtension(file);
                    string filePathNew = String.Format("{0}\\{1}{2}", path, filenameNew, ext);

                    try
                    {
                        System.IO.File.Move(file, filePathNew);

                        queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("{0} succesfully renamed to {1}", filenameOld, filenameNew)));

                        ret = true;

                    }
                    catch (Exception e)
                    {
                        ret = false;
                        queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.ERROR, String.Format("Error renaming file {0} to {1}: {2}", filenameOld, filenameNew, e.Message)));                        
                    }
                }
                else
                {
                    ret = false;
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.ERROR, "new filename was blank, check the tag inside file or the patten"));
                }
            }

            return ret;
        }

        #endregion

        #region Methods Tag

        /// <summary>
        /// TagFile with lastfm
        /// </summary>
        /// <param name="file"></param>
        /// <param name="track"></param>
        /// <param name="artist"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        //[Obsolete("", true)]
        //public bool TagFileLastfm(string file, string track, string artist, TAG_FIELDS mode)
        //{
        //    bool ret = false;

        //    List<TrackInfo> trackInfos = new List<TrackInfo>();

        //    ModelTag input = new ModelTag();

        //    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, "calling lastfm service"));

        //    string xml = lastfmWSDelegate.GetTrackInfo(track, artist);

        //    if (String.IsNullOrEmpty(xml) == false)
        //    {
        //        input = FillInputTrackFromLastfm(input, xml, mode);

        //        if ((mode & TAG_FIELDS.YEAR) != TAG_FIELDS.NONE && String.IsNullOrEmpty(input.Album) == false)
        //        {
        //            xml = lastfmWSDelegate.GetAlbumInfo(input.Album, artist);

        //            input = FillInputReleaseFromLastfm(input, xml, TAG_FIELDS.YEAR);
        //        }

        //        ret = TagFile(file, input, mode);
        //    }
        //    else
        //        queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.ERROR, "some errors were reported while calling lastfm services"));

        //    return ret;
        //}


        private string GenerateFilenameWithTag(TagLib.File tagFile, string pattern)
        {
            //StringBuilder sb = new StringBuilder(pattern);

            string filenameReplaced = pattern;

            // title
            Regex rgx = new Regex(PatternTitle, RegexOptions.IgnoreCase);

            filenameReplaced = rgx.Replace(filenameReplaced, tagFile.Tag.Title ?? String.Empty);

            // album artist
            rgx = new Regex(PatternAlbumArtist, RegexOptions.IgnoreCase);

            filenameReplaced = rgx.Replace(filenameReplaced, String.Join(",", tagFile.Tag.AlbumArtists));

            // track artist
            rgx = new Regex(PatternTrackArtist, RegexOptions.IgnoreCase);

            filenameReplaced = rgx.Replace(filenameReplaced, String.Join(",", tagFile.Tag.Performers));

            // album
            rgx = new Regex(PatternAlbum, RegexOptions.IgnoreCase);

            filenameReplaced = rgx.Replace(filenameReplaced, tagFile.Tag.Album ?? String.Empty);

            // year
            rgx = new Regex(PatternYear, RegexOptions.IgnoreCase);

            filenameReplaced = rgx.Replace(filenameReplaced, tagFile.Tag.Year != null ? tagFile.Tag.Year.ToString() : String.Empty);

            // position
            rgx = new Regex(PatternPos, RegexOptions.IgnoreCase);

            filenameReplaced = rgx.Replace(filenameReplaced, tagFile.Tag.Track != null ? tagFile.Tag.Track.ToString("D2") : String.Empty);

            // replace illegal characters
            filenameReplaced = Path.GetInvalidFileNameChars().Aggregate(filenameReplaced, (current, c) => current.Replace(c.ToString(), string.Empty));

            return filenameReplaced;
        }

        [Obsolete("Do not use this")]
        private string GenerateFilenameWithTagOld(TagLib.File tagFile, string pattern)
        {
            StringBuilder sb = new StringBuilder(pattern);

            //TagLib.File tagFile = TagLib.File.Create(filename);

            // title
            Regex rgx = new Regex(PatternTitle, RegexOptions.IgnoreCase);

            MatchCollection matches = rgx.Matches(sb.ToString());

            sb.Append(rgx.Replace(sb.ToString(), tagFile.Tag.Title));

            //foreach (Match match in matches)
            //    sb = sb.Replace(match.Value, tagFile.Tag.Title);

            // album artist
            rgx = new Regex(PatternAlbumArtist, RegexOptions.IgnoreCase);

            matches = rgx.Matches(sb.ToString());

            foreach (Match match in matches)
                sb = sb.Replace(match.Value, String.Join(",", tagFile.Tag.AlbumArtists));

            // track artist
            rgx = new Regex(PatternTrackArtist, RegexOptions.IgnoreCase);

            matches = rgx.Matches(sb.ToString());

            foreach (Match match in matches)
                sb = sb.Replace(match.Value, String.Join(",", tagFile.Tag.Performers));

            // album
            rgx = new Regex(PatternAlbum, RegexOptions.IgnoreCase);

            matches = rgx.Matches(sb.ToString());

            foreach (Match match in matches)
                sb = sb.Replace(match.Value, tagFile.Tag.Album);

            // year
            rgx = new Regex(PatternYear, RegexOptions.IgnoreCase);

            matches = rgx.Matches(sb.ToString());

            foreach (Match match in matches)
                sb = sb.Replace(match.Value, tagFile.Tag.Year.ToString());

            // position
            rgx = new Regex(PatternPos, RegexOptions.IgnoreCase);

            matches = rgx.Matches(sb.ToString());

            foreach (Match match in matches)
                sb = sb.Replace(match.Value, tagFile.Tag.Track.ToString("D2"));

            string filenameReplaced = sb.ToString();

            // replace illegal characters
            filenameReplaced = Path.GetInvalidFileNameChars().Aggregate(filenameReplaced, (current, c) => current.Replace(c.ToString(), string.Empty));

            return filenameReplaced;
        }

        private Func<string, List<TrackInfo>, TrackInfo> ComparerByFilename(FILENAME_MATCH mode)
        {
            switch (mode)
            {
                case FILENAME_MATCH.TRACK_POSITION:
                    return MTUtility.GetTrackInfoByPosition;
                case FILENAME_MATCH.LEVENSHTEIN_DISTANCE:
                    return MTUtility.GetTrackInfoByLevenshteinDistance;
                case FILENAME_MATCH.JAROWINKLER_DISTANCE:
                    return MTUtility.GetTrackInfoByJaroWinklerDistance;
            }

            return null;
        }

        [Obsolete(null, true)]
        private Picture GetPictureFromInput(InputTag input)
        {
            TagLib.Picture picture = null;

            if (String.IsNullOrEmpty(input.ImagePath) == false)
            {
                if (MTUtility.IsLocalPath(input.ImagePath))
                    picture = new TagLib.Picture(input.ImagePath);
                else
                    picture = MTUtility.PictureFromImage(MTUtility.ImageFromUri(input.ImagePath, proxy));
            }
            else if (input.ImageData != null)
            {
                picture = MTUtility.PictureFromImage(input.ImageData);
            }

            return picture;
        }

        public TrackInfo GetTag(string file)
        {
            TrackInfo output = new TrackInfo();

            if (IsExtensionSupported(file))
            {
                TagLib.File tagFile = TagLib.File.Create(file);

                Tag tag = tagFile.Tag;

                ArtistInfo[] artists = new ArtistInfo[tag.Performers.Length];

                int i = 0;

                foreach (string artist in tag.Performers)
                {
                    artists[i++] = new ArtistInfo
                    {
                        Name = artist,
                        Mbid = tag.MusicBrainzArtistId
                    };
                }

                output.Artists = artists;
                output.Mbid = tag.MusicBrainzTrackId;

                i = 0;

                ArtistInfo[] albumArtists = new ArtistInfo[tag.AlbumArtists.Length];

                foreach (string artist in tag.AlbumArtists)
                {
                    albumArtists[i++] = new ArtistInfo
                    {
                        Name = artist,
                        Mbid = tag.MusicBrainzReleaseArtistId
                    };
                }

                output.Releases = new ReleaseInfo[] { 
                    new ReleaseInfo{ 
                        Title = tag.Album,
                        Mbid = tag.MusicBrainzReleaseId,
                        Year = tag.Year.ToString(),                      
                        Artists = albumArtists,
                        Picture = tag.Pictures.Length > 0 ? MTUtility.ImageFromPicture(tag.Pictures[0]) : null
                    } 

                };

                output.Title = tag.Title;
                output.Track = tag.Track;
                output.Genres = tag.Genres;
            }

            return output;
        }

        //[Obsolete()]
        //public OutputInfo GetTag(string file)
        //{
        //    OutputInfo output = new OutputInfo();

        //    if (IsExtensionSupported(file))
        //    {
        //        TagLib.File tagFile = TagLib.File.Create(file);

        //        Tag tag = tagFile.Tag;

        //        output.Album = tag.Album;
        //        output.ArtistsAlbum = tag.AlbumArtists;
        //        output.ArtistsTrack = tag.Composers;
        //        output.Genres = tag.Genres;
        //        output.ArtistMbid = tag.MusicBrainzArtistId;
        //        if (tag.Pictures.Length > 0)
        //            output.ImageData = MTUtility.ImageFromPicture(tag.Pictures[0]);
        //        output.ReleaseMbid = tag.MusicBrainzReleaseId;
        //        output.Year = tag.Year;
        //        output.TrackInfos = new TrackInfo[] { 
        //            new TrackInfo
        //            {
        //                Mbid = tag.MusicBrainzTrackId,
        //                Title = tag.Title,
        //                Track = tag.Track
        //            }
        //        };
        //    }

        //    return output;
        //}

        /// <summary>
        /// TagFile
        /// </summary>
        /// <param name="file"></param>
        /// <param name="inputTag"></param>
        /// <param name="mode"></param>
        /// <returns></returns>        
        public bool TagFile(string file, ModelTag inputTag, TAG_FIELDS mode)
        {
            bool result = false;

            if (IsExtensionSupported(file))
            {
                queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO,
                    String.Format("Processing file: {0}", Path.GetFileName(file))));

                TagLib.File tagFile = TagLib.File.Create(file);

                if ((mode & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing album tag: {0}", inputTag.Album)));
                    tagFile.Tag.Album = inputTag.Album;
                }
                if ((mode & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing album artist tag: {0}", inputTag.AlbumArtists == null ? String.Empty : String.Join(",", inputTag.AlbumArtists))));
                    // artist album
                    tagFile.Tag.AlbumArtists = inputTag.AlbumArtists;
                }
                if ((mode & TAG_FIELDS.TRACK_ARTIST) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing track artist tag: {0}", inputTag.TrackArtists == null ? String.Empty : String.Join(",", inputTag.TrackArtists))));
                    // performers
                    tagFile.Tag.Performers = inputTag.TrackArtists;
                }
                if ((mode & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE)
                {
                    if (inputTag.Picture != null)
                    {
                        tagFile.Tag.Pictures = new TagLib.IPicture[] { MTUtility.PictureFromImage(inputTag.Picture) };
                        if (tagFile.Tag.Pictures.Length > 0)
                        {
                            queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, "writing front cover image"));
                            tagFile.Tag.Pictures[0].Type = TagLib.PictureType.FrontCover;
                        }
                    }
                    else
                    {
                        queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, "no input image was specified, removing front cover image"));
                        tagFile.Tag.Pictures = new TagLib.IPicture[0];
                    }
                }
                if ((mode & TAG_FIELDS.YEAR) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing year tag: {0}", inputTag.Year)));
                    tagFile.Tag.Year = inputTag.Year;
                }
                if ((mode & TAG_FIELDS.GENRES) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing genres tag: {0}", inputTag.Genres != null ? string.Join(",", inputTag.Genres) : String.Empty)));
                    tagFile.Tag.Genres = inputTag.Genres;
                }
                if ((mode & TAG_FIELDS.TITLE) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing title tag: {0}", inputTag.Title)));
                    tagFile.Tag.Title = inputTag.Title;
                }
                if ((mode & TAG_FIELDS.TRACK_POS) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing position tag: {0}", inputTag.Position)));
                    tagFile.Tag.Track = inputTag.Position;
                }
                if ((mode & TAG_FIELDS.MUSICBRAINZ_ID) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, "writing MusicBrainzId tags"));

                    tagFile.Tag.MusicBrainzTrackId = inputTag.TrackMbid;
                    tagFile.Tag.MusicBrainzArtistId = inputTag.ArtistMbid;
                    tagFile.Tag.MusicBrainzReleaseId = inputTag.ReleaseMbid;
                }

                try
                {
                    tagFile.Save();                    
                    
                    result = true;

                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("tag info saved for file {0}", Path.GetFileName(file))));
                }
                catch (Exception e)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.ERROR, String.Format("error while tagging file {0}: {1}", Path.GetFileName(file), e.Message)));
                    throw e;
                }
            }

            return result;
        }

        /// <summary>
        /// TagFile
        /// </summary>
        /// <param name="file"></param>
        /// <param name="input"></param>
        /// <param name="mode"></param>
        /// <param name="picture"></param>
        /// <returns></returns>
        [Obsolete("InputTag should not be used", true)]
        private bool TagFile(string file, InputTag input, TAG_FIELDS mode, Picture picture)
        {
            if (IsExtensionSupported(file))
            {
                queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO,
                    String.Format("Processing file: {0}", Path.GetFileName(file))));

                TagLib.File tagFile = TagLib.File.Create(file);

                if ((mode & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing album tag: {0}", input.Album)));
                    tagFile.Tag.Album = input.Album;
                }
                if ((mode & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing artist tag: {0}", input.AlbumArtists)));
                    // performers
                    tagFile.Tag.Performers = input.TrackArtists;
                    // artist album
                    tagFile.Tag.AlbumArtists = input.AlbumArtists;
                }
                if ((mode & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE)
                {
                    if (picture != null)
                    {
                        tagFile.Tag.Pictures = new TagLib.IPicture[] { picture };
                        if (tagFile.Tag.Pictures.Length > 0)
                        {
                            queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, "writing front cover image"));
                            tagFile.Tag.Pictures[0].Type = TagLib.PictureType.FrontCover;
                        }
                    }
                    else
                    {
                        queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, "removing front cover image"));
                        tagFile.Tag.Pictures = new TagLib.IPicture[0];
                    }
                }
                if ((mode & TAG_FIELDS.YEAR) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing year tag: {0}", input.Year)));
                    tagFile.Tag.Year = input.Year;
                }
                if ((mode & TAG_FIELDS.GENRES) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing genres tag: {0}", input.Genres != null ? string.Join(",", input.Genres) : String.Empty)));
                    tagFile.Tag.Genres = input.Genres;
                }
                if ((mode & TAG_FIELDS.TITLE) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing title tag: {0}", input.Title)));
                    tagFile.Tag.Title = input.Title;
                }
                if ((mode & TAG_FIELDS.TRACK_POS) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("writing position tag: {0}", input.Position)));
                    tagFile.Tag.Track = input.Position;
                }
                if ((mode & TAG_FIELDS.MUSICBRAINZ_ID) != TAG_FIELDS.NONE)
                {
                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, "writing MusicBrainzId tags"));
                    tagFile.Tag.MusicBrainzTrackId = input.TrackMbid;
                    tagFile.Tag.MusicBrainzArtistId = input.ArtistMbid;
                    tagFile.Tag.MusicBrainzReleaseId = input.ReleaseMbid;
                }

                tagFile.Save();

                queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("tag info saved for file {0}", Path.GetFileName(file))));
            }

            return true;
        }


        /// <summary>
        /// TagFiles
        /// </summary>
        /// <param name="files"></param>
        /// <param name="input"></param>
        /// <param name="mode"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        //[Obsolete("InputTag should not be used", true)]
        //public bool TagFiles(string[] files, InputTag input, TAG_FIELDS mode)
        //{
        //    bool ret = false;

        //    List<TrackInfo> trackInfos = new List<TrackInfo>();

        //    TagLib.Picture picture = GetPictureFromInput(input);

        //    foreach (string file in files)
        //    {
        //        TagFile(file, input, mode, picture);
        //    }

        //    ret = true;

        //    return ret;
        //}


        //public bool TagFiles(string[] files, ModelTag input, TAG_FIELDS mode)
        //{
        //    bool ret = false;

        //    foreach (string file in files)
        //    {
        //        TagFile(file, input, mode);
        //    }

        //    ret = true;

        //    return ret;
        //}


        /// <summary>
        /// TagFile
        /// </summary>
        /// <param name="file"></param>
        /// <param name="trackInfo"></param>
        /// <param name="input"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [Obsolete("InputTag should not be used", true)]
        public bool TagFile(string file, InputTag input, TAG_FIELDS mode)
        {
            //input = FillInputTagFromTrackInfo(input, trackInfo);

            Picture picture = GetPictureFromInput(input);

            return TagFile(file, input, mode, picture);
        }


        /// <summary>
        /// TagFilesLastfm
        /// </summary>
        /// <param name="path"></param>
        /// <param name="album"></param>
        /// <param name="artist"></param>
        /// <param name="mode"></param>
        /// <param name="ext"></param>
        /// <param name="searchOption"></param>
        /// <param name="seekTitleMode"></param>
        /// <returns></returns>
        //[Obsolete()]
        //public bool TagFilesLastfm(string[] files, string album, string artist, TAG_FIELDS mode, FILENAME_MATCH seekTitleMode)
        //{
        //    bool ret = false;

        //    List<TrackInfo> trackInfos = new List<TrackInfo>();

        //    ModelTag input = new ModelTag();

        //    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, "calling lastfm service"));

        //    string xml = lastfmWSDelegate.GetAlbumInfo(album, artist);

        //    if (String.IsNullOrEmpty(xml) == false)
        //    {
        //        input = FillInputReleaseFromLastfm(input, xml, mode);

        //        if ((mode & (TAG_FIELDS.TITLE | TAG_FIELDS.TRACK_POS | TAG_FIELDS.MUSICBRAINZ_ID)) != TAG_FIELDS.NONE)
        //        {
        //            XmlDocument xmlDoc = new XmlDocument();

        //            xmlDoc.LoadXml(xml);

        //            trackInfos = lastfmParser.GetTrackInfosFromAlbum(xmlDoc);
        //        }                

        //        bool tracksFound = trackInfos.Count > 0;

        //        if (tracksFound)
        //            queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("{0} tracks were found: ", trackInfos.Count)));
        //        else
        //            queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.WARNING, "No tracks found for this search"));

        //        foreach (TrackInfo track in trackInfos)
        //        {
        //            queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("{0}: {1}", track.Track.ToString("00"), track.Title)));
        //        }

        //        bool isAnyTrackTagRequested = ((mode & (TAG_FIELDS.TITLE | TAG_FIELDS.TRACK_POS | TAG_FIELDS.MUSICBRAINZ_ID)) != TAG_FIELDS.NONE);

        //        Func<string, List<TrackInfo>, TrackInfo> comparerByFilename = ComparerByFilename(seekTitleMode);

        //        foreach (string file in files)
        //        {
        //            if (IsExtensionSupported(file))
        //            {
        //                if (isAnyTrackTagRequested && tracksFound)
        //                {
        //                    string filename = Path.GetFileNameWithoutExtension(file);

        //                    TrackInfo trackInfo = comparerByFilename(filename, trackInfos);

        //                    queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.INFO, String.Format("title found for file {0}: {1}", Path.GetFileNameWithoutExtension(file), trackInfo.Title)));

        //                    input = FillInputTagFromTrackInfo(input, trackInfo);
        //                }

        //                TagFile(file, input, mode);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        queueMessage.Enqueue(new UMTMessage(UMTMessage.M_TYPE.ERROR, "some errors were reported while calling lastfm services"));
        //    }

        //    ret = true;

        //    return ret;
        //}

        #endregion

        #region Methods Lastfm


        ///// <summary>
        ///// GetAlbumCoverFromLastfm
        ///// </summary>
        ///// <param name="album"></param>
        ///// <param name="artist"></param>
        ///// <param name="size"></param>
        ///// <returns></returns>
        //public Image GetAlbumCoverFromLastfm(string album, string artist, IMAGE_RELEASE_SIZE size)
        //{
        //    string xml = lastfmWSDelegate.GetAlbumInfo(album, artist);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    Image output = null;

        //    string imageUri = lastfmParser.GetAlbumImage(xmlDoc, IMAGE_RELEASE_SIZE.MEDIUM);
        //    if (String.IsNullOrEmpty(imageUri) == false)
        //    {
        //        output = MTUtility.ImageFromUri(imageUri, this.proxy);
        //    }

        //    return output;
        //}

        //private TrackInfo GetTrackInfoLastfm(XmlDocument xmlDoc)
        //{
        //    TrackInfo output = new TrackInfo();

        //    ModelTag input = new ModelTag();

        //    string xml = xmlDoc.InnerXml;

        //    TAG_FIELDS mode = 
        //        TAG_FIELDS.ALBUM | TAG_FIELDS.ALBUM_ARTIST | TAG_FIELDS.TRACK_ARTIST | 
        //        TAG_FIELDS.GENRES | TAG_FIELDS.IMAGE | TAG_FIELDS.MUSICBRAINZ_ID |
        //        TAG_FIELDS.TITLE | TAG_FIELDS.TRACK_POS | TAG_FIELDS.YEAR;

        //    output = FillInputTrackInfoFromLastfm(input, xml, mode);

        //    string album = input.Album;

        //    string artist = input.AlbumArtists[0];

        //    if (String.IsNullOrEmpty(album) == false)
        //    {
        //        xml = lastfmWSDelegate.GetAlbumInfo(album, artist);

        //        ReleaseInfo release = FillInputReleaseInfoFromLastfm(input, xml, TAG_FIELDS.YEAR);

        //        output.Releases[0].Year = release.Year;
        //    }

        //    return output;
        //}

        //[Obsolete()]
        //private OutputInfo GetTrackOutputInfoLastfm(XmlDocument xmlDoc)
        //{

        //    OutputInfo output = new OutputInfo();

        //    ModelTag input = new ModelTag();

        //    string xml = xmlDoc.InnerXml;

        //    TAG_FIELDS mode = TAG_FIELDS.ALBUM | TAG_FIELDS.ALBUM_ARTIST | TAG_FIELDS.GENRES |
        //        TAG_FIELDS.IMAGE | TAG_FIELDS.MUSICBRAINZ_ID |
        //        TAG_FIELDS.TITLE | TAG_FIELDS.TRACK_POS | TAG_FIELDS.YEAR;

        //    input = FillInputTrackFromLastfm(input, xml, mode);

        //    string album = input.Album;

        //    string artist = input.AlbumArtists[0];

        //    if (String.IsNullOrEmpty(album) == false)
        //    {
        //        xml = lastfmWSDelegate.GetAlbumInfo(album, artist);

        //        input = FillInputReleaseFromLastfm(input, xml, TAG_FIELDS.YEAR);

        //    }

        //    output.Album = input.Album;
        //    output.ReleaseMbid = input.ReleaseMbid;
        //    output.ArtistsAlbum = input.AlbumArtists ;
        //    output.ArtistsTrack = input.TrackArtists ;
        //    output.ArtistMbid = input.ArtistMbid;
        //    output.Genres = input.Genres;
        //    output.ImageData = input.Picture;
        //    output.Year = input.Year;

        //    TrackInfo trackInfo = new TrackInfo();
        //    trackInfo.Track = input.Position;
        //    trackInfo.Mbid = input.TrackMbid;
        //    trackInfo.Title = input.Title;

        //    output.TrackInfos = new TrackInfo[] { trackInfo };

        //    return output;
        //}

        //public TrackInfo GetTrackInfoLastfm(string mbid)
        //{
        //    string xml = lastfmWSDelegate.GetTrackInfo(mbid);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    return GetTrackInfoLastfm(xmlDoc);
        //} 


        //public TrackInfo GetTrackInfoLastfm(string title, string artist)
        //{
        //    string xml = lastfmWSDelegate.GetTrackInfo(title, artist);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    return GetTrackInfoLastfm(xmlDoc);
        //}

        ///// <summary>
        ///// GetTopTracksFromLastfm
        ///// </summary>
        ///// <param name="artist"></param>
        ///// <returns></returns>
        //public List<TrackInfo> GetTopTracksFromLastfm(string artist)
        //{
        //    string xml = lastfmWSDelegate.GetTopTracks(artist);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    List<TrackInfo> listTracks = lastfmParser.GetTopTracks(xmlDoc);

        //    return listTracks;
        //}


        //public List<ReleaseInfo> GetTopAlbumsFromLastfm(string artist)
        //{
        //    string xml = lastfmWSDelegate.GetTopAlbums(artist);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    List<ReleaseInfo> listAlbums = lastfmParser.GetTopAlbums(xmlDoc);

        //    return listAlbums;
        //}



        //private ReleaseInfo GetAlbumInfoLastfm(XmlDocument xmlDoc)
        //{
        //    ReleaseInfo output = null;

        //    output = lastfmParser.GetAlbum(xmlDoc);

        //    return output;
        //}

        //[Obsolete()]
        //private OutputInfo GetReleaseOutputInfoLastfm(XmlDocument xmlDoc)
        //{
        //    ModelTag input = new ModelTag();

        //    TAG_FIELDS mode = TAG_FIELDS.ALBUM | TAG_FIELDS.ALBUM_ARTIST | TAG_FIELDS.GENRES |
        //        TAG_FIELDS.IMAGE | TAG_FIELDS.MUSICBRAINZ_ID |
        //        TAG_FIELDS.TITLE | TAG_FIELDS.TRACK_POS | TAG_FIELDS.YEAR;

        //    input = FillInputReleaseFromLastfm(input, xmlDoc.InnerXml, mode);

        //    List<TrackInfo> trackInfos = lastfmParser.GetTrackInfosFromAlbum(xmlDoc);

        //    OutputInfo output = new OutputInfo();

        //    output.Album = input.Album;
        //    output.ReleaseMbid = input.ReleaseMbid;
        //    output.ArtistsTrack = input.AlbumArtists ;
        //    output.ArtistsAlbum = input.AlbumArtists ;
        //    output.ArtistMbid = input.ArtistMbid;
        //    output.Genres = input.Genres;
        //    output.ImageData = input.Picture;
        //    output.Year = input.Year;
        //    output.TrackInfos = trackInfos.ToArray();

        //    return output;
        //}


        ///// <summary>
        ///// GetReleaseOutputInfo
        ///// </summary>
        ///// <param name="album"></param>
        ///// <param name="artist"></param>
        ///// <returns></returns>
        //public ReleaseInfo GetReleaseInfoLastfm(string album, string artist)
        //{
        //    string xml = lastfmWSDelegate.GetAlbumInfo(album, artist);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    return GetAlbumInfoLastfm(xmlDoc);
        //}


        //public ReleaseInfo GetReleaseInfoLastfm(string mbid)
        //{
        //    string xml = lastfmWSDelegate.GetAlbumInfo(mbid);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    return GetAlbumInfoLastfm(xmlDoc);
        //}


        #endregion

        #region Methods MusicBrainz


        //public ReleaseInfo[] GetReleaseMusicBrainz(string album, string artist)
        //{
        //    string xml = musicBrainzWSDelegate.GetReleaseList(album, artist);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    return GetReleaseMusicBrainz(xmlDoc);
        //}



        //public List<ReleaseInfo> GetReleaseArtistMusicBrainz(string artist)
        //{
        //    string xml = musicBrainzWSDelegate.GetReleaseArtist(artist);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    return musicBrainzParser.GetReleaseList(xmlDoc);
        //}

        //private ReleaseInfo[] GetReleaseMusicBrainz(XmlDocument xmlDoc)
        //{
        //    ReleaseInfo[] output = null;

        //    musicBrainzParser.InitXmlNamespaceManager(xmlDoc);

        //    output = musicBrainzParser.GetReleaseList(xmlDoc).ToArray();

        //    return output;
        //}



        //[Obsolete(null, true)]
        //private OutputInfo GetReleaseOutputInfoMusicBrainz(XmlDocument xmlDoc)
        //{
        //    InputTag input = new InputTag();

        //    TAG_FIELDS mode = TAG_FIELDS.ALBUM | TAG_FIELDS.ALBUM_ARTIST | TAG_FIELDS.GENRES |
        //        TAG_FIELDS.IMAGE | TAG_FIELDS.MUSICBRAINZ_ID |
        //        TAG_FIELDS.TITLE | TAG_FIELDS.TRACK_POS | TAG_FIELDS.YEAR;

        //    input = FillInputReleaseFromMusicBrainz(input, xmlDoc, mode);

        //    List<TrackInfo> trackInfos = lastfmParser.GetTrackInfosFromAlbum(xmlDoc);

        //    OutputInfo output = new OutputInfo();

        //    output.Album = input.Album;
        //    output.ReleaseMbid = input.ReleaseMbid;
        //    output.ArtistsTrack = input.TrackArtists ;
        //    output.ArtistsAlbum = input.AlbumArtists ;
        //    output.ArtistMbid = input.ArtistMbid;
        //    output.Genres = input.Genres;
        //    output.ImageData = input.ImageData;
        //    output.Year = input.Year;
        //    output.TrackInfos = trackInfos.ToArray();

        //    return output;
        //}


        ///// <summary>
        ///// GetReleaseOutputInfoMusicBrainz
        ///// </summary>
        ///// <param name="album"></param>
        ///// <param name="artist"></param>
        ///// <returns></returns>
        //public OutputInfo GetReleaseOutputInfoMusicBrainz(string album, string artist)
        //{
        //    string xml = musicBrainzWSDelegate.GetReleaseGroup(album, artist);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    return GetReleaseOutputInfoLastfm(xmlDoc);
        //}


        //public TrackInfo GetRecordingInfoMusicBrainz(string mbid)
        //{
        //    string xml = musicBrainzWSDelegate.GetRecording(mbid);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    musicBrainzParser.InitXmlNamespaceManager(xmlDoc);

        //    return GetRecordingMusicBrainz(xmlDoc)[0];
        //}

        //public TrackInfo[] GetRecordingInfoMusicBrainz(string title, string artist)
        //{
        //    string xml = musicBrainzWSDelegate.GetRecording(title, artist);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    musicBrainzParser.InitXmlNamespaceManager(xmlDoc);

        //    return GetRecordingMusicBrainz(xmlDoc);
        //}

        //private TrackInfo[] GetRecordingMusicBrainz(XmlDocument xmlDoc)
        //{
        //    TrackInfo[] output = null;

        //    output = GetRecordingsFromMusicBrainz(xmlDoc);

        //    return output;
        //}

        //public List<ReleaseInfo> GetReleasesFromMusicBrainz(string artist)
        //{
        //    string xml = musicBrainzWSDelegate.GetReleaseArtist(artist);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml(xml);

        //    musicBrainzParser.InitXmlNamespaceManager(xmlDoc);

        //    List<ReleaseInfo> listRelease = musicBrainzParser.GetReleaseList(xmlDoc);

        //    return listRelease;
        //}


        #endregion

        #region Methods FillObject

        private ModelTag FillInputTagFromTrackInfo(ModelTag input, TrackInfo trackInfo)
        {
            if (trackInfo == null)
                trackInfo = new TrackInfo();

            input.TrackMbid = trackInfo.Mbid;
            input.Title = trackInfo.Title;
            input.Position = trackInfo.Track;
            input.TrackArtists = trackInfo.Artists.Select(x => x.Name).ToArray();

            return input;
        }

        [Obsolete("InputTag", true)]
        private InputTag FillInputTagFromTrackInfo(InputTag input, TrackInfo trackInfo)
        {
            if (trackInfo == null)
                trackInfo = new TrackInfo();

            input.TrackMbid = trackInfo.Mbid;
            input.Title = trackInfo.Title;
            input.Position = trackInfo.Track;

            return input;
        }

        private ReleaseInfo FillInputReleaseInfoFromLastfm(ModelTag input, string xml, TAG_FIELDS mode)
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            string imageUri = null;

            if ((mode & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE)
            {
                input.Album = lastfmParser.GetReleaseName(xmlDoc);
            }
            if ((mode & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE)
            {
                input.AlbumArtists = lastfmParser.GetReleaseArtist(xmlDoc);
            }
            if ((mode & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE)
            {
                imageUri = lastfmParser.GetAlbumImage(xmlDoc, IMAGE_RELEASE_SIZE.MEDIUM);
                if (String.IsNullOrEmpty(imageUri) == false)
                {
                    input.Picture = MTUtility.ImageFromUri(imageUri, this.proxy);
                }
            }
            if ((mode & TAG_FIELDS.YEAR) != TAG_FIELDS.NONE)
            {
                input.Year = lastfmParser.GetYear(xmlDoc);
            }
            if ((mode & TAG_FIELDS.MUSICBRAINZ_ID) != TAG_FIELDS.NONE)
            {
                input.ReleaseMbid = lastfmParser.GetReleaseMbid(xmlDoc);
                input.ArtistMbid = lastfmParser.GetArtistMbid(xmlDoc);
            }

            ReleaseInfo output = new ReleaseInfo
            {
                Artists = new ArtistInfo[]
                {
                    new ArtistInfo
                    {
                        Mbid = input.ArtistMbid,
                        Name = input.AlbumArtists[0],                        
                    }
                },
                ImagePath = imageUri,
                Mbid = input.ReleaseMbid,
                Title = input.Title
            };

            return output;
        }

        private ModelTag FillInputReleaseFromLastfm(ModelTag input, string xml, TAG_FIELDS mode)
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            if ((mode & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE)
            {
                input.Album = lastfmParser.GetReleaseName(xmlDoc);
            }
            if ((mode & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE)
            {
                input.AlbumArtists = lastfmParser.GetReleaseArtist(xmlDoc);
            }
            if ((mode & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE)
            {
                string imageUri = lastfmParser.GetAlbumImage(xmlDoc, IMAGE_RELEASE_SIZE.MEDIUM);
                if (String.IsNullOrEmpty(imageUri) == false)
                {
                    input.Picture = MTUtility.ImageFromUri(imageUri, this.proxy);
                }
            }
            if ((mode & TAG_FIELDS.YEAR) != TAG_FIELDS.NONE)
            {
                input.Year = lastfmParser.GetYear(xmlDoc);
            }
            if ((mode & TAG_FIELDS.MUSICBRAINZ_ID) != TAG_FIELDS.NONE)
            {
                input.ReleaseMbid = lastfmParser.GetReleaseMbid(xmlDoc);
                input.ArtistMbid = lastfmParser.GetArtistMbid(xmlDoc);
            }

            return input;
        }

        [Obsolete("InputTag", true)]
        private InputTag FillInputReleaseFromLastfm(InputTag input, string xml, TAG_FIELDS mode)
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            if ((mode & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE)
            {
                input.Album = lastfmParser.GetReleaseName(xmlDoc);
            }
            if ((mode & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE)
            {
                input.AlbumArtists = lastfmParser.GetReleaseArtist(xmlDoc);
            }
            if ((mode & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE)
            {
                string imageUri = lastfmParser.GetAlbumImage(xmlDoc, IMAGE_RELEASE_SIZE.MEDIUM);
                if (String.IsNullOrEmpty(imageUri) == false)
                {
                    input.ImageData = MTUtility.ImageFromUri(imageUri, this.proxy);
                }
            }
            if ((mode & TAG_FIELDS.YEAR) != TAG_FIELDS.NONE)
            {
                input.Year = lastfmParser.GetYear(xmlDoc);
            }
            if ((mode & TAG_FIELDS.MUSICBRAINZ_ID) != TAG_FIELDS.NONE)
            {
                input.ReleaseMbid = lastfmParser.GetReleaseMbid(xmlDoc);
            }

            return input;
        }

        private TrackInfo[] GetRecordingsFromMusicBrainz(XmlDocument xmlDoc)
        {
            string xml = xmlDoc.InnerXml;

            List<TrackInfo> output = musicBrainzParser.GetRecordingList(xmlDoc);

            return output.ToArray();
        }

        //[Obsolete(null, true)]
        //private InputTag FillInputTrackFromMusicBrainz(InputTag input, XmlDocument xmlDoc, TAG_FIELDS mode)
        //{
        //    string xml = xmlDoc.InnerXml;

        //    input.ReleaseMbid = musicBrainzParser.GetReleaseMbid(xmlDoc);
        //    input.TrackMbid = musicBrainzParser.GetRecordingMbid(xmlDoc);
        //    input.ArtistMbid = musicBrainzParser.GetArtistMbid(xmlDoc);

        //    string album = musicBrainzParser.GetReleaseTitle(xmlDoc);

        //    if ((mode & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE)
        //    {
        //        input.Album = album;
        //    }
        //    if ((mode & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE)
        //    {
        //        input.AlbumArtists = musicBrainzParser.GetTrackArtist(xmlDoc) ;
        //    }
        //    if ((mode & TAG_FIELDS.TITLE) != TAG_FIELDS.NONE)
        //    {
        //        input.Title = musicBrainzParser.GetRecordingTitle(xmlDoc);
        //    }
        //    if ((mode & TAG_FIELDS.YEAR) != TAG_FIELDS.NONE)
        //    {
        //        input.Year = musicBrainzParser.GetReleaseYear(xmlDoc);
        //    }

        //    if ((mode & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE)
        //    {
        //        string imageUri = coverArtClient.GetImageUrl("release", input.ReleaseMbid, CoverArtClient.IMAGE_TYPE.FRONT, CoverArtClient.IMAGE_SIZE.SMALL);
        //        if (String.IsNullOrEmpty(imageUri) == false)
        //        {
        //            input.ImageData = MTUtility.ImageFromUri(imageUri, this.proxy);
        //        }
        //    }

        //    return input;
        //}

        [Obsolete("InputTag", true)]
        private InputTag FillInputTrackFromLastfm(InputTag input, string xml, TAG_FIELDS mode)
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            string album = lastfmParser.GetReleaseTitle(xmlDoc);

            if ((mode & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE)
            {
                input.Album = album;
            }
            if ((mode & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE)
            {
                input.AlbumArtists = lastfmParser.GetReleaseArtist(xmlDoc);
            }
            if ((mode & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE)
            {
                string imageUri = lastfmParser.GetAlbumImage(xmlDoc, IMAGE_RELEASE_SIZE.MEDIUM);
                if (String.IsNullOrEmpty(imageUri) == false)
                {
                    input.ImageData = MTUtility.ImageFromUri(imageUri, this.proxy);
                }
            }
            if ((mode & TAG_FIELDS.TITLE) != TAG_FIELDS.NONE)
            {
                input.Title = lastfmParser.GetTrackName(xmlDoc);
            }
            if ((mode & TAG_FIELDS.TRACK_POS) != TAG_FIELDS.NONE)
            {
                input.Position = lastfmParser.GetTrackPosition(xmlDoc);
            }
            if ((mode & TAG_FIELDS.MUSICBRAINZ_ID) != TAG_FIELDS.NONE)
            {
                input.ReleaseMbid = lastfmParser.GetReleaseMbid(xmlDoc);
                input.TrackMbid = lastfmParser.GetTrackMbid(xmlDoc);
                input.ArtistMbid = lastfmParser.GetArtistMbid(xmlDoc);
            }

            return input;
        }

        private ModelTag FillInputTrackFromLastfm(ModelTag input, string xml, TAG_FIELDS mode)
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            string album = lastfmParser.GetReleaseTitle(xmlDoc);

            if ((mode & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE)
            {
                input.Album = album;
            }
            if ((mode & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE)
            {
                input.AlbumArtists = lastfmParser.GetReleaseArtist(xmlDoc);
            }
            if ((mode & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE)
            {
                string imageUri = lastfmParser.GetAlbumImage(xmlDoc, IMAGE_RELEASE_SIZE.MEDIUM);
                if (String.IsNullOrEmpty(imageUri) == false)
                {
                    input.Picture = MTUtility.ImageFromUri(imageUri, this.proxy);
                }
            }
            if ((mode & TAG_FIELDS.TITLE) != TAG_FIELDS.NONE)
            {
                input.Title = lastfmParser.GetTrackName(xmlDoc);
            }
            if ((mode & TAG_FIELDS.TRACK_POS) != TAG_FIELDS.NONE)
            {
                input.Position = lastfmParser.GetTrackPosition(xmlDoc);
            }
            if ((mode & TAG_FIELDS.MUSICBRAINZ_ID) != TAG_FIELDS.NONE)
            {
                input.ReleaseMbid = lastfmParser.GetReleaseMbid(xmlDoc);
                input.TrackMbid = lastfmParser.GetTrackMbid(xmlDoc);
                input.ArtistMbid = lastfmParser.GetArtistMbid(xmlDoc);
            }

            return input;
        }


        private TrackInfo FillInputTrackInfoFromLastfm(ModelTag input, string xml, TAG_FIELDS mode)
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xml);

            string album = lastfmParser.GetReleaseTitle(xmlDoc);

            if ((mode & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE)
            {
                input.Album = album;
            }
            if ((mode & TAG_FIELDS.TRACK_ARTIST) != TAG_FIELDS.NONE)
            {
                input.TrackArtists = lastfmParser.GetTrackArtist(xmlDoc);
            }
            if ((mode & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE)
            {
                input.AlbumArtists = lastfmParser.GetReleaseArtist(xmlDoc);
            }
            if ((mode & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE)
            {
                string imageUri = lastfmParser.GetAlbumImage(xmlDoc, IMAGE_RELEASE_SIZE.MEDIUM);
                if (String.IsNullOrEmpty(imageUri) == false)
                {
                    input.Picture = MTUtility.ImageFromUri(imageUri, this.proxy);
                }
            }
            if ((mode & TAG_FIELDS.TITLE) != TAG_FIELDS.NONE)
            {
                input.Title = lastfmParser.GetTrackName(xmlDoc);
            }
            if ((mode & TAG_FIELDS.TRACK_POS) != TAG_FIELDS.NONE)
            {
                input.Position = lastfmParser.GetTrackPosition(xmlDoc);
            }
            if ((mode & TAG_FIELDS.MUSICBRAINZ_ID) != TAG_FIELDS.NONE)
            {
                input.ReleaseMbid = lastfmParser.GetReleaseMbid(xmlDoc);
                input.TrackMbid = lastfmParser.GetTrackMbid(xmlDoc);
                input.ArtistMbid = lastfmParser.GetArtistMbid(xmlDoc);
            }

            TrackInfo trackInfo = new TrackInfo
            {
                Mbid = input.TrackMbid,
                Title = input.Title,
                Track = input.Position,
                Artists = new ArtistInfo[]
                { 
                    new ArtistInfo
                    {
                        Mbid = input.ArtistMbid,
                        Name = input.TrackArtists != null && input.TrackArtists.Length > 0 ? 
                            input.TrackArtists[0] : null
                    }
                },
                Releases = new ReleaseInfo[]
                {
                    new ReleaseInfo
                    {
                        Mbid = input.ReleaseMbid,
                        Artists = new ArtistInfo[]
                        {
                            new ArtistInfo
                            {
                                 Name = input.AlbumArtists != null && input.AlbumArtists.Length > 0 ? 
                                    input.AlbumArtists[0] : null
                            }
                        },
                        Title = input.Album,
                        Picture = input.Picture
                    }
                }
            };


            return trackInfo;
        }



        #endregion

        #region Properties

        private Queue<UMTMessage> queueMessage;

        #endregion

        #region EventHandler

        //public delegate void NotifierEventHandler(UMTMessage umtm);

        //public event NotifierEventHandler NotifierEvent;

        //public void OnNotifierEvent(UMTMessage umtm)
        //{
        //    if (NotifierEvent != null)
        //        NotifierEvent(umtm);
        //}

        #endregion
    }

    #region Old
    /*
    class MusicBrainzUtility : WebUtility
    {
        #region Constructor

        #endregion

        #region Fields

        private WebProxy proxy = null;
        private MusicBrainzClient musicBrainzClient;

        #endregion

        public string GetRecordings(string artist)
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

        public List<TrackInfo> GetRecordings(XmlDocument xmlDoc)
        {
            List<TrackInfo> tracks = new List<TrackInfo>();

            XmlNodeList nodeList = xmlDoc.SelectNodes("//recording");

            foreach (XmlNode node in nodeList)
            {
                TrackInfo output = new TrackInfo();

                XmlNode subNode = null;

                string mbid = node.Attributes["id"].Value;

                subNode = node.SelectSingleNode("title");

                string album = null;

                if (subNode != null)
                    album = subNode.InnerText;

                output.Title = album;
                output.Mbid = mbid;

                tracks.Add(output);
            }

            return tracks;
        }
    }
    */
    #endregion

    #region Old

    /*
    class LastfmUtility : WebUtility
    {
        #region Constructor

        public LastfmUtility(string apikey)
        {
            this.lastfmClient = new LastfmClient(apikey, null);
            this.lastfmClient.UserAgent = String.Format("{0} [ / {1}]", Globals.APP_NAME, MTUtility.GetVersion());
        }

        #endregion

        #region Fields

        private WebProxy proxy = null;
        private LastfmClient lastfmClient;

        #endregion

        #region Methods

        public string GetTopTracks(string artist)
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

        public string GetTopAlbums(string artist)
        {
            string ret = null;

            IDictionary<string, string> param = new Dictionary<string, string>();

            param.Add("artist", artist);
            param.Add("autocorrect", "1");

            try
            {
                ret = lastfmClient.CallMethod("artist.getTopAlbums", param);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }

            return ret;
        }


        public List<TrackInfo> GetTopTracksFromLastfm(XmlDocument xmlDoc)
        {
            List<TrackInfo> tracks = new List<TrackInfo>();

            XmlNodeList nodeList = xmlDoc.SelectNodes("//track ");

            foreach (XmlNode node in nodeList)
            {
                TrackInfo output = new TrackInfo();

                XmlNode subNode = null;

                subNode = node.SelectSingleNode("name");

                string album = null;
                string image = null;

                if (subNode != null)
                    album = subNode.InnerText;

                //subNode = node.SelectSingleNode("image[@size='small']");

                //if (subNode != null)
                //    image = subNode.InnerText;

                output.Title = album;
                //output.ImagePath = image;

                tracks.Add(output);
            }

            return tracks;
        }

        public List<OutputInfo> GetTopAlbumsFromLastfm(XmlDocument xmlDoc)
        {
            List<OutputInfo> albums = new List<OutputInfo>();

            XmlNodeList nodeList = xmlDoc.SelectNodes("//album");

            foreach (XmlNode node in nodeList)
            {
                OutputInfo output = new OutputInfo();

                XmlNode subNode = null;

                subNode = node.SelectSingleNode("name");

                string album = null;
                string image = null;

                if (subNode != null)
                    album = subNode.InnerText;

                subNode = node.SelectSingleNode("image[@size='small']");

                if (subNode != null)
                    image = subNode.InnerText;

                output.Album = album;
                output.ImagePath = image;

                albums.Add(output);
            }

            return albums;
        }

        public List<TrackInfo> GetTrackInfosFromLastfm(XmlDocument xmlDoc)
        {
            List<TrackInfo> trackInfos = new List<TrackInfo>();

            XmlNodeList nodeList = xmlDoc.SelectNodes("//album/tracks/track");

            foreach (XmlNode node in nodeList)
            {
                XmlNode subNode = node.Attributes["rank"];

                string pos = null;
                string title = null;
                string mbid = null;
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

                TrackInfo track = new TrackInfo { Mbid = mbid, Title = title, TrackPosition = trackpos };

                trackInfos.Add(track);
            }


            return trackInfos;
        }

        public string GetAlbumInfoFromLastfm(string album, string artist)
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


        public string GetTrackInfoFromLastfm(string track, string artist)
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

        public uint GetTrackPositionFromLastfm(XmlDocument xmlDoc)
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

        public string GetReleaseMbidFromLastfm(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//album/mbid");

            string mbid = null;

            if (node != null)
                mbid = node.InnerText;

            return mbid;
        }

        public string GetTrackMbidFromLastfm(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//track/mbid");

            string mbid = null;

            if (node != null)
                mbid = node.InnerText;

            return mbid;
        }

        public string GetArtistMbidFromLastfm(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//artist/mbid");

            string mbid = null;

            if (node != null)
                mbid = node.InnerText;

            return mbid;
        }


        public string GetReleaseNameFromLastfm(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//album/name");

            string name = null;

            if (node != null)
                name = node.InnerText;

            return name;
        }

        public string GetReleaseTitleFromLastfm(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//album/title");

            string name = null;

            if (node != null)
                name = node.InnerText;

            return name;
        }

        public string GetTrackNameFromLastfm(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//track/name");

            string name = null;

            if (node != null)
                name = node.InnerText;

            return name;
        }


        public string GetReleaseArtistFromLastfm(XmlDocument xmlDoc)
        {
            XmlNode node = null;

            node = xmlDoc.SelectSingleNode("//album/artist");

            string artist = null;

            if (node != null)
                artist = node.InnerText;

            return artist;
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

        public Image GetAlbumImageFromLastfmUri(XmlDocument xmlDoc, IMAGE_RELEASE_SIZE releaseSize)
        {
            Image retImg = null;

            try
            {
                XmlNode node = null;

                string size = GetDescriptionFromSize(releaseSize);

                node = xmlDoc.SelectSingleNode(String.Format("//album/image[@size='{0}']", size));

                string imageUri = null;

                if (node != null)
                {
                    imageUri = node.InnerText;
                    retImg = MTUtility.ImageFromUri(imageUri, proxy);
                }

            }
            catch (Exception e)
            {

            }

            return retImg;
        }

        public uint GetYearFromLastfm(XmlDocument xmlDoc)
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

        #endregion
    } 
     * */
    #endregion

}