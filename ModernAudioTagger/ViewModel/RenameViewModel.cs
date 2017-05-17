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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using UltimateMusicTagger.Model;

namespace ModernAudioTagger.ViewModel
{
    public class RenameViewModel : BaseViewModel
    {

        #region Constructor

        public RenameViewModel()
        {
            //Files = new ObservableCollectionExt<FileVM>();
            SelectedFiles = new System.Collections.ObjectModel.ObservableCollection<FileVM>();
            SelectedTarget = RENAME_TARGET.FILE;
            SelectedPattern = String.Empty;
            UpdatePatterns();
            PreviewPatternResult();
        }

        #endregion

        #region Fields

        const int DELAY_FOR_PREVIEW = 3000;

        ManualResetEvent mre = new ManualResetEvent(false);
        
        #endregion

        #region Enums

        public enum RENAME_TARGET { FILE, FOLDER }

        #endregion

        #region Events

        public event EventHandler OnFilesRenamedEvent;
        public event EventHandler OnRenameErrorEvent;
        public event EventHandler OnFolderRenamedEvent;

        #endregion

        #region Properties

        private string selectedPattern;

        public string SelectedPattern
        {
            get { return selectedPattern; }
            set
            {
                selectedPattern = value;
                RaisePropertyChanged(() => SelectedPattern);
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


        private RENAME_TARGET selectedTarget;

        public RENAME_TARGET SelectedTarget
        {
            get { return selectedTarget; }
            set
            {
                selectedTarget = value;
                RaisePropertyChanged(() => SelectedTarget);
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<FileVM> SelectedFiles
        {
            get;
            set;
        }

        private readonly IDictionary<string, string> filePatterns = new Dictionary<string, string>
        {
            {"--", "[Common examples for file]"},
            {"%p - %t", "Position - Title"},
            {"%aa - %t", "Artist - Title"},
            {"%r - %t", "Album - Title"},
            {"%t - %r - %aa", "Title - Album - Artist"},
            {"(%y) - %r - %aa - %t", "(Year) - Album - Artist - Title"},
        };

        private readonly IDictionary<string, string> folderPatterns = new Dictionary<string, string>
        {
            {"---", "[Common examples for folder]"},
            {"%aa - %r", "Artist - Album"},
            {"%aa - %r (%y)", "Artist - Album (Year)"},
            {"%aa - %r [%y]", "Artist - Album [Year]"},
            {"%r [%y]", "Album [Year]"},
            {"(%y) - %r", "(Year) - Album"},
            {"[%y] - %r", "[Year] - Album"},            
        };

        private readonly IDictionary<string, string> commonPatterns = new Dictionary<string, string>
        {
            {"-", "[Legenda]"},
            {"%r", "%r %R %album - Album"},
            {"%t", "%t %T %title - Title"},
            {"%aa", "%aa %AA %aartist - Album Artist"},            
            {"%ta", "%ta %TA %tartist - Track Artist"},            
            {"%p", "%p %P %pos - Track position"},            
            {"%y", "%y %Y %d %D %year - Year"},            
        };

        private IDictionary<string, string> patterns;

        public IDictionary<string, string> Patterns
        {
            get { return patterns; }
            set
            {
                patterns = value;
                RaisePropertyChanged(() => Patterns);
            }
        }

        private string inputPattern;

        public string InputPattern
        {
            get { return inputPattern; }
            set
            {
                inputPattern = value;
                RaisePropertyChanged(() => InputPattern);
            }
        }

        private string preview;

        public string Preview
        {
            get { return preview; }
            set
            {
                preview = value;
                RaisePropertyChanged(() => Preview);
            }
        }

        void RenameObject()
        {
            string[] filelist = SelectedFiles
                //.Where((o) => { return o.IsSelected; })
                .Select((o) => o.FileName)
                .ToArray();

            UltimateMusicTagger.UltiMp3Tagger umtagger = UltimateMusicTaggerLocator.Instance.UMTagger;

            LocalSystemViewModel localSystemVM = ViewModelLocator.Instance.LocalSystemVM;

            string selectedPath = localSystemVM.SelectedPath;

            UltimateMusicTaggerLocator.Instance.UMTagger.UnqueueMessages();

            IUnityContainer container = UnityContainerProvider.Instance;
            IWindowService processingService = container.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_PROCESSING_TAG);

            RootDispatcher.BeginInvoke((Action)delegate()
            {
                processingService.ShowDialog();
            });

            bool result = false;
            string newpathname = null;

            Task.Factory.StartNew(() =>
            {
                switch (selectedTarget)
                {
                    case RENAME_TARGET.FILE:
                        result = umtagger.RenameFilesByTag(filelist, inputPattern);
                        if (result)
                            Status = String.Format("{0} files renamed", filelist.Length);
                        break;
                    case RENAME_TARGET.FOLDER:                        
                        string oldpath = selectedPath;
                        result = umtagger.RenameFolderByTag(selectedPath, filelist, inputPattern, out newpathname);
                        string message = result ? String.Format("folder renamed to {0}", newpathname) : "error while renaming folder";
                        if (result)
                        {
                            Status = message;
                            //localSystemVM.SelectedPath = newpathname;
                            //if (oldpath.Equals(localSystemVM.SelectedPath))
                            //{
                            //    localSystemVM.SelectedPath = newpathname;
                            //}                            
                        }
                        break;
                }


            }).ContinueWith((t) =>
            {
                RootDispatcher.Invoke((Action)delegate()
                {
                    processingService.CloseDialog();

                    if (t.IsFaulted || result == false)
                    {
                        Status = "Some errors were reported while executing the operation selected";
                        RaiseEventInvoker(OnRenameErrorEvent);

                        UltimateMusicTagger.UMTMessage[] messages = umtagger.UnqueueMessages();

                        ViewModelLocator.Instance.MainVM.Logs = messages;

                        IWindowService service = container.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_LOG_VIEWER);

                        service.ShowDialog();
                    }

                    switch (selectedTarget)
                    {
                        case RENAME_TARGET.FILE:
                            RaiseEventInvoker(OnFilesRenamedEvent);
                            break;
                        case RENAME_TARGET.FOLDER:
                            if (result)
                                RaiseEventInvoker(OnFolderRenamedEvent, new Model.DefaultEventArgs { ObjArg = newpathname });
                            break;
                    }

                });
            });



        }

        bool CanExecuteRenameObject()
        {
            //int filescount = files.Count((o) => { return o.IsSelected; });

            int filescount = SelectedFiles.Count();

            return (filescount > 0
                && String.IsNullOrEmpty(inputPattern) == false);
        }

        #endregion

        #region Methods

        void PreviewPatternResult()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    mre.WaitOne();

                    PreviewPattern();

                    Thread.Sleep(DELAY_FOR_PREVIEW);

                }
            }, TaskCreationOptions.LongRunning);
        }

        public void StartUpdatingPreview()
        {
            mre.Set();
        }

        public void StopUpdatingPreview()
        {
            mre.Reset();
        }

        void UpdatePatterns()
        {
            switch (selectedTarget)
            {
                case RENAME_TARGET.FILE:
                    Patterns = commonPatterns.Concat(filePatterns).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    break;
                case RENAME_TARGET.FOLDER:
                    Patterns = commonPatterns.Concat(folderPatterns).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    break;
            }
        }

        //void SelectAllItems()
        //{
        //    IEnumerable<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

        //    SelectedFiles.Clear();

        //    foreach (FileVM file in files)
        //        SelectedFiles.Add(file);

        //    RaisePropertyChanged(() => SelectedFiles);
        //}

        void AddToInput(Boolean clear)
        {
            if (clear)
                inputPattern = String.Empty;
            StringBuilder sb = new StringBuilder(inputPattern);
            sb.Append(selectedPattern);

            InputPattern = sb.ToString();
        }

        bool CanExecuteAddToInput(Boolean param)
        {
            return (String.IsNullOrEmpty(selectedPattern) == false && selectedPattern.StartsWith("-") == false);
        }

        //void OpenFolderDialog()
        //{
        //    IUnityContainer container = UnityContainerProvider.Instance;
        //    IDialogService service = container.Resolve<IDialogService>(ProviderServiceConstants.DLG_OPEN_PATH);

        //    string selectedPath = localSystemVM.SelectedPath;

        //    string path = service.OpenPath(selectedPath);

        //    if (String.IsNullOrEmpty(path) == false)
        //    {
        //        SelectedPath = path;

        //        RefreshFilelist();
        //    }
        //}

        //public void RefreshFilelist()
        //{
        //    if (String.IsNullOrEmpty(selectedPath) == false && String.IsNullOrEmpty(selectedExtension) == false)
        //    {
        //        string path = selectedPath;

        //        //string filter = NormalizeFilterPath(selectedExtension);

        //        string filter = selectedExtension;

        //        Files.Clear();

        //        Files.AddRange(GetFilelist(path, filter));

        //        SelectedPath = path;

        //        RaisePropertyChanged(() => Files);
        //    }

        //}

        IEnumerable<FileVM> GetFilelist(string path, string filter)
        {
            IList<FileVM> output = null;

            string[] files = Utility.GetFiles(path, filter, SearchOption.TopDirectoryOnly);

            output = files.Select((filename) => { return new FileVM { FileName = filename }; }).ToList<FileVM>();

            return output;
        }

        //bool CanRefreshFilelist()
        //{
        //    return String.IsNullOrEmpty(selectedPath) == false && Directory.Exists(selectedPath);
        //}

        //void CheckSelectedItems()
        //{
        //    Utility.CheckSelectedItems(SelectedFiles);
        //}

        //bool CanCheckAllItems()
        //{
        //    return files.Count > 0;
        //}

        //void CheckAllItems()
        //{
        //    bool select = files.Count((o) => o.IsSelected == true) != files.Count();

        //    files.All((o) => { o.IsSelected = select; return true; });

        //    RaisePropertyChanged(() => Files);
        //}

        void PreviewPattern()
        {
            if (SelectedFiles.Count() > 0 && String.IsNullOrEmpty(inputPattern) == false)
            {
                string filename = SelectedFiles.Last().FileName;

                Preview = UltimateMusicTaggerLocator.Instance.UMTagger.ShowNameByTag(filename, inputPattern);
            }
        }

        bool CanExecutePreviewPattern()
        {
            return SelectedFiles.Count() > 0 && String.IsNullOrEmpty(inputPattern) == false;
        }

        void ShowFileTag()
        {
            try
            {
                IEnumerable<FileVM> list = SelectedFiles;

                string filename = list.Last().FileName;

                TrackInfo output = UltimateMusicTaggerLocator.Instance.UMTagger.GetTag(list.Last().FileName);

                SelectedModelTag = new ModelTagVM(Utility.GetModelTag(output));

                Status = Path.GetFileName(filename);
            }
            catch (Exception)
            {
                Status = "Error while retrieving tag from file";
            }
        }

        bool CanExecuteShowFileTag()
        {
            return SelectedFiles.Count() > 0;
        }

        //void ExlusiveSelectItem(ISelectable input)
        //{
        //    Utility.ExclusiveSelectItem(input, files);
        //}

        //void SyncronizePath()
        //{
        //    SelectedPath = ViewModelLocator.Instance.LocalSystemVM.SelectedPath;

        //    RefreshFilelist();
        //}

        //bool CanExecuteSyncronizePath()
        //{
        //    return String.IsNullOrEmpty(ViewModelLocator.Instance.LocalSystemVM.SelectedPath) == false;
        //}

        #endregion

        #region Commands

        RelayCommand renameObjectCommand;
        RelayCommand<bool> addToInputCommand;
        //RelayCommand openFolderDialogCommand;
        //RelayCommand refreshFilelistCommand;
        //RelayCommand checkSelectedItemsCommand;
        //RelayCommand checkAllItemsCommand;
        RelayCommand previewPatternCommand;
        RelayCommand showFileTagCommand;
        RelayCommand updatePatternsCommand;
        //RelayCommand selectAllItemsCommand;
        //RelayCommand<ISelectable> exlusiveSelectItemCommand;
        //RelayCommand syncronizePathCommand;

        //public ICommand SelectAllItemsCommand
        //{
        //    get { return selectAllItemsCommand ?? (selectAllItemsCommand = new RelayCommand(SelectAllItems)); }
        //}

        public ICommand UpdatePatternsCommand
        {
            get { return updatePatternsCommand ?? (updatePatternsCommand = new RelayCommand(UpdatePatterns)); }
        }

        public ICommand RenameObjectCommand
        {
            get { return renameObjectCommand ?? (renameObjectCommand = new RelayCommand(RenameObject, CanExecuteRenameObject)); }
        }

        public ICommand AddToInputCommand
        {
            get { return addToInputCommand ?? (addToInputCommand = new RelayCommand<Boolean>(AddToInput, CanExecuteAddToInput)); }
        }

        //public ICommand OpenFolderDialogCommand
        //{
        //    get { return openFolderDialogCommand ?? (openFolderDialogCommand = new RelayCommand(OpenFolderDialog)); }
        //}

        //public ICommand RefreshFilelistCommand
        //{
        //    get { return refreshFilelistCommand ?? (refreshFilelistCommand = new RelayCommand(RefreshFilelist, CanRefreshFilelist)); }
        //}

        //public ICommand CheckSelectedItemsCommand
        //{
        //    get { return checkSelectedItemsCommand ?? (checkSelectedItemsCommand = new RelayCommand(CheckSelectedItems)); }
        //}

        //public ICommand CheckAllItemsCommand
        //{
        //    get { return checkAllItemsCommand ?? (checkAllItemsCommand = new RelayCommand(CheckAllItems, CanCheckAllItems)); }
        //}

        public ICommand PreviewPatternCommand
        {
            get { return previewPatternCommand ?? (previewPatternCommand = new RelayCommand(PreviewPattern, CanExecutePreviewPattern)); }
        }

        public ICommand ShowFileTagCommand
        {
            get { return showFileTagCommand ?? (showFileTagCommand = new RelayCommand(ShowFileTag, CanExecuteShowFileTag)); }
        }

        //public ICommand ExlusiveSelectItemCommand
        //{
        //    get { return exlusiveSelectItemCommand ?? (exlusiveSelectItemCommand = new RelayCommand<ISelectable>(ExlusiveSelectItem)); }
        //}

        //public ICommand SyncronizePathCommand
        //{
        //    get { return syncronizePathCommand ?? (syncronizePathCommand = new RelayCommand(SyncronizePath, CanExecuteSyncronizePath)); }
        //}

        #endregion

    }
}
