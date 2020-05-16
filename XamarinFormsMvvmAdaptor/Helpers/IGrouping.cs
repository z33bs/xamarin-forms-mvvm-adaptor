using System.Collections.Generic;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// Grouping of items by key into ObservableRange
    /// </summary>
    public interface IGrouping<TKey, TItem>
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        TKey Key { get; }
        /// <summary>
        /// Returns list of items in the grouping.
        /// </summary>
        IList<TItem> Items { get; }
    }
}