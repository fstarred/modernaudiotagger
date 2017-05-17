using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ModernAudioTagger.Converter
{
    [ValueConversion(typeof(UltimateMusicTagger.UMTMessage[]), typeof(string))]
    class LogsToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StringBuilder sb = new StringBuilder();

            UltimateMusicTagger.UMTMessage[] logs = (UltimateMusicTagger.UMTMessage[])value;

            if (logs == null)
                return String.Empty;

            foreach (UltimateMusicTagger.UMTMessage message in logs)
            {
                sb.Append(message.TypeMsg.ToString());
                sb.Append(":\t\t");
                sb.Append(message.Message);
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
