using MicroMvvm;

using ModernAudioTagger.BusinessLogic;
using ModernAudioTagger.Helpers;
using ModernAudioTagger.Model;
using ModernAudioTagger.Provider;
using ModernAudioTagger.ViewModelElement;
using ModernUILogViewer.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UltimateMusicTagger;
using UltimateMusicTagger.Model;
using Unity;

namespace ModernAudioTagger.ViewModel
{
    public class TrackTagViewModel : BaseViewModel
    {

        #region Constructor

        public TrackTagViewModel()
        {
            IncludeTitle = true;
            IncludePosition = true;
            IncludeTrackArtist = true;
            IncludeGenres = true;
            IncludeRelease = true;
            IncludeReleaseArtist = true;
            IncludeYear = true;
            IncludePicture = true;
            IncludeMbId = true;

            //prop = new MvvmFoundation.Wpf.PropertyObserver<TrackTagViewModel>(this);
            //prop.RegisterHandler(n => n.SelectedFiles, n => {  });
        }

        #endregion

        #region Enums

        public enum ITEM_TYPE_SELECTED { FILE, TRACK }

        public enum TAG_TYPE_SELECTED { SELECTION, BY_NAME, BY_NUMBER }
        
        #endregion

        #region Events

        public event EventHandler OnTagErrorEvent;
        
        #endregion

        #region Fields

        private TAG_TYPE_SELECTED tagTypeSelected; 

        #endregion

        #region Properties

        //private bool isProcessing;

        //public bool IsProcessing
        //{
        //    get { return isProcessing; }
        //    set
        //    {
        //        isProcessing = value;
        //        RaisePropertyChanged(() => IsProcessing);
        //    }
        //}
        

