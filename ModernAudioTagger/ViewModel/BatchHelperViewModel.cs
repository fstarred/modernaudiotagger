using MicroMvvm;
using ModernAudioTagger.BusinessLogic;
using ModernAudioTagger.Provider;
using ModernUILogViewer.BusinessLogic;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Unity;

namespace ModernAudioTagger.ViewModel
{
    public class BatchHelperViewModel : BaseViewModel    
    {
        #region Constructor

        public BatchHelperViewModel()
        {
            CheckAllTag();
            SelectedTagMode = TAG_MODE.MANUAL;
            SelectedAction = ACTION.TAG;
            SelectedDestinationType = DESTINATION_TYPE.FILE;
            //SelectedTarget = DESTINATION_TYPE.FILE;
            SelectedMatch = MATCH_MODE.NAME;
            FileExtension = "mp3";
            GenerateCommandLineTask();
        }
        
        #endregion

        //public event EventHandler OnNavigatingToEvent;

        #region Enums

        public enum ACTION { TAG, RENAME, READ }

        public enum TAG_MODE { MANUAL, LASTFM }

        public enum DESTINATION_TYPE { FILE, MULTIPLE_FILES, FOLDER }

        public enum MATCH_MODE { NAME, NUMBER }
        
        #endregion

        #region Fields

        //private readonly string[] operations = new string[] { "tag", "rename" };

        //private readonly string[] mode = new string[] { "manual", "lastfm" };

        //private readonly string[] destinationType = new string[] { "file", "folder" };

        ManualResetEvent mre = new ManualResetEvent(false);

        const string EXECUTABLE_NAME = "UMTaggerShell.exe";

        //CancellationTokenSource token = new CancellationTokenSource();

        #endregion

        #region Properties

        private MATCH_MODE selectedMatch;

