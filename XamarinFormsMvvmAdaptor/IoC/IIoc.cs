using System;
using System.Collections.Generic;
using XamarinFormsMvvmAdaptor.FluentApi;

namespace XamarinFormsMvvmAdaptor
{
    public interface IIoc
    {
        void ConfigureResolveMode(bool isStrictMode);
        bool IsRegistered<T>() where T : notnull;
        bool IsRegistered(Type typeToResolve);
        bool IsRegistered(string key);
        string ListRegistrations(Scope scope);
        Scope GetScope<T>() where T : notnull;
        Scope GetScope(Type typeToResolve);
        Scope GetScope(string key);
        IRegisterOptions Register<T>(Scope scope = Scope.Local) where T : notnull;
        IInstanceRegisterOptions Register(object concreteInstance, Scope scope = Scope.Local);
        void Remove<T>() where T : notnull;
        void Remove<T>(Scope scope) where T : notnull;
        void Remove(string key);
        void Remove(string key, Scope scope);
        T Resolve<T>() where T : notnull;
        object Resolve(Type typeToResolve);
        object Resolve(string key);
        void Teardown();
    }
}