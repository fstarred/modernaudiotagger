using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ModernAudioTagger.Converter
{
    [ValueConversion(typeof(string[]), typeof(string))]
    public class ArraySplitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string output = null;

            if (value is string[])
            {                
                string[] input = (string[])value;

                output = String.Join(",", input);
            }

            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] val = ((String)value).Split(',');
            return val;
        }
    }
}
