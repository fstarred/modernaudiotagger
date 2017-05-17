using System;
using System.Collections.Generic;
using System.Linq;
using ModernAudioTagger.Helpers;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ModernAudioTagger.Helpers
{
    public static class ListBoxMultipleSelectorHelper
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(System.Collections.IList),
                typeof(ListBoxMultipleSelectorHelper),
                new FrameworkPropertyMetadata(SelectedItemsChanged));

        public static System.Collections.IList GetSelectedItems(DependencyObject dp)
        {
            return (System.Collections.IList)dp.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject dp, System.Collections.IList value)
        {
            dp.SetValue(SelectedItemsProperty, value);
        }

        static void SelectedItemsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is System.Windows.Controls.Primitives.Selector)
            {
                System.Windows.Controls.Primitives.Selector lb = obj as System.Windows.Controls.Primitives.Selector;                
                lb.SelectionChanged += lb_SelectionChanged;
            }
            else if (obj is System.Windows.Controls.Primitives.MultiSelector)
            {
                System.Windows.Controls.Primitives.MultiSelector lb = obj as System.Windows.Controls.Primitives.MultiSelector;
                lb.SelectionChanged += lb_SelectionChanged;
            }
        }

        

        static void lb_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Collections.IList listSelectedItems = null;

            if (sender is System.Windows.Controls.ListBox)
                listSelectedItems = ((System.Windows.Controls.ListBox)sender).SelectedItems;
            else if (sender is System.Windows.Controls.Primitives.MultiSelector)
                listSelectedItems = ((System.Windows.Controls.Primitives.MultiSelector)sender).SelectedItems;

            BindingExpression bindingExpression = BindingOperations.GetBindingExpression((DependencyObject)sender, SelectedItemsProperty);

            if (bindingExpression != null)
            {
                System.Collections.IList listBinding = bindingExpression.GetValue<System.Collections.IList>(bindingExpression.DataItem);

                listBinding.Clear();

                foreach (object o in listSelectedItems)
                    listBinding.Add(o);
            }

        }
    }
}
