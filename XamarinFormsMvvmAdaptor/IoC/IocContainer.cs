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
            return RegisteredObjects.FirstOrDefault(
                o => o.TypeToResolve == service)
                != null;
        }
        public bool IsRegistered<TService>() where TService : notnull
        {
            return IsRegistered(typeof(TService));
        }

        public object Resolve(Type service)
        {
            return ResolveObject(service);
        }

        public TService Resolve<TService>() where TService : notnull
        {
            return (TService)ResolveObject(typeof(TService));
        }
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

    }

    public interface IInternalIocContainer
    {
        IList<RegisteredObject> RegisteredObjects { get; }
    }
}
