using System;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Exception thrown when <c>Resolve</c> is called for a type that has not been registered
    /// </summary>
    public class TypeNotRegisteredException : Exception
    {
        /// <summary>
        /// Exception thrown when <c>Resolve</c> is called for a type that has not been registered
        /// </summary>
        public TypeNotRegisteredException(string message)
            : base(message)
        {
        }
        /// <summary>
        /// Exception thrown when <c>Resolve</c> is called for a type that has not been registered
        /// </summary>
        public TypeNotRegisteredException(string message, Exception innerException)
            : base(message,innerException)
        {
        }

    }
}