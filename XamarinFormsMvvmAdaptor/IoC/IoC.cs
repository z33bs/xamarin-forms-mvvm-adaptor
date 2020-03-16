using System;
using System.Linq;
using XamarinFormsMvvmAdaptor.FluentGrammar;

namespace XamarinFormsMvvmAdaptor
{
    internal sealed class IoC : IIoC
        //Interfaces controll fluent-Api grammer
        , ICanAddCondition, ICanAddLifeCycle, ICanAddAsType
    {
        private IIocContainer container = new IocContainer();

        const string CANT_REGISTER_EXCEPTION =
            "Can't Register this way because you're using 3rd party container. " +
            "Use the 3rd party's interface to register your classes " +
            "before swapping-in the container";

        public void Use3rdPartyIoc(IIocContainer iocContainer)
        {
            if (iocContainer is IIocContainer)
                container = iocContainer;
            else
                throw new NotImplementedException(
                    $"Container provided does not implement {nameof(IIocContainer)}." +
                    $"Use the Adaptor pattern if needed");
        }


        #region Registration
        public ICanAddCondition Register<TConcrete>()
        {
            if (container is IInternalIocContainer internalIocContainer)
                internalIocContainer.RegisteredObjects.Add(new RegisteredObject(
                typeof(TConcrete)
                , typeof(TConcrete)
                , LifeCycle.Transient
                ));
            else
                throw new InvalidOperationException(CANT_REGISTER_EXCEPTION);
            return this;
        }

        public ICanAddAsType RegisterInstance(object concreteInstance)
        {
            if (container is IInternalIocContainer internalIocContainer)
                internalIocContainer.RegisteredObjects.Add(
                    new RegisteredObject(concreteInstance));
            else
                throw new InvalidOperationException(CANT_REGISTER_EXCEPTION);
            return this;
        }

        public ICanAddLifeCycle As<TInterface>()
        {
            AsType<TInterface>();
            return this;
        }

        public void AsType<TInterface>()
        {
            if (container is IInternalIocContainer internalIocContainer)
                internalIocContainer.RegisteredObjects.Last()
                    .TypeToResolve = typeof(TInterface);
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
    }
}
namespace XamarinFormsMvvmAdaptor.FluentGrammar
{ 
    public interface ICanAddCondition : ICanAddLifeCycle
    {
        ICanAddLifeCycle As<TInterfase>();
    }

    public interface ICanAddLifeCycle
    {
        void SingleInstance();
        void MultiInstance();
    }

    public interface ICanAddAsType
    {
        void AsType<TInterfase>();
    }

}
