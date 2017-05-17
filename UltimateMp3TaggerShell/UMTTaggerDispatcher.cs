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
        const char TrackMatchByName = 'n';

        const string ModeManual = "manual";
        const string ModeLastfm = "lastfm";

        readonly static string[] validModes = new string[] { ModeLastfm, ModeManual };

        bool isTaggerProcessEnd = false;

        UltiMp3Tagger umTagger;
        LastfmUtility lastfmUtility;

        WebProxy proxy;

        public UMTTaggerDispatcher(WebProxy proxy)
        {
            umTagger = new UltiMp3Tagger(proxy);
            lastfmUtility = new LastfmUtility(null, proxy);
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
                        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                        Console.WriteLine("Enter fields to tag <a r R t T g y i m | ALL>");
                        Console.WriteLine("Please note: t, i, p, m tag will be erased if selected");
                        Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                        tagmode = Console.ReadLine();
                    }

                    bool isTagModeAll = tagmode.Equals(TagModeAll);

                    tagfields = ParseTagMode(tagmode);
                    
                    if (isTagModeAll)
                    {
                        tagfields ^= TAG_FIELDS.TITLE;
                        tagfields ^= TAG_FIELDS.IMAGE;
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
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter album (leave blank to delete it)");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                album = Console.ReadLine();
            }
            // album artist
            if (((tagfields & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(albumArtists))
            {
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter album artist  (leave blank to delete it)");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                albumArtists = Console.ReadLine();
            }
            // track artist
            if (((tagfields & TAG_FIELDS.TRACK_ARTIST) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(trackArtists))
            {
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter track artist  (leave blank to delete it)");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
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
                        Console.ForegroundColor = MessageDispatcher.ColorQuestion;                        
                        Console.WriteLine("Enter year (leave blank to delete it)");
                        Console.ForegroundColor = MessageDispatcher.ColorAnswer;
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
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter genres, comma separated (leave blank to delete it)");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
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
                        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                        Console.WriteLine("Enter image path for applying as front cover (leave blank to delete it)");
                        Console.ForegroundColor = MessageDispatcher.ColorAnswer;
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

        //[Obsolete(null , true)]
        //private void TagPathManually(string[] files, string tagmode, InputTag input)
        //{
            
        //    TAG_FIELDS tagfield = TAG_FIELDS.NONE;

        //    // tagmode            
        //    bool isValidTagMode = false;

        //    do
        //    {
        //        try
        //        {
        //            if (String.IsNullOrEmpty(tagmode))
        //            {
        //                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //                Console.WriteLine("Enter tag mode <a r t g y i m | ALL>");
        //                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
        //                tagmode = Console.ReadLine();
        //            }

        //            tagfield = ParseTagMode(tagmode);

        //            isValidTagMode = String.IsNullOrEmpty(tagmode) == false;

        //        }
        //        catch (ApplicationException)
        //        {
        //            tagmode = String.Empty;
        //        }

        //    }
        //    while (isValidTagMode == false);

        //    // album
        //    if (((tagfield & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(input.Album))
        //    {
        //        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //        Console.WriteLine("Enter album (leave blank to delete it)");
        //        Console.ForegroundColor = MessageDispatcher.ColorAnswer;
        //        input.Album = Console.ReadLine();
        //    }

        //    // artist
        //    if (((tagfield & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE) && input.AlbumArtists != null)
        //    {
        //        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //        Console.WriteLine("Enter artist  (leave blank to delete it)");
        //        Console.ForegroundColor = MessageDispatcher.ColorAnswer;
        //        input.AlbumArtists = Console.ReadLine().Split(',');
        //    }
        //    // year
        //    if (((tagfield & TAG_FIELDS.YEAR) != TAG_FIELDS.NONE) && input.Year == 0)
        //    {
        //        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //        Console.WriteLine("Enter year (leave blank to delete it)");
        //        Console.ForegroundColor = MessageDispatcher.ColorAnswer;
        //        string inputYear = Console.ReadLine();
        //        uint year;
        //        UInt32.TryParse(inputYear, out year);
        //        input.Year = year;
        //    }      
        //    // genres
        //    if (((tagfield & TAG_FIELDS.GENRES) != TAG_FIELDS.NONE) && (input.Genres == null || input.Genres.Length == 0))
        //    {
        //        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //        Console.WriteLine("Enter genres, comma separated (leave blank to delete it)");
        //        Console.ForegroundColor = MessageDispatcher.ColorAnswer;              
        //        string inputGenres = Console.ReadLine();
        //        string[] genres = inputGenres.Split(',').Select(z => z.Trim()).ToArray();
        //        input.Genres = genres;
        //    }
        //    // image path
        //    if (((tagfield & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(input.ImagePath))
        //    {
        //        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //        Console.WriteLine("Enter image path for applying as front cover (leave blank to delete it)");
        //        Console.ForegroundColor = MessageDispatcher.ColorAnswer;     
        //        bool isValidPath = true;
        //        do
        //        {
        //            if (isValidPath == false)
        //            {
        //                Console.ForegroundColor = ConsoleColor.Red;
        //                Console.WriteLine("enter a valid image path (or blank to remove tag)");
        //            }
        //            input.ImagePath = Console.ReadLine();
        //            isValidPath = (String.IsNullOrEmpty(input.ImagePath) || File.Exists(input.ImagePath));
        //        }
        //        while (isValidPath == false);
        //    }

        //    MessageDispatcher.PrintTagMode(tagfield);

        //    Task.Factory.StartNew(PrintMessages);

        //    umTagger.TagFiles(files, input, tagfield);

        //    isTaggerProcessEnd = true;

        //}

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
            Console.ForegroundColor = MessageDispatcher.ColorInfo;
            Console.WriteLine("END");
            Console.ResetColor();
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
            TAG_FIELDS tagfields = TAG_FIELDS.NONE;

            Image picture = null;
            uint nposition = 0;
            uint nyear = 0;

            // tagmode            
            bool isValidTagMode = false;

            do
            {
                try
                {
                    if (String.IsNullOrEmpty(tagmode))
                    {
                        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                        Console.WriteLine("Enter fields to tag <a r R t T g y i m | ALL>");
                        Console.ForegroundColor = MessageDispatcher.ColorAnswer;
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
            while (isValidTagMode == false);

            // title
            if (((tagfields & TAG_FIELDS.TITLE) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(title))
            {
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter title (leave blank to delete it)");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                title = Console.ReadLine();
            }
            // album
            if (((tagfields & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(album))
            {
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter album (leave blank to delete it)");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                album = Console.ReadLine();
            }
            // album artist
            if (((tagfields & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(albumArtists))
            {
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter album artists  (leave blank to delete it)");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                albumArtists = Console.ReadLine();
            }
            // track artist
            if (((tagfields & TAG_FIELDS.TRACK_ARTIST) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(trackArtists))
            {
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter track artists (leave blank to delete it)");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
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
                        Console.ForegroundColor = MessageDispatcher.ColorQuestion;                        
                        Console.WriteLine("Enter year (leave blank to delete it)");
                        Console.ForegroundColor = MessageDispatcher.ColorAnswer;
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
                    if (isValid == false)
                    {
                        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                        Console.WriteLine("Enter track position (leave blank to delete it)");
                        Console.ForegroundColor = MessageDispatcher.ColorAnswer;
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
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter genres, comma separated (leave blank to delete it)");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
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
                        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                        Console.WriteLine("Enter image path for applying as front cover (leave blank to delete it)");
                        Console.ForegroundColor = MessageDispatcher.ColorAnswer;
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

        
        //[Obsolete(null, true)]
        //private void TagFileManually(string file, string tagmode, InputTag input)
        //{
        //    TAG_FIELDS tagfields = TAG_FIELDS.NONE;

        //    // tagmode            
        //    bool isValidTagMode = false;

        //    do
        //    {
        //        try
        //        {
        //            if (String.IsNullOrEmpty(tagmode))
        //            {
        //                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //                Console.WriteLine("Enter tag mode <a r t g y i m | ALL>");
        //                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
        //                tagmode = Console.ReadLine();
        //            }

        //            tagfields = ParseTagMode(tagmode);

        //            isValidTagMode = String.IsNullOrEmpty(tagmode) == false;

        //        }
        //        catch (ApplicationException)
        //        {
        //            tagmode = String.Empty;
        //        }

        //    }
        //    while (isValidTagMode == false);

        //    // title
        //    if (((tagfields & TAG_FIELDS.TITLE) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(input.Title))
        //    {
        //        Console.ForegroundColor = MessageDispatcher.ColorQuestion;     
        //        Console.WriteLine("Enter title (leave blank to delete it)");
        //        Console.ForegroundColor = MessageDispatcher.ColorAnswer;     
        //        input.Title = Console.ReadLine();
        //    }
        //    // album
        //    if (((tagfields & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(input.Album))
        //    {
        //        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //        Console.WriteLine("Enter album (leave blank to delete it)");
        //        Console.ForegroundColor = MessageDispatcher.ColorAnswer;     
        //        input.Album = Console.ReadLine();
        //    }
        //    // artist
        //    if (((tagfields & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE) && input.AlbumArtists != null)
        //    {
        //        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //        Console.WriteLine("Enter artist  (leave blank to delete it)");
        //        Console.ForegroundColor = MessageDispatcher.ColorAnswer;     
        //        input.AlbumArtists = Console.ReadLine().Split(',');
        //    }
        //    // year
        //    if (((tagfields & TAG_FIELDS.YEAR) != TAG_FIELDS.NONE) && input.Year == 0)
        //    {
        //        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //        Console.WriteLine("Enter year (leave blank to delete it)");
        //        Console.ForegroundColor = MessageDispatcher.ColorAnswer;     
        //        string inputYear = Console.ReadLine();
        //        uint year;
        //        UInt32.TryParse(inputYear, out year);
        //        input.Year = year;
        //    }
        //    // year
        //    if (((tagfields & TAG_FIELDS.TRACK_POS) != TAG_FIELDS.NONE) && input.Position == 0)
        //    {
        //        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //        Console.WriteLine("Enter track position (leave blank to delete it)");
        //        Console.ForegroundColor = MessageDispatcher.ColorAnswer;     
        //        string inputPosition = Console.ReadLine();
        //        uint position;
        //        UInt32.TryParse(inputPosition, out position);
        //        input.Position = position;
        //    }
        //    // genres
        //    if (((tagfields & TAG_FIELDS.GENRES) != TAG_FIELDS.NONE) && (input.Genres == null || input.Genres.Length == 0))
        //    {
        //        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //        Console.WriteLine("Enter genres, comma separated (leave blank to delete it)");
        //        Console.ForegroundColor = MessageDispatcher.ColorAnswer;     
        //        string inputGenres = Console.ReadLine();
        //        string[] genres = inputGenres.Split(',').Select(z => z.Trim()).ToArray();
        //        input.Genres = genres;
        //    }
        //    // image path
        //    if (((tagfields & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE) && String.IsNullOrEmpty(input.ImagePath))
        //    {
        //        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
        //        Console.WriteLine("Enter image path for applying as front cover (leave blank to delete it)");
        //        Console.ForegroundColor = MessageDispatcher.ColorAnswer;     
        //        bool isValidPath = true;
        //        do
        //        {
        //            if (isValidPath == false)
        //            {
        //                Console.ForegroundColor = ConsoleColor.Red;
        //                Console.WriteLine("enter a valid image path (or blank to remove tag)");
        //            }
        //            input.ImagePath = Console.ReadLine();
        //            isValidPath = (String.IsNullOrEmpty(input.ImagePath) || File.Exists(input.ImagePath));
        //        }
        //        while (isValidPath == false);
        //    }

        //    MessageDispatcher.PrintTagMode(tagfields);

        //    string[] files = new string[] { file };

        //    umTagger.TagFiles(files, input, tagfields);

        //    UMTMessage[] messages = umTagger.GetMessageQueue();

        //    MessageDispatcher.PrintMessages(messages);
          
        //}

        private void TagPathLastfm(string[] files, string album, string artist, string tagmode, char seekmode)
        {
            TAG_FIELDS tagfields = TAG_FIELDS.NONE;

            FILENAME_MATCH match = FILENAME_MATCH.LEVENSHTEIN_DISTANCE;

            // album
            while (String.IsNullOrEmpty(album))
            {
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter album to find");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                album = Console.ReadLine();
            }

            // artist
            while (String.IsNullOrEmpty(artist))
            {
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter artist to find");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                artist = Console.ReadLine();
            }

            // track/filename match algo
            bool isValidTrackMatch = false;

            do
            {
                string trackmatch = String.Empty;
                if (seekmode == ' ')
                {
                    Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                    Console.WriteLine("Enter track/filename match algo <p|n>");
                    Console.ForegroundColor = MessageDispatcher.ColorAnswer;
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
            } while (isValidTrackMatch == false);


            // tagmode            
            bool isValidTagMode = false;

            do
            {
                try
                {
                    if (String.IsNullOrEmpty(tagmode))
                    {
                        Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                        Console.WriteLine("Enter fields to tag <a r R t T g y i m | ALL>");
                        Console.ForegroundColor = MessageDispatcher.ColorAnswer;
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
            while (isValidTagMode == false);

            Console.ResetColor();

            // start tagging
            MessageDispatcher.PrintParameters(new ModelTag { AlbumArtists = artist.Split(','), Album = album });

            MessageDispatcher.PrintTagMode(tagfields);

            MessageDispatcher.PrintTrackMatch(match);

            Task.Factory.StartNew(PrintMessages);

            ReleaseInfo releaseInfo = null;

            try
            {
                releaseInfo = lastfmUtility.GetRelease(album, artist);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = MessageDispatcher.ColorFatal;
                Console.WriteLine(e.Message);
                Console.ForegroundColor = MessageDispatcher.ColorInfo;
            }

            if (releaseInfo != null && releaseInfo.TrackInfos.Count() > 0)
            {
                List<TrackInfo> list = releaseInfo.TrackInfos.ToList();

                Func<string, List<TrackInfo>, TrackInfo> comparer = null;

                switch (match)
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

        /// <summary>
        /// TagFileLastfm
        /// </summary>
        /// <param name="file"></param>
        /// <param name="title"></param>
        /// <param name="artist"></param>
        /// <param name="tagmode"></param>
        private void TagFileLastfm(string file, string title, string artist, string tagmode)
        {

            TAG_FIELDS tagfields = TAG_FIELDS.NONE;

            const ConsoleColor colorQuestion = ConsoleColor.Cyan;
            const ConsoleColor colorAnswer = ConsoleColor.Green;

            // title
            while (String.IsNullOrEmpty(title))
            {
                Console.ForegroundColor = colorQuestion;
                Console.WriteLine("Enter title to find");
                Console.ForegroundColor = colorAnswer;
                title = Console.ReadLine();
            }

            // artist
            while (String.IsNullOrEmpty(artist))
            {
                Console.ForegroundColor = colorQuestion;
                Console.WriteLine("Enter artist to find");
                Console.ForegroundColor = colorAnswer;
                artist = Console.ReadLine();
            }

            
            // tagmode            
            bool isValidTagMode = false;

            do
            {
                try
                {
                    if (String.IsNullOrEmpty(tagmode))
                    {
                        Console.ForegroundColor = colorQuestion;
                        Console.WriteLine("Enter fields to tag <a r R t T g y i m | ALL>");
                        Console.ForegroundColor = colorAnswer;
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
            while (isValidTagMode == false);

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
                Console.ForegroundColor = MessageDispatcher.ColorFatal;                
                Console.WriteLine(e.Message);
                Console.ForegroundColor = MessageDispatcher.ColorInfo;
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
            Console.ForegroundColor = MessageDispatcher.ColorInfo;
            ReadTag(file);
        }

        public void ReadTagFromMultipleFile(string[] args, string input)
        {
            Console.ForegroundColor = MessageDispatcher.ColorInfo;
            
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
            string tagmode = null;
            string albumArtists = null;
            string trackArtists = null;
            string title = null;
            string album = null;
            string mode = null;
            string year = null;
            string trackpos = null;
            string imagepath = null;
            string genres = null;

            var p = new OptionSet() {           
                { "h|help", "help for this mode",
                    v => isHelpRequested = true },                                             
                { "mode=", "manual / lastfm <manual|lastfm>",
                    v => mode = v },
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

            while (validModes.Contains(mode) == false)
            {
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter a valid mode <manual|lastfm>");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                mode = Console.ReadLine();
            }

            bool useLastfm = mode.Equals(ModeLastfm);

            if (isHelpRequested == false)
            {
                if (useLastfm)
                    TagFileLastfm(file, title, trackArtists, tagmode);
                else                    
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
            }
            else
            {
                Console.WriteLine("umtagger.exe tag input=filename [options]");
                Console.WriteLine(Environment.NewLine);

                OptionSet opt = null;

                if (useLastfm)
                {
                    Console.WriteLine("options for lastfm mode:");

                    opt = new OptionSet() {                            
                            { "fields=", "fields to tag (a t T r R y p i g m | ALL)",
                                v => tagmode = v },                                                               
                            { "artist=", "artist name",
                                v => albumArtists = v },                            
                            { "album=", "album name",
                                v => album = v },   
                            { "title=", "track title",
                                v => title = v },   
                        };
                }
                else
                {
                    Console.WriteLine("options for manual mode:");

                    opt = new OptionSet() {                            
                        { "fields=", "fields to tag (a t T r R y p i g m | ALL)",
                          v => tagmode = v },                                            
                        { "artist=", "artist name",
                          v => albumArtists = v },
                        { "title=", "track title",
                          v => title = v },
                        { "album=", "album name",
                          v => album = v },   
                        { "year=", "release year",
                          v => year = v },
                        { "pos=", "track position",
                          v => trackpos = v },
                        { "image=", "image path",
                          v => imagepath = v },
                        { "genres=", "genres, embraced and comma separated (ex. \"rock, hard rock\")",
                          v => genres = v },  
                        };
                }

                opt.WriteOptionDescriptions(Console.Out);

            }
        }

        public void TagPath(string[] args, string input)
        {
            bool isHelpRequested = false;
            string tagmode = null;
            string mode = null;
            string fileextension = null;
            string albumArtists = null;
            string trackArtists = null;
            string album = null;
            string year = null;
            string imagepath = null;
            char match = ' ';
            string genres = null;

            var p = new OptionSet() {           
                { "h|help", "help for this mode",
                    v => isHelpRequested = true },                          
                { "mode=", "manual / lastfm <manual|lastfm>",
                    v => mode = v },
                { "fields=", "fields to tag (a t T r R y p i g m | ALL)",
                    v => tagmode = v },                       
                //{ "ext=", "file extension (folder mode only)",
                //    v => fileextension = v },             
                { "rartist=", "album artist name",
                    v => albumArtists = v },
                { "tartist=", "track artist name",
                    v => trackArtists = v },
                { "artist=", "album/track artist name",
                    v => albumArtists = trackArtists = v },
                { "album=", "album name",
                    v => album = v },   
                { "year=", "release year",
                    v => year = v },
                { "image=", "image path",
                    v => imagepath = v },
                { "genres=", "genres, embraced and comma separated (ex. \"rock, hard rock\")",
                    v => genres = v }, 
                { "match=", "track / filename match mode <p|f>",
                    v => match = v.ToCharArray()[0] }, 
            };

            p.Parse(args);
            
            while (validModes.Contains(mode) == false)
            {
                Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                Console.WriteLine("Enter a valid mode <manual|lastfm>");
                Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                mode = Console.ReadLine();                
            }

            bool useLastfm = mode.Equals(ModeLastfm);

            string path = Path.GetDirectoryName(input);
            fileextension = Path.GetFileName(input);

            Console.ResetColor();

            if (isHelpRequested == false)
            {
                if (String.IsNullOrEmpty(fileextension))
                {
                    Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                    Console.WriteLine("Enter an audio file extension to process in path (default mp3)");
                    Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                    fileextension = Console.ReadLine();
                    if (String.IsNullOrEmpty(fileextension))
                    {
                        fileextension = DefaultExtension;
                    }
                }

                Console.ForegroundColor = MessageDispatcher.ColorInfo;

                Console.WriteLine(String.Format("extension selected: {0}", UMTShellUtility.NormalizeFilterPath(fileextension)));

                string[] files = UMTShellUtility.GetFilesFromPath(path, fileextension, SearchOption.TopDirectoryOnly);

                Console.WriteLine(String.Format("{0} files found in path", files.Count()));

                Console.ResetColor();
                
                if (useLastfm)
                    TagPathLastfm(files, album, albumArtists, tagmode, match);
                else
                    TagPathManually(files, tagmode, album, albumArtists, trackArtists, genres, year, imagepath);

            }
            else
            {

                Console.ForegroundColor = MessageDispatcher.ColorInfo;

                Console.WriteLine("umtagger.exe tag input=folder [options]");
                Console.WriteLine(Environment.NewLine);

                OptionSet opt = null;

                if (useLastfm)
                {
                    Console.WriteLine("options for lastfm mode:");

                    opt = new OptionSet() {                            
                            { "fields=", "fields to tag (a t T r R y p i g m | ALL)",
                                v => tagmode = v },                                   
                            //{ "ext=", "file extension (default mp3)",
                            //    v => fileextension = v },             
                            { "rartist=", "album artist name",
                                v => albumArtists = v },                            
                            { "tartist=", "track artist name",
                                v => trackArtists = v },                            
                            { "artist=", "album/track artist name",
                                v => trackArtists = albumArtists = v },                            
                            { "album=", "album name",
                                v => album = v },   
                            { "match=", "track / filename match mode <p|f>",
                                v => match = v.ToCharArray()[0] },                        
                        };
                }
                else
                {                    
                    Console.WriteLine("options for manual mode:");

                    opt = new OptionSet() {                            
                        { "fields=", "fields to tag (a t T r R y p i g m | ALL)",
                          v => tagmode = v },                    
                        //{ "ext=", "file extension (default mp3)",
                        //  v => fileextension = v },                    
                        { "rartist=", "album artist name",
                                v => albumArtists = v },                            
                        { "tartist=", "track artist name",
                            v => trackArtists = v },                            
                        { "artist=", "album/track artist name",
                            v => trackArtists = albumArtists = v },         
                        { "album=", "album name",
                          v => album = v },   
                        { "year=", "release year",
                          v => year = v },                        
                        { "image=", "image path",
                          v => imagepath = v },
                        { "genres=", "genres, embraced and comma separated (ex. \"rock, hard rock\")",
                          v => genres = v  },  
                        };
                }

                opt.WriteOptionDescriptions(Console.Out);

            }
        }
    }
}
