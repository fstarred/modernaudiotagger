using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UltimateMp3TaggerShell
{
    class UMTShellUtility
    {

        [Flags]
        public enum PATTERN_TYPE
        {
            NONE = 0,
            FILE = 1,
            MULTIPLE_FILES = 2,
            DIRECTORY = 4
        }

        public static string NormalizeFilterPath(string filter)
        {
            string result = null;

            if (String.IsNullOrEmpty(filter) == false)
            {
                string pattern = "[\\.\\*]*";
                string replacement = String.Empty;
                Regex rgx = new Regex(pattern);
                result = rgx.Replace(filter, replacement);
            }

            result = String.IsNullOrEmpty(result) ? "*" : "*." + result;

            return result;
        }

        public static string[] GetFilesFromPath(string path, string ext, System.IO.SearchOption searchOption)
        {
            string filter = NormalizeFilterPath(ext);

            string[] files = Directory.GetFiles(path, filter, searchOption);

            return files;
        }

        public static string GetPathFromAssembly(System.Reflection.Assembly assembly)
        {
            string codeBase = assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static System.Diagnostics.FileVersionInfo GetFileVersion(System.Reflection.Assembly assembly)
        {
            System.Diagnostics.FileVersionInfo output = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return output;
        }
    }
}
