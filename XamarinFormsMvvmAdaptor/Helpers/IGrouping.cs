using System.Collections.Generic;

namespace XamarinFormsMvvmAdaptor
{
    public interface IGrouping<TKey, TItem>
    {
        TKey Key { get; }
        IList<TItem> Items { get; }
    }
}