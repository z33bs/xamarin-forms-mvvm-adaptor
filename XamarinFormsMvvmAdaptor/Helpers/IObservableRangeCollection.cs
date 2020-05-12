using System.Collections.Generic;
using System.Collections.Specialized;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary> 
    /// Represents a dynamic data collection that provides notifications
    /// when items get added, removed, or when the whole list is refreshed. 
    /// </summary> 
    /// <typeparam name="T"></typeparam> 
    public interface IObservableRangeCollection<T>
    {
        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
        /// </summary> 
        void AddRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add);

        /// <summary> 
        /// Removes the first occurence of each item in the specified collection from ObservableCollection(Of T).
        /// NOTE: with notificationMode = Remove, removed items starting index is not set
        /// because items are not guaranteed to be consecutive.
        /// </summary> 
        void RemoveRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Reset);

        /// <summary> 
        /// Clears the current collection and replaces it with the specified item. 
        /// </summary> 
        void Replace(T item);

        /// <summary> 
        /// Clears the current collection and replaces it with the specified collection. 
        /// </summary> 
        void ReplaceRange(IEnumerable<T> collection);
    }
}