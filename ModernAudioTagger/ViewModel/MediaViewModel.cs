using MicroMvvm;
using ModernAudioTagger.BusinessLogic;
using ModernAudioTagger.Provider;
using ModernUILogViewer.BusinessLogic;
using System.Windows.Input;
using Unity;

namespace ModernAudioTagger.ViewModel
{
    public class MediaViewModel : BaseViewModel
    {
        #region Enum

        public enum OPEN_MEDIA_MODE { INTERNAL, EXTERNAL }
        
        #endregion

        #region Properties

        private OPEN_MEDIA_MODE openMediaMode;

        public OPEN_MEDIA_MODE OpenMediaMode
        {
            get { return openMediaMode; }
            set
            {
                openMediaMode = value;
                RaisePropertyChanged(() => OpenMediaMode);
            }
        }

        private string externalApplicationPath;

        public string ExternalApplicationPath
        {
            get { return externalApplicationPath; }
            set
            {
                externalApplicationPath = value;
                RaisePropertyChanged(() => ExternalApplicationPath);
            }
        }

        private string commandArguments;

        public string CommandArguments
        {
            get { return commandArguments; }
            set { 
                commandArguments = value;
                RaisePropertyChanged(() => CommandArguments);
            }
        }
        
        
        #endregion

        #region Methods

        void OpenExternalApplication()
        {
            IUnityContainer container = UnityContainerProvider.Instance;
            IDialogService service = container.Resolve<IDialogService>(ProviderServiceConstants.DLG_OPEN_EXE);

            string[] files = service.OpenFile(false);

            if (files.Length > 0)
            {
                ExternalApplicationPath = files[0];
            }
        }
        
        #endregion

        #region Commands

        RelayCommand openExternalApplicationCommand;

        public ICommand OpenExternalApplicationCommand
        {
            get { return openExternalApplicationCommand ?? (openExternalApplicationCommand = new RelayCommand(OpenExternalApplication)); }
        }

        #endregion

    }
}
