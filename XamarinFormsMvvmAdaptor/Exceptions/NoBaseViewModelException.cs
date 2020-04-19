using System;
using XamarinFormsMvvmAdaptor.Helpers;

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
            return $"ViewModel is expected to implement {nameof(IBaseViewModel)}";
        }

        private static string DefaultMessage(Type type)
        {
            return $"{type.Name} is expected to implement {nameof(IBaseViewModel)}";
        }
    }
}
