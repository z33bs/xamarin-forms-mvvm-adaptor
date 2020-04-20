using System;
namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Implements the <see cref="OnViewDisappearing(object, EventArgs)"/> method
    /// </summary>
    public interface IOnViewDisappearing
    {
        /// <summary>
        /// Handles the <see cref="Xamarin.Forms.Page.Disappearing"/> event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnViewDisappearing(object sender, EventArgs e);
    }
}
