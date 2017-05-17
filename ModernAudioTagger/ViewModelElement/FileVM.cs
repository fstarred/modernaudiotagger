using MicroMvvm;

namespace ModernAudioTagger.ViewModelElement
{
    public class FileVM : ObservableObject , ISelectable
    {
        public string FileName { get; set; }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { 
                isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }
        
    }
}
