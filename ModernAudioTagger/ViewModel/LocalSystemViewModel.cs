using MicroMvvm;
using Microsoft.Practices.Unity;
using ModernAudioTagger.BusinessLogic;
using ModernAudioTagger.Model;
using ModernAudioTagger.Provider;
using ModernAudioTagger.ViewModelElement;
using ModernUILogViewer.BusinessLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UltimateMusicTagger;
using UltimateMusicTagger.Model;

namespace ModernAudioTagger.ViewModel
{
    public class LocalSystemViewModel : BaseViewModel    
    {
        #region Constructor

        public LocalSystemViewModel()
        {
            SelectedFiles = new System.Collections.ObjectModel.ObservableCollection<FileVM>();
            Files = new ObservableCollectionExt<FileVM>();
            //SelectedPath = @"C:\Users\Fabrizio\Music\Artists\Foo Fighters\[2011] - Wasting Light";
            SelectedExtension = Filters[0];
            //RefreshFilelist();          

            IncludeTitle = true;
            IncludePosition = true;
            IncludeTrackArtist = true;
            IncludeGenres = true;
            IncludeRelease = true;
            IncludeReleaseArtist = true;
            IncludeYear = true;
            IncludePicture = true;
            IncludeMbId = true;

            prop = new MvvmFoundation.Wpf.PropertyObserver<LocalSystemViewModel>(this);

            prop.RegisterHandler(n => n.SelectedPath, n => { RefreshFilelist(); });
        }
        
        #endregion

        #region Events

        public event EventHandler OnFilelistUpdatedEvent;        
        public event EventHandler OnTagErrorEvent;
        //public event EventHandler OnExecutingTagging;
        
        #endregion

        #region Fields

        private readonly MvvmFoundation.Wpf.PropertyObserver<LocalSystemViewModel> prop;
        
        #endregion

        #region Properties

        public readonly string[] filters = new string[]
        {
            "*.3gp|*.act|*.AIFF*.|*.aac*.|*.ALAC*.|*.amr*.|*.atrac*.|*.Au*.|*.awb*.|*.dct*.|*.dss*.|*.dvf*.|*.flac*.|*.gsm*.|*.iklax*.|*.IVS*.|*.m4a*.|*.m4p*.|*.mmf*.|*.mp3*.|*.mpc*.|*.msv*.|*.ogg*.|*.Opus*.|*.ra*.|*.raw*.|*.TTA*.|*.vox*.|*.wav*.|*.wma*.|*.wv",
            "*.*"
        };

        public string[] Filters
        {
            get { return filters; }
        }

        public System.Collections.ObjectModel.ObservableCollection<FileVM> SelectedFiles { get; set; }

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

        private bool isPanelFrozen;

        public bool IsPanelFrozen
        {
            get { return isPanelFrozen; }
            set
            {
                isPanelFrozen = value;
                RaisePropertyChanged(() => IsPanelFrozen);
            }
        }

        private ModelTagVM selectedModelTag;

        public ModelTagVM SelectedModelTag
        {
            get { return selectedModelTag; }
            set { 
                selectedModelTag = value;
                RaisePropertyChanged(() => SelectedModelTag);
            }
        }
        

        private string selectedExtension;

        public string SelectedExtension
        {
            get { return selectedExtension; }
            set { 
                selectedExtension = value;
                RaisePropertyChanged(() => SelectedExtension);
            }
        }

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

        private string selectedPath;

        public string SelectedPath
        {
            get { return selectedPath; }
            set { 
                selectedPath = value;
                RaisePropertyChanged(() => SelectedPath);
            }
        }


        private ObservableCollectionExt<FileVM> files;

        public ObservableCollectionExt<FileVM> Files
        {
            get { return files; }
            set { 
                files = value;
                RaisePropertyChanged(() => Files);
            }
        }

        #endregion

        #region Methods

