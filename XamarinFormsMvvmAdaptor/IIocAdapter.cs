using System;
using System.Reflection;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Class that wraps an externally provided Di/Ioc engine to implement the <see cref="IIoc"/> interface.
    /// </summary>
    public class IIocAdapter : IIoc
    {
        readonly object container;
        readonly MethodInfo resolveMethod;
        readonly string interfaceName;
        readonly bool isExtensionMethod;

        /// <summary>
        /// Wrap your chosen Di/Ioc engine so that it can be used by 
        /// the <see cref="ViewModelLocator"/>
        /// </summary>
        /// <param name="container">Your chosen external Di/Ioc engine</param>
        /// <param name="resolveMethod">Name of the method that takes a <c>Type</c> as a parameter and returns an <c>object</c>. Hint: use <c>nameof()</c></param>
        /// <param name="interfaceName">Only necessary if you need to call an explicit interface member</param>
        public IIocAdapter(object container, string resolveMethod, string interfaceName = null)
        {
            this.container = container;
            this.interfaceName = interfaceName;
            this.resolveMethod = container.GetType().GetMethod(resolveMethod, new[] { typeof(Type) }) ?? throw new ArgumentException(nameof(IIocAdapter)+" could not attach to provided method '"+resolveMethod+"'");
        }

        /// <summary>
        /// Wrap your chosen Di/Ioc engine so that it can be used by 
        /// the <see cref="ViewModelLocator"/>. Overload for engines that use an extension method./>
        /// </summary>
        /// <param name="container">Your chosen external Di/Ioc engine</param>
        /// <param name="resolveMethod">Name of the method that takes a <c>Type</c> as a parameter and returns an <c>object</c>. Hint: use <c>nameof()</c></param>
        /// <param name="resolutionExtensionType">Class that contains the extension method with the <paramref name="resolveMethod"/></param>
        public IIocAdapter(object container, Type resolutionExtensionType, string resolveMethod)
        {
            this.isExtensionMethod = true;
            this.container = container;
            this.resolveMethod = resolutionExtensionType.GetMethod(resolveMethod, new[] { container.GetType(), typeof(Type) }) ?? throw new ArgumentException(nameof(IIocAdapter) + " could not attach to provided method '" + resolveMethod + "'");
        }

        /// <summary>
        /// Resolve an object from the Di container
        /// </summary>
        public object Resolve(Type typeToResolve)
        {
            if(isExtensionMethod)
                return resolveMethod.Invoke(null, new object[] { container, typeToResolve });

            if (!string.IsNullOrEmpty(interfaceName))
                return
                    container.GetType().GetInterface(interfaceName)
                    .InvokeMember(
                        resolveMethod.Name,
                        BindingFlags.InvokeMethod,
                        null,
                        container,
                        new object[] { typeToResolve });

            return resolveMethod.Invoke(container, new object[] { typeToResolve });
        }
    }
}
