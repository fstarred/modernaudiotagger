using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ModernAudioTagger.Helpers
{
    /// <summary>
    /// Attached Property for TextBoxBase
    /// Invoke a Command after some delay fired from TextChanged event 
    /// </summary>
    /// NOTE: Doesn't work yet
    static class TextChangedCommandDelay
    {
        #region Fields

        const double DEFAULT_DELAY = 2.0;

        #endregion

        #region DependencyProperty

        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.RegisterAttached("Command", typeof(ICommand),
           typeof(TextChangedCommandDelay), new FrameworkPropertyMetadata(OnCommandPropertyChanged));

        public static readonly DependencyProperty CommandParameterProperty =
           DependencyProperty.RegisterAttached("CommandParameter", typeof(object),
           typeof(TextChangedCommandDelay));

        public static readonly DependencyProperty DelayProperty =
           DependencyProperty.RegisterAttached("Delay", typeof(Double),
           typeof(TextChangedCommandDelay), new PropertyMetadata(DEFAULT_DELAY));

        private static readonly DependencyProperty MreProperty =
           DependencyProperty.RegisterAttached("Mre", typeof(ManualResetEvent),
           typeof(TextChangedCommandDelay));

        private static readonly DependencyProperty IsUpdatingProperty =
           DependencyProperty.RegisterAttached("IsUpdating", typeof(bool),
           typeof(TextChangedCommandDelay));

        #endregion

        #region Properties

        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        private static ManualResetEvent GetMre(DependencyObject dp)
        {
            return (ManualResetEvent)dp.GetValue(MreProperty);
        }

        private static void SetMre(DependencyObject dp, ManualResetEvent value)
        {
            dp.SetValue(MreProperty, value);
        }

        public static Double GetDelay(DependencyObject dp)
        {
            return (Double)dp.GetValue(DelayProperty);
        }

        public static void SetDelay(DependencyObject dp, Double value)
        {
            dp.SetValue(DelayProperty, value);
        }

        public static void SetCommand(DependencyObject dp, ICommand value)
        {
            dp.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject dp)
        {
            return (ICommand)dp.GetValue(CommandProperty);
        }

        public static void SetCommandParameter(DependencyObject dp, object value)
        {
            dp.SetValue(CommandParameterProperty, value);
        }

        public static object GetCommandParameter(DependencyObject dp)
        {
            return (object)dp.GetValue(CommandParameterProperty);
        }

        #endregion

        #region Metadata

        private static void OnCommandPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TextBoxBase txtboxbase = obj as TextBoxBase;

            Double delay = (Double)txtboxbase.GetValue(DelayProperty);            
            ICommand command = (ICommand)e.NewValue;                        

            txtboxbase.TextChanged -= txtboxbase_TextChanged;
            txtboxbase.TextChanged += txtboxbase_TextChanged;

            int delayInMs = (int)(delay * 1000);

            ManualResetEvent mre = new ManualResetEvent(false);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    mre.WaitOne();

                    command.Execute(obj.GetValue(CommandParameterProperty));

                    Thread.Sleep(delayInMs);

                    if (GetIsUpdating(obj) == false)
                        mre.Reset();

                }
            }, TaskCreationOptions.LongRunning);
            
            //var input = Observable.FromEventPattern<TextChangedEventArgs>(txtboxbase, "TextChanged")
            //    .Select(evt => ((TextBox)evt.Sender).Text)
            //    .Throttle(TimeSpan.FromSeconds(delay))
            //    .DistinctUntilChanged()
            //    .ObserveOnDispatcher()
            //    .Subscribe((o) =>
            //    {
            //        command.Execute(obj.GetValue(CommandParameterProperty));
            //    });
        }

        static void txtboxbase_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetIsUpdating((DependencyObject)sender, true);            
        }


        #endregion

    }
}
