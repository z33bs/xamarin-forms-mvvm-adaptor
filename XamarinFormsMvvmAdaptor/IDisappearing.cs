using System;
namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Handles the Page.Disappearing event
    /// </summary>
    public interface IDisappearing
    {
        void OnViewDisappearing(object sender, EventArgs e);
    }
}
