using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace UltimateMusicTagger.Business
{
    internal interface WServiceBase
    {
        void SetProxy(IWebProxy proxy);        
    }
}
