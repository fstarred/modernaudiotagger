using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ModernAudioTagger.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    class FilenameConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            string file = null;

            if (value != null)
                file = value.ToString();

            if (String.IsNullOrEmpty(file) == false)
            {
                file = Path.GetFileName(file);
            }

            return file;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
