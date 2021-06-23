// <copyright file="DisposableObservableCollection.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace BluetoothLEExplorer.Models
{
    /// <summary>
    /// Helper collection class that disposes of items when they are removed
    /// </summary>
    public class DisposableObservableCollection<T> : ObservableCollection<T>, IDisposable where T : IDisposable
    {
        public void Dispose()
        {
            this.ToList().ForEach(disposable => disposable.Dispose());
        }

        new public void Clear()
        {
            var temp = this.ToList();
            base.Clear();
            temp.ForEach(disposable => disposable.Dispose());
        }

        new public void ClearItems()
        {
            var temp = this.ToList();
            base.ClearItems();
            temp.ForEach(disposable => disposable.Dispose());
        }

        new public bool Remove(T item)
        {
            T temp = item;
            bool result = base.Remove(item);
            if (result)
            {
                item.Dispose();
            }
            return result;
        }

        new public void RemoveAt(int index)
        {
            T temp = this.ElementAt(index);
            base.RemoveAt(index);
            temp.Dispose();
        }

        new public void RemoveItem(int index)
        {
            T temp = this.ElementAt(index);
            base.RemoveItem(index);
            temp.Dispose();
        }
    }
}