using MicroMvvm;
using Microsoft.Practices.Unity;
using ModernAudioTagger.BusinessLogic;
using ModernAudioTagger.Helpers;
using ModernAudioTagger.Model;
using ModernAudioTagger.Provider;
using ModernAudioTagger.ViewModelElement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using UltimateMusicTagger.Business;
using UltimateMusicTagger.Model;

namespace ModernAudioTagger.ViewModel
{
    public class MusicDbViewModel : BaseViewModel
    {
        #region Fields

        private const int MBID_LENGTH = 36;

        private UltimateMusicTagger.Business.MusicBrainzUtility musicBrainzUtility;

        private UltimateMusicTagger.Business.LastfmUtility lastfmUtility;

        private ObservableCollection<TrackInfoVM> listDbResult;

        public ObservableCollection<TrackInfoVM> ListDbResult
        {
            get { return listDbResult; }
            set
            {
                listDbResult = value;
                RaisePropertyChanged(() => ListDbResult);
            }
        }

        public enum DB_SITE
        {
            LASTFM,
            MUSICBRAINZ
        }

        #endregion

        #region Events

        public event EventHandler OnDbSearchUpdatedEvent;
        
        #endregion

        #region Constructor

        public MusicDbViewModel()
        {
            IWebProxy proxy = WebRequest.DefaultWebProxy;

            musicBrainzUtility = new MusicBrainzUtility(proxy);

            lastfmUtility = new LastfmUtility(null, proxy);

            SelectedDb = DB_SITE.LASTFM;

            ListDbResult = new ObservableCollectionExt<TrackInfoVM>();

            SelectedDbResult = new ObservableCollection<TrackInfoVM>();

            ListTrackFilter = new List<TrackInfo>();

            ListReleaseFilter = new List<ReleaseInfo>();

            //ManualArtistFilter = "Foo Fighters";

            //ManualReleaseFilter = "Wasting Light";

            RaisePropertyChanged(() => SelectedDb);
        }

        #endregion

        #region Properties

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

        public DB_SITE SelectedDb { get; set; }

        private string manualTitleFilter;

        public string ManualTitleFilter
        {
            get { return manualTitleFilter; }
            set { manualTitleFilter = value; RaisePropertyChanged(() => ManualTitleFilter); }
        }

        private string manualArtistFilter;

        public string ManualArtistFilter
        {
            get { return manualArtistFilter; }
            set { manualArtistFilter = value; RaisePropertyChanged(() => ManualArtistFilter); }
        }

        private string manualReleaseFilter;

        public string ManualReleaseFilter
        {
            get { return manualReleaseFilter; }
            set { manualReleaseFilter = value; RaisePropertyChanged(() => ManualReleaseFilter); }
        }

        public IList<TrackInfo> ListTrackFilter { get; set; }

        public ObservableCollection<TrackInfoVM> SelectedDbResult { get; set; }

        public IList<ReleaseInfo> ListReleaseFilter { get; set; }

        public TrackInfo SelectedTrackFilter { get; set; }

        public ReleaseInfo SelectedReleaseFilter { get; set; }

        #endregion

        #region Methods

        void MoveTrackUp(IEnumerable<TrackInfoVM> input)
        {
            IList<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            IEnumerable<TrackInfoVM> list = input.OrderBy((o) => (ListDbResult.IndexOf(o)));

            list.ToList().ForEach((o) =>
            {
                int oldindex = ListDbResult.IndexOf(o);

                int newindex = oldindex - 1;

                ListDbResult.Move(oldindex, newindex);

                listDbResult[oldindex].IsSelected = false;
                listDbResult[newindex].IsSelected = false;

                if (files.Count > oldindex)
                    files[oldindex].IsSelected = false;
                if (files.Count > newindex)
                    files[newindex].IsSelected = false;
            });

            RaisePropertyChanged(() => ListDbResult);
        }

        bool CanMoveTrackUp(IEnumerable<TrackInfoVM> input)
        {
            bool result = input.Count() > 0;

            if (result)
            {
                int firstElementIndex = input.Min((o) => { return ListDbResult.IndexOf(o); });

                result = firstElementIndex != 0;
            }

            return result;
        }