        public MATCH_MODE SelectedMatch
        {
            get { return selectedMatch; }
            set { 
                selectedMatch = value;
                RaisePropertyChanged(() => SelectedMatch);
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

        private string refTag;

        public string RefTag
        {
            get { return refTag; }
            set { 
                refTag = value;
                RaisePropertyChanged(() => RefTag);
            }
        }
        

        private string pattern;

        public string Pattern
        {
            get { return pattern; }
            set
            {
                pattern = value;
                RaisePropertyChanged(() => Pattern);
            }
        }

        private string position;

        public string Position
        {
            get { return position; }
            set
            {
                position = value;
                RaisePropertyChanged(() => Position);
            }
        }

        private string artistsTrack;

        public string ArtistsTrack
        {
            get { return artistsTrack; }
            set
            {
                artistsTrack = value;
                RaisePropertyChanged(() => ArtistsTrack);
            }
        }


        private string fileExtension;

        public string FileExtension
        {
            get { return fileExtension; }
            set
            {
                fileExtension = value;
                RaisePropertyChanged(() => FileExtension);
            }
        }

        private string genres;

        public string Genres
        {
            get { return genres; }
            set
            {
                genres = value;
                RaisePropertyChanged(() => Genres);
            }
        }


        private string musicBrainzId;

        public string MusicBrainzId
        {
            get { return musicBrainzId; }
            set
            {
                musicBrainzId = value;
                RaisePropertyChanged(() => MusicBrainzId);
            }
        }


        private string album;

        public string Album
        {
            get { return album; }
            set
            {
                album = value;
                RaisePropertyChanged(() => Album);
            }
        }

        private string albumArtists;

        public string AlbumArtists
        {
            get { return albumArtists; }
            set
            {
                albumArtists = value;
                RaisePropertyChanged(() => AlbumArtists);
            }
        }

        private string year;

        public string Year
        {
            get { return year; }
            set
            {
                year = value;
                RaisePropertyChanged(() => Year);
            }
        }


        private string cover;

        public string Cover
        {
            get { return cover; }
            set
            {
                cover = value;
                RaisePropertyChanged(() => Cover);
            }
        }

        private string preview;

        public string Preview
        {
            get { return preview; }
            set { 
                preview = value;
                RaisePropertyChanged(() => Preview);
            }
        }

        private string searchAlbum;

        public string SearchAlbum
        {
            get { return searchAlbum; }
            set
            {
                searchAlbum = value;
                RaisePropertyChanged(() => SearchAlbum);
            }
        }

        private string searchTitle;

        public string SearchTitle
        {
            get { return searchTitle; }
            set
            {
                searchTitle = value;
                RaisePropertyChanged(() => SearchTitle);
            }
        }


        private string searchArtist;

        public string SearchArtist
        {
            get { return searchArtist; }
            set
            {
                searchArtist = value;
                RaisePropertyChanged(() => SearchArtist);
            }
        }

        private bool includeTitle;

        public bool IncludeTitle
        {
            get { return includeTitle; }
            set
            {
                includeTitle = value;
                RaisePropertyChanged(() => IncludeTitle);
            }
        }


        private bool includePosition;

        public bool IncludePosition
        {
            get { return includePosition; }
            set
            {
                includePosition = value;
                RaisePropertyChanged(() => IncludePosition);
            }
        }

        private bool includeTrackArtist;

        public bool IncludeTrackArtist
        {
            get { return includeTrackArtist; }
            set
            {
                includeTrackArtist = value;
                RaisePropertyChanged(() => IncludeTrackArtist);
            }
        }


        private bool includeGenres;

        public bool IncludeGenres
        {
            get { return includeGenres; }
            set
            {
                includeGenres = value;
                RaisePropertyChanged(() => IncludeGenres);
            }
        }

        private bool includeMbId;

        public bool IncludeMbId
        {
            get { return includeMbId; }
            set
            {
                includeMbId = value;
                RaisePropertyChanged(() => IncludeMbId);
            }
        }

        private bool includeRelease;

        public bool IncludeRelease
        {
            get { return includeRelease; }
            set
            {
                includeRelease = value;
                RaisePropertyChanged(() => IncludeRelease);
            }
        }

        private bool includeReleaseArtist;

        public bool IncludeReleaseArtist
        {
            get { return includeReleaseArtist; }
            set
            {
                includeReleaseArtist = value;
                RaisePropertyChanged(() => IncludeReleaseArtist);
            }
        }

        private bool includeYear;

        public bool IncludeYear
        {
            get { return includeYear; }
            set
            {
                includeYear = value;
                RaisePropertyChanged(() => IncludeYear);
            }
        }

        private bool includePicture;

        public bool IncludePicture
        {
            get { return includePicture; }
            set
            {
                includePicture = value;
                RaisePropertyChanged(() => IncludePicture);
            }
        }

        private string destination;

        public string Destination
        {
            get { return destination; }
            set { 
                destination = value;
                RaisePropertyChanged(() => Destination);
            }
        }
        

        private ACTION selectedAction;

        public ACTION SelectedAction
        {
            get { return selectedAction; }
            set
            {
                selectedAction = value;
                RaisePropertyChanged(() => SelectedAction);
            }
        }

        private DESTINATION_TYPE selectedDestinationType;

        public DESTINATION_TYPE SelectedDestinationType
        {
            get { return selectedDestinationType; }
            set
            {
                selectedDestinationType = value;
                RaisePropertyChanged(() => SelectedDestinationType);
            }
        }

        private TAG_MODE selectedTagMode;

        public TAG_MODE SelectedTagMode
        {
            get { return selectedTagMode; }
            set
            {
                selectedTagMode = value;
                RaisePropertyChanged(() => SelectedTagMode);
            }
        }
        
        #endregion

        #region Methods

        //public string[] GetDestinationsType()
        //{
        //    string[] output = Enum.GetNames(typeof(DESTINATION_TYPE));

        //    switch (selectedAction)
        //    {
        //        case ACTION.READ:
        //        case ACTION.TAG:
        //            break;
        //        case ACTION.RENAME:
        //            break;
        //    }

        //    return output;
        //}

        void AddToPattern(string input)
        {            
            string patternPart = input.Substring(0, input.IndexOf(' '));

            Pattern = patternPart;
        }

        void SelectModelTag()
        {
            IUnityContainer container = UnityContainerProvider.Instance;

            IDialogService service = null;

            service = container.Resolve<IDialogService>(ProviderServiceConstants.DLG_OPEN_AUDIO);
            string[] output = service.OpenFile(false);
            if (output.Length > 0)
            {
                RefTag = output[0];
            }
        }

        void SelectDestination()
        {
            IUnityContainer container = UnityContainerProvider.Instance;

            IDialogService service = null;

            string result = null;

            switch (selectedDestinationType)
            {
                case DESTINATION_TYPE.FILE:
                    service = container.Resolve<IDialogService>(ProviderServiceConstants.DLG_OPEN_AUDIO);
                    string[] output = service.OpenFile(false);
                    if (output.Length > 0)
                    {
                        result = output[0];
                    }
                    break;
                case DESTINATION_TYPE.FOLDER:
                    service = container.Resolve<IDialogService>(ProviderServiceConstants.DLG_OPEN_PATH);
                    result = service.OpenPath(null);
                    break;
                case DESTINATION_TYPE.MULTIPLE_FILES:
                    service = container.Resolve<IDialogService>(ProviderServiceConstants.DLG_OPEN_PATH);
                    result = service.OpenPath(null);
                    if (String.IsNullOrEmpty(result) == false)
                    {
                        result = Path.Combine(result, "*");
                    }                    
                    break;
            }

            if (String.IsNullOrEmpty(result) == false)
            {                
                Destination = result;
            }
        }

        void UpdatePreview()
        {
            Func<string> funcDestinationOption = () =>
                {
                    StringBuilder sb0 = new StringBuilder();

                    sb0.Append("-input=");
                    sb0.Append('"');
                    sb0.Append(destination);
                    sb0.Append('"');
                    //sb0.Append(' ');

                    return sb0.ToString();
                };

            Func<string> funcLastFmOption = () =>
                {
                    StringBuilder sb2 = new StringBuilder();

                    switch (selectedDestinationType)
                    {
                        case DESTINATION_TYPE.FILE:
                            sb2.Append("-artist=");
                            sb2.Append('"');
                            sb2.Append(searchArtist);
                            sb2.Append('"');
                            sb2.Append(' ');
                            sb2.Append("-title=");
                            sb2.Append('"');
                            sb2.Append(searchTitle);
                            sb2.Append('"');
                            break;
                        case DESTINATION_TYPE.MULTIPLE_FILES:
                            sb2.Append("-artist=");
                            sb2.Append('"');
                            sb2.Append(searchArtist);
                            sb2.Append('"');                            
                            sb2.Append(" -album=");
                            sb2.Append('"');
                            sb2.Append(searchAlbum);
                            sb2.Append('"');
                            sb2.Append(' ');
                            sb2.Append("-match=");
                            switch (selectedMatch)
                            {
                                case MATCH_MODE.NAME:
                                    sb2.Append('n');
                                    break;
                                case MATCH_MODE.NUMBER:
                                    sb2.Append('p');
                                    break;
                            }                            
                            break;
                    }

                    return sb2.ToString();
                };

            Func<string> funcTagKeysOption = () =>
                {
                    StringBuilder sb3 = new StringBuilder();

                    sb3.Append("-fields ");


                    if (includeRelease && includeReleaseArtist && includeTrackArtist
                        && includeTitle && includePosition && includeYear
                        && includeGenres && includePicture && includeMbId)
                        sb3.Append("ALL");
                    else
                    {
                        if (includeRelease) sb3.Append('r');
                        if (includeReleaseArtist) sb3.Append('R');
                        if (includeTrackArtist) sb3.Append('T');
                        if (includeTitle) sb3.Append('t');
                        if (includePosition) sb3.Append('p');
                        if (includeYear) sb3.Append('y');
                        if (includeGenres) sb3.Append('g');
                        if (includePicture) sb3.Append('i');
                        if (includeMbId) sb3.Append('m');
                    }

                    return sb3.ToString();
                };

            Func<string> funcTagModeOption = () =>
                {
                    StringBuilder sb4 = new StringBuilder();

                    sb4.Append("-mode ");
                    sb4.Append(selectedTagMode.ToString().ToLower());
                    //sb4.Append(' ');

                    return sb4.ToString();
                };

            Func<string> funcTagValuesOption = () =>
                {
                    StringBuilder sb5 = new StringBuilder();

                    if (String.IsNullOrEmpty(title) == false)
                    {
                        sb5.Append("-title=");
                        sb5.Append('"');
                        sb5.Append(title);
                        sb5.Append('"');
                        sb5.Append(' ');
                    }

                    if (String.IsNullOrEmpty(position) == false)
                    {
                        sb5.Append("-position=");
                        sb5.Append('"');
                        sb5.Append(position);
                        sb5.Append('"');
                        sb5.Append(' ');
                    }

                    if (String.IsNullOrEmpty(artistsTrack) == false)
                    {
                        sb5.Append("-tartist=");
                        sb5.Append('"');
                        sb5.Append(artistsTrack);
                        sb5.Append('"');
                        sb5.Append(' ');
                    }

                    if (String.IsNullOrEmpty(genres) == false)
                    {
                        sb5.Append("-genres=");
                        sb5.Append('"');
                        sb5.Append(genres);
                        sb5.Append('"');
                        sb5.Append(' ');
                    }

                    if (String.IsNullOrEmpty(album) == false)
                    {
                        sb5.Append("-album=");
                        sb5.Append('"');
                        sb5.Append(album);
                        sb5.Append('"');
                        sb5.Append(' ');
                    }

                    if (String.IsNullOrEmpty(albumArtists) == false)
                    {
                        sb5.Append("-rartist=");
                        sb5.Append('"');
                        sb5.Append(albumArtists);
                        sb5.Append('"');
                        sb5.Append(' ');
                    }

                    if (String.IsNullOrEmpty(year) == false)
                    {
                        sb5.Append("-year=");
                        sb5.Append('"');
                        sb5.Append(year);
                        sb5.Append('"');
                        sb5.Append(' ');
                    }

                    if (String.IsNullOrEmpty(cover) == false)
                    {
                        sb5.Append("-image=");
                        sb5.Append('"');
                        sb5.Append(cover);
                        sb5.Append('"');
                        sb5.Append(' ');
                    }

                    return sb5.ToString();
                };

            //Func<string> funcTargetOption = () =>
            //    {
            //        string output = null;

            //        switch (selectedTarget)
            //        {
            //            case DESTINATION_TYPE.FILE:
            //                output = "-target file";
            //                break;
            //            case DESTINATION_TYPE.FOLDER:
            //                output = "-target folder";
            //                break;
            //        }

            //        return output;
            //    };

            Func<string> funcPatternOption = () =>
            {
                StringBuilder sb6 = new StringBuilder();

                sb6.Append("-pattern=");
                sb6.Append('"');
                sb6.Append(pattern);
                sb6.Append('"');

                return sb6.ToString() ;
            };

            Func<string> funcRefTagOption = () =>
            {
                StringBuilder sb7 = new StringBuilder();

                sb7.Append("-ref=");
                sb7.Append('"');
                sb7.Append(refTag);
                sb7.Append('"');

                return sb7.ToString();
            };


            StringBuilder sb = new StringBuilder();

            sb.Append(EXECUTABLE_NAME);
            sb.Append(' ');

            switch (selectedAction)
            {
                case ACTION.TAG:
                    sb.Append("tag ");
                    sb.Append(funcDestinationOption());
                    sb.Append(' ');
                    sb.Append(funcTagModeOption());
                    sb.Append(' ');
                    sb.Append(funcTagKeysOption());
                    sb.Append(' ');
                    if (selectedTagMode == TAG_MODE.LASTFM)
                        sb.Append(funcLastFmOption());
                    else
                        sb.Append(funcTagValuesOption());
                    sb.Append(' ');
                    break;
                case ACTION.RENAME:
                    sb.Append("rename ");
                    sb.Append(funcDestinationOption());
                    sb.Append(' ');
                    sb.Append(funcPatternOption());
                    sb.Append(' ');
                    if (selectedDestinationType == DESTINATION_TYPE.FOLDER)
                    {
                        sb.Append(funcRefTagOption());
                        sb.Append(' ');
                    }
                    break;
                case ACTION.READ:
                    sb.Append("read ");
                    sb.Append(funcDestinationOption());
                    sb.Append(' ');
                    break;
            }

            Preview = sb.ToString();

        }

        void CheckAllTag()
        {
            IncludeGenres = true;
            IncludeMbId = true;
            IncludePicture = true;
            IncludePosition = true;
            IncludeRelease = true;
            IncludeReleaseArtist = true;
            IncludeTrackArtist = true;
            IncludeTitle = true;
            IncludeYear = true;
        }

        void StartGenerateCommandLine()
        {
            mre.Set();    
        }

        void GenerateCommandLineTask()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    mre.WaitOne();

                    //if (token.IsCancellationRequested)
                    //    break;

                    UpdatePreview();

                    Thread.Sleep(3000);

                }
            }, TaskCreationOptions.LongRunning);    
        }

        void StopGenerateCommandLine()
        {
            //token.Cancel();
            mre.Reset();
        }
        
        #endregion

        #region Commands

        RelayCommand selectDestinationCommand;
        RelayCommand updatePreviewCommand;
        RelayCommand checkAllTagCommand;
        RelayCommand selectModelTagCommand;
        RelayCommand startGenerateCommandLineCommand;
        RelayCommand stopGenerateCommandLineCommand;
        RelayCommand <string>addToPatternCommand;

        public ICommand AddToPatternCommand
        {
            get { return addToPatternCommand ?? (addToPatternCommand = new RelayCommand<string>(AddToPattern)); }
        }

        public ICommand CheckAllTagCommand
        {
            get { return checkAllTagCommand ?? (checkAllTagCommand = new RelayCommand(CheckAllTag)); }
        }

        public ICommand SelectDestinationCommand
        {
            get { return selectDestinationCommand ?? (selectDestinationCommand = new RelayCommand(SelectDestination)); }
        }

        public ICommand UpdatePreviewCommand
        {
            get { return updatePreviewCommand ?? (updatePreviewCommand = new RelayCommand(UpdatePreview)); }
        }

        public ICommand SelectModelTagCommand
        {
            get { return selectModelTagCommand ?? (selectModelTagCommand = new RelayCommand(SelectModelTag)); }
        }

        public ICommand StartGenerateCommandLineCommand
        {
            get { return startGenerateCommandLineCommand ?? (startGenerateCommandLineCommand = new RelayCommand(StartGenerateCommandLine)); }
        }

        public ICommand StopGenerateCommandLineCommand
        {
            get { return stopGenerateCommandLineCommand ?? (stopGenerateCommandLineCommand = new RelayCommand(StopGenerateCommandLine)); }
        }
        
        #endregion
    }
}
