using NDesk.Options;
using Nini.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using UltimateMusicTagger;
using UltimateMusicTagger.Business;
using UltimateMusicTagger.Model;

namespace UltimateMp3TaggerShell
{
    /// <summary>
    /// Program
    /// </summary>
    class Program
    {
        const string ActionTag = "tag";
        const string ActionRename = "rename";
        const string ActionReadTag = "read";

        readonly static string[] validActionNames = new string[] { ActionTag, ActionRename, ActionReadTag };

        enum PATTERN_TYPE { NONE, FILE, MULTIPLE_FILES, DIRECTORY }

        static PATTERN_TYPE getPathType(string input)
        {
            PATTERN_TYPE output = PATTERN_TYPE.NONE;

            bool isFile = false;
            bool isFolder = false;
            bool isMultipleFiles = false;
            try
            {

                isFolder = Directory.Exists(input);
                isFile = File.Exists(input);

                string basedir = Path.GetDirectoryName(input);

                if (isFile == false && isFolder == false)
                {
                    string filter = Path.GetFileName(input);

                    if (Directory.Exists(basedir))
                    {
                        if (Directory.GetFiles(basedir, filter).Length > 0)
                            isMultipleFiles = true;
                    }
                }

            }
            catch (Exception)
            {
                // no prob
            }

            if (isFile) output = PATTERN_TYPE.FILE;
            else if (isFolder) output = PATTERN_TYPE.DIRECTORY;
            else if (isMultipleFiles) output = PATTERN_TYPE.MULTIPLE_FILES;

            return output;
        }

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            try
            {
                string action = null;
                bool isHelpRequired = false;
                string input = null;
                PATTERN_TYPE patternType = PATTERN_TYPE.NONE;

                var p = new OptionSet() {
                        { "h|help", "help",
                          v => isHelpRequired = v != null },
                        { "a|action=", "<tag|read|rename>",
                          v => action = v },
                    };

                List<string> extra = p.Parse(args);

                input = extra.FirstOrDefault();

                if (!isHelpRequired)
                {                    
                    //bool isValidAction = validActionNames.Contains(action);

                    //if (isValidAction == false)
                    //    throw new ApplicationException("no valid action name was specified: <tag|rename>");

                    //bool isFile = false;
                    //bool isFolder = false;
                    //bool isMultipleFiles = false;

                    //isFolder = Directory.Exists(input);
                    //isFile = File.Exists(input);

                    //string basedir = Path.GetDirectoryName(input);

                    //if (isFile == false && isFolder == false)
                    //{
                    //    string filter = Path.GetFileName(input);

                    //    if (Directory.Exists(basedir))
                    //    {
                    //        try
                    //        {

                    //            if (Directory.GetFiles(basedir, filter).Length > 0)                                
                    //                isMultipleFiles = true;
                    //        }
                    //        catch (Exception) { }
                    //    }
                    //    //else
                    //    //    throw new ApplicationException("input is neither a valid file nor a folder");   
                    //}


                    Func<string, IEnumerable<PATTERN_TYPE>, bool> funcValidatePath = (inp, patterns) =>
                    {
                        input = inp;
                        patternType = getPathType(inp);
                        return patterns.Contains(patternType);
                    };

                    if (action.Equals(ActionTag))
                    {
                        // tag mode
                        WebProxy proxy = null;

                        try
                        {
                            proxy = GetProxyFromConfig();

                            if (proxy != null)
                            {
                                Console.ForegroundColor = MessageDispatcher.ColorInfo;
                                Console.WriteLine("using proxy");
                                Console.ResetColor();
                            }
                        }
                        catch (FileNotFoundException e)
                        {
                            Console.ForegroundColor = MessageDispatcher.ColorWarning;
                            Console.WriteLine("file umtagger.ini not found, running with default settings");
                            Console.ResetColor();
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = MessageDispatcher.ColorWarning;
                            Console.WriteLine("some errors were reported while reading umtagger.ini");
                            Console.WriteLine(e.Message);
                            Console.ResetColor();
                        }

                        // tag mode
                        UMTTaggerDispatcher umtTaggerDelegate = new UMTTaggerDispatcher(proxy);

                        IEnumerable<PATTERN_TYPE> patns = new PATTERN_TYPE[] {
                                PATTERN_TYPE.FILE,
                                PATTERN_TYPE.MULTIPLE_FILES
                            };

                        while (!funcValidatePath(input, patns))
                        {
                            Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                            Console.WriteLine("Enter a valid file path or pattern (ex. /D/Music/*mp3)");
                            Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                            input = Console.ReadLine();
                        }

                        Action<string[], string> tagAction;
                        
                        switch (patternType)
                        {
                            case PATTERN_TYPE.FILE:
                                tagAction = umtTaggerDelegate.TagFile;
                                break;
                            case PATTERN_TYPE.MULTIPLE_FILES:
                                tagAction = umtTaggerDelegate.TagPath;
                                break;
                            default:
                                tagAction = null;
                                break;
                        }

                        tagAction(args, input);

                        Console.ForegroundColor = MessageDispatcher.ColorInfo;
                    }
                    else if (action.Equals(ActionRename))
                    {
                        // rename mode
                        UMTRenamerDispatcher umtTaggerRenamer = new UMTRenamerDispatcher();

                        Action<string[], string> renameAction = null;

                        IEnumerable<PATTERN_TYPE> patns = new PATTERN_TYPE[] {
                                PATTERN_TYPE.FILE,
                                PATTERN_TYPE.MULTIPLE_FILES,
                                PATTERN_TYPE.DIRECTORY
                            };

                        while (!funcValidatePath(input, patns))
                        {
                            Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                            Console.WriteLine("Enter a valid file, directory, or pattern (ex. /D/Music/*.mp3)");
                            Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                            input = Console.ReadLine();
                        }
                        
                        switch (patternType)
                        {
                            case PATTERN_TYPE.FILE:
                                renameAction = umtTaggerRenamer.RenameFile;
                                break;
                            case PATTERN_TYPE.MULTIPLE_FILES:
                                renameAction = umtTaggerRenamer.RenameFilesInPath;
                                break;
                            case PATTERN_TYPE.DIRECTORY:
                                renameAction = umtTaggerRenamer.RenameFolder;
                                break;
                            default:
                                renameAction = null;
                                break;
                        }
                        
                        renameAction(args, input);
                    }
                    else if (action.Equals(ActionReadTag))
                    {
                        UMTTaggerDispatcher umtTaggerDelegate = new UMTTaggerDispatcher(null);

                        IEnumerable<PATTERN_TYPE> patns = new PATTERN_TYPE[] {
                                PATTERN_TYPE.FILE,
                                PATTERN_TYPE.MULTIPLE_FILES
                            };

                        while (!funcValidatePath(input, patns))
                        {
                            Console.ForegroundColor = MessageDispatcher.ColorQuestion;
                            Console.WriteLine("Enter a valid file path or pattern (ex. /D/Music/*.mp3)");
                            Console.ForegroundColor = MessageDispatcher.ColorAnswer;
                            input = Console.ReadLine();
                        }

                        Action<string[], string> readAction = null;

                        switch (patternType)
                        {
                            case PATTERN_TYPE.FILE:
                                readAction = umtTaggerDelegate.ReadTagFromSingleFile;
                                break;
                            case PATTERN_TYPE.MULTIPLE_FILES:
                                readAction = umtTaggerDelegate.ReadTagFromMultipleFile;
                                break;
                            default:
                                readAction = null;
                                break;
                        }

                        readAction(args, input);
                        
                    }
                }
                else
                    PrintGeneralUsage(args.FirstOrDefault(), p);

            }
            catch (ApplicationException e)
            {
                Console.ForegroundColor = MessageDispatcher.ColorWarning;
                Console.WriteLine(e.Message);
            }
            catch (OptionException e)
            {
                Console.ForegroundColor = MessageDispatcher.ColorWarning;
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = MessageDispatcher.ColorFatal;
                Console.WriteLine(e);
            }
            finally
            {
                Thread.Sleep(300); // this is bad trick to dequeque last messages
                Console.ResetColor();
            }

