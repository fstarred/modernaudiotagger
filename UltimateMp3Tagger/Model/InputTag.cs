using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace UltimateMusicTagger.Model
{
    [Obsolete("InputTag should not be used", true)]
    public class InputTag
    {
        //public InputTag()
        //{
            
        //}

        public string[] AlbumArtists { get; set; }

        public string[] TrackArtists { get; set; }

        public string Album { get; set; }

        public string ImagePath { get; set; }

        public uint Year { get; set; }

        public Image ImageData { get; set; }

        public string ArtistMbid { get; set; }

        public string ReleaseMbid { get; set; }

        public string TrackMbid { get; set; }

        public string Title { get; set; }

        public uint Position { get; set; }

        public string[] Genres { get; set; }
        
    }
}
