using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ModernAudioTagger.Converter
{
    class ImageSwitchConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            object result = null;

            foreach (object val in values)
            {
                if (val != null)
                {
                    System.Drawing.Image imgsource = null;

                    if (val is string)
                    {
                        imgsource = UltimateMusicTagger.MTUtility.ImageFromUri((string)val, System.Net.WebRequest.DefaultWebProxy);
                    }                    
                    else if (val is System.Drawing.Image)
                    {
                        imgsource = (System.Drawing.Image)val;
                    }

                    if (imgsource != null)
                    {
                        System.IO.MemoryStream memStream = new System.IO.MemoryStream();

                        //save the image to memStream as a png
                        imgsource.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);

                        //gets a decoder from this stream
                        System.Windows.Media.Imaging.PngBitmapDecoder decoder = new System.Windows.Media.Imaging.PngBitmapDecoder(memStream, System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat, System.Windows.Media.Imaging.BitmapCacheOption.Default);

                        result = decoder.Frames[0];
                    }
                }
                
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