            Console.Read();
        }

        /// <summary>
        /// PrintUsage
        /// </summary>
        /// <param name="p"></param>
        private static void PrintGeneralUsage(string program, OptionSet p)
        {
            Console.ForegroundColor = MessageDispatcher.ColorInfo;

            Console.WriteLine("Ultimate Music Tagger usage:\n");
            Console.WriteLine(String.Format("{0} <input> -action <value> [options]", program));

            p.WriteOptionDescriptions(Console.Out);

            Console.ResetColor();
        }


        /// <summary>
        /// GetProxyFromConfig
        /// </summary>
        /// <returns></returns>
        private static WebProxy GetProxyFromConfig()
        {
            WebProxy proxy = null;

            //try
            //{
            IConfigSource configSource = new IniConfigSource("umtagger.ini");

            IConfig configSection = configSource.Configs["Proxy"];

            string enabled = configSection.Get("enable");

            bool isProxyEnabled = false;

            Boolean.TryParse(enabled, out isProxyEnabled);

            if (isProxyEnabled)
            {
                string address = configSection.Get("address");
                int port = int.Parse(configSection.Get("port", "8080"));
                string domain = configSection.Get("domain");
                string username = configSection.Get("user");
                string password = configSection.Get("password");

                if (address != null)
                {
                    if (Uri.IsWellFormedUriString(address, UriKind.RelativeOrAbsolute) == false)
                        address = Dns.GetHostName();

                    proxy = new WebProxy(address, port);
                    NetworkCredential credentials = new NetworkCredential(username, password, domain);
                    proxy.Credentials = credentials;
                }

            }

            return proxy;
        }



    }
}
