using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace SortedObservableCollection
{
    /// <summary>
    /// Sorted version of ObserverableCollection.
    /// </summary>
    /// <remarks>This code has some semaphores that make it look thread-safe. However, it is not
    /// and there are some known race conditions. To make a truely thread-safe sorted observerable collection
    /// this entire class would have to be rewritten and not inherit for ObserverableCollection.</remarks>
    /// <typeparam name="T"></typeparam>
    public class SortedObservableCollection<T> : ObservableCollection<T>
    {
        private IComparer comparer = null;
        private string comparePropertyName = string.Empty;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Compares 2 items based on given compare functions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int compare(T x, T y)
        {
            if (comparer == null)
            {
                return ((IComparable)x).CompareTo(y);
            }
            else
            {
                return comparer.Compare(x, y);
            }
        }

        /// <summary>
        /// Find the current index of an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private int FindCurrIndex(T item)
        {
            int ret = this.Count;

            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Equals(item) == true)
                {
                    ret = i;
                    return ret;
                }
            }

            return ret;
        }

        /// <summary>
        /// Find what index the item needs to move or inserted to
        /// </summary>
        /// <param name="item">Item to move or insert</param>
        /// <param name="newItem">Is this a new item or existing item</param>
        /// <returns></returns>
        private int FindNewIndex(T item, bool newItem = true)
        {
            int index = -1;

            if (this.Count == 0)
            {   // if the collection is empty, just return 0
                return 0;
            }
            if( this.Count == 1)
            {
                // if the collection only has 1 item, it's an easy compare
                if(compare(item, this[0]) >= 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            int firstLargerItem = -1;

            // Linear search through collection finding the first larger item
            for (int i = 0; i < this.Count; i++)
            {
                if (compare(item, this[i]) < 0)
                {
                    // break out of loop
                    firstLargerItem = i;
                    break;
                }
            }

            if(firstLargerItem == -1)
            {
                if(newItem)
                {
                    index = this.Count;
                }
                else
                {
                    index = this.Count - 1;
                }
            }
            else
            { 
                index = firstLargerItem;
            }

            return index;
        }

        public SortedObservableCollection()
        {
            if (typeof(IComparable).IsAssignableFrom(typeof(T)) == false)
            {
                throw new InvalidOperationException(String.Format("{0} does not impliment IComparable and no ICompare function is given", typeof(T).Name));
            }
        }

        /// <summary>
        /// Constructor for SortedObservableCollection
        /// </summary>
        /// <param name="comparer">Compare function used to sort. If none is given then item.compare is used.</param>
        /// <param name="comparePropertyName">(Optional) Name of the property that has to change for a resort to happen if one property is all that is used. 
        /// If a single property is all that is used to compare then this is checked when an item changes before a resort is calculacted which can
        /// create large optimizations. If none is given, resort will always be calculated and applied if required.</param>
        public SortedObservableCollection(IComparer comparer, string comparePropertyName = "")
        {
            this.comparer = comparer;
            this.comparePropertyName = comparePropertyName;
        }

        private async void NotifyableItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(comparePropertyName != String.Empty && e.PropertyName != this.comparePropertyName)
            if(String.IsNullOrEmpty(comparePropertyName) == false && String.Compare(e.PropertyName, comparePropertyName) != 0)
            {
                // The item changed, but not the property used to compare, no need to recalculate the sorting
                return;
            }

            try
            {
                await semaphore.WaitAsync();

                int curr = FindCurrIndex((T)sender);
                int next = FindNewIndex((T)sender, false);

                // If the updated item is shifting up, then there is room below it so we have to adjust
                // the index. The FindNextIndex essentially finds the next largest without knowing where
                // the current is
                if (next > 0 && next > curr)
                {
                    next--;
                    Debug.Assert(next >= 0);
                }

                if (curr != next)
                {
                    this.MoveItem(curr, next);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        protected async override void InsertItem(int index, T item)
        {
            INotifyPropertyChanged notifyableItem = item as INotifyPropertyChanged;

            if (notifyableItem != null)
            {
                notifyableItem.PropertyChanged += NotifyableItem_PropertyChanged;
            }

            try
            {
                await semaphore.WaitAsync();
                base.InsertItem(FindNewIndex(item), item);
            }
            finally
            {
                semaphore.Release();
            }
            
        }

        protected async override void RemoveItem(int index)
        {
            INotifyPropertyChanged notifyableItem = this[index] as INotifyPropertyChanged;

            if (notifyableItem != null)
            {
                notifyableItem.PropertyChanged -= NotifyableItem_PropertyChanged;
            }

            ///<remarks>This is a known race condition. A new item could get inserted
            ///after this function is called, before the semaphore is gotten which would
            ///invalidate the index. </remarks>
            try
            {
                await semaphore.WaitAsync();
                base.RemoveItem(index);
            }
            finally
            {
                semaphore.Release();
            }
        }

        protected async override void ClearItems()
        {
            try
            {
                await semaphore.WaitAsync();
                base.ClearItems();
            }
            finally
            {
                semaphore.Release();
            }
        }

        protected async override void SetItem(int index, T item)
        {
            try
            {
                await semaphore.WaitAsync();
                base.SetItem(index, item);
            }
            finally
            {
                semaphore.Release();
            }
            
        }
    }
}
