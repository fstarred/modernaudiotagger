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


        private void ShowPatternUsage()
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

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Environment.NewLine);
            Console.WriteLine("pattern legend:");

            string separator = ": ";

            printAction("title", separator, "%t|%title");
            printAction("artist", separator, "%a|%artist");
            printAction("album", separator, "%r|%album");
            printAction("year", separator, "%d|%y|%year");
            printAction("position", separator, "%p|%pos");

            separator = " = ";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Environment.NewLine);
            Console.WriteLine("some examples:");
            printAction("\"%p - %t\"", separator, "pattern - title");
            printAction("\"%a - %r - %t\"", separator, "artist - album - title");
            printAction("\"(%y) %r - %a - %t\"", separator, "(year) album - artist - title");                        

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
                bool isPatternEmpty = String.IsNullOrEmpty(pattern);

                string fieldRequired = String.Empty;

                bool argumentsValid = true;

                if (isPatternEmpty)
                {
                    fieldRequired = "pattern (-pattern)";
                    argumentsValid = false;
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
                Console.WriteLine("umtagger.exe rename input=file pattern=pattern");

                p.WriteOptionDescriptions(Console.Out);

                ShowPatternUsage();

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

                if (isPatternEmpty)
                {
                    fieldRequired = "pattern (-pattern)";
                    argumentsValid = false;
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
                        string message = String.Format(MessageDispatcher.MESSAGE_FIELD_REQUESTED, "-ref");
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
                Console.WriteLine("umtagger.exe rename -input=folder -pattern=pattern");

                p.WriteOptionDescriptions(Console.Out);

                ShowPatternUsage();

            }
        }

        public void RenameFilesInPath(string[] args, string input)
        {
            bool isHelpRequested = false;
            string pattern = null;
            //string target = null;
            string refFile = null;
            //string fileextension = DefaultExtension;

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
                bool isPatternEmpty = String.IsNullOrEmpty(pattern);
                //bool isTargetValid = new string[] { TargetFile, TargetFolder }.Contains(target);

                string fieldRequired = String.Empty;

                bool argumentsValid = true;

                if (isPatternEmpty)
                {
                    fieldRequired = "pattern (-pattern)";
                    argumentsValid = false;
                }

                string searchpattern = Path.GetFileName(input);
                string folder = Path.GetDirectoryName(input);
                
                //while (isTargetValid == false)
                //{                    
                //    Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                //    Console.WriteLine("Enter target to rename <file|dir> (default file)");
                //    Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                //    target = Console.ReadLine();
                //    if (String.IsNullOrEmpty(target))
                //        target = DefaultTarget;
                //    isTargetValid = new string[] { TargetFile, TargetFolder }.Contains(target);                    
                    
                //    //fieldRequired = "target (-target <file|dir>)";
                //    //argumentsValid = false;
                //}

                if (argumentsValid)
                {
                    string[] files = UMTShellUtility.GetFilesFromPath(folder, searchpattern, SearchOption.TopDirectoryOnly);

                    Console.ForegroundColor = MessageDispatcher.ColorInfo;
                    Console.WriteLine(String.Format("{0} files found in path", files.Count()));

                    //string newfoldername = null;

                    //if (target.Equals(TargetFile))
                        umTagger.RenameFilesByTag(files, pattern);
                    //else
                    //{
                    //    if (String.IsNullOrEmpty(refFile) == false && File.Exists(refFile))
                    //    {
                    //        umTagger.RenameFolderByTag(path, refFile, pattern, out newfoldername);
                    //    }
                    //    else
                    //    {
                    //        string message = String.Format(MessageDispatcher.MESSAGE_FIELD_REQUESTED, "-ref");
                    //        throw new ApplicationException(message);
                    //    }

                    //    //while (String.IsNullOrEmpty(refFile) || File.Exists(refFile))
                    //    //{
                    //    //    Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                    //    //    Console.WriteLine("Enter file as tag reference");
                    //    //    Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                    //    //    target = Console.ReadLine();
                    //    //}

                    //    //umTagger.RenameFolderByTag(path, files, pattern, out newfoldername);
                        
                    //}

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
                Console.WriteLine("umtagger.exe rename -input=folder -pattern=pattern");
                
                p.WriteOptionDescriptions(Console.Out);

                ShowPatternUsage();
                
            }

        }
    }
}
