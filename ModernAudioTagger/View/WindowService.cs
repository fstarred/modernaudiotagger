using FirstFloor.ModernUI.Windows.Controls;
using ModernAudioTagger.BusinessLogic;
using ModernUILogViewer.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ModernAudioTagger.View
{
    class WindowService : IWindowService
    {
        public Window Win { get; set; }
        //public UserControl Cnt { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }                

        public void ShowDialog()
        {
            //var wnd = new ModernWindow
            //{
            //    Content = Cnt,
            //    Style = (Style)App.Current.Resources["EmptyWindow"],
            //    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,                
            //    Width = Width,
            //    Height = Height
            //};

            var wnd = Win;
            //wnd.Style = (Style)App.Current.Resources["EmptyWindow"];
            wnd.Width = Width;
            wnd.Height = Height;
            
            wnd.ShowDialog();
        }

        public void CloseDialog()
        {
            var wnd = Win;            
            wnd.Close();
        }
    }
}
