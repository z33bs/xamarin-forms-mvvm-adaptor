﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Extension Methods for Navigation Stacks
    /// </summary>
    public static class StackExtensions
	{
        /// <summary>
        /// Removes all Pages behind the Top Page of the <see cref="INavigationService.NavigationStack"/>
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static async Task Collapse(this IReadOnlyList<Page> stack)
        {
            if (stack.Count > 1)
            {
                while (stack.Count > 1
                    && stack[stack.Count - 2] != null) //In shell, stack[0] is null
                {
                    var viewModel = stack.GetPreviousViewModel();
                    Shell.Current.Navigation.RemovePage(stack.GetPreviousPage());

                    if(viewModel is IOnViewRemoved removedViewModel)
                        await removedViewModel.OnViewRemovedAsync();
                }
            }
        }

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
            if (stack.Count > 1
                && stack[stack.Count-2] != null) //In shell, stack[0] is null
            {
                var page = stack[stack.Count - 2];
                if (page is NavigationPage)
                    return (page as NavigationPage).RootPage;

                return page;
            }

            return null;
        }

        /// <summary>
        /// Returns the ViewModel bound with the top-most page of the stack.
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static object GetCurrentViewModel(this IReadOnlyList<Page> stack)
        {
            return InternalGetCurrentPage(stack).BindingContext;
        }

        /// <summary>
        /// Returns the ViewModel bound with the page beneath the top-most page of the stack
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static object GetPreviousViewModel(this IReadOnlyList<Page> stack)
        {
            if (stack.Count > 1)
            {
                return InternalGetPreviousPage(stack)?.BindingContext;
            }

            return null;
        }

    }
}
