using System;
namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Exception thrown when <see cref="ViewModelLocator.AutoWireViewModel(Xamarin.Forms.Page)"/> fails
    /// </summary>
    public class ViewModelBindingException : Exception
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ViewModelBindingException"/> class
        /// </summary>
        public ViewModelBindingException() : base(DefaultMessage())
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="ViewModelBindingException"/> class
        /// </summary>
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
