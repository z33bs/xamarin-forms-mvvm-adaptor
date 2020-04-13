using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using XamarinFormsMvvmAdaptor.FluentApi;

//todo
//Separate project? Same Assembly?
namespace XamarinFormsMvvmAdaptor
{
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
        public IRegisterOptions Register<T>() where T : notnull
        {
            return Register<T>(Scope.Local);
        }

        public IRegisterOptions Register<T>(Scope scope) where T : notnull
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

        public IInstanceRegisterOptions Register(object concreteInstance)
        {
            return Register(concreteInstance, Scope.Local);
        }

        public IInstanceRegisterOptions Register(object concreteInstance, Scope scope)
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

        public Scope GetScope<T>() where T : notnull
        {
            return GetScope(typeof(T));
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

        public Scope GetScope(Type typeToResolve)
        {
            var registeredObjectTuple = GetRegisteredObjectAndScope(typeToResolve);

            if (registeredObjectTuple == null)
                throw new TypeNotRegisteredException(
                    $"The type {typeToResolve.Name} has not been registered. " +
                    $"Register the class or run {nameof(IsRegistered)} " +
                    $"before calling {nameof(GetScope)}.");

            return registeredObjectTuple.Item2;
        }

        public object Resolve(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException($"{nameof(key)} cannot be null");

            return ResolveObject(key);
        }

        public bool IsRegistered(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException($"{nameof(key)} cannot be null");

            return GetRegisteredObjectAndScope(key) != null;
        }

        public Scope GetScope(string key)
        {
            var registeredObjectTuple = GetRegisteredObjectAndScope(key);

            if (registeredObjectTuple == null)
                throw new TypeNotRegisteredException(
                    $"The type with key of '{key}' has not been registered. " +
                    $"Register the class or run {nameof(IsRegistered)} " +
                    $"before calling {nameof(GetScope)}.");

            return registeredObjectTuple.Item2;
        }

        public override string ToString()
        {
            return $"Local entries:{RegisteredObjects.Count} Global entries:{GlobalRegisteredObjects.Count}";
        }

        public string ListRegistrations()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Local:");
            foreach (var registration in RegisteredObjects)
            {
                builder.AppendLine(registration.ToString());
            }

            builder.AppendLine("Global:");
            foreach (var registration in GlobalRegisteredObjects)
            {
                builder.AppendLine(registration.ToString());
            }

            return builder.ToString();
        }
        #endregion

        #region Internal methods

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

        private Tuple<RegisteredObject, Scope> GetRegisteredObjectAndScope(string key)
        {
            var registeredObject = RegisteredObjects
                .FirstOrDefault(o => o.Key == key);
            if (registeredObject != null)
                return new Tuple<RegisteredObject, Scope>(registeredObject, Scope.Local);

            registeredObject = GlobalRegisteredObjects
                .FirstOrDefault(o => o.Key == key);
            if (registeredObject != null)
                return new Tuple<RegisteredObject, Scope>(registeredObject, Scope.Global);

            return null;
        }

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

