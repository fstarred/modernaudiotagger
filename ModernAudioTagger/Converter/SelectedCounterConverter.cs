using ModernAudioTagger.ViewModelElement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ModernAudioTagger.Converter
{
    [ValueConversion(typeof(IEnumerable<ISelectable>), typeof(int))]
    class SelectedCounterConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            int output = 0;

            if (value != null)
            {
                IEnumerable<ISelectable> list = (IEnumerable<ISelectable>)value;

                output = list.Count((o) => { return o.IsSelected == true; });
            }

            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
