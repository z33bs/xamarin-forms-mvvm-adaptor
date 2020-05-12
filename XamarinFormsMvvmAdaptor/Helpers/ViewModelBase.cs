namespace XamarinFormsMvvmAdaptor.Helpers
{
    ///<inheritdoc/>
    public abstract class ViewModelBase : ObservableObject, IViewModelBase
    {
        string? title = string.Empty;
        ///<inheritdoc/>
        public string? Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        string? icon = string.Empty;
        ///<inheritdoc/>
        public string? Icon
        {
            get => icon;
            set => SetProperty(ref icon, value);
        }

        bool isBusy;
        ///<inheritdoc/>
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
        ///<inheritdoc/>
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
