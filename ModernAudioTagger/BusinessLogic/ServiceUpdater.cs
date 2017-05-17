using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace ModernAudioTagger.BusinessLogic
{
    public class ServiceUpdater
    {
        public const int REQUEST_TIMEOUT_MS = 4000;

        public class VersionInfo
        {
            public Version LatestVersion { get; set; }
            public string LatestVersionUrl { get; set; }        
        }

        public ServiceUpdater()
        {

        }

        public ServiceUpdater(WebProxy proxy) : base()
        {
            this.Proxy = proxy;
        }

        public WebProxy Proxy { get; set; }

        public VersionInfo CheckForUpdates(string url)
        {
            VersionInfo version = null;

            XmlDocument xmldoc = new XmlDocument();
            string contents = null;
            using (WebClient client = new WebClient())
            {
                client.Proxy = Proxy;
                contents = client.DownloadString(url);
            }
            if (string.IsNullOrEmpty(contents) == false)
            {
                xmldoc.LoadXml(contents);

                version = new VersionInfo();

                string latestversion = xmldoc.SelectSingleNode("//latestversion").InnerText;
                version.LatestVersionUrl = xmldoc.SelectSingleNode("//latestversionurl").InnerText;

                version.LatestVersion = new Version(latestversion);
            }

            return version;
        }
    }
}
