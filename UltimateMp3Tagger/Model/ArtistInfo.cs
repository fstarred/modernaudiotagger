using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltimateMusicTagger.Model
{
    public class ArtistInfo
    {
        public string Mbid { get; set; }

        public string Name { get; set; }

        public ReleaseInfo[] Releases { get; set; }

        public TrackInfo[] Trackinfos { get; set; }
    }
}
