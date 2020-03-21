using System.Collections.Generic;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Extension Methods for NavController
    /// </summary>
    public static class NavControllerExtensions
	{
        /// <summary>
        /// Returns the top-most page of the stack.
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static Page GetCurrentPage(this IReadOnlyList<Page> stack)
        {
            return InternalGetCurrentPage(stack);
        }

        private static Page InternalGetCurrentPage(IReadOnlyList<Page> stack)
        {
            var page = stack[stack.Count - 1];
            if (page is NavigationPage)
                return (page as NavigationPage).RootPage;

            return page;
        }

        /// <summary>
        /// Returns the page beneath the top-most page of the stack
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static Page GetPreviousPage(this IReadOnlyList<Page> stack)
        {
            return InternalGetPreviousPage(stack);
        }

        private static Page InternalGetPreviousPage(IReadOnlyList<Page> stack)
        {
            if (stack.Count > 1)
            {
                var page = stack[stack.Count - 2];
                if (page is NavigationPage)
                    return (page as NavigationPage).RootPage;

                return page;
            }

            return null;
        }

        /// <summary>
        /// Returns the <see cref="IAdaptorViewModel"/> associated with the top-most page of the stack.
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static IAdaptorViewModel GetCurrentViewModel(this IReadOnlyList<Page> stack)
        {
            return InternalGetCurrentPage(stack).BindingContext as IAdaptorViewModel;
        }

        /// <summary>
        /// Returns the <see cref="IAdaptorViewModel"/> associated with the page beneath the top-most page of the stack
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static IAdaptorViewModel GetPreviousViewModel(this IReadOnlyList<Page> stack)
        {
            if (stack.Count > 1)
            {
                return InternalGetPreviousPage(stack).BindingContext as IAdaptorViewModel;
            }

            return null;
        }

    }
}
