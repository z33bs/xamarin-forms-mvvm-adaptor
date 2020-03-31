using System;

namespace XamarinFormsMvvmAdaptor
{
    public class TypeNotRegisteredException : Exception
    {
        public TypeNotRegisteredException(string message)
            : base(message)
        {
        }
        public TypeNotRegisteredException(string message, Exception innerException)
            : base(message,innerException)
        {
        }

    }
}