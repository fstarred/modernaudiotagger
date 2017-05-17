using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TagLib;

namespace UltimateMusicTagger.Model
{
    public class ModelTag
    {
        public string[] AlbumArtists { get; set; }

        public string[] TrackArtists { get; set; }

        public string Album { get; set; }

        public uint Year { get; set; }

        public string[] Genres { get; set; }

        public Image Picture { get; set; }

        public string TrackMbid { get; set; }

        public string ArtistMbid { get; set; }

        public string ReleaseMbid { get; set; }

        public string Title { get; set; }

        public uint Position { get; set; }
    }
}
