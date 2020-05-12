using System;
namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Implements the <see cref="OnViewAppearing(object, EventArgs)"/> method
    /// </summary>
    public interface IOnViewAppearing
    {
        /// <summary>
        /// Handles the <see cref="Xamarin.Forms.Page.Appearing"/> event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnViewAppearing(object sender, EventArgs e);
    }
}
