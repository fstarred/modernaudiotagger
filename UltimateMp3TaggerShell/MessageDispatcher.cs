using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltimateMusicTagger;
using UltimateMusicTagger.Model;
using static UltimateMp3TaggerShell.UMTShellUtility;

namespace UltimateMp3TaggerShell
{
    class MessageDispatcher
    {        
        public const string MESSAGE_FIELD_REQUESTED = "{0} is a required parameter";

        public const ConsoleColor ColorInfo = ConsoleColor.White;
        public const ConsoleColor ColorQuestion = ConsoleColor.Cyan;
        public const ConsoleColor ColorAnswer = ConsoleColor.Green;
        public const ConsoleColor ColorFatal = ConsoleColor.Red;
        public const ConsoleColor ColorWarning = ConsoleColor.Yellow;
        public const ConsoleColor ColorError = ConsoleColor.Yellow;

        public static void PrintMessages(UMTMessage[] messages)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            
            foreach (UMTMessage msg in messages)
            {
                ConsoleColor color = ColorInfo;
                switch (msg.TypeMsg)
                {

                    case UMTMessage.M_TYPE.INFO:
                        color = ColorInfo;
                        break;
                    case UMTMessage.M_TYPE.ERROR:
                        color = ColorError;
                        break;
                    case UMTMessage.M_TYPE.WARNING:
                        color = ColorWarning;
                        break;
                }

                Console.ForegroundColor = color;
                Console.WriteLine(msg.Message);
            }

            Console.ForegroundColor = oldColor;
        }

        public static void PrintParameters(ModelTag input)
        {
            Func<string, string, string> message = (f, v) => {
                return String.IsNullOrEmpty(v) ?
                    String.Empty :
                    String.Format("{0}: {1}{2}", f, v, Environment.NewLine);
            };

            Func<string, uint, string> message2 = (f, v) =>
            {
                return v == 0 ?
                    String.Empty :
                    String.Format("{0}: {1}{2}", f, v, Environment.NewLine);
            };

            StringBuilder sb = new StringBuilder();
            
            sb.Append(message("artist", String.Join(", ", input.AlbumArtists)));
            sb.Append(message("title", input.Title));
            sb.Append(message("album", input.Album));
            //sb.Append(message("image path", input.ImagePath));
            sb.Append(message2("track position", input.Position));
            sb.Append(message("genres", input.Genres != null ? input.Genres.ToString() : null));
            sb.Append(message2("year", input.Year));

            ConsoleColor oldColor = Console.ForegroundColor;

            Console.ForegroundColor = ColorInfo;
            Console.WriteLine("INPUT PARAMETERS");
            Console.Write(Environment.NewLine);
            Console.WriteLine(sb.ToString());
            Console.Write(Environment.NewLine);

            Console.ForegroundColor = oldColor;
        }

        public static void PromptPath(PATTERN_TYPE type)
        {
            Console.ForegroundColor = ColorQuestion;

            Console.WriteLine("Enter a valid input from:");

            if ((type & PATTERN_TYPE.FILE) == PATTERN_TYPE.FILE)
            {
                Console.WriteLine("- File (ex. /music/foo.mp3)");
            }
            if ((type & PATTERN_TYPE.MULTIPLE_FILES) == PATTERN_TYPE.MULTIPLE_FILES)
            {
                Console.WriteLine("- More files (ex. /music/*.mp3)");
            }
            if ((type & PATTERN_TYPE.DIRECTORY) == PATTERN_TYPE.DIRECTORY)
            {
                Console.WriteLine("- Directory (ex. /music/album)");
            }

            Console.ForegroundColor = ColorAnswer;
        }

        public static void PrintTrackMatch(FILENAME_MATCH mode)
        {
            ConsoleColor oldColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("track match mode:");

            string seekMode = null;

            switch (mode)
            {
                case FILENAME_MATCH.LEVENSHTEIN_DISTANCE:
                case FILENAME_MATCH.JAROWINKLER_DISTANCE:
                    seekMode = "TITLE - FILENAME";
                    break;
                case FILENAME_MATCH.TRACK_POSITION:
                    seekMode = "POSITION - FILENAME";
                    break;                    
            }

            Console.WriteLine(seekMode);

            Console.ForegroundColor = oldColor;
        }

        public static void PrintTagMode(TAG_FIELDS mode)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("tag mode");

            StringBuilder sb = new StringBuilder();

            if ((mode == TAG_FIELDS.NONE))
            {
                sb.Append("no tag selected");
            }
            if ((mode & TAG_FIELDS.ALBUM) != TAG_FIELDS.NONE)
            {
                sb.Append("album,");                
            }
            if ((mode & TAG_FIELDS.ALBUM_ARTIST) != TAG_FIELDS.NONE)
            {
                sb.Append("album artist,");
            }
            if ((mode & TAG_FIELDS.TRACK_ARTIST) != TAG_FIELDS.NONE)
            {
                sb.Append("track artist,");
            }
            if ((mode & TAG_FIELDS.IMAGE) != TAG_FIELDS.NONE)
            {
                sb.Append("image,");
            }
            if ((mode & TAG_FIELDS.TITLE) != TAG_FIELDS.NONE)
            {
                sb.Append("title,");
            }
            if ((mode & TAG_FIELDS.TRACK_POS) != TAG_FIELDS.NONE)
            {
                sb.Append("position,");
            }
            if ((mode & TAG_FIELDS.YEAR) != TAG_FIELDS.NONE)
            {
                sb.Append("year,");
            }
            if ((mode & TAG_FIELDS.GENRES) != TAG_FIELDS.NONE)
            {
                sb.Append("genres,");                
            }
            if ((mode & TAG_FIELDS.MUSICBRAINZ_ID) != TAG_FIELDS.NONE)
            {
                sb.Append("musicbrainz id,");                
            }

            sb.Remove(sb.Length - 1, 1);

            sb.Append(Environment.NewLine);
            
            Console.WriteLine(sb.ToString());

            Console.ForegroundColor = oldColor;

        }


    }
}
