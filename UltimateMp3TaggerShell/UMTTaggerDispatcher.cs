using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UltimateMusicTagger;
using UltimateMusicTagger.Business;
using UltimateMusicTagger.Model;

namespace UltimateMp3TaggerShell
{
    class UMTTaggerDispatcher
    {
        const string TagModeAll = "ALL";
        const char TagModeArtist = 'a';
        const char TagModeAlbumArtist = 'R';
        const char TagModeTrackArtist = 'T';
        const char TagModeAlbum = 'r';
        const char TagModeTitle = 't';
        const char TagModeGenres = 'g';
        const char TagModeYear = 'y';
        const char TagModePosition = 'p';
        const char TagModeImage = 'i';
        const char TagModeMusicBrainzId = 'm';

        const string DefaultExtension = "mp3";
        const char DefaultTrackMatch = TrackMatchByPosition;

        const char TrackMatchByPosition = 'p';
        const char TrackMatchByName = 't';

        const string ModeManual = "manual";
        const string ModeLastfm = "lastfm";
        const string ModeMBrainz = "mbrainz";

        const string ModeResourceMbid = "mbid";
        const string ModeResourceTitle = "title";

        const int MBID_LENGTH = 36;

        readonly static string[] validModes = new string[] { ModeLastfm, ModeManual, ModeMBrainz };
        readonly static string[] validMusicBrainzModes = new string[] { ModeResourceMbid, ModeResourceTitle };

        bool isTaggerProcessEnd = false;

        UltiMp3Tagger umTagger;
        LastfmUtility lastfmUtility;
        MusicBrainzUtility musicBrainzUtility;

        WebProxy proxy;

        public UMTTaggerDispatcher(WebProxy proxy)
        {
            umTagger = new UltiMp3Tagger(proxy);
            lastfmUtility = new LastfmUtility(null, proxy);
            musicBrainzUtility = new MusicBrainzUtility(proxy);
            this.proxy = proxy;
        }

        private uint GetYearFromDate(string date)
        {
            string pattern = "\\d{4}";

            Match match = Regex.Match(date ?? String.Empty, pattern);

            uint ret = 0;

            uint.TryParse(match.Value, out ret);

            return ret;
        }

        private TAG_FIELDS ParseTagMode(string tagmode)
        {
            TAG_FIELDS tagFields = TAG_FIELDS.NONE;

            if (tagmode == null)
                tagmode = String.Empty;

            if (tagmode.Equals(TagModeAll))
            {
                StringBuilder sb = new StringBuilder();
                
                sb.Append(TagModeAlbum);
                sb.Append(TagModeArtist);
                sb.Append(TagModeGenres);
                sb.Append(TagModeImage);
                sb.Append(TagModeMusicBrainzId);
                sb.Append(TagModeTitle);
                sb.Append(TagModePosition);
                sb.Append(TagModeYear);

                tagmode = sb.ToString();
            }

            char[] tagModeArray = tagmode.ToCharArray();

            foreach (char c in tagmode)
            {
                switch (c)
                {
                    case TagModeArtist:
                        tagFields |= TAG_FIELDS.ALBUM_ARTIST | TAG_FIELDS.TRACK_ARTIST;                        
                        break;
                    case TagModeAlbumArtist:
                        tagFields |= TAG_FIELDS.ALBUM_ARTIST;
                        break;
                    case TagModeTrackArtist:
                        tagFields |= TAG_FIELDS.TRACK_ARTIST;
                        break;
                    case TagModeAlbum:
                        tagFields |= TAG_FIELDS.ALBUM;
                        break;
                    case TagModeGenres:
                        tagFields |= TAG_FIELDS.GENRES;
                        break;
                    case TagModeImage:
                        tagFields |= TAG_FIELDS.IMAGE;
                        break;
                    case TagModeMusicBrainzId:
                        tagFields |= TAG_FIELDS.MUSICBRAINZ_ID;
                        break;
                    case TagModePosition:
                        tagFields |= TAG_FIELDS.TRACK_POS;
                        break;
                    case TagModeTitle:
                        tagFields |= TAG_FIELDS.TITLE;
                        break;
                    case TagModeYear:
                        tagFields |= TAG_FIELDS.YEAR;
                        break;
                    default:
                        throw new ApplicationException(String.Format("{0}: invalid fields for whole parameter {1}", c, tagmode));
                }
            }

            return tagFields;
        }

        private FILENAME_MATCH ParseFilenameMatch(char c)
        {
            FILENAME_MATCH trackMatch = FILENAME_MATCH.TRACK_POSITION;

            switch (c)
            {
                case TrackMatchByName:
                    trackMatch = FILENAME_MATCH.LEVENSHTEIN_DISTANCE;
                    break;
                case TrackMatchByPosition:
                    trackMatch = FILENAME_MATCH.TRACK_POSITION;
                    break;
                case ' ':
                    throw new ApplicationException(String.Format(MessageDispatcher.MESSAGE_FIELD_REQUESTED, "track/filename match mode (-match)"));
                default:
                    throw new ApplicationException("Invalid track/filename match mode");
            }

            return trackMatch;
        }

        private bool ValidateTrackMatch(string match)
        {
            if (String.IsNullOrEmpty(match) || match.Length != 1)
                throw new ApplicationException(String.Format("invalid track match {0}: ", match));

            return true;
        }

