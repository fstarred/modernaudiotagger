using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Practices.Unity;
using ModernAudioTagger.BusinessLogic;
using ModernAudioTagger.Model;
using ModernAudioTagger.Provider;
using ModernUILogViewer.BusinessLogic;
using MvvmFoundation.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ModernAudioTagger.View
{
    static class ViewCommands
    {

        static RelayCommand<DefaultEventArgs> showMessageOutOfDateVersionCommand;
        static RelayCommand initContainerCommand;
        
        public static ICommand CopyTextToClipboard { get { return new MvvmFoundation.Wpf.RelayCommand<Object>(CopyTextToClipboardExecute); } }
        public static ICommand OpenUrl { get { return new RelayCommand<string>(OpenUrlExecute); } }
        public static ICommand OpenUri { get { return new RelayCommand<Uri>(OpenUriExecute); } }
        public static ICommand OpenFolderLocation { get { return new RelayCommand<string>(OpenFolderLocationExecute, CanOpenFolderLocationExecute); } }
        public static ICommand InitContainerCommand { get { return initContainerCommand ?? (initContainerCommand = new RelayCommand(InitContainer)); } }
        public static ICommand InitResourcesCommand { get { return new RelayCommand(InitResources); } }
        
        static void InitResources()
        {
            string uri = (string)App.Current.FindResource("updateuri");

            ViewModelLocator.Instance.NetworkVM.AppUri = uri;
        }

        static bool CanOpenFolderLocationExecute(string selectedPath)
        {
            return String.IsNullOrEmpty(selectedPath) == false && (Directory.Exists(selectedPath) || File.Exists(selectedPath));
        }

        static void InitContainer()
        {
            IUnityContainer container = UnityContainerProvider.Instance;

            container.RegisterType<IWindowService>(ProviderServiceConstants.WND_SHOW_PREVIEW_TAG,
                new InjectionFactory(cont =>
                {
                    WindowService wservice = new WindowService
                    {
                        Win = new Windows.TagPreview(),
                        Width = 600,
                        Height = 300
                    };                    

                    return wservice;
                })
             );

            container.RegisterType<IWindowService>(ProviderServiceConstants.WND_SHOW_PROCESSING_TAG,
                new InjectionFactory(cont =>
                {
                    WindowService wservice = new WindowService
                    {
                        Win = new Windows.ProcessingDialog(),
                        Width = 300,
                        Height = 200
                    };

                    return wservice;
                })
             );

            container.RegisterType<IWindowService>(ProviderServiceConstants.WND_SHOW_LOG_VIEWER,
                new InjectionFactory(cont =>
                {
                    WindowService wservice = new WindowService
                    {
                        Win = new Windows.LogViewer(),
                        Width = 600,
                        Height = 300
                    };

                    return wservice;
                })
             );

            container.RegisterType<IDialogService>(ProviderServiceConstants.DLG_OPEN_PATH,
                new InjectionFactory(cont =>
                {
                    DialogService dservice = new DialogService();
                    
                    return dservice;
                })
            );

            container.RegisterType<IDialogService>(ProviderServiceConstants.DLG_OPEN_AUDIO,
                new InjectionFactory(cont =>
                {
                    DialogService dservice = new DialogService();
                    //dservice.Filter = "*.3gp|*.act|*.AIFF*.|*.aac*.|*.ALAC*.|*.amr*.|*.atrac*.|*.Au*.|*.awb*.|*.dct*.|*.dss*.|*.dvf*.|*.flac*.|*.gsm*.|*.iklax*.|*.IVS*.|*.m4a*.|*.m4p*.|*.mmf*.|*.mp3*.|*.mpc*.|*.msv*.|*.ogg*.|*.Opus*.|*.ra*.|*.raw*.|*.TTA*.|*.vox*.|*.wav*.|*.wma*.|*.wv";

                    return dservice;
                })
            );

            container.RegisterType<IDialogService>(ProviderServiceConstants.DLG_OPEN_EXE,
                new InjectionFactory(cont =>
                {
                    DialogService dservice = new DialogService();
                    dservice.Filter = "*Executable (*.exe)|*.exe";

                    return dservice;
                })
            );

            container.RegisterType<IDialogService>(ProviderServiceConstants.DLG_LOAD_IMAGE_PATH,
                new InjectionFactory(cont =>
                {
                    DialogService dservice = new DialogService();
                    
                    System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
                    string sep = string.Empty;
                    string filter = String.Empty;
                    string filterAllImages = String.Empty;

                    foreach (var c in codecs)
                    {
                        string codecName = c.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                        filter = String.Format("{0}{1}{2} ({3})|{3}", filter, sep, codecName, c.FilenameExtension);
                        filterAllImages = String.Format("{0}{1};", filterAllImages, c.FilenameExtension);
                        sep = "|";
                    }

                    dservice.Filter = String.Format("{4} |{5}{1}{0}{1}{2} ({3})|{3}", filter, sep, "All Files", "*.*", "All Images", filterAllImages);
                    dservice.Title = "Import image file";

                    return dservice;
                })
            );

            container.RegisterType<IDialogService>(ProviderServiceConstants.DLG_UPLOAD_PNG,
                new InjectionFactory(cont =>
                {
                    DialogService dservice = new DialogService();

                    dservice.Filter = "Png Image|*.png";
                    dservice.Title = "Upload background image";

                    return dservice;
                })
            );

            container.RegisterType<IDialogService>(ProviderServiceConstants.DLG_SAVE_IMAGE_PATH,
                new InjectionFactory(cont =>
                {
                    DialogService dservice = new DialogService();

                    dservice.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
                    dservice.Title = "Save image cover as file";

                    return dservice;
                })
            );            
        }

        static void OpenFolderLocationExecute(string location)
        {
            //Process.Start(new ProcessStartInfo("explorer.exe", "/select, " + location));        
            string arg = File.Exists(location) ? "/select, " : String.Empty;
            Process.Start(new ProcessStartInfo("explorer.exe", arg + location));
        }

        private static void OpenUrlExecute(string url)
        {
            Process.Start(url);
        }

        private static void OpenUriExecute(Uri uri)
        {
            if (uri != null)
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
        }

        private static void CopyTextToClipboardExecute<T>(T obj)
        {
            Clipboard.SetText(obj.ToString());
        }

        private static void ShowMessageOutOfDateVersion(DefaultEventArgs eventArgs)
        {
            ModernAudioTagger.BusinessLogic.ServiceUpdater.VersionInfo lastVersionInfo = (ModernAudioTagger.BusinessLogic.ServiceUpdater.VersionInfo)eventArgs.ObjArg;

            MessageBoxResult result = ModernDialog.ShowMessage(String.Format("A new version ({0}) is available, do you want to go to the homepage?", lastVersionInfo.LatestVersion), "New Version", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                Process.Start(lastVersionInfo.LatestVersionUrl);
            }
        }

        public static ICommand ShowMessageOutOfDateVersionCommand
        {
            get { return showMessageOutOfDateVersionCommand ?? (showMessageOutOfDateVersionCommand = new RelayCommand<DefaultEventArgs>(ShowMessageOutOfDateVersion)); }
        }

    }
}
