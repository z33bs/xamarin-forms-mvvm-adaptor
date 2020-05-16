using System;
namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Generic interface for basic IoC implementation
    /// </summary>
    public interface IIocContainer
    {
        /// <summary>
        /// Resolve the specified <typeparamref name="TService"/>
        /// </summary>
        TService Resolve<TService>() where TService : notnull;

        /// <summary>
        /// Resolve the specified <paramref name="service"/>
        /// </summary>
        object Resolve(Type service);

        /// <summary>
        /// Check if the specified <typeparamref name="TService"/>
        ///  is registered
        /// </summary>
        bool IsRegistered<TService>() where TService : notnull;

        /// <summary>
        /// Check if the specified <paramref name="service"/>
        ///  is registered
        /// </summary>
        bool IsRegistered(Type service);
    }
}
