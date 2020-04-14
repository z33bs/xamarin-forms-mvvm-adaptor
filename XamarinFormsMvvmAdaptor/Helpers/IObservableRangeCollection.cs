using System.Collections.Generic;
using System.Collections.Specialized;

namespace XamarinFormsMvvmAdaptor
{
    public interface IObservableRangeCollection<T>
    {
        void AddRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add);
        void RemoveRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Reset);
        void Replace(T item);
        void ReplaceRange(IEnumerable<T> collection);
    }
}