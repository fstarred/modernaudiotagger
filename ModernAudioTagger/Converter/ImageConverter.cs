using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ModernAudioTagger.Converter
{
    [ValueConversion(typeof(System.Drawing.Image), typeof(System.Windows.Controls.Image))]
    class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {

                System.Drawing.Image img = (System.Drawing.Image)value;

                System.IO.MemoryStream memStream = new System.IO.MemoryStream();

                //save the image to memStream as a png
                img.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);

                //gets a decoder from this stream
                System.Windows.Media.Imaging.PngBitmapDecoder decoder = new System.Windows.Media.Imaging.PngBitmapDecoder(memStream, System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat, System.Windows.Media.Imaging.BitmapCacheOption.Default);

                return decoder.Frames[0];
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
