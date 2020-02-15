﻿using System.Threading.Tasks;
using MvvmHelpers;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// An optional BaseViewModel that implements the <see cref="IAdaptorViewModel"/> interface
    /// and extends <see cref="BaseViewModel"/> from the dependency <see cref="MvvmHelpers"/>
    /// </summary>
    public abstract class AdaptorViewModel : BaseViewModel, IAdaptorViewModel
    {
        /// <summary>
        /// Runs automatically once the associated page is pushed onto the <see cref="NavController.NavigationStack"/>
        /// </summary>
        /// <param name="navigationData">Any data which could be useful for ViewModel Initialisation</param>
        /// <returns></returns>
        public virtual Task InitializeAsync(object navigationData)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Runs automatically after the associated page appears ontop of the stack
        /// More precisely, if the page is pushed, or if a page above it was popped
        /// </summary>
        /// <returns></returns>
        public virtual Task OnAppearing()
        {
            return Task.FromResult(false);
        }
    }
}