        private string status;

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                RaisePropertyChanged(() => Status);
            }
        }

        private ITEM_TYPE_SELECTED itemTypeSelected;

        public ITEM_TYPE_SELECTED ItemTypeSelected
        {
            get { return itemTypeSelected; }
            set { 
                itemTypeSelected = value;
                RaisePropertyChanged(() => ItemTypeSelected);
            }
        }

        private string imagePath;

        public string ImagePath
        {
            get { return imagePath; }
            set { 
                imagePath = value;
                RaisePropertyChanged(() => ImagePath);
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

        private ModelTagVM selectedModelTag;

        public ModelTagVM SelectedModelTag
        {
            get { return selectedModelTag; }
            set
            {
                selectedModelTag = value;
                RaisePropertyChanged(() => SelectedModelTag);
            }
        }

        public IList<TrackInfo> TracksToTag { get; set; }

        public IEnumerable<FileVM> FilesToTag { get; set; }

        #endregion

        #region Methods

        bool CanShowTrackTag(IEnumerable<TrackInfoVM> list)
        {
            return list.Count() > 0;
        }

        void ShowTrackTag(IEnumerable<TrackInfoVM> list)
        {
            ItemTypeSelected = ITEM_TYPE_SELECTED.TRACK;

            TrackInfoVM selectedTagTrack = list.Last();

            SelectedModelTag = new ModelTagVM(Utility.GetModelTag(selectedTagTrack));

            ImagePath = Utility.GetImagePathFromReleases(selectedTagTrack.Releases);

            Status = String.Format("{0} ({1})", selectedTagTrack.Title, 
                Utility.GetTimeFormattedFromTrackLength(selectedTagTrack.Length));
        }

        bool CanRemovePicture(ModelTagVM input)
        {
            bool result = false;

            if (input != null && input.Picture != null)
                result = true;

            return result;
        }

        bool CanExportPicture(ModelTagVM input)
        {
            bool result = false;

            if (input != null && input.Picture != null)
                result = true;

            return result;
        }

        bool CanImportPicture(ModelTagVM input)
        {
            bool result = false;

            if (input != null)
                result = true;

            return result;
        }

        void RemovePicture(ModelTagVM input)
        {
            input.Picture = null;
        }

        void ExportPicture(ModelTagVM input)
        {
            IUnityContainer container = UnityContainerProvider.Instance;
            IDialogService service = container.Resolve<IDialogService>(ProviderServiceConstants.DLG_SAVE_IMAGE_PATH);

            SaveDialogResult result = service.SaveFile();

            string filename = result.Filename;

            if (String.IsNullOrEmpty(filename) == false)
            {
                System.Drawing.Imaging.ImageFormat format = null;

                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs = System.IO.File.Open(filename, System.IO.FileMode.Create);
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.
                switch (result.FilterIndex)
                {
                    case 1:
                        format = ImageFormat.Jpeg;
                        break;
                    case 2:
                        format = ImageFormat.Bmp;
                        break;
                    case 3:
                        format = ImageFormat.Gif;
                        break;
                }

                input.Picture.Save(fs, format);

                fs.Close();
            }
        }

        void ImportPicture(ModelTagVM input)
        {
            IUnityContainer container = UnityContainerProvider.Instance;
            IDialogService service = container.Resolve<IDialogService>(ProviderServiceConstants.DLG_LOAD_IMAGE_PATH);

            string[] files = service.OpenFile(false);

            if (files.Length > 0)
            {
                input.Picture = System.Drawing.Image.FromFile(files[0]);
            }
        }

        public TAG_FIELDS GetTagFields()
        {
            TAG_FIELDS tagFields = TAG_FIELDS.NONE;

            if (includeRelease) tagFields |= TAG_FIELDS.ALBUM;
            if (includeReleaseArtist) tagFields |= TAG_FIELDS.ALBUM_ARTIST;
            if (includeTrackArtist) tagFields |= TAG_FIELDS.TRACK_ARTIST;
            if (includeTitle) tagFields |= TAG_FIELDS.TITLE;
            if (includePosition) tagFields |= TAG_FIELDS.TRACK_POS;
            if (includeYear) tagFields |= TAG_FIELDS.YEAR;
            if (includeGenres) tagFields |= TAG_FIELDS.GENRES;
            if (includePicture) tagFields |= TAG_FIELDS.IMAGE;
            if (includeMbId) tagFields |= TAG_FIELDS.MUSICBRAINZ_ID;

            return tagFields;
        }

        

        void ShowFileTag(IEnumerable<FileVM> list)
        {
            ItemTypeSelected = ITEM_TYPE_SELECTED.FILE;

            try
            {
                string filename = list.Last().FileName;

                TrackInfo output = UltimateMusicTaggerLocator.Instance.UMTagger.GetTag(list.Last().FileName);

                SelectedModelTag = new ModelTagVM(Utility.GetModelTag(output));

                Status = Path.GetFileName( filename );
            }
            catch (Exception)
            {
                Status = "Error while retrieving tag from file";
            }
        }

        void TagBySelection()
        {
            TAG_FIELDS tagFields = GetTagFields();

            IUnityContainer container = UnityContainerProvider.Instance;
            IWindowService processingService = container.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_PROCESSING_TAG);
            
            LocalSystemViewModel localSystemVM = ViewModelLocator.Instance.LocalSystemVM;
            MusicDbViewModel musicDbVM = ViewModelLocator.Instance.MusicDbVM;

            IList<FileVM> files = localSystemVM.Files.Where((o) => { return o.IsSelected == true; }).ToList();
            IList<TrackInfoVM> tracks = musicDbVM.ListDbResult.Where((o) => { return o.IsSelected == true; }).ToList();

            UltimateMusicTagger.UltiMp3Tagger umtagger = UltimateMusicTaggerLocator.Instance.UMTagger;

            // clear cache message
            umtagger.UnqueueMessages();

            int success = 0;
            int failure = 0;

            Image picture = null;

            int index = 0;

            //processingService.ShowDialog();
            
            RootDispatcher.BeginInvoke((Action)delegate()
            {
                processingService.ShowDialog();
            });      

            Task.Factory.StartNew(() =>
            {
                //IsProcessing = true;

                foreach (TrackInfoVM trackInfo in tracks)
                {
                    string[] albumartists = null;
                    string albumtitle = null;
                    string releasembid = null;
                    uint year = 0;

                    ReleaseInfo releaseInfo = null;

                    if (trackInfo.Releases.Length > 0)
                    {
                        releaseInfo = trackInfo.Releases[0];
                        albumartists = releaseInfo.Artists != null ? releaseInfo.Artists.Select(a => a.Name).ToArray() : null;
                        albumtitle = releaseInfo.Title;
                        releasembid = releaseInfo.Mbid;
                        year = Utility.GetYearFromDate(releaseInfo.Year);
                        if (picture == null && String.IsNullOrEmpty(releaseInfo.ImagePath) == false)
                            picture = MTUtility.ImageFromUri(releaseInfo.ImagePath, System.Net.WebRequest.DefaultWebProxy);

                    }

                    try
                    {
                        ModelTag input = new ModelTag
                        {
                            Title = trackInfo.Title,
                            Album = albumtitle,
                            AlbumArtists = albumartists,
                            Genres = null,
                            ArtistMbid = trackInfo.Artists != null ? trackInfo.Artists[0].Mbid : null,
                            Picture = picture,
                            ReleaseMbid = releasembid,
                            TrackMbid = trackInfo.Mbid,
                            Position = trackInfo.Track,
                            Year = year,
                            TrackArtists = trackInfo.Artists != null ? trackInfo.Artists.Select(a => a.Name).ToArray() : null,
                        };

                        umtagger.TagFile(files[index].FileName, input, tagFields);

                        index++;

                        success++;
                    }
                    catch (Exception e)
                    {
                        failure++;
                    }
                }

            }).ContinueWith((o) =>
            {

                RootDispatcher.Invoke((Action)delegate()
                {
                    //IsProcessing = false;

                    processingService.CloseDialog();

                    Status = String.Format("Tag operation completed. {0} success, {1} failures", success, failure);

                    if (failure > 0)
                    {
                        RaiseEventInvoker(OnTagErrorEvent);

                        UltimateMusicTagger.UMTMessage[] messages = umtagger.UnqueueMessages();

                        ViewModelLocator.Instance.MainVM.Logs = messages;
                        
                        IWindowService logService = container.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_LOG_VIEWER);
                        logService.ShowDialog();
                    }

                });

                //processingService.CloseDialog();

                

            });
            
        }

        bool CanExecuteTag()
        {
            int filesCount = ViewModelLocator.Instance.LocalSystemVM.Files.Count((o) => { return o.IsSelected; });

            int tracksCount = ViewModelLocator.Instance.MusicDbVM.ListDbResult.Count;

            return tracksCount > 0 && filesCount > 0;
        }

        void TagByMatchingField(Func<string, IEnumerable<TrackInfo>, TrackInfo> comparer)
        {
            int success = 0;
            int failure = 0;
            int skipped = 0;

            TAG_FIELDS tagFields = GetTagFields();

            Image picture = null;

            IUnityContainer container = UnityContainerProvider.Instance;
            IWindowService processingService = container.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_PROCESSING_TAG);

            LocalSystemViewModel localSystemVM = ViewModelLocator.Instance.LocalSystemVM;
            MusicDbViewModel musicDbVM = ViewModelLocator.Instance.MusicDbVM;

            UltiMp3Tagger umtagger = UltimateMusicTaggerLocator.Instance.UMTagger;

            // clear messages cache
            umtagger.UnqueueMessages();

            IEnumerable<FileVM> files = localSystemVM.Files.Where( (o) => { return o.IsSelected;} );
            IEnumerable<TrackInfo> tracks = musicDbVM.ListDbResult.Select( (o) =>
            {
                return new TrackInfo
                {
                    Releases = o.Releases,
                    Artists = o.Artists,
                    Mbid = o.Mbid,
                    Title = o.Title,
                    Track = o.Track
                };
            }
            );
            
            RootDispatcher.BeginInvoke((Action)delegate()
            {
                processingService.ShowDialog();
            });

            Task.Factory.StartNew(() =>
            {

                foreach (FileVM fileVM in files)
                {
                    string filenameWithoutExtension = Path.GetFileNameWithoutExtension(fileVM.FileName);

                    TrackInfo trackInfo = comparer(filenameWithoutExtension, tracks);

                    string[] albumartists = null;
                    string albumtitle = null;
                    string releasembid = null;
                    uint year = 0;

                    ReleaseInfo releaseInfo = null;

                    if (trackInfo != null)
                    {
                        if (trackInfo.Releases.Length > 0)
                        {
                            releaseInfo = trackInfo.Releases[0];
                            albumartists = releaseInfo.Artists != null ? releaseInfo.Artists.Select(a => a.Name).ToArray() : null;
                            albumtitle = releaseInfo.Title;
                            releasembid = releaseInfo.Mbid;
                            year = Utility.GetYearFromDate(releaseInfo.Year);
                            if (picture == null && String.IsNullOrEmpty(releaseInfo.ImagePath) == false)
                                picture = MTUtility.ImageFromUri(releaseInfo.ImagePath, System.Net.WebRequest.DefaultWebProxy);

                        }

                        ModelTag input = new ModelTag
                        {
                            Title = trackInfo.Title,
                            Album = albumtitle,
                            AlbumArtists = albumartists,
                            Genres = null,
                            ArtistMbid = trackInfo.Artists != null ? trackInfo.Artists[0].Mbid : null,
                            Picture = picture,
                            ReleaseMbid = releasembid,
                            TrackMbid = trackInfo.Mbid,
                            Position = trackInfo.Track,
                            Year = year,
                            TrackArtists = trackInfo.Artists != null ? trackInfo.Artists.Select(a => a.Name).ToArray() : null,
                        };

                        try
                        {
                            umtagger.TagFile(fileVM.FileName, input, tagFields);

                            success++;
                        }
                        catch (Exception)
                        {
                            failure++;
                        }
                    }
                    else
                        skipped++;
                }

            }).ContinueWith((o) =>
            {
                RootDispatcher.Invoke((Action)delegate()
                {
                    processingService.CloseDialog();

                    ShowFileTag(files);

                    Status = String.Format("Tag operation completed. {0} success, {1} failures, {2} skipped", success, failure, skipped);

                    if (failure > 0)
                    {
                        RaiseEventInvoker(OnTagErrorEvent);

                        UltimateMusicTagger.UMTMessage[] messages = umtagger.UnqueueMessages();

                        ViewModelLocator.Instance.MainVM.Logs = messages;
                        
                        IWindowService logService = container.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_LOG_VIEWER);                        
                        logService.ShowDialog();
                    }
                });                
            });

            
            
        }

        void TagByMatchingName()
        {
            TagByMatchingField(MTUtility.GetTrackInfoByLevenshteinDistance);
        }

        void PreviewTagByMatchingField(Func<string, IEnumerable<TrackInfo>, TrackInfo> comparer)
        {
            int success = 0;
            int failure = 0;
            int skipped = 0;

            TAG_FIELDS tagFields = GetTagFields();

            LocalSystemViewModel localSystemVM = ViewModelLocator.Instance.LocalSystemVM;
            MusicDbViewModel musicDbVM = ViewModelLocator.Instance.MusicDbVM;

            IEnumerable<FileVM> files = localSystemVM.Files.Where((o) => { return o.IsSelected; });
            IEnumerable<TrackInfo> tracks = musicDbVM.ListDbResult.Select((o) =>
            {
                return new TrackInfo
                {
                    Releases = o.Releases,
                    Artists = o.Artists,
                    Mbid = o.Mbid,
                    Title = o.Title,
                    Track = o.Track
                };
            }
            );

            TracksToTag = new List<TrackInfo>();

            foreach (FileVM fileVM in files)
            {                
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(fileVM.FileName);

                TrackInfo trackInfo = comparer(filenameWithoutExtension, tracks);

                if (trackInfo != null)
                {
                    TracksToTag.Add(trackInfo);

                }
                else
                {
                    TracksToTag.Add(null);

                    skipped++;
                }
            }

            FilesToTag = files;

            RaisePropertyChanged(() => FilesToTag);
            RaisePropertyChanged(() => TracksToTag);
        }

        void PreviewTagByMatchingNumber()
        {
            tagTypeSelected = TAG_TYPE_SELECTED.BY_NUMBER;

            PreviewTagByMatchingField(MTUtility.GetTrackInfoByPosition);

            IWindowService wservice = Provider.UnityContainerProvider.Instance.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_PREVIEW_TAG);

            wservice.ShowDialog();
        }

        void PreviewTagBySelection()
        {
            tagTypeSelected = TAG_TYPE_SELECTED.SELECTION;

            LocalSystemViewModel localSystemVM = ViewModelLocator.Instance.LocalSystemVM;
            MusicDbViewModel musicDbVM = ViewModelLocator.Instance.MusicDbVM;

            FilesToTag = localSystemVM.Files.Where((o) => { return o.IsSelected == true; });
            TracksToTag = musicDbVM.ListDbResult.Where((o) => { return o.IsSelected == true; }).Select( (o) => 
            {
                return new TrackInfo { Track = o.Track, Title = o.Title };
            }).ToList();

            IWindowService wservice = Provider.UnityContainerProvider.Instance.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_PREVIEW_TAG);

            wservice.ShowDialog();

        }

        void PreviewTagByMatchingName()
        {
            tagTypeSelected = TAG_TYPE_SELECTED.BY_NAME;

            PreviewTagByMatchingField(MTUtility.GetTrackInfoByJaroWinklerDistance);

            IWindowService wservice = Provider.UnityContainerProvider.Instance.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_PREVIEW_TAG);

            wservice.ShowDialog();
        }

        void TagByMatchingNumber()
        {
            TagByMatchingField(MTUtility.GetTrackInfoByPosition);
        }

        void ShowReleaseTag(ReleaseInfo release)
        {
            if (release != null)
            {
                ItemTypeSelected = ITEM_TYPE_SELECTED.TRACK;

                SelectedModelTag = new ModelTagVM(Utility.GetModelTag(release));

                ImagePath = release.ImagePath;
            }
        }

        void ShowSingleTrackTag(TrackInfo track)
        {
            if (track != null)
            {
                ItemTypeSelected = ITEM_TYPE_SELECTED.TRACK;

                SelectedModelTag = new ModelTagVM(Utility.GetModelTag(track));
            }
        }

        void StartTag()
        {
            switch (tagTypeSelected)
            {
                case TAG_TYPE_SELECTED.SELECTION:
                    TagBySelection();
                    break;
                case TAG_TYPE_SELECTED.BY_NAME:
                    TagByMatchingName();
                    break;
                case TAG_TYPE_SELECTED.BY_NUMBER:
                    TagByMatchingNumber();
                    break;
                default:
                    break;
            }
        }

        void MoveFileUp(IEnumerable<FileVM> input)
        {
            IList<TrackInfoVM> tracks = ViewModelLocator.Instance.MusicDbVM.ListDbResult;

            ObservableCollection<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            IEnumerable<FileVM> list = input.OrderBy((o) => (files.IndexOf(o)));

            list.ToList().ForEach((o) =>
            {
                int oldindex = files.IndexOf(o);

                int newindex = oldindex - 1;

                files.Move(oldindex, newindex);

                files[newindex].IsSelected = false;
                files[oldindex].IsSelected = false;

                if (tracks.Count > oldindex)
                    tracks[oldindex].IsSelected = false;
                if (tracks.Count > newindex)
                    tracks[newindex].IsSelected = false;
            });

            //RaisePropertyChanged(() => Files);
        }

        bool CanMoveFileUp(IEnumerable<FileVM> input)
        {
            bool result = input.Count() > 0;

            IList<FileVM> selectedFiles = ViewModelLocator.Instance.LocalSystemVM.SelectedFiles;

            IList<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            if (result)
            {
                int firstElementIndex = selectedFiles.Min((o) => { return files.IndexOf(o); });

                result = firstElementIndex != 0;
            }

            return result;
        }

        void MoveFileDown()
        {
            IList<TrackInfoVM> tracks = ViewModelLocator.Instance.MusicDbVM.ListDbResult;

            ObservableCollection<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            IList<FileVM> selectedFiles = ViewModelLocator.Instance.LocalSystemVM.SelectedFiles;

            int tracksCount = tracks.Count();

            IEnumerable<FileVM> list = selectedFiles.OrderByDescending((o) => (files.IndexOf(o)));

            list.ToList().ForEach((o) =>
            {
                int oldindex = files.IndexOf(o);

                int newindex = oldindex + 1;

                files.Move(oldindex, newindex);

                files[newindex].IsSelected = false;
                files[oldindex].IsSelected = false;

                if (tracks.Count > oldindex)
                    tracks[oldindex].IsSelected = false;
                if (tracks.Count > newindex)
                    tracks[newindex].IsSelected = false;
            });

            //RaisePropertyChanged(() => Files);
        }

        bool CanMoveFileDown()
        {
            IList<FileVM> selectedFiles = ViewModelLocator.Instance.LocalSystemVM.SelectedFiles;

            IList<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            bool result = selectedFiles.Count > 0;

            if (result)
            {
                int lastElementIndex = selectedFiles.Max((o) => { return files.IndexOf(o); });

                result = lastElementIndex != (files.Count - 1);
            }

            return result;
        }

        void ExlusiveSelectItem(ISelectable input)
        {
            IList<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            Utility.ExclusiveSelectItem(input, files);
        }

        void CheckSelectionChanged(FileVM input)
        {
            IList<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            IList<FileVM> listSource = files;
            IList<TrackInfoVM> list = ModernAudioTagger.Provider.ViewModelLocator.Instance.MusicDbVM.ListDbResult;

            int index = listSource.IndexOf(input);

            if (input.IsSelected == true)
            {
                if (index >= list.Count)
                    input.IsSelected = false;
                else
                    list[index].IsSelected = true;
            }
            else
            {
                if (list.Count > index)
                    list[index].IsSelected = false;
            }

            //RaisePropertyChanged(() => Files);
        }

        void CheckSelectedItems()
        {
            IList<FileVM> selectedFiles = ViewModelLocator.Instance.LocalSystemVM.SelectedFiles;

            Utility.CheckSelectedItems(selectedFiles);
        }

        void OrderFilesByName()
        {
            OrderFiles(UltimateMusicTagger.MTUtility.GetFilenameByLevenshteinDistance);
        }

        bool CanExecuteOrderFilesByName()
        {
            int tracksCount = ViewModelLocator.Instance.MusicDbVM.ListDbResult.Count;

            IList<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            return files.Count > 0 && tracksCount > 0;
        }

        void OrderFilesByNumber()
        {
            OrderFiles(UltimateMusicTagger.MTUtility.GetFilenameByPosition);
        }

        bool CanExecuteOrderFilesByNumber()
        {
            int tracksCount = ViewModelLocator.Instance.MusicDbVM.ListDbResult.Count;

            IList<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            return files.Count > 0 && tracksCount > 0;
        }

        void OrderFiles(Func<string[], TrackInfo, string> func)
        {
            IList<TrackInfo> list = ViewModelLocator.Instance.MusicDbVM.ListDbResult.Select((o) =>
            {
                return new TrackInfo
                {
                    Releases = o.Releases,
                    Artists = o.Artists,
                    Mbid = o.Mbid,
                    Title = o.Title,
                    Track = o.Track
                };
            }).ToList();

            MusicDbViewModel musicDbVM = ViewModelLocator.Instance.MusicDbVM;

            ObservableCollectionExt<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            files.All(x => { x.IsSelected = false; return true; });

            musicDbVM.ListDbResult.ToList().All(x => { x.IsSelected = false; return true; });

            string[] listFilename = files.Select((o) => { return Path.GetFileNameWithoutExtension(o.FileName); }).ToArray();

            IList<FileVM> listfiles = files.ToList();

            int indexTrack = 0;

            foreach (TrackInfo track in list)
            {
                if (indexTrack > (listFilename.Length - 1))
                    break;

                string fileFound = func(listFilename, track);

                int newindex = Array.IndexOf(listFilename, fileFound);

                if (newindex > -1)
                {
                    listfiles.Swap(newindex, indexTrack);

                    string temp = listFilename[newindex]; // temp = "two"
                    listFilename[newindex] = listFilename[indexTrack]; // items[1] = "three"
                    listFilename[indexTrack] = temp; // items[2] = "two"                                                                
                }

                indexTrack++;
            }

            files.Clear();

            foreach (FileVM file in listfiles)
                files.Add(file);

            //files.AddRange(listfiles);

            //RaisePropertyChanged(() => Files);

        }

        bool CanCheckAllItems()
        {
            int tracksCount = ViewModelLocator.Instance.MusicDbVM.ListDbResult.Count;

            IList<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            return files.Count > 0 && tracksCount > 0;
        }

        void CheckAllItems()
        {
            int tracksCount = ViewModelLocator.Instance.MusicDbVM.ListDbResult.Count;

            IList<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            int maxItemsSelectable = Math.Min(files.Count, tracksCount);

            Utility.CheckAllItems(files, maxItemsSelectable);

            //RaisePropertyChanged(() => Files);
        }

        bool CanExecuteCheckSelectedItems()
        {
            int filesCount = ViewModelLocator.Instance.MusicDbVM.ListDbResult.Count;

            bool result = false;

            IList<FileVM> selectedFiles = ViewModelLocator.Instance.LocalSystemVM.SelectedFiles;

            IList<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            IEnumerable<FileVM> sortedList = selectedFiles.OrderBy((o) => files.IndexOf(o));

            if (sortedList.Count() > 0)
            {
                int index = files.IndexOf(sortedList.Last());

                if (index < filesCount)
                    result = true;
            }

            return result;
        }


        bool CanShowFileTag(IEnumerable<FileVM> list)
        {
            return list.Count() > 0;
        }



        #endregion

        #region Commands

        RelayCommand<IEnumerable<TrackInfoVM>> showTrackTagCommand;
        RelayCommand<ModelTagVM> importPictureCommand;
        RelayCommand<ModelTagVM> exportPictureCommand;
        RelayCommand<ModelTagVM> removePictureCommand;        
        RelayCommand<IEnumerable<FileVM>> showFileTagCommand;        
        RelayCommand tagBySelectionCommand;
        RelayCommand tagByMatchingNameCommand;
        RelayCommand tagByMatchingNumberCommand;
        RelayCommand<ReleaseInfo> showReleaseTagCommand;
        RelayCommand<TrackInfo> showSingleTrackTagCommand;
        RelayCommand previewTagByMatchingNumberCommand;
        RelayCommand previewTagByMatchingNameCommand;
        RelayCommand previewTagBySelectionCommand;
        RelayCommand startTagCommand;
        RelayCommand<IEnumerable<FileVM>> moveFileUpCommand;
        RelayCommand moveFileDownCommand;
        RelayCommand<ISelectable> exlusiveSelectItemCommand;
        RelayCommand<FileVM> checkSelectionChangedCommand;
        RelayCommand checkSelectedItemsCommand;
        RelayCommand orderFilesByNameCommand;
        RelayCommand orderFilesByNumberCommand;
        RelayCommand checkAllItemsCommand;


        public ICommand RemovePictureCommand
        {
            get { return removePictureCommand ?? (removePictureCommand = new RelayCommand<ModelTagVM>(RemovePicture, CanRemovePicture)); }
        }

        public ICommand ImportPictureCommand
        {
            get { return importPictureCommand ?? (importPictureCommand = new RelayCommand<ModelTagVM>(ImportPicture, CanImportPicture)); }
        }

        public ICommand ExportPictureCommand
        {
            get { return exportPictureCommand ?? (exportPictureCommand = new RelayCommand<ModelTagVM>(ExportPicture, CanExportPicture)); }
        }

        public ICommand ShowTrackTagCommand
        {
            get { return showTrackTagCommand ?? (showTrackTagCommand = new RelayCommand<IEnumerable<TrackInfoVM>>(ShowTrackTag, CanShowTrackTag)); }
        }

        public ICommand ShowSingleTrackTagCommand
        {
            get { return showSingleTrackTagCommand ?? (showSingleTrackTagCommand = new RelayCommand<TrackInfo>(ShowSingleTrackTag)); }
        }

        public ICommand ShowFileTagCommand
        {
            get { return showFileTagCommand ?? (showFileTagCommand = new RelayCommand<IEnumerable<FileVM>>(ShowFileTag, CanShowFileTag)); }
        }

        public ICommand TagBySelectionCommand
        {
            get { return tagBySelectionCommand ?? (tagBySelectionCommand = new RelayCommand(TagBySelection, CanExecuteTag)); }
        }

        public ICommand TagByMatchingNameCommand
        {
            get { return tagByMatchingNameCommand ?? (tagByMatchingNameCommand = new RelayCommand(TagByMatchingName, CanExecuteTag)); }
        }

        public ICommand TagByMatchingNumberCommand
        {
            get { return tagByMatchingNumberCommand ?? (tagByMatchingNumberCommand = new RelayCommand(TagByMatchingNumber, CanExecuteTag)); }
        }

        public ICommand PreviewTagByMatchingNumberCommand
        {
            get { return previewTagByMatchingNumberCommand ?? (previewTagByMatchingNumberCommand = new RelayCommand(PreviewTagByMatchingNumber, CanExecuteTag)); }
        }

        public ICommand PreviewTagByMatchingNameCommand
        {
            get { return previewTagByMatchingNameCommand ?? (previewTagByMatchingNameCommand = new RelayCommand(PreviewTagByMatchingName, CanExecuteTag)); }
        }

        public ICommand PreviewTagBySelectionCommand
        {
            get { return previewTagBySelectionCommand ?? (previewTagBySelectionCommand = new RelayCommand(PreviewTagBySelection, CanExecuteTag)); }
        }

        public ICommand ShowReleaseTagCommand
        {
            get { return showReleaseTagCommand ?? (showReleaseTagCommand = new RelayCommand<ReleaseInfo>(ShowReleaseTag)); }
        }

        public ICommand StartTagCommand
        {
            get { return startTagCommand ?? (startTagCommand = new RelayCommand(StartTag)); }
        }

        public ICommand ExlusiveSelectItemCommand
        {
            get { return exlusiveSelectItemCommand ?? (exlusiveSelectItemCommand = new RelayCommand<ISelectable>(ExlusiveSelectItem)); }
        }

        public ICommand MoveFileUpCommand
        {
            get { return moveFileUpCommand ?? (moveFileUpCommand = new RelayCommand<IEnumerable<FileVM>>(MoveFileUp, CanMoveFileUp)); }
        }

        public ICommand MoveFileDownCommand
        {
            get { return moveFileDownCommand ?? (moveFileDownCommand = new RelayCommand(MoveFileDown, CanMoveFileDown)); }
        }

        public ICommand CheckSelectionChangedCommand
        {
            get { return checkSelectionChangedCommand ?? (checkSelectionChangedCommand = new RelayCommand<FileVM>(CheckSelectionChanged)); }
        }

        public ICommand CheckSelectedItemsCommand
        {
            get { return checkSelectedItemsCommand ?? (checkSelectedItemsCommand = new RelayCommand(CheckSelectedItems, CanExecuteCheckSelectedItems)); }
        }

        public ICommand OrderFilesByNameCommand
        {
            get { return orderFilesByNameCommand ?? (orderFilesByNameCommand = new RelayCommand(OrderFilesByName, CanExecuteOrderFilesByName)); }
        }

        public ICommand OrderFilesByNumberCommand
        {
            get { return orderFilesByNumberCommand ?? (orderFilesByNumberCommand = new RelayCommand(OrderFilesByNumber, CanExecuteOrderFilesByNumber)); }
        }

        public ICommand CheckAllItemsCommand
        {
            get { return checkAllItemsCommand ?? (checkAllItemsCommand = new RelayCommand(CheckAllItems, CanCheckAllItems)); }
        }
        
        #endregion

    }
}
