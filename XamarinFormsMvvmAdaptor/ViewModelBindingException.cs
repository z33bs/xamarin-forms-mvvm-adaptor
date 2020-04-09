using System;
namespace XamarinFormsMvvmAdaptor
{
    public class ViewModelBindingException : Exception
    {
        public ViewModelBindingException() : base(DefaultMessage())
        { }

        public ViewModelBindingException(Type type) : base(DefaultMessage(type))
        { }

        private static string DefaultMessage()
        {
            return "AutoWireViewModel failed. " +
                "Check if you are following the naming conventions.";
        }

        private static string DefaultMessage(Type type)
        {
            return $"AutoWireViewModel failed for {type.Name}. " +
                "Check if you are following the naming conventions.";
        }

    }
}
