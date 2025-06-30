//Collections/ObservableRangeCollection.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Collections
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        public ObservableRangeCollection() : base() { }

        public ObservableRangeCollection(IEnumerable<T> collection) : base(collection) { }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _suppressNotification = true;

            foreach (var item in items)
            {
                Add(item);
            }

            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _suppressNotification = true;

            foreach (var item in items.ToList())
            {
                Remove(item);
            }

            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Replace(T item, T replacement)
        {
            ReplaceRange(new[] { item }, new[] { replacement });
        }

        public void ReplaceRange(IEnumerable<T> itemsToRemove, IEnumerable<T> itemsToAdd)
        {
            if (itemsToRemove == null)
                throw new ArgumentNullException(nameof(itemsToRemove));
            if (itemsToAdd == null)
                throw new ArgumentNullException(nameof(itemsToAdd));

            _suppressNotification = true;

            foreach (var item in itemsToRemove.ToList())
            {
                Remove(item);
            }

            foreach (var item in itemsToAdd)
            {
                Add(item);
            }

            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset(IEnumerable<T> range)
        {
            _suppressNotification = true;

            Clear();

            AddRange(range);

            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnPropertyChanged(e);
        }
    }
}