using System.Windows;
using System.Windows.Input;

namespace ModernAudioTagger.Helpers
{
    class DragDropBehaviour
    {
        #region DependencyProperties

        public static readonly DependencyProperty DragEnterCommandProperty =
            DependencyProperty.RegisterAttached("DragEnterCommand", typeof(ICommand), typeof(DragDropBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(DragEnterCommandChanged)));

        public static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.RegisterAttached("DropCommand", typeof(ICommand), typeof(DragDropBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(DropCommandChanged))); 

        #endregion

        #region DependencyPropertiesCallback

        private static void DragEnterCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.DragEnter += element_DragEnter;
        }

        private static void DropCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.Drop += element_Drop;
        } 
        #endregion

        #region Events
        
        static void element_DragEnter(object sender, DragEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetDragEnterCommand(element);

            command.Execute(e);
        }


        static void element_Drop(object sender, DragEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetDropCommand(element);

            command.Execute(e);
        } 

        #endregion

        #region Properties

        public static void SetDragEnterCommand(UIElement element, ICommand value)
        {
            element.SetValue(DragEnterCommandProperty, value);
        }

        public static ICommand GetDragEnterCommand(UIElement element)
        {
            return (ICommand)element.GetValue(DragEnterCommandProperty);
        }


        public static void SetDropCommand(UIElement element, ICommand value)
        {
            element.SetValue(DropCommandProperty, value);
        }

        public static ICommand GetDropCommand(UIElement element)
        {
            return (ICommand)element.GetValue(DropCommandProperty);
        } 
        #endregion
    }
}
