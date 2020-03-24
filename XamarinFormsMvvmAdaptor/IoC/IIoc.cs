using System;
using XamarinFormsMvvmAdaptor.FluentApi;

namespace XamarinFormsMvvmAdaptor
{
    public interface IIoc
    {
        void ConfigureResolveMode(bool isStrictMode = true);
        bool IsRegistered<T>() where T : notnull;
        bool IsRegistered(Type typeToResolve);
        Scope IsRegisteredScope<T>() where T : notnull;
        Scope IsRegisteredScope(Type typeToResolve);
        IRegisterOptions Register<T>(Scope scope = Scope.Local) where T : notnull;
        IInstanceRegisterOptions Register(object concreteInstance, Scope scope = Scope.Local);
        T Resolve<T>() where T : notnull;
        object Resolve(Type typeToResolve);
    }
}