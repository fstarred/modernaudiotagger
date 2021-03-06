﻿using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using static UltimateMp3TaggerShell.UMTShellUtility;

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
                          v => isHelpRequired = v != null }                        
                    };

                List<string> extra = p.Parse(args);

                if (extra.Count > 1)
                {
                    action = extra.FirstOrDefault();
                    input = extra.Count() > 1 ? extra[1] : null;

                    Func<string, PATTERN_TYPE, bool> funcValidatePath = (inp, patterns) =>
                    {
                        input = inp;
                        patternType = getPathType(inp);
                        return (patternType & patterns) != 0;
                    };

                    while (String.IsNullOrEmpty(action) || !validActionNames.Contains(action) )
                    {
                        Console.WriteLine("Enter a valid action <tag|read|rename>");                        
                        action = Console.ReadLine();
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
                                Console.WriteLine("using proxy");                                
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            Console.WriteLine("file umtagger.ini not found, running with default settings");                            
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("some errors were reported while reading umtagger.ini");
                            Console.WriteLine(e.Message);
                        }

                        // tag mode
                        UMTTaggerDispatcher umtTaggerDelegate = new UMTTaggerDispatcher(proxy);

                        PATTERN_TYPE patns = 
                            PATTERN_TYPE.FILE | 
                            PATTERN_TYPE.MULTIPLE_FILES;

                        while (!funcValidatePath(input, patns))
                        {
                            MessageDispatcher.PromptPath(
                                PATTERN_TYPE.FILE | 
                                PATTERN_TYPE.MULTIPLE_FILES
                            );
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
                        
                    }
                    else if (action.Equals(ActionRename))
                    {
                        // rename mode
                        UMTRenamerDispatcher umtTaggerRenamer = new UMTRenamerDispatcher();

                        Action<string[], string> renameAction = null;

                        PATTERN_TYPE patns =
                            PATTERN_TYPE.FILE |
                            PATTERN_TYPE.MULTIPLE_FILES |
                            PATTERN_TYPE.DIRECTORY;

                        while (!funcValidatePath(input, patns))
                        {
                            MessageDispatcher.PromptPath(patns);
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
                                renameAction = umtTaggerRenamer.RenameDirectory;
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

                        PATTERN_TYPE patns =
                            PATTERN_TYPE.FILE | 
                            PATTERN_TYPE.MULTIPLE_FILES;

                        while (!funcValidatePath(input, patns))
                        {
                            MessageDispatcher.PromptPath(patns);
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
                Console.WriteLine(e.Message);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Thread.Sleep(300); // this is bad trick to dequeque last messages
            }

            //Console.Read();
        }

        /// <summary>
        /// PrintUsage
        /// </summary>
        /// <param name="p"></param>
        private static void PrintGeneralUsage(string program, OptionSet p)
        {
            Console.WriteLine("Ultimate Music Tagger information");            
            Console.WriteLine(String.Format("Version: {0}", UMTShellUtility.GetFileVersion(System.Reflection.Assembly.GetExecutingAssembly()).FileVersion));
            Console.WriteLine("Usage:");
            Console.Write(Environment.NewLine);
            Console.WriteLine(String.Format("{0} <tag|read|rename> <path> [options]", program));            

            p.WriteOptionDescriptions(Console.Out);            
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
            Nini.Config.IConfigSource configSource = new Nini.Config.IniConfigSource(Path.Combine(UMTShellUtility.GetPathFromAssembly(System.Reflection.Assembly.GetExecutingAssembly()), "umtagger.ini"));

            Nini.Config.IConfig configSection = configSource.Configs["Proxy"];

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
