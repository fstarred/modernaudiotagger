using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace UltimateMusicTagger.Business
{
    public class MyHttpUtility
    {
        public enum REQUEST_METHOD { GET, POST }

        public IWebProxy Proxy { get; set; }

        public string UserAgent { get; set; }

        public int RequestTimeout { get; set; }

        public bool AllowRedirect { get; set; }

        private UTF8Encoding utf8 = null;

        private const int DEFAULT_REQUEST_TIMEOUT_MS = 5000;

        private string appver = null;

        public MyHttpUtility()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            appver = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion.ToString();
            utf8 = new UTF8Encoding();
            this.RequestTimeout = DEFAULT_REQUEST_TIMEOUT_MS;
            this.AllowRedirect = true;
        }

        private static bool IsWindowsOS()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    return true;
                default:
                    return false;
            }
        }

        protected string GetUTF8(string value)
        {
            return utf8.GetString(Encoding.UTF8.GetBytes(value));
        }


        public string GetResponse(string url)
        {
            return GetResponse(url, REQUEST_METHOD.GET);
        }

        public string GetResponse(string url, REQUEST_METHOD method)
        {

            Func<string, REQUEST_METHOD, string> func = IsWindowsOS() ? (Func<string, REQUEST_METHOD, string>)GetResponseWebClient : GetResponseWebClientMono;

            return func(url, method);

        }

        /// <summary>
        /// doesn't fail on mono
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private string GetResponseWebClientMono(string url, REQUEST_METHOD method)
        {
            string response = null;

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", this.UserAgent);
                client.Proxy = this.Proxy;
                client.Encoding = System.Text.Encoding.UTF8;                    

                try
                {
                    if (method == REQUEST_METHOD.GET)
                        response = client.DownloadString(url);
                    else
                        response = client.UploadString(url, String.Empty);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return response;
        }

        /// <summary>
        /// it doesn't work with mono
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private string GetResponseWebClient(string url, REQUEST_METHOD method)
        {
            string response = null;

            using (MyWebClient client = new MyWebClient())
            {
                client.Headers.Add("user-agent", this.UserAgent);
                client.Proxy = this.Proxy;
                client.Timeout = this.RequestTimeout;
                client.Encoding = System.Text.Encoding.UTF8;
                client.AllowRedirect = this.AllowRedirect;

                try
                {
                    if (method == REQUEST_METHOD.GET)
                        response = client.DownloadString(url);
                    else
                        response = client.UploadString(url, String.Empty);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return response;
        }


        //private string GetResponseHttpWebRequest(string url, REQUEST_METHOD method)
        //{
        //    System.Net.HttpWebRequest request = (HttpWebRequest)(WebRequest.Create(new Uri(url)));
        //    request.Timeout = DEFAULT_REQUEST_TIMEOUT_MS;
        //    request.UserAgent = this.UserAgent;
        //    request.Method = method.ToString();
        //    request.MaximumAutomaticRedirections = 4;
        //    request.MaximumResponseHeadersLength = 4;
        //    request.ContentLength = 0;

        //    StreamReader ReadStream = null;
        //    HttpWebResponse Response = null;
        //    string ResponseText = string.Empty;

        //    request.Proxy = this.Proxy;

        //    try
        //    {
        //        Response = (HttpWebResponse)(request.GetResponse());
        //        Stream ReceiveStream = Response.GetResponseStream();
        //        ReadStream = new StreamReader(ReceiveStream, System.Text.Encoding.UTF8);
        //        ResponseText = ReadStream.ReadToEnd();
        //        Response.Close();
        //        ReadStream.Close();

        //    }
        //    catch (Exception e)
        //    {
        //        ResponseText = string.Empty;
        //        throw e;
        //    }

        //    return ResponseText;
        //}

    }
}
