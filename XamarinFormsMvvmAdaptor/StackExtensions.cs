using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Extension Methods for Navigation Stacks
    /// </summary>
    public static class StackExtensions
	{
        private static readonly IStack defaultImplementation = new Stack();

        /// <summary>
        /// For unit testing and mocking of <see cref="StackExtensions"/>
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static IStack Implementation { private get; set; } = defaultImplementation;
        /// <summary>
        /// For unit testing and mocking of <see cref="StackExtensions"/>
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void RevertToDefaultImplementation() => Implementation = defaultImplementation;

        /// <summary>
        /// Removes all Pages behind the Top Page of the <see cref="INavigationService.NavigationStack"/>
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static Task Collapse(this IReadOnlyList<Page> stack)
            => Implementation.Collapse(stack);

        /// <summary>
        /// Returns the top-most page of the stack.
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static Page GetCurrentPage(this IReadOnlyList<Page> stack)
            => Implementation.GetCurrentPage(stack);

        /// <summary>
        /// Returns the page beneath the top-most page of the stack
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static Page GetPreviousPage(this IReadOnlyList<Page> stack)
            => Implementation.GetPreviousPage(stack);

        /// <summary>
        /// Returns the ViewModel bound with the top-most page of the stack.
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static object GetCurrentViewModel(this IReadOnlyList<Page> stack)
            => Implementation.GetCurrentViewModel(stack);

        /// <summary>
        /// Returns the ViewModel bound with the page beneath the top-most page of the stack
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static object GetPreviousViewModel(this IReadOnlyList<Page> stack)
            => Implementation.GetPreviousViewModel(stack);
    }
}
