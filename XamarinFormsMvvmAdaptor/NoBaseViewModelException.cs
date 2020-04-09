using System;
namespace XamarinFormsMvvmAdaptor
{
    public class NoBaseViewModelException : Exception
    {
        public NoBaseViewModelException() : base(DefaultMessage())
        { }

        public NoBaseViewModelException(Type type) : base(DefaultMessage(type))
        {}

        private static string DefaultMessage()
        {
            return $"ViewModel is expected to implement {nameof(IMvvmViewModelBase)}";
        }

        private static string DefaultMessage(Type type)
        {
            return $"{type.Name} is expected to implement {nameof(IMvvmViewModelBase)}";
        }
    }
}
