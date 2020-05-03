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
    public class Stack : IStack
    {
        /// <inheritdoc/>
        public async Task Collapse(IReadOnlyList<Page> stack)
        {
            if (stack.Count > 1)
            {
                while (stack.Count > 1
                    && stack[stack.Count - 2] != null) //In shell, stack[0] is null
                {
                    var viewModel = stack.GetPreviousViewModel();
                    Shell.Current.Navigation.RemovePage(stack.GetPreviousPage());

                    if (viewModel is IOnViewRemoved removedViewModel)
                        await removedViewModel.OnViewRemovedAsync();
                }
            }
        }

        /// <inheritdoc/>
        public Page GetCurrentPage(IReadOnlyList<Page> stack)
        {
            return InternalGetCurrentPage(stack);
        }

        private Page InternalGetCurrentPage(IReadOnlyList<Page> stack)
        {
            var page = stack[stack.Count - 1];
            if (page is NavigationPage)
                return (page as NavigationPage).RootPage;

            return page;
        }

        /// <inheritdoc/>
        public Page GetPreviousPage(IReadOnlyList<Page> stack)
        {
            return InternalGetPreviousPage(stack);
        }

        private Page InternalGetPreviousPage(IReadOnlyList<Page> stack)
        {
            if (stack.Count > 1
                && stack[stack.Count - 2] != null) //In shell, stack[0] is null
            {
                var page = stack[stack.Count - 2];
                if (page is NavigationPage)
                    return (page as NavigationPage).RootPage;

                return page;
            }

            return null;
        }

        /// <inheritdoc/>
        public object GetCurrentViewModel(IReadOnlyList<Page> stack)
        {
            return InternalGetCurrentPage(stack).BindingContext;
        }

        /// <inheritdoc/>
        public object GetPreviousViewModel(IReadOnlyList<Page> stack)
        {
            if (stack.Count > 1)
            {
                return InternalGetPreviousPage(stack)?.BindingContext;
            }

            return null;
        }
    }
}
