using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModernAudioTagger.Converter
{
    public class DataGridRowIndexConverter : IValueConverter
    {

        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            //ListBoxItem item = (ListBoxItem)value;
            //ListBox listView = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
            DataGridRow row = (DataGridRow)value;
            DataGrid list = ItemsControl.ItemsControlFromItemContainer(row) as DataGrid;
            int index = list.ItemContainerGenerator.IndexFromContainer(row);
            if (parameter != null && (Boolean)parameter == true)
                index++;
            return index.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
