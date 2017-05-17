using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltimateMusicTagger.Model
{
    public class TrackInfo
    {
        public string Title { get; set; }

        public uint Track { get; set; }

        public string Mbid { get; set; }

        public ReleaseInfo[] Releases { get; set; }

        public ArtistInfo[] Artists { get; set; }

        public string[] Genres { get; set; }

        public uint Length { get; set; }
    }
}
