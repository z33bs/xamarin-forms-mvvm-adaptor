using System;
namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Handles the Page.Appearing event
    /// </summary>
    public interface IAppearing
    {
        void OnViewAppearing(object sender, EventArgs e);
    }
}
