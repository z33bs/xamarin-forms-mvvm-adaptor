using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XamarinFormsMvvmAdaptor
{
    //todo consider packing logic into IoC, then using field to determine if
    // 3rd party being used instead of interface IInternalIocContainer

    internal class IocContainer : IIocContainer, IInternalIocContainer
    {
        public IList<RegisteredObject> RegisteredObjects { get; } = new List<RegisteredObject>();

        public bool IsRegistered(Type service)
        {
        }
        public bool IsRegistered<TService>() where TService : notnull
        {            
        }

        public object Resolve(Type service)
        {
        }

        public TService Resolve<TService>() where TService : notnull
        {
            
        }


    }

    public interface IInternalIocContainer
    {
        IList<RegisteredObject> RegisteredObjects { get; }
    }
}
