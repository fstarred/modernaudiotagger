using FirstFloor.ModernUI.Windows.Controls;

namespace ModernAudioTagger.Windows
{
    /// <summary>
    /// Interaction logic for ProcessingDialog.xaml
    /// </summary>
    public partial class ProcessingDialog : ModernDialog
    {
        public ProcessingDialog()
        {
            InitializeComponent();
            this.CloseButton.Visibility = System.Windows.Visibility.Collapsed;
            //this.Loaded += ProcessingDialog_Loaded;
        }

        //void ProcessingDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        //{
            
        //}


    }
}
