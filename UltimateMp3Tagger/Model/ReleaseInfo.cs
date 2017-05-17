using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace UltimateMusicTagger.Model
{
    public class ReleaseInfo
    {
        public string Title { get; set; }

        public string Mbid { get; set; }

        public string ImagePath { get; set; }

        public Image Picture { get; set; }

        public string Year { get; set; }

        public TrackInfo[] TrackInfos { get; set; }

        public ArtistInfo[] Artists { get; set; }
    }
}
