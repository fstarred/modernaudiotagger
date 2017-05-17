using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace UltimateMusicTagger.Business
{
    internal class CoverArtClient : MyHttpUtility
    {
        const string ROOT_URL = "http://coverartarchive.org/";

        public enum IMAGE_TYPE { FRONT, BACK };

        public enum IMAGE_SIZE { SMALL, LARGE };

        public enum ENTITY { RELEASE, RELEASE_GROUP };

        public string GetFirstUrl(ENTITY entity, string mbid, IMAGE_TYPE type, IMAGE_SIZE size)
        {
            string imageSize = size == IMAGE_SIZE.LARGE ? "500" : "250";

            string imageType = type.ToString().ToLower();

            string entityValue = entity == ENTITY.RELEASE ? "release" : "release-group";

            string url = String.Format("{0}{1}/{2}/{3}-{4}", ROOT_URL, entityValue, mbid, imageType, imageSize);

            return url;
        }

        public string GetRedirectUrl(ENTITY entity, string mbid, IMAGE_TYPE type, IMAGE_SIZE size)
        {
            string firstUrl = GetFirstUrl(entity, mbid, type, size);

            string response = null;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(new Uri(firstUrl));
            req.Proxy = this.Proxy;
            req.Method = "HEAD";
            req.AllowAutoRedirect = false;

            HttpWebResponse myResp = (HttpWebResponse)req.GetResponse();
            if (myResp.StatusCode == HttpStatusCode.TemporaryRedirect)
            {
                response = myResp.GetResponseHeader("Location");
            }

            return response;
        }
    }
}
