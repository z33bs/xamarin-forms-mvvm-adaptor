using System;
using System.Linq.Expressions;
using XamarinFormsMvvmAdaptor.FluentApi;

namespace XamarinFormsMvvmAdaptor
{
    public interface IIoc
    {
        IRegisterOptions Register<T>() where T : notnull;
        IInstanceRegisterOptions Register(object concreteInstance);
        IInstanceRegisterOptions Register(Func<object> @delegate);//Expression<Func<object>> expression);
        T Resolve<T>() where T : notnull;
        bool IsRegistered<T>() where T : notnull;
        void Use3rdPartyContainer(IIocContainer iocContainer);
    }
}
