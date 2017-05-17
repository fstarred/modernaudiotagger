using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace UltimateMusicTagger.Business
{
    class MyWebClient : WebClient
    {
        public int Timeout { get; set; }
        public bool AllowRedirect { get; set; }

        public MyWebClient()
        {

        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = Timeout;
            ((HttpWebRequest)w).AllowAutoRedirect = AllowRedirect;
            return w;
        }
    }
}
