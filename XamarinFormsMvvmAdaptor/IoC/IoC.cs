using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XamarinFormsMvvmAdaptor.FluentApi;

//todo Add constructor decorator attribute
//Resolve unregistered class if can find di stuff
//Functionality: Remove, ListAll, Dispose
//See if SetMainPage methods commented out
//  in static partial class are adaptable/relevant to instance?
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
        private static bool mustBeRegisteredToResolve = false;

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

        public void ConfigureResolveMode(bool isStrictMode)
            => mustBeRegisteredToResolve = isStrictMode;

        #region Registration
        public IRegisterOptions Register<T>(Scope scope = Scope.Local) where T : notnull
        {
            var container = GetContainerForScope(scope);

            container
                .Add(DefaultRegisteredObject<T>());
            return new RegisterOptions(container);
        }

        private IList<RegisteredObject> GetContainerForScope(Scope scope)
            => scope == Scope.Global
                ? GlobalRegisteredObjects
                : RegisteredObjects;



        //todo move to RO as constructor
        private static RegisteredObject DefaultRegisteredObject<T>()
            => new RegisteredObject(
                typeof(T),
                typeof(T),
                LifeCycle.Transient
                );

        public IInstanceRegisterOptions Register(object concreteInstance, Scope scope = Scope.Local)
        {
            var container = GetContainerForScope(scope);

            container.Add(
                new RegisteredObject(concreteInstance));
            return new InstanceRegisterOptions(container);
        }

        ////todo if lambda could concievably have singleton / multi, but need to store delegate in RegisteredObject
        //public IInstanceRegisterOptions Register(Func<object> @delegate)//Expression<Func<object>> expression)
        //{
        //    //todo find out how to pass this context in like in autofac
        //    Register(@delegate.Invoke());
        //    return new InstanceRegisterOptions(RegisteredObjects);
        //}

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

        public Scope IsRegisteredScope<T>() where T : notnull
        {
            return IsRegisteredScope(typeof(T));
        }

        public object Resolve(Type typeToResolve)
        {
            if (typeToResolve == null)
                throw new ArgumentNullException($"{nameof(typeToResolve)} cannot be null");

            return ResolveObject(typeToResolve);
        }

        public bool IsRegistered(Type typeToResolve)
        {
            if (typeToResolve == null)
                throw new ArgumentNullException($"{nameof(typeToResolve)} cannot be null");

            return GetRegisteredObjectAndScope(typeToResolve) != null;
        }

        public Scope IsRegisteredScope(Type typeToResolve)
        {
            var registeredObjectTuple = GetRegisteredObjectAndScope(typeToResolve);

            if (registeredObjectTuple == null)
                throw new TypeNotRegisteredException(
                    $"The type {typeToResolve.Name} has not been registered. " +
                    $"Register the class or run {nameof(IsRegistered)} " +
                    $"before calling {nameof(IsRegisteredScope)}.");

            return registeredObjectTuple.Item2;
        }

        private Tuple<RegisteredObject, Scope> GetRegisteredObjectAndScope(Type typeToResolve)
        {
            var registeredObject = RegisteredObjects
                .FirstOrDefault(o => o.TypeToResolve == typeToResolve);
            if (registeredObject != null)
                return new Tuple<RegisteredObject, Scope>(registeredObject, Scope.Local);

            registeredObject = GlobalRegisteredObjects
                .FirstOrDefault(o => o.TypeToResolve == typeToResolve);
            if (registeredObject != null)
                return new Tuple<RegisteredObject, Scope>(registeredObject, Scope.Global);

            return null;
        }
        #endregion

        #region Internal methods

        private object ResolveObject(Type typeToResolve)
        {
            var registeredObject = GetRegisteredObjectAndScope(typeToResolve)?.Item1;

            //is registered
            if (registeredObject != null)
                return GetInstance(registeredObject);

            if (mustBeRegisteredToResolve)
                throw new TypeNotRegisteredException(
                    $"The type {typeToResolve.Name} has not been registered. Either " +
                    $"register the class, or configure {nameof(ConfigureResolveMode)}.");

            //not registered - but try anyway
            var parameters = ResolveGreediestConstructorParameters(typeToResolve);
            try
            {
                return Activator.CreateInstance(typeToResolve, parameters.ToArray());
            }
            catch(Exception ex)
            {
                throw new TypeNotRegisteredException(
                    $"Could not Resolve or Create {typeToResolve.Name}" +
                    $". It is not registered in {nameof(Ioc)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.",ex);
            }
        }

        private bool HasParamaterlessConstructor(Type type)
            => type.GetConstructor(Type.EmptyTypes) != null;

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
            foreach (var dependency in ResolveGreediestConstructorParameters(registeredObject.ConcreteType))
            {
                yield return dependency;
            }
        }

        private IEnumerable<object> ResolveGreediestConstructorParameters(Type typeToResolve)
        {
            var constructors = typeToResolve.
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