            return CreateUnregisteredObject(typeToResolve);
        }

        private object ResolveObject(string key)
        {
            var registeredObject = GetRegisteredObjectAndScope(key)?.Item1;
            if (registeredObject is null)
                throw new TypeNotRegisteredException(
                    $"The type with provided Key of '{key}' has not been registered.");

            return GetInstance(registeredObject);
        }

        private object CreateUnregisteredObject(Type typeToResolve)
        {
            //not registered - but try anyway
            var parameters = ResolveConstructorParameters(typeToResolve);
            try
            {
                return Activator.CreateInstance(typeToResolve, parameters.ToArray());
            }
            catch (Exception ex)
            {
                if (ex is TypeNotRegisteredException)
                    throw ex;

                throw new TypeNotRegisteredException(
                    $"Could not Resolve or Create {typeToResolve.Name}" +
                    $". It is not registered in {nameof(Ioc)}. Furthermore, " +
                    $"smart resolve couldn't create an instance.", ex);
            }
        }

        private bool HasParamaterlessConstructor(Type type)
            => type.GetConstructor(Type.EmptyTypes) != null;

        private object GetInstance(RegisteredObject registeredObject)
        {
            if (registeredObject.Instance == null ||
                registeredObject.LifeCycle == LifeCycle.Transient)
            {
                var parameters = ResolveConstructorParameters(registeredObject);
                registeredObject.CreateInstance(parameters.ToArray());
            }
            return registeredObject.Instance;
        }

        private IEnumerable<object> ResolveConstructorParameters(RegisteredObject registeredObject)
        {
            foreach (var dependency in ResolveConstructorParameters(registeredObject.ConcreteType))
            {
                yield return dependency;
            }
        }

        private IEnumerable<object> ResolveConstructorParameters(Type typeToResolve)
        {
            var constructors = typeToResolve.
                    GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => !x.IsPrivate) // Includes internal constructors but not private constructors
                    .ToList();

            if (constructors.Count > 1)
            {
                //if flagged, shorten to only flagged constructors
                var flaggedConstructors = constructors.Where(c => c.GetCustomAttribute<ResolveUsingAttribute>() != null);

                //todo if strict mode throw if more than one
                if (flaggedConstructors.Any())
                    constructors = flaggedConstructors.ToList();

                //order by number of parameters
                constructors = constructors.OrderBy(
                    ctor => ctor.GetParameters().Count()).ToList();
            }

            foreach (var parameter in constructors.Last().GetParameters())
            {
                var namedDependencyAttribute = parameter.GetCustomAttribute<ResolveNamedAttribute>();
                if (namedDependencyAttribute != null)
                    yield return ResolveObject(namedDependencyAttribute.Key);
                else
                    yield return ResolveObject(parameter.ParameterType);
            }
        }
        #endregion
        #region DESTRUCTIVE
        public void Teardown()
        {
            foreach (var registeredObject in RegisteredObjects.ToList())
            {
                RegisteredObjects.Remove(registeredObject);
                registeredObject.Dispose();
            }

            foreach (var registeredObject in GlobalRegisteredObjects.ToList())
            {
                GlobalRegisteredObjects.Remove(registeredObject);
                registeredObject.Dispose();
            }
        }


        public void Remove<T>() where T : notnull
        {
            RemoveAndDispose(RegisteredObjects, typeof(T));
            RemoveAndDispose(GlobalRegisteredObjects, typeof(T));
        }

        public void Remove<T>(Scope scope) where T : notnull
        {
            switch (scope)
            {
                case Scope.Local:
                    RemoveAndDispose(RegisteredObjects, typeof(T));
                    break;
                case Scope.Global:
                    RemoveAndDispose(GlobalRegisteredObjects, typeof(T));
                    break;
            }
        }

        public void Remove(string key)
        {
            RemoveAndDispose(RegisteredObjects, key);
            RemoveAndDispose(GlobalRegisteredObjects, key);
        }

        public void Remove(string key, Scope scope)
        {
            switch (scope)
            {
                case Scope.Local:
                    RemoveAndDispose(RegisteredObjects, key);
                    break;
                case Scope.Global:
                    RemoveAndDispose(GlobalRegisteredObjects, key);
                    break;
            }
        }


        private void RemoveAndDispose(IList<RegisteredObject> container, Type typeToResolve)
        {
            var matchingObjects = container
                    .Where(o => o.TypeToResolve == typeToResolve);
            ExecuteRemoveAndDispose(container, matchingObjects);
        }


        private void RemoveAndDispose(IList<RegisteredObject> container, string key)
        {
            var matchingObjects = container
                    .Where(o => o.Key == key);
            ExecuteRemoveAndDispose(container, matchingObjects);
        }

        private void ExecuteRemoveAndDispose(IList<RegisteredObject> container, IEnumerable<RegisteredObject> objectsToDispose)
        {
            foreach (var registeredObject in objectsToDispose.ToList())
            {
                container.Remove(registeredObject);
                registeredObject.Dispose();
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

            public IWithKey As<TypeToResolve>() where TypeToResolve : notnull
            {

                //todo throw error if doesn't implement interface
                container.Last()
                    .TypeToResolve = typeof(TypeToResolve);
                container.Last()
                    .LifeCycle = LifeCycle.Singleton;
                return this;
            }

            public IAs WithKey(string key)
            {
                container
                    .Last()
                    .Key = key;
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
    public interface IRegisterOptions : IWithKey, IAs
    {
    }

    public interface ILifeCycleOptions
    {
        void SingleInstance();
        void MultiInstance();
    }

    public interface IWithKey : ILifeCycleOptions
    {
        IAs WithKey(string key);
    }

    public interface IAs : ILifeCycleOptions
    {
        IWithKey As<TypeToResolve>() where TypeToResolve : notnull;
    }

    public interface IInstanceRegisterOptions
    {
        void As<TypeToResolve>() where TypeToResolve : notnull;
    }
}
