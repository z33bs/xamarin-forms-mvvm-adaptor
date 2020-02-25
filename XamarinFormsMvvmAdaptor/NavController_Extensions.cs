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
            return stack[stack.Count - 1];
        }

        /// <summary>
        /// Returns the page beneath the top-most page of the stack
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static Page GetPreviousPage(this IReadOnlyList<Page> stack)
        {
            if(stack.Count > 1)
                return stack[stack.Count - 2];

            return null;
        }

        /// <summary>
        /// Returns the <see cref="IAdaptorViewModel"/> associated with the top-most page of the stack.
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static IAdaptorViewModel GetCurrentViewModel(this IReadOnlyList<Page> stack)
        {
            var page = stack[stack.Count - 1] as Page;
            
            if (page.BindingContext is null)
                throw new BindingContextNotSetException();
            if (!(page.BindingContext is IAdaptorViewModel))
                throw new NotIAdaptorViewModelException();

            return page.BindingContext as IAdaptorViewModel;
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
                var page = stack[stack.Count - 2] as Page;

                if (page.BindingContext is null)
                    throw new BindingContextNotSetException();
                if (!(page.BindingContext is IAdaptorViewModel))
                    throw new NotIAdaptorViewModelException();

                return page.BindingContext as IAdaptorViewModel;
            }

            return null;
        }

    }
}
