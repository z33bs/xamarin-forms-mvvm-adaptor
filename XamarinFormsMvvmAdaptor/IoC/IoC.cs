using System;
using System.Linq;
using XamarinFormsMvvmAdaptor.FluentGrammar;

namespace XamarinFormsMvvmAdaptor
{
    internal sealed class IoC : IIoc
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
        public ICanAddCondition Register<T>() where T : notnull
        {
            if (container is IInternalIocContainer internalIocContainer)
                internalIocContainer.RegisteredObjects.Add(new RegisteredObject(
                typeof(T)
                , typeof(T)
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

        public ICanAddLifeCycle As<TypeToResolve>() where TypeToResolve : notnull
        {
            AsType<TypeToResolve>();
            return this;
        }

        public void AsType<TypeToResolve>() where TypeToResolve : notnull
        {
            if (container is IInternalIocContainer internalIocContainer)
                internalIocContainer.RegisteredObjects.Last()
                    .TypeToResolve = typeof(TypeToResolve);
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
        ICanAddLifeCycle As<TypeToResolve>() where TypeToResolve : notnull;
    }

    public interface ICanAddLifeCycle
    {
        void SingleInstance();
        void MultiInstance();
    }

    public interface ICanAddAsType
    {
        void AsType<TypeToResolve>() where TypeToResolve : notnull;
    }

}
