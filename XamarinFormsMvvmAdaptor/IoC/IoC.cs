using System;
using System.Linq;
using XamarinFormsMvvmAdaptor.FluentApi;

namespace XamarinFormsMvvmAdaptor
{
    internal sealed class Ioc : IIoc
    //Interfaces controll fluent-Api grammer
    {
        private IIocContainer container = new IocContainer();

        const string CANT_REGISTER_EXCEPTION =
            "Can't Register this way because you're using 3rd party container. " +
            "Use the 3rd party's interface to register your classes " +
            "before swapping-in the container";

        public void Use3rdPartyContainer(IIocContainer iocContainerAdaptor)
        {
            if (iocContainerAdaptor is IIocContainer)
                container = iocContainerAdaptor;
            else
                throw new NotImplementedException(
                    $"Container provided does not implement {nameof(IIocContainer)}." +
                    $"Use the Adaptor pattern if needed");
        }


        #region Registration
        public IRegisterOptions Register<T>() where T : notnull
        {
            if (container is IInternalIocContainer internalIocContainer)
                internalIocContainer.RegisteredObjects.Add(new RegisteredObject(
                typeof(T)
                , typeof(T)
                , LifeCycle.Transient
                ));
            else
                throw new InvalidOperationException(CANT_REGISTER_EXCEPTION);
            return new RegisterOptions(container);
        }

        public IInstanceRegisterOptions Register(object concreteInstance)
        {
            if (container is IInternalIocContainer internalIocContainer)
                internalIocContainer.RegisteredObjects.Add(
                    new RegisteredObject(concreteInstance));
            else
                throw new InvalidOperationException(CANT_REGISTER_EXCEPTION);
            return new InstanceRegisterOptions(container);
        }
        #endregion
        #region Resolution
        public T Resolve<T>() where T : notnull
        {
            return container.Resolve<T>();
        }

        public bool IsRegistered<T>() where T : notnull
        {
            return container.IsRegistered<T>();
        }
        #endregion

        private class RegisterOptions : IRegisterOptions
        {
            readonly IIocContainer container;
            public RegisterOptions(IIocContainer container)
            {
                this.container = container;
            }
            public ILifeCycleOptions As<TypeToResolve>() where TypeToResolve : notnull
            {
                if (container is IInternalIocContainer internalIocContainer)
                    internalIocContainer.RegisteredObjects.Last()
                        .TypeToResolve = typeof(TypeToResolve);
                return this;
            }


            public void SingleInstance()
            {
                if (container is IInternalIocContainer internalIocContainer)
                    internalIocContainer.RegisteredObjects.Last()
                        .LifeCycle = LifeCycle.Singleton;
            }

            public void MultiInstance()
            {
                if (container is IInternalIocContainer internalIocContainer)
                    internalIocContainer.RegisteredObjects.Last()
                        .LifeCycle = LifeCycle.Transient;
            }
        }
        private class InstanceRegisterOptions : IInstanceRegisterOptions
        {
            readonly IIocContainer container;
            public InstanceRegisterOptions(IIocContainer container)
            {
                this.container = container;
            }

            public void As<TypeToResolve>() where TypeToResolve : notnull
            {
                if (container is IInternalIocContainer internalIocContainer)
                    internalIocContainer.RegisteredObjects.Last()
                        .TypeToResolve = typeof(TypeToResolve);
            }



        }
    }
}
namespace XamarinFormsMvvmAdaptor.FluentApi
{
    public interface IRegisterOptions : ILifeCycleOptions
    {
        ILifeCycleOptions As<TypeToResolve>() where TypeToResolve : notnull;
    }

    public interface ILifeCycleOptions
    {
        void SingleInstance();
        void MultiInstance();
    }

    public interface IInstanceRegisterOptions
    {
        void As<TypeToResolve>() where TypeToResolve : notnull;
    }

}
