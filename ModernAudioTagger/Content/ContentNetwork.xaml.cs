
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using ModernAudioTagger.ViewModel;
using System.Windows.Controls;



namespace ModernAudioTagger.Content
{
    /// <summary>
    /// Interaction logic for ContentNetwork.xaml
    /// </summary>
    public partial class ContentNetwork : UserControl, IContent
    {
        public ContentNetwork()
        {
            InitializeComponent();
        }

        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
        }
        //
        // Summary:
        //     Called when this instance is no longer the active content in a frame.
        //
        // Parameters:
        //   e:
        //     An object that contains the navigation data.
        public void OnNavigatedFrom(NavigationEventArgs e) 
        {
        }
        //
        // Summary:
        //     Called when a this instance becomes the active content in a frame.
        //
        // Parameters:
        //   e:
        //     An object that contains the navigation data.
        public void OnNavigatedTo(NavigationEventArgs e)
        {
        }
        //
        // Summary:
        //     Called just before this instance is no longer the active content in a frame.
        //
        // Parameters:
        //   e:
        //     An object that contains the navigation data.
        //
        // Remarks:
        //     The method is also invoked when parent frames are about to navigate.
        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ((NetworkViewModel)this.DataContext).ProxyChangedCommand.Execute(null);
        }
    }
}
