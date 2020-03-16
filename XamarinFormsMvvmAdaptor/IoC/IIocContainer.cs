using System;
namespace XamarinFormsMvvmAdaptor
{
    public interface IIocContainer
    {
        TService Resolve<TService>() where TService : notnull;
        bool IsRegistered<TService>() where TService : notnull;
    }
}
