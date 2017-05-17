using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltimateMusicTagger.Model;

namespace ModernAudioTagger.ViewModelElement
{
    public class TrackInfoVM : ObservableObject, ISelectable
    {
        public TrackInfoVM()
        {
        }

        public TrackInfoVM(TrackInfo track)
        {
            Title = track.Title;
            Track = track.Track;
            Mbid = track.Mbid;
            Releases = track.Releases;
            Artists = track.Artists;
            Length = track.Length;
            
            //RaisePropertyChanged(() => Title);
            //RaisePropertyChanged(() => Track);
            //RaisePropertyChanged(() => Mbid);
            RaisePropertyChanged(() => Releases);
            RaisePropertyChanged(() => Artists);
            //RaisePropertyChanged(() => Length);
        }

        private string title;

        public string Title 
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(() => Title); }
        }

        private uint track;

        public uint Track 
        {
            get { return track; }
            set { track = value; RaisePropertyChanged(() => Track); }
        }

        private string mbid;

        public string Mbid 
        {
            get { return mbid; }
            set { mbid = value; RaisePropertyChanged(() => Mbid); }
        }

        private uint length;

        public uint Length
        {
            get { return length; }
            set { length = value; RaisePropertyChanged(() => Length); }
        }
        

        public ReleaseInfo[] Releases { get; set; }

        public ArtistInfo[] Artists { get; set; }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; RaisePropertyChanged(() => IsSelected); }
        }
    }
}
