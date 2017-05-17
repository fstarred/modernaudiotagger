using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ModernAudioTagger.Helpers
{
    public class ListBoxItemRefreshBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            ((INotifyCollectionChanged)AssociatedObject.Items).CollectionChanged += (sender, e) => 
            {
                if (e.OldItems != null)
                    AssociatedObject.Items.Refresh();
            };
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
