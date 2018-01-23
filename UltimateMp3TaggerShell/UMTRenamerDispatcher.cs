using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UltimateMusicTagger;

namespace UltimateMp3TaggerShell
{
    class UMTRenamerDispatcher
    {
        UltiMp3Tagger umTagger;

        //const string DefaultExtension = "mp3";

        const string DefaultExtension = "*";

        const string TargetFolder = "dir";
        const string TargetFile = "file";

        const string DefaultTarget = TargetFile;

        public UMTRenamerDispatcher()
        {
            umTagger = new UltiMp3Tagger();            
        }

        enum PATTERN_TYPE { FILE, FOLDER }

        private void ShowPatternUsage(PATTERN_TYPE patternType)
        {
            ConsoleColor color4field = ConsoleColor.White;
            ConsoleColor color4pattern = ConsoleColor.Cyan;
            ConsoleColor defaultColor = Console.ForegroundColor;

            Action<string, string, string> printAction = (field, sep, pattern) =>
            {
                Console.ForegroundColor = color4field;
                StringBuilder sb = new StringBuilder();
                sb.Append(field);
                sb.Append(sep);                
                Console.Write(sb.ToString());
                Console.ForegroundColor = color4pattern;
                Console.WriteLine(pattern);
            };

            //Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Environment.NewLine);
            Console.WriteLine("pattern legend:");

            string separator = ": ";

            if (patternType == PATTERN_TYPE.FILE)
            {
                printAction("title", separator, "%t|%title");
            }            
            printAction("album artist", separator, "%aa|%album artist");
            printAction("track artist", separator, "%ta|%track artist");
            printAction("album", separator, "%r|%album");
            printAction("year", separator, "%d|%y|%year");
            if (patternType == PATTERN_TYPE.FILE)
            {
                printAction("position", separator, "%p|%pos");
            }

            separator = " = ";
            
            Console.Write(Environment.NewLine);
            Console.WriteLine("some examples:");
            if (patternType == PATTERN_TYPE.FILE)
            {
                printAction("\"%p - %t\"", separator, "pattern - title");
                printAction("\"%aa - %r - %t\"", separator, "artist - album - title");
                printAction("\"(%y) %r - %aa - %t\"", separator, "(year) album - artist - title");
            }
            else
            {
                printAction("\"(%y) %r - %aa", separator, "(year) album - artist");
            }

            Console.ForegroundColor = defaultColor;
        }


        public void RenameFile(string[] args, string file)
        {
            bool showHelp = false;            
            string pattern = null;

            var p = new OptionSet() {           
                { "h|help", "help for this mode",
                    v => showHelp = true },                                                                             
                { "p|pattern=", "pattern for rename ",
                    v => pattern = v },                 
            };

            p.Parse(args);

            if (showHelp == false)
            {
                string fieldRequired = String.Empty;

                bool argumentsValid = true;

                while (String.IsNullOrEmpty(pattern))
                {
                    Console.ForegroundColor = MessageDispatcher.ColorQuestion;

                    Console.WriteLine("Enter a valid pattern format");

                    ShowPatternUsage(PATTERN_TYPE.FILE);

                    Console.ForegroundColor = MessageDispatcher.ColorAnswer;

                    pattern = Console.ReadLine();
                }

                if (argumentsValid)
                {
                    string[] files = new string[] { file };

                    umTagger.RenameFilesByTag(files, pattern);

                    UMTMessage[] messages = umTagger.UnqueueMessages();

                    MessageDispatcher.PrintMessages(messages);
                }
                else
                {
                    string message = String.Format(MessageDispatcher.MESSAGE_FIELD_REQUESTED, fieldRequired);
                    throw new ApplicationException(message);
                }
            }
            else
            {
                p.WriteOptionDescriptions(Console.Out);

                ShowPatternUsage(PATTERN_TYPE.FILE);

            }
        }

