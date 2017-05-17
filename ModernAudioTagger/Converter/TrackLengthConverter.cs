using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ModernAudioTagger.Converter
{
    [ValueConversion(typeof(uint), typeof(string))]
    class TrackLengthConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            uint duration = (uint)value;

            string output = null;

            // if value is > 6000 is probably expressed in ms
            const int ESTABILISHED_SECONDS_LIMIT = 6000;

            if (duration > ESTABILISHED_SECONDS_LIMIT)
            {
                duration /= 1000;
            }

            TimeSpan ts = new TimeSpan(0, 0, (int)duration);

            if (ts.Hours == 0)
                output = String.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
            else
                output = String.Format("{0:D3}:{1:D2}", ts.Minutes, ts.Seconds);

            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
