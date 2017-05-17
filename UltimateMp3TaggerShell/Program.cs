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

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            try
            {
                if (args.Length > 0)
                {
                    // action
                    string action = args[0];

                    bool isValidAction = validActionNames.Contains(action);

                    if (isValidAction == false)
                        throw new ApplicationException("no valid action name was specified: <tag|rename>");


                    string input = null;

                    /*
                    var p = new OptionSet() {                    
                        { "file=", "target file",
                          v => file = v },
                        { "path=", "destination folder",
                          v => path = v },                          
                    };
                     * */
                    var p = new OptionSet() {                    
                        { "input=", "target file / directory",
                          v => input = v },
                    };

                    p.Parse(args);

                    // path / file argument
                    bool isInputEmpty = String.IsNullOrEmpty(input);

                    if (isInputEmpty)
                        throw new ApplicationException("input argument must be specified");

                    //if ((isInputEmpty ^ isFileEmpty) == false)
                    //    throw new ApplicationException("only one between path and file argument must be specified");

                    // path check
                    //if (isFileEmpty)
                    //{
                    //    // path mode
                    //    if (Directory.Exists(input) == false)
                    //        throw new ApplicationException(String.Format("path {0} does not exist", input));
                    //}
                    //else
                    //{
                    //    // file mode
                    //    if (File.Exists(file) == false)
                    //        throw new ApplicationException(String.Format("file {0} does not exist", file));
                    //}

                    bool isFile = false;
                    bool isFolder = false;
                    bool isMultipleFiles = false;

                    isFolder = Directory.Exists(input);
                    isFile = File.Exists(input);

                    string basedir = Path.GetDirectoryName(input);

                    if (isFile == false && isFolder == false)
                    {
                        string filter = Path.GetFileName(input);

                        if (Directory.Exists(basedir))
                        {
                            try
                            {

                                if (Directory.GetFiles(basedir, filter).Length > 0)                                
                                    isMultipleFiles = true;
                            }
                            catch (Exception) { }
                        }
                        else
                            throw new ApplicationException("input is neither a valid file nor a folder");   
                    }

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

                        Action<string[], string> tagAction = null;

                        if (isFile)
                            tagAction = umtTaggerDelegate.TagFile;
                        else if (isMultipleFiles)
                            tagAction = umtTaggerDelegate.TagPath;
                        else
                            throw new ApplicationException("Input must be a file or a directory followed by a search pattern (eg. *.mp3)");

                        tagAction(args, input);

                        Console.ForegroundColor = MessageDispatcher.ColorInfo;
                    }
                    else if (action.Equals(ActionRename))
                    {
                        // rename mode
                        UMTRenamerDispatcher umtTaggerRenamer = new UMTRenamerDispatcher();

                        Action<string[], string> renameAction = null;

                        if (isFile)
                            renameAction = umtTaggerRenamer.RenameFile;
                        else if (isMultipleFiles)
                            renameAction = umtTaggerRenamer.RenameFilesInPath;
                        else if (isFolder)
                            renameAction = umtTaggerRenamer.RenameFolder;

                        //Action<string[], string> renameAction = isFolder ?
                        //    (Action<string[], string>)umtTaggerRenamer.RenameFilesInPath :
                        //    umtTaggerRenamer.RenameFile;

                        renameAction(args, input);
                    }
                    else if (action.Equals(ActionReadTag))
                    {
                        UMTTaggerDispatcher umtTaggerDelegate = new UMTTaggerDispatcher(null);

                        if (isFile)
                            umtTaggerDelegate.ReadTagFromSingleFile(args, input);
                        else if (isMultipleFiles)
                            umtTaggerDelegate.ReadTagFromMultipleFile(args, input);
                        else
                            throw new ApplicationException("Input must be a file or a directory followed by a search pattern (eg. *.mp3)");

                    }
                }
                else
                    PrintGeneralUsage(new OptionSet());

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

            //Console.Read();
        }

        /// <summary>
        /// PrintUsage
        /// </summary>
        /// <param name="p"></param>
        private static void PrintGeneralUsage(OptionSet p)
        {
            Console.ForegroundColor = MessageDispatcher.ColorInfo;

            Console.WriteLine("Ultimate Music Tagger usage:\n");
            Console.WriteLine("UMTagger.exe <tag|rename|read> <-input> [options]");

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