        public void RenameFolder(string[] args, string input)
        {
            bool isHelpRequested = false;
            string pattern = null;            
            string referenceFile = null;

            var p = new OptionSet() {           
                { "h|help", "help for this mode",
                    v => isHelpRequested = true },                                                                             
                { "p|pattern=", "pattern for rename ",
                    v => pattern = v }, 
                { "ref=", "tag file reference" ,
                    v => referenceFile = v },               
            };

            p.Parse(args);

            if (isHelpRequested == false)
            {
                bool isPatternEmpty = String.IsNullOrEmpty(pattern);
                
                string fieldRequired = String.Empty;

                bool argumentsValid = true;

                while (String.IsNullOrEmpty(pattern))
                {
                    Console.ForegroundColor = MessageDispatcher.ColorQuestion;

                    Console.WriteLine("Enter a valid pattern format");

                    ShowPatternUsage(PATTERN_TYPE.FOLDER);

                    Console.ForegroundColor = MessageDispatcher.ColorAnswer;

                    pattern = Console.ReadLine();
                }

                if (String.IsNullOrEmpty(referenceFile))
                {
                    referenceFile = "---"; // will pass on the loop at least once
                }

                while (!String.IsNullOrEmpty(referenceFile) && !File.Exists(referenceFile) )
                {
                    Console.ForegroundColor = MessageDispatcher.ColorQuestion;

                    Console.WriteLine("Enter a valid file reference or leave it blank to take a random mp3 file inside the target directory");
                    
                    Console.ForegroundColor = MessageDispatcher.ColorAnswer;

                    referenceFile = Console.ReadLine();

                    if (String.IsNullOrEmpty(referenceFile))
                    {
                        referenceFile = Directory.GetFiles(input)
                            .FirstOrDefault((i) => {
                                return Path.GetExtension(i).Equals(".mp3", StringComparison.InvariantCultureIgnoreCase);
                            });

                        if (String.IsNullOrEmpty(referenceFile))
                        {
                            argumentsValid = false;
                            break;
                        }
                    }
                }                

                if (argumentsValid)
                {                    
                    if (String.IsNullOrEmpty(referenceFile) == false && File.Exists(referenceFile))
                    {
                        string newfoldername = null;

                        umTagger.RenameFolderByTag(input, referenceFile, pattern, out newfoldername);
                    }
                    else
                    {
                        string message = String.Format("No valid files to read inside specified directory");
                        throw new ApplicationException(message);
                    }
                    

                    UMTMessage[] messages = umTagger.UnqueueMessages();

                    MessageDispatcher.PrintMessages(messages);
                }
                else
                {
                    string message = String.Format(MessageDispatcher.MESSAGE_FIELD_REQUESTED, fieldRequired);
                    throw new ApplicationException(message);
                }
            }
            else
            {
                p.WriteOptionDescriptions(Console.Out);

                ShowPatternUsage(PATTERN_TYPE.FOLDER);

            }
        }

        public void RenameFilesInPath(string[] args, string input)
        {
            bool isHelpRequested = false;
            string pattern = null;
            string refFile = null;

            var p = new OptionSet() {           
                { "h|help", "help for this mode",
                    v => isHelpRequested = true },                                                                             
                { "p|pattern=", "pattern for rename ",
                    v => pattern = v }, 
                //{ "t|target=", "file / folder <file|folder> (mode folder only, default file)" ,
                //    v => target = v }, 
                { "ref=", "tag file reference" ,
                    v => refFile = v },               
                //{ "ext=", "file extension (mode folder only)",
                //    v => fileextension = v },            
            };

            p.Parse(args);

            if (isHelpRequested == false)
            {
                //bool isTargetValid = new string[] { TargetFile, TargetFolder }.Contains(target);

                string fieldRequired = String.Empty;

                bool argumentsValid = true;

                while (String.IsNullOrEmpty(pattern))
                {
                    Console.ForegroundColor = MessageDispatcher.ColorQuestion;

                    Console.WriteLine("Enter a valid pattern format");

                    ShowPatternUsage(PATTERN_TYPE.FILE);

                    Console.ForegroundColor = MessageDispatcher.ColorAnswer;

                    pattern = Console.ReadLine();
                }

                string searchpattern = Path.GetFileName(input);
                string folder = Path.GetDirectoryName(input);
                
                if (argumentsValid)
                {
                    string[] files = UMTShellUtility.GetFilesFromPath(folder, searchpattern, SearchOption.TopDirectoryOnly);

                    Console.ForegroundColor = MessageDispatcher.ColorInfo;
                    Console.WriteLine(String.Format("{0} files found in path", files.Count()));

                    umTagger.RenameFilesByTag(files, pattern);
                    
                    UMTMessage[] messages = umTagger.UnqueueMessages();

                    MessageDispatcher.PrintMessages(messages);
                }
                else
                {
                    string message = String.Format(MessageDispatcher.MESSAGE_FIELD_REQUESTED, fieldRequired);
                    throw new ApplicationException(message);
                }
            }
            else
            {
                p.WriteOptionDescriptions(Console.Out);

                ShowPatternUsage(PATTERN_TYPE.FILE);
                
            }

        }
    }
}