        private void TagPathManually(string[] files, string tagmode,
            string album,
            string albumArtists,
            string trackArtists,
            string genres,
            string year,
            string imagepath)
        {

            uint nyear = 0;
            Image picture = null;

            TAG_FIELDS tagfields = TAG_FIELDS.NONE;

            // tagmode            
            bool isValidTagMode = false;

            do
            {
                try
                {
                    if (String.IsNullOrEmpty(tagmode))
                    {
                        Console.WriteLine("Enter fields to tag <a r R t T g y i m | ALL (r, R, T, g, y)>");
                        Console.WriteLine("Please note: the following tag will be erased if specified: t (title), i (image), p (position), m (mbid)");
                        tagmode = Console.ReadLine();
                    }

                    bool isTagModeAll = tagmode.Equals(TagModeAll);

                    tagfields = ParseTagMode(tagmode);
                    
                    if (isTagModeAll)
                    {
                        tagfields ^= TAG_FIELDS.TITLE;
                        tagfields ^= TAG_FIELDS.MUSICBRAINZ_ID;
                        tagfields ^= TAG_FIELDS.TRACK_POS;
                    }

                    isValidTagMode = String.IsNullOrEmpty(tagmode) == false;

                }
                catch (ApplicationException)
                {
                    tagmode = String.Empty;
                }

            }
            while (isValidTagMode == false);
            
            // album
            if (((tagfields & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(album))
            {
                Console.WriteLine("Enter album (leave blank to delete it)");
                album = Console.ReadLine();
            }
            // album artist
            if (((tagfields & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(albumArtists))
            {
                Console.WriteLine("Enter album artist  (leave blank to delete it)");
                albumArtists = Console.ReadLine();
            }
            // track artist
            if (((tagfields & TAG_FIELDS.TRACK_ARTIST) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(trackArtists))
            {
                Console.WriteLine("Enter track artist  (leave blank to delete it)");
                trackArtists = Console.ReadLine();
            }
            // year
            if (((tagfields & TAG_FIELDS.YEAR) != TAG_FIELDS.NONE))
            {
                bool isValid = true;
                do
                {
                    if (isValid == false)
                    {
                        Console.WriteLine("Enter year (leave blank to delete it)");
                        year = Console.ReadLine();
                        if (String.IsNullOrEmpty(year))
                            year = "0";
                    }                    
                    isValid = UInt32.TryParse(year, out nyear);
                }
                while (isValid == false);                
            }
            // genres
            if (((tagfields & TAG_FIELDS.GENRES) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(genres))
            {
                Console.WriteLine("Enter genres, comma separated (leave blank to delete it)");
                genres = Console.ReadLine();
            }

            if (String.IsNullOrEmpty(imagepath))
                imagepath = "---";

            // image path
            if (((tagfields & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE))
            {
                while (!File.Exists(imagepath))                 {                    
                    Console.WriteLine("Enter image path for applying as front cover (leave blank to delete it)");
                    imagepath = Console.ReadLine();                    
                }
                picture = Image.FromFile(imagepath);
            }

            ModelTag input = new ModelTag
                {
                    Album = album,
                    AlbumArtists = albumArtists != null ? albumArtists.Split(',').Select(z => z.Trim()).ToArray() : null,
                    TrackArtists = trackArtists != null ? trackArtists.Split(',').Select(z => z.Trim()).ToArray() : null,
                    Genres = genres != null ? genres.Split(',').Select(z => z.Trim()).ToArray() : null,
                    Picture = imagepath != null ? Image.FromFile(imagepath) : null,                    
                    Year = nyear
                };

            MessageDispatcher.PrintTagMode(tagfields);

            Task.Factory.StartNew(PrintMessages);

            foreach (string file in files)
            {
                umTagger.TagFile(file, input, tagfields);
            }

            isTaggerProcessEnd = true;
        }
        
        private void PrintMessages()
        {
            // this bool allows to print last message in queue after main process is end
            bool hasProcessEnd = false;

            while (hasProcessEnd == false)
            {
                hasProcessEnd = isTaggerProcessEnd;
                UMTMessage[] messages = umTagger.UnqueueMessages();
                MessageDispatcher.PrintMessages(messages);
                Thread.Sleep(300);
            }

            Console.Write(Environment.NewLine);
            Console.WriteLine("END");
        }


        /// <summary>
        /// TagFileManually
        /// </summary>
        /// <param name="file"></param>
        /// <param name="tagmode"></param>
        /// <param name="album"></param>
        /// <param name="albumArtists"></param>
        /// <param name="trackArtists"></param>
        /// <param name="title"></param>
        /// <param name="genres"></param>
        /// <param name="year"></param>
        /// <param name="position"></param>
        /// <param name="imagepath"></param>
        private void TagFileManually(string file, string tagmode,             
            string album,
            string albumArtists,
            string trackArtists,
            string title,
            string genres,
            string year,
            string position,
            string imagepath)
        {
            Image picture = null;
            uint nposition = 0;
            uint nyear = 0;

            // tag fields
            TAG_FIELDS tagfields = ReadTagModeIn(tagmode);

            // title
            if (((tagfields & TAG_FIELDS.TITLE) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(title))
            {
                Console.WriteLine("Enter title (leave blank to delete it)");
                title = Console.ReadLine();
            }
            // album
            if (((tagfields & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(album))
            {
                Console.WriteLine("Enter album (leave blank to delete it)");
                album = Console.ReadLine();
            }
            // album artist
            if (((tagfields & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(albumArtists))
            {
                Console.WriteLine("Enter album artists  (leave blank to delete it)");
                albumArtists = Console.ReadLine();
            }
            // track artist
            if (((tagfields & TAG_FIELDS.TRACK_ARTIST) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(trackArtists))
            {
                Console.WriteLine("Enter track artists (leave blank to delete it)");
                trackArtists = Console.ReadLine();
            }
            // year
            if (((tagfields & TAG_FIELDS.YEAR) != TAG_FIELDS.NONE))
            {
                bool isValid = true;
                do
                {
                    if (isValid == false)
                    {
                        Console.WriteLine("Enter year (leave blank to delete it)");
                        year = Console.ReadLine();
                        if (String.IsNullOrEmpty(year))
                            year = "0";
                    }                    
                    isValid = UInt32.TryParse(year, out nyear);
                }
                while (isValid == false);                
            }
            // position
            if (((tagfields & TAG_FIELDS.TRACK_POS) != TAG_FIELDS.NONE))
            {
                bool isValid = true;
                do
                {
                    if (!isValid)
                    {
                        Console.WriteLine("Enter track position (leave blank to delete it)");
                        position = Console.ReadLine();
                        if (String.IsNullOrEmpty(position))
                            position = "0";
                    }
                    isValid = UInt32.TryParse(position, out nposition);
                }
                while (isValid == false);                               
            }
            // genres
            if (((tagfields & TAG_FIELDS.GENRES) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(genres))
            {
                Console.WriteLine("Enter genres, comma separated (leave blank to delete it)");
                //string inputGenres = Console.ReadLine();
                //string[] genres = inputGenres.Split(',').Select(z => z.Trim()).ToArray();
                //input.Genres = genres;
                genres = Console.ReadLine();
            }
            // image path
            if (((tagfields & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(imagepath))
            {                
                bool isValid = true;
                do
                {
                    if (isValid == false)
                    {
                        Console.WriteLine("Enter image path for applying as front cover (leave blank to delete it)");
                        imagepath = Console.ReadLine();
                    }                    
                    isValid = (String.IsNullOrEmpty(imagepath) || File.Exists(imagepath));
                    if (String.IsNullOrEmpty(imagepath))
                        isValid = true;
                    else
                    {
                        picture = Image.FromFile(imagepath);
                        isValid = false;
                    }

                }
                while (isValid == false);
            }

            ModelTag input = new ModelTag 
                {
                     Album = album,
                     AlbumArtists = albumArtists != null ? albumArtists.Split(',').Select(z => z.Trim()).ToArray() : null,
                     TrackArtists = trackArtists != null ? trackArtists.Split(',').Select(z => z.Trim()).ToArray() : null,
                     Genres = genres != null ? genres.Split(',').Select(z => z.Trim()).ToArray() : null,
                     Picture = imagepath != null ? Image.FromFile(imagepath) : null,
                     Position = nposition,
                     Title = title,
                     Year = nyear
                };

            MessageDispatcher.PrintTagMode(tagfields);

            umTagger.TagFile(file, input, tagfields);

            UMTMessage[] messages = umTagger.UnqueueMessages();

            MessageDispatcher.PrintMessages(messages);

        }


        private void TagPathMusicBrainzIdWithTitle(string[] files, string album, string artist, string tagmode, char seekmode, bool force)
        {
            // album
            while (String.IsNullOrEmpty(album))
            {
                Console.WriteLine("Enter album to find");
                album = Console.ReadLine();
            }

            // artist
            while (String.IsNullOrEmpty(artist))
            {
                Console.WriteLine("Enter artist to find");
                artist = Console.ReadLine();
            }

            IList<ReleaseInfo> releases = null;

            try
            {
                // release lists
                releases = musicBrainzUtility.GetReleaseList(album, artist);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // release
            ReleaseInfo releaseInfo = (!force) ?
                ReadInputReleaseInfo(null, releases.ToArray()) :
                releases.FirstOrDefault();


            // tag fields
            TAG_FIELDS tagfields = ReadTagModeIn(tagmode);

            // filename match mode
            FILENAME_MATCH filenameMatchMode = ReadInputFilenameMatchMode(seekmode);
            
            if (releaseInfo != null)
            {
                // obtain tracks
                releaseInfo = musicBrainzUtility.GetRelease(releaseInfo.Mbid);

                MessageDispatcher.PrintTagMode(tagfields);

                MessageDispatcher.PrintTrackMatch(filenameMatchMode);

                MessageDispatcher.PrintParameters(new ModelTag { AlbumArtists = artist.Split(','), Album = album });

                Task.Factory.StartNew(PrintMessages);

                List<TrackInfo> list = releaseInfo.TrackInfos.ToList();

                Func<string, List<TrackInfo>, TrackInfo> comparer = null;

                switch (filenameMatchMode)
                {
                    case FILENAME_MATCH.LEVENSHTEIN_DISTANCE:
                        comparer = MTUtility.GetTrackInfoByLevenshteinDistance;
                        break;
                    case FILENAME_MATCH.TRACK_POSITION:
                        comparer = MTUtility.GetTrackInfoByPosition;
                        break;
                }

                int success = 0;
                int failure = 0;
                int skipped = 0;

                foreach (string filename in files)
                {
                    string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

                    TrackInfo trackInfo = comparer(filenameWithoutExtension, list);

                    if (trackInfo != null)
                    {
                        ModelTag input = new ModelTag
                        {
                            Title = trackInfo.Title,
                            Album = releaseInfo.Title,
                            AlbumArtists = releaseInfo.Artists != null ? releaseInfo.Artists.Select(a => a.Name).ToArray() : null,
                            Genres = null,
                            ArtistMbid = trackInfo.Artists != null ? trackInfo.Artists[0].Mbid : null,
                            Picture = MTUtility.ImageFromUri(releaseInfo.ImagePath, proxy),
                            ReleaseMbid = releaseInfo.Mbid,
                            TrackMbid = trackInfo.Mbid,
                            Position = trackInfo.Track,
                            Year = GetYearFromDate(releaseInfo.Year),
                            TrackArtists = trackInfo.Artists != null ? trackInfo.Artists.Select(a => a.Name).ToArray() : null,
                        };

                        try
                        {
                            umTagger.TagFile(filename, input, tagfields);

                            Console.Write(Environment.NewLine);

                            success++;
                        }
                        catch (Exception)
                        {
                            failure++;
                        }
                    }
                    else
                        skipped++;
                }

                isTaggerProcessEnd = true;
            }
            else
            {
                Console.WriteLine(String.Format("No release found for release: {0} - artist: {1}", album, artist));
            }
        }


        private void TagPathMusicBrainzIdWithMbid(string[] files, string releaseMBID, string tagmode, char seekmode)
        {
            // release mbid
            while (String.IsNullOrEmpty(releaseMBID) || releaseMBID.Length != MBID_LENGTH)
            {
                Console.WriteLine("Enter a valid release MBID");
                releaseMBID = Console.ReadLine();
            }

            // tag fields
            TAG_FIELDS tagfields = ReadTagModeIn(tagmode);

            // filename match mode
            FILENAME_MATCH filenameMatchMode = ReadInputFilenameMatchMode(seekmode);
            
            ReleaseInfo releaseInfo = null;

            try
            {
                releaseInfo = musicBrainzUtility.GetRelease(releaseMBID);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (releaseInfo != null)
            {
                MessageDispatcher.PrintTagMode(tagfields);

                MessageDispatcher.PrintTrackMatch(filenameMatchMode);

                Task.Factory.StartNew(PrintMessages);

                List<TrackInfo> list = releaseInfo.TrackInfos.ToList();

                Func<string, List<TrackInfo>, TrackInfo> comparer = null;

                switch (filenameMatchMode)
                {
                    case FILENAME_MATCH.LEVENSHTEIN_DISTANCE:
                        comparer = MTUtility.GetTrackInfoByLevenshteinDistance;
                        break;
                    case FILENAME_MATCH.TRACK_POSITION:
                        comparer = MTUtility.GetTrackInfoByPosition;
                        break;
                }

                int success = 0;
                int failure = 0;
                int skipped = 0;

                foreach (string filename in files)
                {
                    string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

                    TrackInfo trackInfo = comparer(filenameWithoutExtension, list);

                    if (trackInfo != null)
                    {
                        ModelTag input = new ModelTag
                        {
                            Title = trackInfo.Title,
                            Album = releaseInfo.Title,
                            AlbumArtists = releaseInfo.Artists != null ? releaseInfo.Artists.Select(a => a.Name).ToArray() : null,
                            Genres = null,
                            ArtistMbid = trackInfo.Artists != null ? trackInfo.Artists[0].Mbid : null,
                            Picture = MTUtility.ImageFromUri(releaseInfo.ImagePath, proxy),
                            ReleaseMbid = releaseInfo.Mbid,
                            TrackMbid = trackInfo.Mbid,
                            Position = trackInfo.Track,
                            Year = GetYearFromDate(releaseInfo.Year),
                            TrackArtists = trackInfo.Artists != null ? trackInfo.Artists.Select(a => a.Name).ToArray() : null,
                        };

                        try
                        {
                            umTagger.TagFile(filename, input, tagfields);

                            Console.Write(Environment.NewLine);

                            success++;
                        }
                        catch (Exception)
                        {
                            failure++;
                        }
                    }
                    else
                        skipped++;
                }

                isTaggerProcessEnd = true;
            }
            else
            {
                Console.WriteLine(String.Format("No release found for mbid: {0}", releaseMBID));
            }
        }


        FILENAME_MATCH ReadInputFilenameMatchMode(char seekmode)
        {
            FILENAME_MATCH match = FILENAME_MATCH.LEVENSHTEIN_DISTANCE;

            // track/filename match algo
            bool isValidTrackMatch = false;

            do
            {
                string trackmatch = String.Empty;
                if (seekmode == ' ')
                {
                    Console.WriteLine("Enter a valid filename tagging criteria (position or title)");
                    Console.WriteLine("Hint: if filename matches track position (ex. Track 05.mp3) choose 'p', " +
                        "otherwise choose 't'");
                    Console.WriteLine("<p|t>:");
                    trackmatch = Console.ReadLine();
                }

                try
                {
                    if (trackmatch.Length > 0)
                        seekmode = trackmatch.ToCharArray()[0];

                    match = ParseFilenameMatch(seekmode);
                    isValidTrackMatch = true;

                }
                catch (ApplicationException)
                {
                    seekmode = ' ';
                }
            } while (!isValidTrackMatch);

            return match;
        }



        private void TagPathLastfm(string[] files, string album, string artist, string tagmode, char seekmode)
        {            
            // album
            while (String.IsNullOrEmpty(album))
            {
                Console.WriteLine("Enter album to find");
                album = Console.ReadLine();
            }

            // artist
            while (String.IsNullOrEmpty(artist))
            {
                Console.WriteLine("Enter artist to find");
                artist = Console.ReadLine();
            }

            // filename match mode
            FILENAME_MATCH filenameMatchMode = ReadInputFilenameMatchMode(seekmode);

            // tag fields
            TAG_FIELDS tagfields = ReadTagModeIn(tagmode);

            MessageDispatcher.PrintParameters(new ModelTag { AlbumArtists = artist.Split(','), Album = album });
            
            // start tagging
            MessageDispatcher.PrintTagMode(tagfields);

            MessageDispatcher.PrintTrackMatch(filenameMatchMode);

            Task.Factory.StartNew(PrintMessages);

            ReleaseInfo releaseInfo = null;

            try
            {
                releaseInfo = lastfmUtility.GetRelease(album, artist);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (releaseInfo != null && releaseInfo.TrackInfos.Count() > 0)
            {
                List<TrackInfo> list = releaseInfo.TrackInfos.ToList();

                Func<string, List<TrackInfo>, TrackInfo> comparer = null;

                switch (filenameMatchMode)
                {
                    case FILENAME_MATCH.LEVENSHTEIN_DISTANCE:
                        comparer = MTUtility.GetTrackInfoByLevenshteinDistance;
                        break;
                    case FILENAME_MATCH.TRACK_POSITION:
                        comparer = MTUtility.GetTrackInfoByPosition;
                        break;
                }

                int success = 0;
                int failure = 0;
                int skipped = 0;

                foreach (string filename in files)
                {
                    string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

                    TrackInfo trackInfo = comparer(filenameWithoutExtension, list);

                    if (trackInfo != null)
                    {
                        ModelTag input = new ModelTag
                        {
                            Title = trackInfo.Title,
                            Album = releaseInfo.Title,
                            AlbumArtists = releaseInfo.Artists != null ? releaseInfo.Artists.Select(a => a.Name).ToArray() : null,
                            Genres = null,
                            ArtistMbid = trackInfo.Artists != null ? trackInfo.Artists[0].Mbid : null,
                            Picture = MTUtility.ImageFromUri(releaseInfo.ImagePath, proxy),
                            ReleaseMbid = releaseInfo.Mbid,
                            TrackMbid = trackInfo.Mbid,
                            Position = trackInfo.Track,
                            Year = GetYearFromDate(releaseInfo.Year),
                            TrackArtists = trackInfo.Artists != null ? trackInfo.Artists.Select(a => a.Name).ToArray() : null,
                        };

                        try
                        {
                            umTagger.TagFile(filename, input, tagfields);

                            Console.Write(Environment.NewLine);

                            success++;
                        }
                        catch (Exception)
                        {
                            failure++;
                        }
                    }
                    else
                        skipped++;
                }

                if (success == 0)
                {

                }
                else if (success > 0 && failure > 0)
                {

                }
                else
                {

                }
            }            

            isTaggerProcessEnd = true;

        }

        void PrintRecordingReleases(ReleaseInfo[] releases)
        {
            Console.WriteLine("Releases associated to current recording");
            Console.Write(Environment.NewLine);
            int idx = 0;
            foreach (ReleaseInfo release in releases)
            {
                Console.WriteLine(String.Format("{0}: {1} - {2} [{3}]", 
                    ++idx, 
                    release.Mbid, 
                    release.Title, 
                    release.Year
                    ));
            }
            Console.Write(Environment.NewLine);
        }

        void PrintRecording(IEnumerable<TrackInfo> tracks)
        {
            Console.WriteLine("Recording list");
            Console.Write(Environment.NewLine);
            int idx = 0;
            foreach (TrackInfo track in tracks)
            {
                Console.WriteLine(String.Format("{0}: {1} - {2} - {3}",
                    ++idx,
                    track.Mbid,
                    track.Title,
                    track.Artists.Select(i => i.Name).FirstOrDefault()                   
                    ));
            }
            Console.Write(Environment.NewLine);
        }

        private void TagFileMusicBrainzIdWithTitle(string file, string title, string artist, string tagmode, bool force)
        {
            // recording 
            while (String.IsNullOrEmpty(title))
            {
                Console.WriteLine("Enter title to find");
                title = Console.ReadLine();
            }
            // artist
            while (String.IsNullOrEmpty(artist))
            {
                Console.WriteLine("Enter artist to find");
                artist = Console.ReadLine();
            }

            // tag fields
            TAG_FIELDS tagfields = ReadTagModeIn(tagmode);

            List<TrackInfo> tracks = musicBrainzUtility.GetRecordingList(title, artist);
            
            if (tracks.Count() > 0)
            {
                // recording
                TrackInfo trackInfo = (!force) ?
                    ReadInputTrackInfo(tracks) :
                    tracks[0];

                // release
                ReleaseInfo releaseInfo = (!force) ?
                    ReadInputReleaseInfo(null, trackInfo.Releases) :
                    trackInfo.Releases.FirstOrDefault();

                // picture
                Image picture = MTUtility.ImageFromUri(releaseInfo.ImagePath, proxy);

                ModelTag input = new ModelTag
                {
                    Title = trackInfo.Title,
                    Album = releaseInfo.Title,
                    AlbumArtists = releaseInfo.Artists != null ? releaseInfo.Artists.Select(a => a.Name).ToArray() : null,
                    Genres = null,
                    ArtistMbid = trackInfo.Artists != null ? trackInfo.Artists[0].Mbid : null,
                    Picture = picture,
                    ReleaseMbid = releaseInfo.Mbid,
                    TrackMbid = trackInfo.Mbid,
                    Position = trackInfo.Track,
                    Year = GetYearFromDate(releaseInfo.Year),
                    TrackArtists = trackInfo.Artists != null ? trackInfo.Artists.Select(a => a.Name).ToArray() : null,
                };

                umTagger.TagFile(file, input, tagfields);
            }
            else
            {
                Console.WriteLine(String.Format("No recording found for title: {0} / artist: {1}", title, artist));
            }
            
            UMTMessage[] messages = umTagger.UnqueueMessages();

            MessageDispatcher.PrintMessages(messages);
        }

        TAG_FIELDS ReadTagModeIn(string tagmode)
        {
            TAG_FIELDS tagfields = TAG_FIELDS.NONE;

            // tagmode            
            bool isValidTagMode = false;

            do
            {
                try
                {
                    if (String.IsNullOrEmpty(tagmode))
                    {
                        Console.WriteLine("Enter fields to tag <a r R t T g y i m | ALL>");
                        tagmode = Console.ReadLine();
                    }

                    tagfields = ParseTagMode(tagmode);

                    isValidTagMode = String.IsNullOrEmpty(tagmode) == false;

                }
                catch (ApplicationException)
                {
                    tagmode = String.Empty;
                }

            }
            while (!isValidTagMode);

            return tagfields;
        }



        TrackInfo ReadInputTrackInfo(IList<TrackInfo> tracks)
        {
            TrackInfo trackInfo;

            if (tracks.Count() > 1)
            {
                bool isValidChoice;

                do
                {                    
                    PrintRecording(tracks);

                    Console.WriteLine(String.Format("Enter a valid choice: [1 - {0}]:", tracks.Count()));
                    string response = Console.ReadLine();
                    int idx;
                    bool isValidNumber = int.TryParse(response, out idx);
                    isValidChoice = isValidNumber && idx > 0 && idx <= tracks.Count;
                    trackInfo = isValidChoice ?
                        tracks[idx - 1] :
                        null;                    
                }
                while (!isValidChoice);
            }
            else if (tracks.Count() == 1)
            {
                trackInfo = tracks[0];
            }
            else
            {
                trackInfo = null;
            }

            return trackInfo;
        }



        ReleaseInfo ReadInputReleaseInfo(string releaseMBID, ReleaseInfo[] releases)
        {
            if (releases.Count() > 1)
            {
                if (!String.IsNullOrEmpty(releaseMBID))
                {
                    //releaseMBID = trackInfo.Releases
                    //    .Where(i => i.Mbid.Equals(releaseMBID))
                    //    .Select(i => i.Mbid)
                    //    .FirstOrDefault();
                    bool exists = 
                        releases
                        .Any(i => i.Mbid.Equals(releaseMBID))
                        ;

                    if (!exists)
                        releaseMBID = null;
                }

                while (String.IsNullOrEmpty(releaseMBID))
                {
                    PrintRecordingReleases(releases);

                    Console.WriteLine(String.Format("Enter a valid choice [1 - {0}]:", releases.Count()));
                    string response = Console.ReadLine();
                    int idx;
                    bool isValidNumber = int.TryParse(response, out idx);
                    if (isValidNumber && idx > 0 && idx <= releases.Length)
                    {
                        releaseMBID = releases[idx - 1].Mbid;
                    }
                }
            }
            else if (releases.Count() == 1)
            {
                releaseMBID = releases[0].Mbid;
            }
            else
            {
                releaseMBID = null;
            }

            ReleaseInfo releaseInfo = 
                releases.
                FirstOrDefault(i => i.Mbid.Equals(releaseMBID))
                ?? (
                    (releases.Length > 0) ?
                    releases[0] :
                    new ReleaseInfo()
                );

            return releaseInfo;
        }



        private void TagFileMusicBrainzIdWithMbid(string file, string recordingMBID, string releaseMBID, string tagmode, bool force)
        {
            // recording mbid
            while (String.IsNullOrEmpty(recordingMBID) || recordingMBID.Length != MBID_LENGTH)
            {
                Console.WriteLine("Enter a valid recording MBID to find");
                recordingMBID = Console.ReadLine();
            }

            // tag fields
            TAG_FIELDS tagfields = ReadTagModeIn(tagmode);

            MessageDispatcher.PrintTagMode(tagfields);

            TrackInfo trackInfo = null;

            try
            {
                trackInfo = musicBrainzUtility.GetRecording(recordingMBID);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (trackInfo != null)
            {
                // release
                ReleaseInfo releaseInfo = (!force) ?
                    ReadInputReleaseInfo(releaseMBID, trackInfo.Releases) :
                    trackInfo.Releases.FirstOrDefault()
                    ;

                // picture
                Image picture = MTUtility.ImageFromUri(releaseInfo.ImagePath, proxy);

                ModelTag input = new ModelTag
                {
                    Title = trackInfo.Title,
                    Album = releaseInfo.Title,
                    AlbumArtists = releaseInfo.Artists != null ? releaseInfo.Artists.Select(a => a.Name).ToArray() : null,
                    Genres = null,
                    ArtistMbid = trackInfo.Artists != null ? trackInfo.Artists[0].Mbid : null,
                    Picture = picture,
                    ReleaseMbid = releaseInfo.Mbid,
                    TrackMbid = trackInfo.Mbid,
                    Position = trackInfo.Track,
                    Year = GetYearFromDate(releaseInfo.Year),
                    TrackArtists = trackInfo.Artists != null ? trackInfo.Artists.Select(a => a.Name).ToArray() : null,
                };

                umTagger.TagFile(file, input, tagfields);
            }
            else
            {
                Console.WriteLine(String.Format("No recording found for mbid: {0}", recordingMBID));
            }

            UMTMessage[] messages = umTagger.UnqueueMessages();

            MessageDispatcher.PrintMessages(messages);

        }

        /// <summary>
        /// TagFileLastfm
        /// </summary>
        /// <param name="file"></param>
        /// <param name="title"></param>
        /// <param name="artist"></param>
        /// <param name="tagmode"></param>
        private void TagFileLastfm(string file, string title, string artist, string tagmode)
        {
            
            // title
            while (String.IsNullOrEmpty(title))
            {
                Console.WriteLine("Enter title to find");
                title = Console.ReadLine();
            }

            // artist
            while (String.IsNullOrEmpty(artist))
            {
                Console.WriteLine("Enter artist to find");
                artist = Console.ReadLine();
            }

            // tag fields
            TAG_FIELDS tagfields = ReadTagModeIn(tagmode);

            // start tagging..
            MessageDispatcher.PrintParameters(new ModelTag { AlbumArtists = artist.Split(','), Title = title });
            
            MessageDispatcher.PrintTagMode(tagfields);

            TrackInfo trackInfo = null;

            try
            {
                trackInfo = lastfmUtility.GetTrack(title, artist);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (trackInfo != null)
            {
                ReleaseInfo releaseInfo = trackInfo.Releases.Length > 0 ? trackInfo.Releases[0] : new ReleaseInfo();

                Image picture = MTUtility.ImageFromUri(releaseInfo.ImagePath, proxy);

                ModelTag input = new ModelTag
                {
                    Title = trackInfo.Title,
                    Album = releaseInfo.Title,
                    AlbumArtists = releaseInfo.Artists != null ? releaseInfo.Artists.Select(a => a.Name).ToArray() : null,
                    Genres = null,
                    ArtistMbid = trackInfo.Artists != null ? trackInfo.Artists[0].Mbid : null,
                    Picture = picture,
                    ReleaseMbid = releaseInfo.Mbid,
                    TrackMbid = trackInfo.Mbid,
                    Position = trackInfo.Track,
                    Year = GetYearFromDate(releaseInfo.Year),
                    TrackArtists = trackInfo.Artists != null ? trackInfo.Artists.Select(a => a.Name).ToArray() : null,
                };

                umTagger.TagFile(file, input, tagfields);            
            }

            UMTMessage[] messages = umTagger.UnqueueMessages();

            MessageDispatcher.PrintMessages(messages);            
            
        }

        public void ReadTagFromSingleFile(string[] args, string file)
        {
            ReadTag(file);
        }

        public void ReadTagFromMultipleFile(string[] args, string input)
        {
            string path = Path.GetDirectoryName(input);
            string fileextension = Path.GetFileName(input);

            string[] files = UMTShellUtility.GetFilesFromPath(path, fileextension, SearchOption.TopDirectoryOnly);

            Console.WriteLine(String.Format("{0} files found in path", files.Count()));

            foreach (string file in files)
            {
                ReadTag(file);
            }
        }


        private void ReadTag(string file)
        {
            TrackInfo output = umTagger.GetTag(file);
            
            Console.Write("Reading file ");
            Console.WriteLine(file);
            Console.Write(Environment.NewLine);
            Console.Write("Title: ");
            Console.WriteLine(output.Title);
            Console.Write("Track: ");
            Console.WriteLine(output.Track.ToString("00"));
            Console.Write("Track Artist: ");
            Console.WriteLine(String.Join(", ", output.Artists.Select((o) => o.Name)));
            Console.Write("Song Musicbrainz id: ");
            Console.WriteLine(output.Mbid);
            if (output.Releases != null && output.Releases.Length > 0)
            {
                ReleaseInfo release = output.Releases[0];
                Console.Write("Release Artist: ");
                Console.WriteLine(String.Join(", ", release.Artists.Select((o) => o.Name)));
                Console.Write("Year: ");
                Console.WriteLine(release.Year);
                Console.Write("Release Musicbrainz id: ");
                Console.WriteLine(release.Mbid);
            }
            Console.Write("Genres: ");
            Console.WriteLine(String.Join(", ", output.Genres));
            Console.Write(Environment.NewLine);
        }

        public void TagFile(string[] args, string file)
        {
            bool isHelpRequested = false;
            bool force = false;
            string tagmode = null;
            string albumArtists = null;
            string trackArtists = null;
            string title = null;
            string album = null;
            string recordingMBID = null;
            string releaseMBID = null;
            string mode = null;
            string mbrainzMode = null;
            string year = null;
            string trackpos = null;
            string imagepath = null;
            string genres = null;

            var p = new OptionSet() {           
                { "h|help", "help for this mode",
                    v => isHelpRequested = true },                                             
                { "mode=", "<manual|lastfm|mbrainz>",
                    v => mode = v },
                { "force", "Force the first selection avoiding prompt",
                    v => force = true },
                { "mbrainzmode=", "<mbid|title>",
                    v => mbrainzMode = v},
                { "fields=", "fields to tag (a t T r R y p i g m)",
                    v => tagmode = v },                                       
                { "rartist=", "album artist name",
                    v => albumArtists = v },
                { "tartist=", "track artist name",
                    v => trackArtists = v },
                { "artist=", "album/track artist name",
                    v => trackArtists = albumArtists = v },
                { "album=", "album name",
                    v => album = v },   
                { "title=", "track title",
                    v => title = v },
                { "recordingMBID=", "recording Music Brainz ID",
                    v => recordingMBID = v},
                { "releaseMBID=", "release Music Brainz ID",
                    v => releaseMBID = v},
                { "year=", "release year",
                    v => year = v },
                { "position=", "track position",
                    v => trackpos = v },
                { "image=", "image path",
                    v => imagepath = v },
                { "genres=", "genres, embraced and comma separated (ex. \"rock, hard rock\")",
                    v => genres = v},                 
            };

            p.Parse(args);
            
            while (!validModes.Contains(mode))
            {
                Console.WriteLine("Enter a valid mode <manual|lastfm|mbrainz>");
                mode = Console.ReadLine();
            }

            if (!isHelpRequested)
            {
                if (mode.Equals(ModeMBrainz))
                {
                    while (!validMusicBrainzModes.Contains(mbrainzMode))
                    {
                        Console.WriteLine("Enter a valid lookup method <mbid|title>");
                        mbrainzMode = Console.ReadLine();
                    }
                }

                switch (mode)
                {
                    case ModeLastfm:
                        TagFileLastfm(file, title, trackArtists, tagmode);
                        break;
                    case ModeMBrainz:
                        if (mbrainzMode.Equals(ModeResourceMbid))
                            TagFileMusicBrainzIdWithMbid(file, recordingMBID, releaseMBID, tagmode, force);
                        else
                            TagFileMusicBrainzIdWithTitle(file, title, trackArtists, tagmode, force);
                        break;
                    default:
                        TagFileManually(file, tagmode,
                            album,
                            albumArtists,
                            trackArtists,
                            title,
                            genres,
                            year,
                            trackpos,
                            imagepath
                         );
                        break;
                }
            }            
            else
            {
                Console.WriteLine(Environment.NewLine);

                OptionSet opt = null;

                string message;

                switch (mode)
                {
                    case ModeLastfm:

                        message = "options for lastfm mode:";

                        opt = new OptionSet() {
                            { "fields=", "fields to tag (a t T r R y p i g m | ALL)",
                                v => tagmode = v },
                            { "artist=", "artist name",
                                v => albumArtists = v },
                            { "title=", "track title",
                                v => title = v },
                        };
                        break;
                    case ModeMBrainz:

                        message = "options for music brainz id mode:";

                        opt = new OptionSet() {
                            { "fields=", "fields to tag (a t T r R y p i g m | ALL)",
                                v => tagmode = v },
                            { "mbrainzmode=", "<mbid|title>",
                                v => mbrainzMode = v},
                            { "album=", "release name (search title mode only)",
                                v => albumArtists = v },
                            { "title=", "recording title (search title mode only)",
                                v => title = v },
                            { "recordingMBID=", "recording Music Brainz ID (search mbid mode only)",
                                v => recordingMBID = v},
                            { "releaseMBID=", "release Music Brainz ID (search mbid mode only)",
                                v => releaseMBID = v},
                            { "force", "Force the first selection avoiding prompt",
                                v => force = true },
                            };
                        break;
                    case ModeManual:

                        message = "options for manual mode:";

                        opt = new OptionSet() {
                            { "fields=", "fields to tag (a t T r R y p i g m | ALL)",
                              v => tagmode = v },
                            { "rartist=", "album artist name",
                              v => albumArtists = v },
                            { "tartist=", "track artist name",
                              v => trackArtists = v },
                            { "title=", "track title",
                              v => title = v },
                            { "album=", "album name",
                              v => album = v },
                            { "year=", "release year",
                              v => year = v },
                            { "position=", "track position",
                              v => trackpos = v },
                            { "image=", "image path",
                              v => imagepath = v },
                            { "genres=", "genres, embraced and comma separated (ex. \"rock, hard rock\")",
                              v => genres = v },
                        };
                        break;
                    default:
                        throw new NotImplementedException();                        
                }

                Console.WriteLine(message);

                opt.WriteOptionDescriptions(Console.Out);
            }
        }

        public void TagPath(string[] args, string input)
        {
            bool isHelpRequested = false;
            bool force = false;
            string tagmode = null;
            string mode = null;
            string mbrainzMode = null;
            string fileextension = null;
            string albumArtists = null;
            string trackArtists = null;
            string album = null;
            string year = null;
            string imagepath = null;
            string releaseMBID = null;
            char match = ' ';
            string genres = null;

            var p = new OptionSet() {           
                { "h|help", "help for this mode",
                    v => isHelpRequested = true },                          
                { "mode=", "manual / lastfm / musicbrainz <manual|lastfm|mbrainz>",
                    v => mode = v },
                { "mbrainzmode=", "<mbid|title>",
                    v => mbrainzMode = v},
                { "force", "Force the first selection avoiding prompt",
                    v => force = true },
                { "fields=", "fields to tag (a t T r R y p i g m | ALL)",
                    v => tagmode = v },                     
                { "rartist=", "album artist name",
                    v => albumArtists = v },
                { "tartist=", "track artist name",
                    v => trackArtists = v },
                { "artist=", "album/track artist name",
                    v => albumArtists = trackArtists = v },
                { "album=", "album name",
                    v => album = v },
                { "releaseMBID=", "release Music Brainz ID",
                    v => releaseMBID = v},
                { "year=", "release year",
                    v => year = v },
                { "image=", "image path",
                    v => imagepath = v },
                { "genres=", "genres, embraced and comma separated (ex. \"rock, hard rock\")",
                    v => genres = v }, 
                { "match=", "filename tagging criteria (position or title)",
                    v => match = v.ToCharArray()[0] }, 
            };

            p.Parse(args);
            
            while (validModes.Contains(mode) == false)
            {
                Console.WriteLine("Enter a valid mode <manual|lastfm|mbrainz>");
                mode = Console.ReadLine();                
            }
            
            string path = Path.GetDirectoryName(input);
            fileextension = Path.GetFileName(input);

            if (!isHelpRequested)
            {
                if (String.IsNullOrEmpty(fileextension))
                {
                    Console.WriteLine("Enter an audio file extension to process in path (default .mp3)");
                    fileextension = Console.ReadLine();
                    if (String.IsNullOrEmpty(fileextension))
                    {
                        fileextension = DefaultExtension;
                    }
                }

                Console.WriteLine(String.Format("extension selected: {0}", UMTShellUtility.NormalizeFilterPath(fileextension)));

                string[] files = UMTShellUtility.GetFilesFromPath(path, fileextension, SearchOption.TopDirectoryOnly);

                Console.WriteLine(String.Format("{0} files found in path", files.Count()));

                if (mode.Equals(ModeMBrainz))
                {
                    while (!validMusicBrainzModes.Contains(mbrainzMode))
                    {
                        Console.WriteLine("Enter a valid lookup method <mbid|title>");
                        mbrainzMode = Console.ReadLine();
                    }
                }

                switch (mode)
                {
                    case ModeLastfm:
                        TagPathLastfm(files, album, albumArtists, tagmode, match);
                        break;
                    case ModeMBrainz:
                        if (mbrainzMode.Equals(ModeResourceMbid))
                            TagPathMusicBrainzIdWithMbid(files, releaseMBID, tagmode, match);
                        else
                            TagPathMusicBrainzIdWithTitle(files, album, trackArtists, tagmode, match, force);
                        break;
                    case ModeManual:
                        TagPathManually(files, tagmode, album, albumArtists, trackArtists, genres, year, imagepath);
                        break;
                }
    
            }
            else
            {
                Console.WriteLine(Environment.NewLine);

                OptionSet opt = null;

                string message;

                switch (mode)
                {
                    case ModeLastfm:
                        message = "options for lastfm mode";
                        opt = new OptionSet() {
                                { "fields=", "fields to tag (a t T r R y p i g m | ALL)",
                                    v => tagmode = v },
                                { "artist=", "album/track artist name",
                                    v => trackArtists = albumArtists = v },
                                { "album=", "album name",
                                    v => album = v },
                                { "match=", "filename tagging criteria (position or title)",
                                    v => match = v.ToCharArray()[0] },
                            };
                        break;
                    case ModeMBrainz:
                        message = "options for mbrainz mode";
                        opt = new OptionSet() {
                            { "fields=", "fields to tag (a t T r R y p i g m | ALL)",
                                v => tagmode = v },
                            { "mbrainzmode=", "<mbid|title>",
                                v => mbrainzMode = v},
                            { "album=", "release name (search title mode only)",
                                v => album = v },
                            { "artist=", "album/track name (search title mode only)",
                                v => trackArtists = albumArtists = v },
                            { "releaseMBID=", "release Music Brainz ID (search mbid mode only)",
                                v => releaseMBID = v},
                            { "force", "Force the first selection avoiding prompt",
                                v => force = true },
                        };

                        break;
                    case ModeManual:
                        message = "options for manual mode";
                        opt = new OptionSet() {
                            { "fields=", "fields to tag (a t T r R y p i g m | ALL)",
                              v => tagmode = v },
                            { "rartist=", "album artist name",
                                    v => albumArtists = v },
                            { "tartist=", "track artist name",
                                v => trackArtists = v },
                            { "album=", "album name",
                              v => album = v },
                            { "year=", "release year",
                              v => year = v },
                            { "image=", "image path",
                              v => imagepath = v },
                            { "genres=", "genres, embraced and comma separated (ex. \"rock, hard rock\")",
                              v => genres = v  },
                            };
                        break;
                    default:
                        throw new NotImplementedException();
                }

                Console.WriteLine(message);

                opt.WriteOptionDescriptions(Console.Out);

            }
        }
    }
}
