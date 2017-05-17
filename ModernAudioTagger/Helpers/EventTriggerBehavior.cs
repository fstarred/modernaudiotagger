using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interactivity;

namespace ModernAudioTagger.Helpers
{
    /// <summary>
    /// Detach EventTrigger from element on Unload event
    /// </summary>
    class EventTriggerBehavior : Behavior<DependencyObject>
    {
        public static readonly DependencyProperty AttacherProperty =
            DependencyProperty.RegisterAttached(
                "Attacher",
                typeof(FrameworkElement),
                typeof(EventTriggerBehavior),
                new FrameworkPropertyMetadata(OnAttacherChanged)
                );

        public static FrameworkElement GetAttacher(DependencyObject dp)
        {
            return (FrameworkElement)dp.GetValue(AttacherProperty);
        }

        public static void SetAttacher(DependencyObject dp, FrameworkElement value)
        {
            dp.SetValue(AttacherProperty, value);
        }

        private static void OnAttacherChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)e.NewValue;

            System.Windows.Interactivity.EventTrigger eve = (System.Windows.Interactivity.EventTrigger)((EventTriggerBehavior)obj).AssociatedObject;

            fe.Unloaded += (sender, re) => { eve.Detach(); };
        }

        public EventTriggerBehavior()
        {
            
        }

        protected override void OnAttached()
        {
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
