using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernAudioTagger.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        #region Fields

        bool isMediaPlaying = false;
        
        #endregion

        #region Constructor

        public MainViewModel()
        {
            MediaPlayer player = Provider.MediaPlayerProvider.Instance.Player;
            
            //player.MediaOpened += (sender, e) => { isMediaPlaying = true; RaiseEventInvoker(OnMediaPlayEvent); };
            //player.MediaEnded += (sender, e) => { isMediaPlaying = false; RaiseEventInvoker(OnMediaStopEvent); };
            //player.MediaOpened += player_MediaOpened;            
            player.MediaEnded += (sender, e) => StopFile();
            player.MediaFailed += (sender, e) => { isMediaPlaying = false; RaiseEventInvoker(OnMediaStopEvent); };            
        }
        
        #endregion

        #region Events

        public event EventHandler OnMediaPlayEvent;
        public event EventHandler OnMediaStopEvent;
        
        #endregion

        #region Properties

        private UltimateMusicTagger.UMTMessage[] logs;

        public UltimateMusicTagger.UMTMessage[] Logs
        {
            get { return logs; }
            set { 
                logs = value;
                RaisePropertyChanged(() => Logs);
            }
        }

        private string playingFile;

        public string PlayingFile
        {
            get { return playingFile; }
            set
            {
                playingFile = value;
                RaisePropertyChanged(() => PlayingFile);
            }
        }
        
        #endregion

        #region Methods

        void PlayFile(string input)
        {
            MediaViewModel mediaVM = Provider.ViewModelLocator.Instance.MediaVM;

            MediaViewModel.OPEN_MEDIA_MODE playMode = mediaVM.OpenMediaMode;

            if (playMode == MediaViewModel.OPEN_MEDIA_MODE.INTERNAL)
            {
                MediaPlayer player = Provider.MediaPlayerProvider.Instance.Player;

                Uri olduri = player.Source;
                Uri newuri = new Uri(input);

                bool isStopRequest = false;

                if (isMediaPlaying)
                {
                    isStopRequest = (olduri != null && newuri.AbsolutePath.Equals(olduri.AbsolutePath));
                    StopFile();
                }

                if (File.Exists(input) && isStopRequest == false)
                {
                    player.Open(newuri);
                    player.Play();
                    isMediaPlaying = true;
                    RaiseEventInvoker(OnMediaPlayEvent);
                    PlayingFile = Path.GetFileName(input);
                }
            }
            else
            {
                string externalApplicationPath = mediaVM.ExternalApplicationPath;
                string arguments = mediaVM.CommandArguments;
                if (File.Exists(externalApplicationPath))
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.UseShellExecute = false;
                    psi.FileName = externalApplicationPath;
                    psi.Arguments = arguments.Replace("%f", '"' + input + '"');
                    Process.Start(psi);
                }                
            }
            
        }

        void StopFile()
        {
            MediaPlayer player = Provider.MediaPlayerProvider.Instance.Player;

            player.Stop();
            player.Close();
            isMediaPlaying = false;
            RaiseEventInvoker(OnMediaStopEvent);
        }
        
        #endregion

        #region Commands

        RelayCommand<string> playFileCommand;
        RelayCommand stopFileCommand;

        public ICommand PlayFileCommand
        {
            get { return playFileCommand ?? (playFileCommand = new RelayCommand<string>(PlayFile)); }
        }

        public ICommand StopFileCommand
        {
            get { return stopFileCommand ?? (stopFileCommand = new RelayCommand(StopFile)); }
        }
        
        #endregion
    }
}