        void DragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0)
                {
                    e.Effects = DragDropEffects.All;                    
                }
            }
        }

        void DropFiles(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0)
                {
                    SelectedPath = Path.GetDirectoryName(files[0]);
                }
            }
        }
        
        void OpenFolderDialog()
        {
            IUnityContainer container = UnityContainerProvider.Instance;
            IDialogService service = container.Resolve<IDialogService>(ProviderServiceConstants.DLG_OPEN_PATH);

            string path = service.OpenPath(selectedPath);

            if (String.IsNullOrEmpty(path) == false)
            {
                SelectedPath = path;

                //RefreshFilelist();
            }            
        }

        

        IEnumerable<FileVM> GetFilelist(string path, string filter)
        {
            IList<FileVM> output = null;

            string[] files = Utility.GetFiles(path, filter, SearchOption.TopDirectoryOnly);

            output = files.Select((filename) => { return new FileVM { FileName = filename }; }).ToList<FileVM>();

            return output;
        }

        string NormalizeFilterPath(string filter)
        {
            string result = null;

            if (String.IsNullOrEmpty(filter) == false)
            {
                string pattern = "[\\.\\*]*";
                string replacement = String.Empty;
                Regex rgx = new Regex(pattern);
                result = rgx.Replace(filter, replacement);
            }

            result = String.IsNullOrEmpty(result) ? "*" : "*." + result;

            return result;
        }

        bool CanRefreshFilelist()
        {
            return String.IsNullOrEmpty(selectedPath) == false && Directory.Exists(selectedPath);
        }

        void RefreshFilelist()
        {
            //ViewModelLocator.Instance.MusicDbVM.ListDbResult.All((o) => { o.IsSelected = false; return true; });

            if (String.IsNullOrEmpty(selectedPath) == false && String.IsNullOrEmpty(selectedExtension) == false && Directory.Exists(selectedPath))
            {
                string path = selectedPath;

                string filter = selectedExtension;

                Files.Clear();

                Files.AddRange(GetFilelist(path, filter));

                //SelectedPath = path;

                RaisePropertyChanged(() => Files);

                Status = String.Format("{0} files", files.Count);
            }

            RaiseEventInvoker(OnFilelistUpdatedEvent);
            
        }

        void OpenPathLocation()
        {
            var runExplorer = new System.Diagnostics.ProcessStartInfo();
            runExplorer.FileName = "explorer.exe";
            runExplorer.Arguments = selectedPath;
            System.Diagnostics.Process.Start(runExplorer); 
        }


        bool CanTagSelectedFiles(IEnumerable<FileVM> selectedFiles)
        {
            return selectedFiles != null && selectedFiles.Count() > 0;
        }

        void TagSelectedFiles(IEnumerable<FileVM> selectedFiles)
        {
            int success = 0;
            int failure = 0;

            UltimateMusicTagger.UltiMp3Tagger umtagger = UltimateMusicTaggerLocator.Instance.UMTagger;

            IUnityContainer container = UnityContainerProvider.Instance;            
            IWindowService processingService = container.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_PROCESSING_TAG);

            // clear cache
            umtagger.UnqueueMessages();

            UltimateMusicTagger.TAG_FIELDS tagFields = GetTagFields();

            if ((tagFields & TAG_FIELDS.MUSICBRAINZ_ID) == TAG_FIELDS.MUSICBRAINZ_ID) tagFields ^= TAG_FIELDS.MUSICBRAINZ_ID;

            if (selectedFiles.Count() > 1)
            {
                if ((tagFields & TAG_FIELDS.TITLE) == TAG_FIELDS.TITLE) tagFields ^= TAG_FIELDS.TITLE;
                if ((tagFields & TAG_FIELDS.TRACK_POS) == TAG_FIELDS.TRACK_POS) tagFields ^= TAG_FIELDS.TRACK_POS;
            }

            ModelTag modelTag = SelectedModelTag.GetModelTag();

            RootDispatcher.BeginInvoke((Action)delegate()
            {
                processingService.ShowDialog();
            });               

            Task.Factory.StartNew(() =>
            {
                //IsProcessing = true;
                
                selectedFiles.ToList<FileVM>().ForEach((o) =>
                {
                    try
                    {
                        umtagger.TagFile(o.FileName, modelTag, tagFields);

                        success++;
                    }
                    catch (Exception)
                    {
                        failure++;
                    }
                });
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

                
            });

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

        bool CanShowFileTag(IEnumerable<FileVM> list)
        {
            return list.Count() > 0;
        }


        void ShowFileTag(IEnumerable<FileVM> list)
        {
            string filename = list.Last().FileName;

            Status = Path.GetFileName(filename);

            if (isPanelFrozen == false)
            {
                //ItemTypeSelected = ITEM_TYPE_SELECTED.FILE;

                try
                {
                    TrackInfo output = UltimateMusicTaggerLocator.Instance.UMTagger.GetTag(filename);

                    SelectedModelTag = new ModelTagVM(Utility.GetModelTag(output));
                }
                catch (Exception)
                {
                    Status = "Error while retrieving tag from file";
                }
            }
        }

        //void SyncronizePath()
        //{
        //    SelectedPath = ViewModelLocator.Instance.RenameVM.SelectedPath;

        //    RefreshFilelist();
        //}

        //bool CanExecuteSyncronizePath()
        //{
        //    return String.IsNullOrEmpty(ViewModelLocator.Instance.RenameVM.SelectedPath) == false;
        //}

        void RefreshSelectedPath(DefaultEventArgs args)
        {
            SelectedPath = args.ObjArg.ToString();
        }

        #endregion

        #region Commands
                
        RelayCommand openFolderDialogCommand;
        RelayCommand refreshFilelistCommand;
        //RelayCommand selectionChangedCommand;
        RelayCommand<IEnumerable<FileVM>> showFileTagCommand;
        RelayCommand toggleFreezePanelCommand;
        RelayCommand<IEnumerable<FileVM>> tagSelectedFilesCommand;
        RelayCommand<DefaultEventArgs> refreshSelectedPathCommand;
        RelayCommand<DragEventArgs> dragEnterCommand;
        RelayCommand<DragEventArgs> dropFilesCommand;

        public ICommand DragEnterCommand 
        {
            get { return dragEnterCommand ?? (dragEnterCommand = new RelayCommand<DragEventArgs>(DragEnter)); }
        }

        public ICommand DropFilesCommand
        {
            get { return dropFilesCommand ?? (dropFilesCommand = new RelayCommand<DragEventArgs>(DropFiles)); }
        }


        public ICommand TagSelectedFilesCommand
        {
            get { return tagSelectedFilesCommand ?? (tagSelectedFilesCommand = new RelayCommand<IEnumerable<FileVM>>(TagSelectedFiles, CanTagSelectedFiles)); }
        }

        public ICommand ToggleFreezePanelCommand
        {
            get { return toggleFreezePanelCommand ?? (toggleFreezePanelCommand = new RelayCommand(() => { IsPanelFrozen = !isPanelFrozen; })); }
        }

        public ICommand ShowFileTagCommand
        {
            get { return showFileTagCommand ?? (showFileTagCommand = new RelayCommand<IEnumerable<FileVM>>(ShowFileTag, CanShowFileTag)); }
        }

        public ICommand OpenFolderDialogCommand
        {
            get { return openFolderDialogCommand ?? (openFolderDialogCommand = new RelayCommand(OpenFolderDialog)); }
        }

        public ICommand RefreshFilelistCommand
        {
            get { return refreshFilelistCommand ?? (refreshFilelistCommand = new RelayCommand(RefreshFilelist, CanRefreshFilelist)); }
        }

        public ICommand RefreshSelectedPathCommand
        {
            get { return refreshSelectedPathCommand ?? (refreshSelectedPathCommand = new RelayCommand<DefaultEventArgs>(RefreshSelectedPath)); }
        }

        #endregion
    }
}
