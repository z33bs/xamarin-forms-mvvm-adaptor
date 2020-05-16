using System;
using System.Collections.Generic;
using XamarinFormsMvvmAdaptor.FluentApi;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Lightweigh Inversion of Control container
    /// </summary>
    public interface IIoc
    {
        /// <summary>
        /// Configures whether to disable Smart Resolve
        /// where Ioc will attempt to resolve unregistered types.
        /// </summary>
        /// <param name="isStrictMode">When <c>true</c> will disable smart resolve</param>
        void ConfigureResolveMode(bool isStrictMode);

        /// <summary>
        /// Checks if the Type <typeparamref name="T"/> is registered
        /// </summary>
        bool IsRegistered<T>() where T : notnull;

        /// <summary>
        /// Checks whether the <paramref name="typeToResolve"/> is registered
        /// </summary>
        bool IsRegistered(Type typeToResolve);

        //todo what if multipe with same key but differet T?
        /// <summary>
        /// Checks whether a type with the specified <paramref name="key"/>
        ///  is registered
        /// </summary>
        bool IsRegistered(string key);

        /// <summary>
        /// Returns a list of all registrations in the container
        /// </summary>
        string ListRegistrations();

        //Scope GetScope<T>() where T : notnull;
        //Scope GetScope(Type typeToResolve);
        //Scope GetScope(string key);

        //IRegisterOptions Register<T>(Scope scope) where T : notnull;

        /// <summary>
        /// Register a type <typeparamref name="T"/>
        /// </summary>
        IRegisterOptions Register<T>() where T : notnull;

        //IInstanceRegisterOptions Register(object concreteInstance, Scope scope);

        /// <summary>
        /// Register a concrete instance as a singleton
        /// </summary>
        IInstanceRegisterOptions Register(object concreteInstance);

        //todo What if there are multiple T with different As?
        /// <summary>
        /// Remove the specified registration
        /// </summary>
        void Remove<T>() where T : notnull;

        //void Remove<T>(Scope scope) where T : notnull;

        /// <summary>
        /// Remove a registration with the specified <paramref name="key"/>
        /// </summary>
        void Remove(string key);

        //void Remove(string key, Scope scope);

        /// <summary>
        /// Resolve the specified Type <typeparamref name="T"/>
        /// </summary>
        T Resolve<T>() where T : notnull;

        /// <summary>
        /// Resolve the specified Type <paramref name="typeToResolve"/>
        /// </summary>
        object Resolve(Type typeToResolve);

        //todo what if multiple registrations with same key and different types?
        /// <summary>
        /// Resolve the registration with the specified <paramref name="key"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object Resolve(string key);

        /// <summary>
        /// Dispose of all registrations
        /// </summary>
        void PurgeContainer();
    }
}