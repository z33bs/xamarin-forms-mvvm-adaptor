using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XamarinFormsMvvmAdaptor
{
    public interface IObservableObject : INotifyPropertyChanged
    {
        void OnPropertyChanged([CallerMemberName] string propertyName = "");

        bool SetProperty<T>(
            ref T backingStore,
            T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null,
            Func<T, T, bool> validateValue = null);
    }
}