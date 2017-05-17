using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace UltimateMusicTagger.Business
{
    /// <summary>
    /// LASTFM API
    /// http://www.last.fm/api
    /// http://www.last.fm/api/scrobbling
    /// </summary>
    public class LastfmClient : MyHttpUtility
    {
        // Get your own API_KEY and API_SECRET from http://www.last.fm/api/account
        string _apiKey = null;
        string _apiSecret = null;
        const string ROOT_URL = "http://ws.audioscrobbler.com/2.0/";


        public LastfmClient(string apikey, string apisecret)
        {
            _apiKey = apikey;
            _apiSecret = apisecret;
        }


        private string ParamToString(IDictionary<string, string> param)
        {
            StringBuilder sburl = new StringBuilder();

            if (param != null)
            {
                foreach (KeyValuePair<String, String> entry in param)
                {
                    sburl.Append('&');
                    sburl.Append(entry.Key);
                    sburl.Append('=');
                    sburl.Append(Uri.EscapeDataString(GetUTF8(entry.Value)));
                }
            }

            return sburl.ToString();
        }

        public string CallMethod(string name, IDictionary<string, string> param, REQUEST_METHOD method)
        {
            string urlparam = ParamToString(param);

            StringBuilder sburl = new StringBuilder();

            sburl.Append(String.Format("{0}?method={1}&api_key={2}{3}", ROOT_URL, name, GetUTF8(_apiKey), urlparam));

            string response = GetResponse(sburl.ToString(), method);

            return response;
        }

        public string CallMethod(string name)
        {
            return CallMethod(name, null, REQUEST_METHOD.GET);
        }

        public string CallMethod(string name, IDictionary<string, string> param)
        {
            return CallMethod(name, param, REQUEST_METHOD.GET);
        }

        public string GenerateSignature(string funcName, IDictionary<string, string> param)
        {
            StringBuilder sb = new StringBuilder();

            Dictionary<string, string> dicto = new Dictionary<string, string>(param);

            dicto.Add("api_key", GetUTF8(_apiKey));
            dicto.Add("method", funcName);

            foreach (var item in dicto.OrderBy(i => i.Key))
            {                
                sb.Append(item.Key);
                sb.Append(item.Value);
            }

            sb.Append(_apiSecret);

            string preMD5 = sb.ToString();

            byte[] temp = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(preMD5));

            StringBuilder apiHash = new StringBuilder();

            foreach (byte i in temp)
                apiHash.Append(i.ToString("x2"));

            return apiHash.ToString();
        }

        public string GetRequestAuthUrl(string token)
        {
            string url = String.Format("http://www.last.fm/api/auth/?api_key={0}&token={1}", _apiKey, token);

            return url;

            //Process.Start(url);
        }

        public static long GetUnixTimeNow()
        {
            TimeSpan timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

    }
}
