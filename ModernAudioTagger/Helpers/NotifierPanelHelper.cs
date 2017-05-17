using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ModernAudioTagger.Helpers
{
    public static class NotifierPanelHelper
    {
        public static readonly DependencyProperty EnableNotifierProperty =
            DependencyProperty.RegisterAttached(
                "EnableNotifier",
                typeof(bool),
                typeof(NotifierPanelHelper),
                new FrameworkPropertyMetadata(EnableNotifierChanged));

        /// <summary>
        /// Alarm Attached Routed Event
        /// </summary>
        public static readonly RoutedEvent ChildrenChangedEvent =
            EventManager.RegisterRoutedEvent("ChildrenChanged",
                                             RoutingStrategy.Bubble,
                                             typeof(RoutedEventHandler),
                                             typeof(NotifierPanelHelper));


        public static bool GetEnableNotifier(DependencyObject dp)
        {
            return (bool)dp.GetValue(EnableNotifierProperty);
        }

        public static void SetEnableNotifier(DependencyObject dp, bool value)
        {
            dp.SetValue(EnableNotifierProperty, value);
        }

        public static void AddChildrenChangedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            UIElement uie = d as UIElement;
            if (uie != null)
            {
                uie.AddHandler(NotifierPanelHelper.ChildrenChangedEvent, handler);
            }
        }
        public static void RemoveChildrenChangedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            UIElement uie = d as UIElement;
            if (uie != null)
            {
                uie.RemoveHandler(NotifierPanelHelper.ChildrenChangedEvent, handler);
            }
        }

        private static readonly DependencyProperty IsInitializedProperty =
           DependencyProperty.RegisterAttached("IsInitialized", typeof(bool),
           typeof(NotifierPanelHelper));

        private static bool GetIsInitialized(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsInitializedProperty);
        }

        private static void SetIsInitialized(DependencyObject dp, bool value)
        {
            dp.SetValue(IsInitializedProperty, value);
        }

        private static void EnableNotifierChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Panel panel = obj as Panel;

                if (panel != null)
                {
                    panel.Loaded += (sender, re) =>
                    {
                        if (GetIsInitialized(panel) == false)
                        {
                            SetIsInitialized(panel, true);
                            RoutedEventArgs args = new RoutedEventArgs(ChildrenChangedEvent, panel);

                            HelperNotifier.AttachTextBoxNotifier(panel, args);
                            HelperNotifier.AttachComboBoxNotifier(panel, args);
                            HelperNotifier.AttachCheckBoxNotifier(panel, args);
                        }
                    };
                }                
            }            
        }

    }
}
