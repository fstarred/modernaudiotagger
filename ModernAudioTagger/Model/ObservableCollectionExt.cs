using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace ModernAudioTagger.Model
{
    public class ObservableCollectionExt<T> : ObservableCollection<T> 
    {
        private bool fireCollectionChanged = true;

        public ObservableCollectionExt()
            : base()
        {
            
        }

        public void AddRange(IEnumerable<T> collection)
        {            
            foreach (var i in collection) 
                Items.Add(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T>(collection)));            
        }

        public void RemoveRange(IEnumerable<T> collection)
        {
            foreach (var i in collection) 
                Items.Remove(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<T>(collection)));
        }

        /*
         * if refreshCurrentIndex = false, clear collection without firing OnCollectionChanged,
         * in order to let the current track playing
         * */
        public void Clear(bool refreshCurrentIndex)
        {
            this.fireCollectionChanged = refreshCurrentIndex;
            base.ClearItems();
            this.fireCollectionChanged = true;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (fireCollectionChanged)
                base.OnCollectionChanged(e);
        }

        protected override void ClearItems()
        {
            base.ClearItems();
        }        
    }
}