        void MoveTrackDown(IEnumerable<TrackInfoVM> input)
        {
            IList<FileVM> files = ViewModelLocator.Instance.LocalSystemVM.Files;

            int filesCount = files.Count();

            IEnumerable<TrackInfoVM> list = input.OrderByDescending((o) => (ListDbResult.IndexOf(o)));

            list.ToList().ForEach((o) =>
            {
                int oldindex = ListDbResult.IndexOf(o);

                int newindex = oldindex + 1;

                ListDbResult.Move(oldindex, newindex);

                listDbResult[oldindex].IsSelected = false;
                listDbResult[newindex].IsSelected = false;

                if (files.Count > oldindex)
                    files[oldindex].IsSelected = false;
                if (files.Count > newindex)
                    files[newindex].IsSelected = false;
            });

            RaisePropertyChanged(() => ListDbResult);
        }

        bool CanMoveTrackDown(IEnumerable<TrackInfoVM> input)
        {
            bool result = input.Count() > 0;

            if (result)
            {
                int lastElementIndex = input.Max((o) => { return ListDbResult.IndexOf(o); });

                result = lastElementIndex != (ListDbResult.Count - 1);
            }

            return result;
        }

        private void SearchArtistTrackList()
        {
            Func<string, List<TrackInfo>> func = null;

            switch (SelectedDb)
            {
                case DB_SITE.LASTFM:
                    func = lastfmUtility.GetTrackList;
                    break;
                case DB_SITE.MUSICBRAINZ:
                    func = musicBrainzUtility.GetRecordingList;
                    break;
            }

            IUnityContainer container = UnityContainerProvider.Instance;
            IWindowService processingService = container.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_PROCESSING_TAG);

            RootDispatcher.BeginInvoke((Action)delegate()
            {
                processingService.ShowDialog();
            });

            Task.Factory.StartNew(() =>
            {

                try
                {
                    ListTrackFilter = func(ManualArtistFilter);
                }
                catch (Exception e)
                {
                    throw e;
                }

            }).ContinueWith((t) =>
            {
                RootDispatcher.Invoke((Action)delegate()
                {
                    processingService.CloseDialog();

                    if (t.IsFaulted == false)
                    {
                        ManualTitleFilter = String.Format("[{0} elements found]", ListTrackFilter.Count);                        
                    }
                    else
                    {
                        ListTrackFilter.Clear();

                        //RaisePropertyChanged(() => ListDbResult);
                    }

                    RaisePropertyChanged(() => ListTrackFilter);

                });
            });

        }

        void SearchArtistReleaseList()
        {
            Func<string, List<ReleaseInfo>> func = null;

            switch (SelectedDb)
            {
                case DB_SITE.LASTFM:
                    func = lastfmUtility.GetReleaseList;
                    break;
                case DB_SITE.MUSICBRAINZ:
                    func = musicBrainzUtility.GetReleaseList;
                    break;
            }

            //try
            //{
            //ListTrackFilter.Clear();

            IUnityContainer container = UnityContainerProvider.Instance;
            IWindowService processingService = container.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_PROCESSING_TAG);

            RootDispatcher.BeginInvoke((Action)delegate()
            {
                processingService.ShowDialog();
            });

