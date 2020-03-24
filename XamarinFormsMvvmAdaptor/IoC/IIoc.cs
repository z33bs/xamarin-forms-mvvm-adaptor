﻿using System;
using System.Linq.Expressions;
using XamarinFormsMvvmAdaptor.FluentApi;

namespace XamarinFormsMvvmAdaptor
{
    public interface IIoc
    {
        IRegisterOptions Register<T>(Scope scope = Scope.Local) where T : notnull;
        IInstanceRegisterOptions Register(object concreteInstance);
        IInstanceRegisterOptions Register(Func<object> @delegate);//Expression<Func<object>> expression);
        T Resolve<T>() where T : notnull;
        object Resolve(Type typeToResolve);
        bool IsRegistered<T>() where T : notnull;
        bool IsRegistered(Type typeToResolve);
        //void Use3rdPartyContainer(IIocContainer iocContainer);
    }
}
