using System;
using System.Text;

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

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(string.IsNullOrEmpty(Key) ? "" : $"{nameof(Key)}:{Key} ");
            builder.Append($"{nameof(TypeToResolve)}:{EvaluateType(TypeToResolve)}");
            builder.Append($" {nameof(ConcreteType)}:{EvaluateType(ConcreteType)}");
            builder.Append($" {nameof(LifeCycle)}:{LifeCycle}");

            return builder.ToString();
        }

        private string EvaluateType(Type type, bool useFullName = false)
        {
            StringBuilder retType = new StringBuilder();

            if (type.IsGenericType)
            {
                string[] parentType = (useFullName?type.FullName:type.Name).Split('`');
                // We will build the type here.
                Type[] arguments = type.GetGenericArguments();

                StringBuilder argList = new StringBuilder();
                foreach (Type t in arguments)
                {
                    // Let's make sure we get the argument list.
                    string arg = EvaluateType(t,useFullName);
                    if (argList.Length > 0)
                    {
                        argList.AppendFormat(", {0}", arg);
                    }
                    else
                    {
                        argList.Append(arg);
                    }
                }

                if (argList.Length > 0)
                {
                    retType.AppendFormat("{0}<{1}>", parentType[0], argList.ToString());
                }
            }
            else
            {
                return useFullName ? type.ToString() : type.Name;
            }

            return retType.ToString();
        }

        public void Dispose()
        {
            if (Instance != null && Instance is IDisposable)
                (Instance as IDisposable).Dispose();
        }
    }
}