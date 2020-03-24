using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XamarinFormsMvvmAdaptor.FluentApi;

//todo Resolve does hierarchy
//todo Add constructor decorator attribute
namespace XamarinFormsMvvmAdaptor
{
    public class Test
    {
        static void Main()
        {
            var ioc = new Ioc();
            ioc.Register<string>(Scope.Global);
        }
    }

    internal sealed class Ioc : IIoc
    //Interfaces controll fluent-Api grammer
    {
        private IList<RegisteredObject> RegisteredObjects { get; } = new List<RegisteredObject>();
        private static IList<RegisteredObject> GlobalRegisteredObjects { get; } = new List<RegisteredObject>();
        //const string CANT_REGISTER_EXCEPTION =
        //    "Can't Register this way because you're using 3rd party container. " +
        //    "Use the 3rd party's interface to register your classes " +
        //    "before swapping-in the container";
        //public void Use3rdPartyContainer(IIocContainer iocContainerAdaptor)
        //{
        //    if (iocContainerAdaptor is IIocContainer)
        //        container = iocContainerAdaptor;
        //    else
        //        throw new NotImplementedException(
        //            $"Container provided does not implement {nameof(IIocContainer)}." +
        //            $"Use the Adaptor pattern if needed");
        //}


        #region Registration
        public IRegisterOptions Register<T>(Scope scope = Scope.Local) where T : notnull
        {
            IList<RegisteredObject> container = scope == Scope.Global
                ? GlobalRegisteredObjects
                : RegisteredObjects;

            container
                .Add(DefaultRegisteredObject<T>());
            return new RegisterOptions(container);
        }

        //todo move to RO as constructor
        private static RegisteredObject DefaultRegisteredObject<T>()
            => new RegisteredObject(
                typeof(T),
                typeof(T),
                LifeCycle.Transient
                );

        public IInstanceRegisterOptions Register(object concreteInstance)
        {
            RegisteredObjects.Add(
                new RegisteredObject(concreteInstance));
            return new InstanceRegisterOptions(RegisteredObjects);
        }

        //todo if lambda could concievably have singleton / multi, but need to store delegate in RegisteredObject
        public IInstanceRegisterOptions Register(Func<object> @delegate)//Expression<Func<object>> expression)
        {
            //todo find out how to pass this context in like in autofac
            Register(@delegate.Invoke());
            return new InstanceRegisterOptions(RegisteredObjects);
        }

        #endregion
        #region Resolution
        public T Resolve<T>() where T : notnull
        {
            return (T)ResolveObject(typeof(T));
        }

        public bool IsRegistered<T>() where T : notnull
        {
            return IsRegistered(typeof(T));
        }

        public object Resolve(Type typeToResolve)
        {
            return ResolveObject(typeToResolve);
        }

        public bool IsRegistered(Type typeToResolve)
        {
            return RegisteredObjects.FirstOrDefault(
                o => o.TypeToResolve == typeToResolve)
                != null;
        }
        #endregion

        #region Internal methods

        private object ResolveObject(Type typeToResolve)
        {
            var registeredObject = RegisteredObjects.FirstOrDefault(o => o.TypeToResolve == typeToResolve);
            if (registeredObject == null)
            {
                throw new TypeNotRegisteredException(string.Format(
                    "The type {0} has not been registered", typeToResolve.Name));
            }
            return GetInstance(registeredObject);
        }
        private object GetInstance(RegisteredObject registeredObject)
        {
            if (registeredObject.Instance == null ||
                registeredObject.LifeCycle == LifeCycle.Transient)
            {
                var parameters = ResolveGreediestConstructorParameters(registeredObject);
                registeredObject.CreateInstance(parameters.ToArray());
            }
            return registeredObject.Instance;
        }

        private IEnumerable<object> ResolveGreediestConstructorParameters(RegisteredObject registeredObject)
        {
            var constructors = registeredObject.ConcreteType.
                    GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => !x.IsPrivate) // Includes internal constructors but not private constructors
                    .ToList();

            if (constructors.Count > 1)
                constructors.OrderBy(
                    ctor => ctor.GetParameters().Count());

            foreach (var parameter in constructors.Last().GetParameters())
            {
                yield return ResolveObject(parameter.ParameterType);
            }
        }

        #endregion

        private class RegisterOptions : IRegisterOptions
        {
            readonly IList<RegisteredObject> container;

            public RegisterOptions(IList<RegisteredObject> container)
            {
                this.container = container;
            }

            public RegisterOptions Global()
            {
                GlobalRegisteredObjects.Add(container.Last());
                container.RemoveAt(container.Count - 1);
                return this;
            }

            public ILifeCycleOptions As<TypeToResolve>() where TypeToResolve : notnull
            {

                //todo throw error if doesn't implement interface
                container
                    .Last()
                    .TypeToResolve = typeof(TypeToResolve);
                return this;
            }


            public void SingleInstance()
            {
                container.Last()
                    .LifeCycle = LifeCycle.Singleton;
            }

            public void MultiInstance()
            {
                container.Last()
                   .LifeCycle = LifeCycle.Transient;
            }
        }
        private class InstanceRegisterOptions : IInstanceRegisterOptions
        {
            readonly IList<RegisteredObject> container;
            public InstanceRegisterOptions(IList<RegisteredObject> container)
            {
                this.container = container;
            }

            public void As<TypeToResolve>() where TypeToResolve : notnull
            {
                container.Last()
                    .TypeToResolve = typeof(TypeToResolve);
            }



        }
    }

    public enum Scope
    {
        Global,
        Local
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