            Task.Factory.StartNew(() =>
            {
                try
                {
                    ListReleaseFilter = func(ManualArtistFilter);
                }
                catch (Exception e)
                {
                    throw e;
                }

            }).ContinueWith((t) =>
            {

                RootDispatcher.Invoke((Action)delegate()
                {
                    processingService.CloseDialog();

                    if (t.IsFaulted == false)
                    {
                        ManualReleaseFilter = String.Format("[{0} elements found]", ListReleaseFilter.Count);
                    }
                    else
                    {
                        ListReleaseFilter.Clear();

                        Status = t.Exception.InnerException.Message;
                    }

                    RaisePropertyChanged(() => ListReleaseFilter);

                });


            });

        }

        private IList<TrackInfoVM> BuildTrackList(List<TrackInfo> input)
        {
            IList<TrackInfoVM> output = new List<TrackInfoVM>();

            //ListDbResult.Clear();

            foreach (TrackInfo track in input)
            {
                TrackInfoVM trackVM = new TrackInfoVM(track);

                output.Add(trackVM);
            }

            return output;
        }

        private IEnumerable<TrackInfoVM> BuildTrackList(ReleaseInfo input)
        {
            List<TrackInfoVM> output = new List<TrackInfoVM>();

            foreach (TrackInfo track in input.TrackInfos)
            {
                track.Releases = new ReleaseInfo[] { input };

                TrackInfoVM trackVM = new TrackInfoVM(track);

                output.Add(trackVM);
            }

            return output;

        }

        private bool IsMbid(string input)
        {
            return input.Length == MBID_LENGTH && Regex.IsMatch(input, "[a-zA-Z0-9]{8}-[a-zA-Z0-9]{4}-[a-zA-Z0-9]{4}-[a-zA-Z0-9]{4}-[a-zA-Z0-9]{12}");
        }

        private void GetTrack()
        {
            string title = (SelectedTrackFilter != null ? SelectedTrackFilter.Title : manualTitleFilter).Trim();
            string artist = ManualArtistFilter;
            string mbid = SelectedTrackFilter != null ? SelectedTrackFilter.Mbid : null;

            if (mbid == null && IsMbid(title))
            {
                mbid = title;                
            }

            ListTrackFilter.Clear();

            List<TrackInfo> output = null;

            IUnityContainer container = UnityContainerProvider.Instance;
            IWindowService processingService = container.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_PROCESSING_TAG);

            RootDispatcher.BeginInvoke((Action)delegate()
            {
                processingService.ShowDialog();
            });

            Task.Factory.StartNew(() =>
            {
                try
                {
                    switch (SelectedDb)
                    {
                        case DB_SITE.LASTFM:
                            output =
                                new List<TrackInfo>(new TrackInfo[] { 
                                String.IsNullOrEmpty(mbid) == false ? 
                                lastfmUtility.GetTrack(mbid) :
                                lastfmUtility.GetTrack(title, artist) 
                            });
                            break;
                        case DB_SITE.MUSICBRAINZ:
                            output = String.IsNullOrEmpty(mbid) == false ?
                                new List<TrackInfo>(new TrackInfo[] { musicBrainzUtility.GetRecording(mbid) }) :
                                musicBrainzUtility.GetRecordingList(title, artist);
                            break;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

            }).ContinueWith((t) =>
            {
                RootDispatcher.Invoke((Action)delegate()
                {
                    processingService.CloseDialog();
                    
                    if (t.IsFaulted == false)
                    {
                        if (output.Count == 1)
                        {
                            if (output[0].Releases.Length > 1)
                            {
                                ListReleaseFilter = output[0].Releases.ToList<ReleaseInfo>();

                                RaisePropertyChanged(() => ListReleaseFilter);

                                ManualReleaseFilter = String.Format("[{0} elements found]", ListReleaseFilter.Count);

                                Status = String.Format("{0} album found", ListReleaseFilter.Count());
                            }
                            else
                            {
                                IEnumerable<TrackInfoVM> list = BuildTrackList(output);

                                ListDbResult.Clear();

                                foreach (TrackInfoVM input in list)
                                    ListDbResult.Add(input);

                                Status = String.Format("{0} elements found", list.Count());
                            }
                        }
                        else if (output.Count > 1)
                        {
                            ListTrackFilter = output;

                            ManualTitleFilter = "Select a track from the list";

                            Status = String.Format("{0} tracks found", ListReleaseFilter.Count());
                        }
                        else
                        {
                            ListTrackFilter = output;

                            ListDbResult.Clear();

                            Status = "No tracks found";
                        }

                        RaisePropertyChanged(() => ListTrackFilter);

                        RaisePropertyChanged(() => ListReleaseFilter);
                    }
                    else
                    {
                        ListDbResult.Clear();

                        Status = t.Exception.InnerException.Message;
                    }

                    RaisePropertyChanged(() => ListDbResult);

                    RaiseEventInvoker(OnDbSearchUpdatedEvent);

                });
            });

        }

        private void GetRelease()
        {
            string release = (SelectedReleaseFilter != null ? SelectedReleaseFilter.Title : manualReleaseFilter).Trim();
            string artist = ManualArtistFilter;
            string mbid = SelectedReleaseFilter != null ? SelectedReleaseFilter.Mbid : null;

            if (mbid == null && IsMbid(release))
                mbid = release;

            List<ReleaseInfo> output = null;

            bool searchByMbid = String.IsNullOrEmpty(mbid) == false; 

            IUnityContainer container = UnityContainerProvider.Instance;
            IWindowService processingService = container.Resolve<IWindowService>(ProviderServiceConstants.WND_SHOW_PROCESSING_TAG);

            RootDispatcher.BeginInvoke((Action)delegate()
            {
                processingService.ShowDialog();
            });

            Task.Factory.StartNew(() =>
            {
                try
                {
                    switch (SelectedDb)
                    {
                        case DB_SITE.LASTFM:
                            output = new List<ReleaseInfo>(new ReleaseInfo[] { searchByMbid ? 
                            lastfmUtility.GetRelease(mbid) :
                            lastfmUtility.GetRelease(release, artist) });
                            break;
                        case DB_SITE.MUSICBRAINZ:
                            output = searchByMbid ?
                                new List<ReleaseInfo>(new ReleaseInfo[] { musicBrainzUtility.GetRelease(mbid) }) :
                                musicBrainzUtility.GetReleaseList(release, artist);
                            break;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

            }).ContinueWith((t) =>
            {
                RootDispatcher.Invoke((Action)delegate()
                {
                    processingService.CloseDialog();

                    if (t.IsFaulted == false)
                    {
                        if (output.Count == 1)
                        {
                            IEnumerable<TrackInfoVM> list = BuildTrackList(output[0]);

                            ListDbResult.Clear();

                            list.ToList().ForEach((o) => ListDbResult.Add(o));

                            RaisePropertyChanged(() => ListDbResult);

                            Status = String.Format("{0} elements found", list.Count());

                        }
                        else
                        {
                            ListReleaseFilter = output;

                            ManualReleaseFilter = String.Format("[{0} elements found]", output.Count);

                            Status = String.Format("{0} album found", output.Count);

                            RaisePropertyChanged(() => ListReleaseFilter);
                        }
                    }
                    else
                    {
                        ListDbResult.Clear();

                        Status = t.Exception.InnerException.Message;
                    }

                    RaisePropertyChanged(() => ListDbResult);

                    RaiseEventInvoker(OnDbSearchUpdatedEvent);

                });
            });

        }

        void ProxyChanged()
        {
            IWebProxy proxy = WebRequest.DefaultWebProxy;

            musicBrainzUtility = new MusicBrainzUtility(proxy);
            lastfmUtility = new LastfmUtility(null, proxy);
        }

        void ExlusiveSelectItem(ISelectable input)
        {
            Utility.ExclusiveSelectItem(input, listDbResult);
        }

        void CheckSelectionChanged(TrackInfoVM input)
        {
            IList<TrackInfoVM> listSource = listDbResult;
            IList<FileVM> list = ModernAudioTagger.Provider.ViewModelLocator.Instance.LocalSystemVM.Files;

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
        }

        void CheckSelectedItems()
        {
            Utility.CheckSelectedItems(SelectedDbResult);
        }

        void OrderTracks(Func<string, IEnumerable<TrackInfo>, TrackInfo> func)
        {
            IList<TrackInfo> list = listDbResult.Select((o) =>
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

            LocalSystemViewModel localSystemVM = ViewModelLocator.Instance.LocalSystemVM;

            localSystemVM.Files.All(x => { x.IsSelected = false; return true; });

            ListDbResult.ToList().All(x => { x.IsSelected = false; return true; });

            string[] files = localSystemVM.Files.Select((o) => { return Path.GetFileNameWithoutExtension(o.FileName); }).ToArray();

            int indexFilename = 0;

            foreach (string file in files)
            {
                if (indexFilename > (list.Count - 1))
                    break;

                TrackInfo track = func(file, list);

                int indexTrackinfo = list.IndexOf(track);

                if (indexTrackinfo != -1)
                {
                    list.Swap(indexFilename, indexTrackinfo);
                }

                indexFilename++;
            }

            IEnumerable<TrackInfoVM> orderedList = list.Select((o) => { return new TrackInfoVM(o); });

            ListDbResult.Clear();

            foreach (TrackInfoVM track in orderedList)
                ListDbResult.Add(track);

            //ListDbResult.AddRange(orderedList);

            RaisePropertyChanged(() => ListDbResult);
        }

        void OrderTracksByName()
        {
            OrderTracks(UltimateMusicTagger.MTUtility.GetTrackInfoByLevenshteinDistance);
        }


        bool CanExecuteOrderTracksByName()
        {
            int filesCount = ViewModelLocator.Instance.LocalSystemVM.Files.Count;

            return listDbResult.Count > 0 && filesCount > 0;
        }

        void OrderTracksByNumber()
        {
            OrderTracks(UltimateMusicTagger.MTUtility.GetTrackInfoByPosition);
        }

        bool CanExecuteOrderTracksByNumber()
        {
            int filesCount = ViewModelLocator.Instance.LocalSystemVM.Files.Count;

            return listDbResult.Count > 0 && filesCount > 0;
        }

        private bool CanExecuteArtistTrackListCommand()
        {
            return (String.IsNullOrEmpty(ManualArtistFilter) == false && ManualArtistFilter.Length >= 2);
        }


        private bool CanExecuteSearchArtistReleaseListCommand()
        {
            return (String.IsNullOrEmpty(ManualArtistFilter) == false && ManualArtistFilter.Length >= 2);
        }

        private bool CanExecuteGetReleaseCommand()
        {
            return ((SelectedReleaseFilter != null && SelectedReleaseFilter.Mbid != null) ||
                (manualReleaseFilter != null && IsMbid(manualReleaseFilter.Trim())) ||
                (String.IsNullOrEmpty(manualArtistFilter) == false && manualArtistFilter.Length >= 2) &&
                (String.IsNullOrEmpty(manualReleaseFilter) == false && manualReleaseFilter.Length >= 2)
            );
        }


        private bool CanExecuteGetTrackCommand()
        {
            return ((SelectedTrackFilter != null && SelectedTrackFilter.Mbid != null) ||
                (manualTitleFilter != null && IsMbid(manualTitleFilter.Trim())) ||
                (String.IsNullOrEmpty(manualArtistFilter) == false && manualArtistFilter.Length >= 2) &&
                (String.IsNullOrEmpty(manualTitleFilter) == false && manualTitleFilter.Length >= 2)
            );
        }

        void OrderTracksByPosition()
        {
            IEnumerable<TrackInfoVM> list = ListDbResult.OrderBy(o => o.Track).ToList();

            ListDbResult.Clear();

            foreach (TrackInfoVM track in list)
                ListDbResult.Add(track);

            //ListDbResult.AddRange(list);

            RaisePropertyChanged(() => ListDbResult);
        }

        bool CanCheckAllItems()
        {
            int filesCount = ViewModelLocator.Instance.LocalSystemVM.Files.Count;

            return listDbResult.Count > 0 && filesCount > 0;
        }

        void CheckAllItems()
        {
            int filesCount = ViewModelLocator.Instance.LocalSystemVM.Files.Count;

            int maxItemsSelectable = Math.Min(filesCount, listDbResult.Count);

            Utility.CheckAllItems(listDbResult, maxItemsSelectable);

            RaisePropertyChanged(() => ListDbResult);
        }

        bool CanExecuteCheckSelectedItems()
        {
            int filesCount = ViewModelLocator.Instance.LocalSystemVM.Files.Count;

            bool result = false;

            IEnumerable<TrackInfoVM> sortedList = SelectedDbResult.OrderBy((o) => listDbResult.IndexOf(o));

            if (sortedList.Count() > 0)
            {
                int index = listDbResult.IndexOf(sortedList.Last());

                if (index < filesCount)
                    result = true;
            }

            return result;
        }

        void ClearTracklist()
        {
            ListTrackFilter = new List<TrackInfo>();

            RaisePropertyChanged(() => ListTrackFilter);

            SelectedTrackFilter = null;

            ManualTitleFilter = String.Empty;
        }

        bool CanExecuteClearTracklist()
        {
            bool result = false;

            if (String.IsNullOrEmpty(manualTitleFilter) == false || (ListTrackFilter != null && ListTrackFilter.Count() > 0))
                result = true;

            return result;
        }

        void ClearReleaselist()
        {
            ListReleaseFilter = new List<ReleaseInfo>();

            RaisePropertyChanged(() => ListReleaseFilter);

            SelectedReleaseFilter = null;

            ManualReleaseFilter = String.Empty;
        }

        bool CanExecuteClearReleaselist()
        {
            bool result = false;

            if (String.IsNullOrEmpty(manualReleaseFilter) == false || (ListReleaseFilter != null && ListReleaseFilter.Count() > 0))
                result = true;

            return result;
        }

        void ClearAllSelected()
        {
            foreach (TrackInfoVM input in listDbResult)
            {
                input.IsSelected = false;
            }
        }

        #endregion

        #region Commands

        RelayCommand searchArtistTrackListCommand;
        RelayCommand searchArtistReleaseListCommand;
        RelayCommand getReleaseCommand;
        RelayCommand getTrackCommand;
        RelayCommand proxyChangedCommand;
        RelayCommand<IEnumerable<TrackInfoVM>> moveTrackUpCommand;
        RelayCommand<IEnumerable<TrackInfoVM>> moveTrackDownCommand;
        RelayCommand<ISelectable> exlusiveSelectItemCommand;
        RelayCommand<TrackInfoVM> checkSelectionChangedCommand;
        RelayCommand checkSelectedItemsCommand;
        RelayCommand orderTracksByNameCommand;
        RelayCommand orderTracksByNumberCommand;
        RelayCommand orderTracksByPositionCommand;
        RelayCommand checkAllItemsCommand;
        RelayCommand clearTracklistCommand;
        RelayCommand clearReleaselistCommand;
        RelayCommand clearAllSelectedCommand;

        public ICommand ClearAllSelectedCommand
        {
            get { return clearAllSelectedCommand ?? (clearAllSelectedCommand = new RelayCommand(ClearAllSelected)); }
        }

        public ICommand ExlusiveSelectItemCommand
        {
            get { return exlusiveSelectItemCommand ?? (exlusiveSelectItemCommand = new RelayCommand<ISelectable>(ExlusiveSelectItem)); }
        }

        public ICommand MoveTrackUpCommand
        {
            get { return moveTrackUpCommand ?? (moveTrackUpCommand = new RelayCommand<IEnumerable<TrackInfoVM>>(MoveTrackUp, CanMoveTrackUp)); }
        }

        public ICommand MoveTrackDownCommand
        {
            get { return moveTrackDownCommand ?? (moveTrackDownCommand = new RelayCommand<IEnumerable<TrackInfoVM>>(MoveTrackDown, CanMoveTrackDown)); }
        }

        public ICommand SearchArtistTrackListCommand
        {
            get { return searchArtistTrackListCommand ?? (searchArtistTrackListCommand = new RelayCommand(SearchArtistTrackList, CanExecuteArtistTrackListCommand)); }
        }

        public ICommand SearchArtistReleaseListCommand
        {
            get { return searchArtistReleaseListCommand ?? (searchArtistReleaseListCommand = new RelayCommand(SearchArtistReleaseList, CanExecuteSearchArtistReleaseListCommand)); }
        }

        public ICommand GetReleaseCommand
        {
            get { return getReleaseCommand ?? (getReleaseCommand = new RelayCommand(GetRelease, CanExecuteGetReleaseCommand)); }
        }

        public ICommand GetTrackCommand
        {
            get { return getTrackCommand ?? (getTrackCommand = new RelayCommand(GetTrack, CanExecuteGetTrackCommand)); }
        }

        public ICommand ProxyChangedCommand
        {
            get { return proxyChangedCommand ?? (proxyChangedCommand = new RelayCommand(ProxyChanged)); }
        }

        public ICommand CheckSelectionChangedCommand
        {
            get { return checkSelectionChangedCommand ?? (checkSelectionChangedCommand = new RelayCommand<TrackInfoVM>(CheckSelectionChanged)); }
        }

        public ICommand CheckSelectedItemsCommand
        {
            get { return checkSelectedItemsCommand ?? (checkSelectedItemsCommand = new RelayCommand(CheckSelectedItems, CanExecuteCheckSelectedItems)); }
        }

        public ICommand OrderTracksByNameCommand
        {
            get { return orderTracksByNameCommand ?? (orderTracksByNameCommand = new RelayCommand(OrderTracksByName, CanExecuteOrderTracksByName)); }
        }

        public ICommand OrderTracksByNumberCommand
        {
            get { return orderTracksByNumberCommand ?? (orderTracksByNumberCommand = new RelayCommand(OrderTracksByNumber, CanExecuteOrderTracksByNumber)); }
        }

        public ICommand OrderTracksByPositionCommand
        {
            get { return orderTracksByPositionCommand ?? (orderTracksByPositionCommand = new RelayCommand(OrderTracksByPosition)); }
        }

        public ICommand CheckAllItemsCommand
        {
            get { return checkAllItemsCommand ?? (checkAllItemsCommand = new RelayCommand(CheckAllItems, CanCheckAllItems)); }
        }

        public ICommand ClearTracklistCommand
        {
            get { return clearTracklistCommand ?? (clearTracklistCommand = new RelayCommand(ClearTracklist, CanExecuteClearTracklist)); }
        }

        public ICommand ClearReleaselistCommand
        {
            get { return clearReleaselistCommand ?? (clearReleaselistCommand = new RelayCommand(ClearReleaselist, CanExecuteClearReleaselist)); }
        }

        #endregion
    }
}
