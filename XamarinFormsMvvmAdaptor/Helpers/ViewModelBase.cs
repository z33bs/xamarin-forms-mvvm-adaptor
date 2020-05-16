namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// Commonly used Properties in ViewModel that Raise PropertyChanged event
    /// </summary>
    public abstract class ViewModelBase : ObservableObject, IViewModelBase
    {
        string title = string.Empty;
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        string icon = string.Empty;
        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        public string Icon
        {
            get => icon;
            set => SetProperty(ref icon, value);
        }

        bool isBusy;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        /// <value><c>true</c> if this instance is busy; otherwise, <c>false</c>.</value>
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (SetProperty(ref isBusy, value))
                    IsNotBusy = !isBusy;
            }
        }

        bool isNotBusy = true;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is not busy.
        /// </summary>
        /// <value><c>true</c> if this instance is not busy; otherwise, <c>false</c>.</value>
        public bool IsNotBusy
        {
            get => isNotBusy;
            set
            {
                if (SetProperty(ref isNotBusy, value))
                    IsBusy = !isNotBusy;
            }
        }
    }
}
