using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ModernAudioTagger.Helpers
{
    static class HelperNotifier
    {
        static IEnumerable<T> GetCollection<T>(Panel panel) { return panel.Children.OfType<T>(); }

        public static void AttachTextBoxNotifier(Panel panel, RoutedEventArgs eve)
        {
            GetCollection<TextBox>(panel).All(t =>
                {                    
                    t.TextChanged += (sender, e) =>
                    {
                        if (eve != null)
                        {
                            eve.Source = t;
                            panel.RaiseEvent(eve);
                        }
                    };
                    return true;
                });
        }

        public static void AttachCheckBoxNotifier(Panel panel, RoutedEventArgs eve)
        {
            GetCollection<CheckBox>(panel).All(t =>
            {
                RoutedEventHandler rout = (sender, e) =>
                {
                    if (eve != null)
                    {
                        eve.Source = t;
                        t.RaiseEvent(eve);
                    }
                };

                t.Checked += rout;
                t.Unchecked += rout;
                return true;
            });
        }

        public static void AttachComboBoxNotifier(Panel panel, RoutedEventArgs eve)
        {
            GetCollection<ComboBox>(panel).All(t =>
            {
                t.SelectionChanged += (sender, e) =>
                {
                    if (eve != null)
                    {
                        eve.Source = t;
                        panel.RaiseEvent(eve);
                    }
                };
                return true;
            });
        }

        //public static void AttachTextBoxNotifier(Panel panel, EventHandler eve)
        //{
        //    GetCollection<TextBox>(panel).All(t =>
        //    {
        //        t.TextChanged += (sender, e) =>
        //        {
        //            if (eve != null)
        //                eve(sender, EventArgs.Empty);
        //        };
        //        return true;
        //    });
        //}


        //public static void AttachCheckBoxNotifier(Panel panel, EventHandler eve)
        //{
        //    GetCollection<CheckBox>(panel).All(t =>
        //    {
        //        RoutedEventHandler rout = (sender, e) =>
        //        {
        //            if (eve != null)
        //                eve(sender, EventArgs.Empty);
        //        };

        //        t.Checked += rout;
        //        t.Unchecked += rout;
        //        return true;
        //    });
        //}

        //public static void AttachComboBoxNotifier(Panel panel, EventHandler eve)
        //{
        //    GetCollection<ComboBox>(panel).All(t =>
        //    {
        //        t.SelectionChanged += (sender, e) =>
        //        {
        //            if (eve != null)
        //                eve(sender, EventArgs.Empty);
        //        };
        //        return true;
        //    });
        //}

    }
}
