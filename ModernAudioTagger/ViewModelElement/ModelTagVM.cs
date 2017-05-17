using MicroMvvm;
using UltimateMusicTagger.Model;

namespace ModernAudioTagger.ViewModelElement
{
    public class ModelTagVM : ObservableObject
    {
        public ModelTagVM() { }

        public ModelTagVM(ModelTag input)
        {
            AlbumArtists = input.AlbumArtists;
            TrackArtists = input.TrackArtists;
            Album = input.Album;
            Year = input.Year;
            Genres = input.Genres;
            Picture = input.Picture;
            TrackMbid = input.TrackMbid;
            ArtistMbid = input.ArtistMbid;
            ReleaseMbid = input.ReleaseMbid;
            Title = input.Title;
            Position = input.Position;
        }

        public ModelTag GetModelTag()
        {
            return new ModelTag
            {
                 Album = this.album,
                 AlbumArtists = this.albumArtists,
                 ArtistMbid = this.artistMbid,
                 Genres = this.genres,
                 Picture = this.picture,
                 Position = this.position,
                 ReleaseMbid = this.releaseMbid,
                 TrackArtists = this.trackArtists,
                 Title = this.title,
                 TrackMbid = this.trackMbid,
                 Year = this.year
            };
        }

        private string[] albumArtists;

        public string[] AlbumArtists
        {
            get { return albumArtists; }
            set { 
                albumArtists = value;
                RaisePropertyChanged(() => AlbumArtists);
            }
        }

        private string[] trackArtists;

        public string[] TrackArtists
        {
            get { return trackArtists; }
            set { 
                trackArtists = value;
                RaisePropertyChanged(() => TrackArtists);
            }
        }


        private string album;

        public string Album
        {
            get { return album; }
            set { 
                album = value;
                RaisePropertyChanged(() => Album);
            }
        }

        private uint year;

        public uint Year
        {
            get { return year; }
            set { 
                year = value;
                RaisePropertyChanged(() => Year);
            }
        }

        private string[] genres;

        public string[] Genres
        {
            get { return genres; }
            set { 
                genres = value;
                RaisePropertyChanged(() => Genres);
            }
        }


        private System.Drawing.Image picture;

        public System.Drawing.Image Picture
        {
            get { return picture; }
            set { 
                picture = value;
                RaisePropertyChanged(() => Picture);
            }
        }


        private string trackMbid;

        public string TrackMbid
        {
            get { return trackMbid; }
            set { 
                trackMbid = value;
                RaisePropertyChanged(() => TrackMbid);
            }
        }


        private string artistMbid;

        public string ArtistMbid
        {
            get { return artistMbid; }
            set { 
                artistMbid = value;
                RaisePropertyChanged(() => ArtistMbid);
            }
        }

        private string releaseMbid;

        public string ReleaseMbid
        {
            get { return releaseMbid; }
            set { 
                releaseMbid = value;
                RaisePropertyChanged(() => ReleaseMbid);
            }
        }


        private string title;

        public string Title
        {
            get { return title; }
            set { 
                title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private uint position;

        public uint Position
        {
            get { return position; }
            set { 
                position = value;
                RaisePropertyChanged(() => Position);
            }

        }
        
        
    }
}
