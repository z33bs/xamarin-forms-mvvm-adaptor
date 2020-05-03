using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// For unit testing and mocking of <see cref="StackExtensions"/>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IStack
    {
        /// <summary>
        /// For unit testing and mocking of <see cref="StackExtensions"/>
        /// </summary>
        Task Collapse(IReadOnlyList<Page> stack);
        /// <summary>
        /// For unit testing and mocking of <see cref="StackExtensions"/>
        /// </summary>
        Page GetCurrentPage(IReadOnlyList<Page> stack);
        /// <summary>
        /// For unit testing and mocking of <see cref="StackExtensions"/>
        /// </summary>
        object GetCurrentViewModel(IReadOnlyList<Page> stack);
        /// <summary>
        /// For unit testing and mocking of <see cref="StackExtensions"/>
        /// </summary>
        Page GetPreviousPage(IReadOnlyList<Page> stack);
        /// <summary>
        /// For unit testing and mocking of <see cref="StackExtensions"/>
        /// </summary>
        object GetPreviousViewModel(IReadOnlyList<Page> stack);
    }
}