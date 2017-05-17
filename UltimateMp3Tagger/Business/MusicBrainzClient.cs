using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace UltimateMusicTagger.Business
{
    class MusicBrainzClient : MyHttpUtility
    {
        #region Fields

        const string ROOT_URL = "http://musicbrainz.org/ws/2/";
        const string ROOT_COVER_ART_URL = "http://coverartarchive.org/";
        
        #endregion

        #region Methods

        private string ParamToStringForQuery(IDictionary<string, string> param)
        {
            StringBuilder sburl = new StringBuilder();

            foreach (KeyValuePair<String, String> entry in param)
            {
                sburl.Append("+AND+");
                sburl.Append(entry.Key);
                sburl.Append(":\"");
                sburl.Append(entry.Value);
                sburl.Append('"');
            }

            if (param.Count > 0)
                sburl.Remove(0, 5);

            return sburl.ToString();
        }


        private string ParamToStringForBrowse(IDictionary<string, string> param)
        {
            StringBuilder sburl = new StringBuilder();

            foreach (KeyValuePair<String, String> entry in param)
            {
                sburl.Append("&");
                sburl.Append(entry.Key);
                sburl.Append('=');
                sburl.Append(entry.Value);
            }

            return sburl.ToString();
        }


        private string ParamToStringForLookup(IDictionary<string, string> param)
        {
            StringBuilder sburl = new StringBuilder();

            foreach (KeyValuePair<String, String> entry in param)
            {
                sburl.Append("?");
                sburl.Append(entry.Key);
                sburl.Append('=');
                sburl.Append(entry.Value);
            }

            return sburl.ToString();
        }

        public string SearchMethod(string name, IDictionary<string, string> queryParam, IDictionary<string, string> param)
        {
            string urlparamquery = ParamToStringForQuery(queryParam);

            string urlstdparam = ParamToStringForBrowse(param);

            StringBuilder sburl = new StringBuilder();

            sburl.Append(String.Format("{0}{1}/?query={2}{3}", ROOT_URL, name, urlparamquery, urlstdparam));

            string response = GetResponse(sburl.ToString());

            return response;
        }

        public string BrowseMethod(string name, string mbid, IDictionary<string, string> param)
        {
            string urlparam = ParamToStringForBrowse(param);

            StringBuilder sburl = new StringBuilder();

            sburl.Append(String.Format("{0}{1}={2}{3}", ROOT_URL, name, mbid, urlparam));

            string response = GetResponse(sburl.ToString());

            return response;
        }


        public string LookupMethod(string name, string mbid, IDictionary<string, string> param)
        {
            string urlparam = ParamToStringForLookup(param);

            StringBuilder sburl = new StringBuilder();

            sburl.Append(String.Format("{0}{1}/{2}{3}", ROOT_URL, name, mbid, urlparam));

            string response = GetResponse(sburl.ToString());

            return response;
        }
        
        #endregion

        //public string GetImageUrl(string name, string id, string objreq)
        //{
        //    return String.Format("{0}{1}/{2}/{3}", ROOT_COVER_ART_URL, name, id, objreq);
        //}

    }
}
