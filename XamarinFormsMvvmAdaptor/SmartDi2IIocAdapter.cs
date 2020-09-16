using System;
namespace XamarinFormsMvvmAdaptor
{
    internal class SmartDi2IIocAdapter : IIoc
    {
        public SmartDi2IIocAdapter()
        {
        }

        public object Resolve(Type typeToResolve)
            => DiContainer.Resolve(typeToResolve);
    }
}
