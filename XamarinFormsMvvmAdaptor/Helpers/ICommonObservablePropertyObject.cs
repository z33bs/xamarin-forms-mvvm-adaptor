namespace XamarinFormsMvvmAdaptor
{
    public interface ICommonObservablePropertyObject : IObservableObject
    {
        string Title { get; set; }
        string Icon { get; set; }
        bool IsBusy { get; set; }
        bool IsNotBusy { get; set; }
    }
}