using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace UltimateMusicTagger.Model
{
    [Obsolete()]
    public class OutputInfo
    {
        public string[] ArtistsAlbum { get; set; }

        public string[] ArtistsTrack { get; set; }

        public string Album { get; set; }

        public string ImagePath { get; set; }

        public uint Year { get; set; }

        public string[] Genres { get; set; }

        public Image ImageData { get; set; }

        public string ArtistMbid { get; set; }

        public string ReleaseMbid { get; set; }

        public TrackInfo[] TrackInfos { get; set; }
    }
}
