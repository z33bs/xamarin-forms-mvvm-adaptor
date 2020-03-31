using System;

namespace XamarinFormsMvvmAdaptor
{
    public class RegisteredObject : IDisposable
    {
        public RegisteredObject(Type typeToResolve, Type concreteType, LifeCycle lifeCycle)
        {
            TypeToResolve = typeToResolve;
            ConcreteType = concreteType;
            LifeCycle = lifeCycle;
        }

        /// <summary>
        /// If you want to pass an instance of an object into 
        /// the container.
        /// </summary>
        /// <param name="instanceToResolve">Instance to resolve.</param>
        public RegisteredObject(object instanceToResolve)
        {
            TypeToResolve = instanceToResolve.GetType();
            ConcreteType = instanceToResolve.GetType();
            LifeCycle = LifeCycle.Singleton;
            //Since registration is not transient (multi-instance)
            //and since it already has an instance asigned,
            //CreateInstance will never be called by Resolve
            //so all works neatly :-)
            Instance = instanceToResolve;
        }

        public RegisteredObject(Type typeToResolve,object instanceToResolve)
        {
            TypeToResolve = typeToResolve;
            ConcreteType = instanceToResolve.GetType();
            LifeCycle = LifeCycle.Singleton;
            //Since registration is not transient (multi-instance)
            //and since it already has an instance asigned,
            //CreateInstance will never be called by Resolve
            //so all works neatly :-)
            Instance = instanceToResolve;
        }

        public string Key { get; set; }

        public Type TypeToResolve { get; set; }

        public Type ConcreteType { get; private set; }

        public object Instance { get; private set; }

        public LifeCycle LifeCycle { get; set; }

        public void CreateInstance(params object[] args)
        {
            this.Instance = Activator.CreateInstance(this.ConcreteType, args);
        }

        public void Dispose()
        {
            if (Instance != null && Instance is IDisposable)
                (Instance as IDisposable).Dispose();
        }
    }
}