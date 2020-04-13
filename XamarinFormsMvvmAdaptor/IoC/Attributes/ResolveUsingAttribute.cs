using System;
namespace XamarinFormsMvvmAdaptor
{
    [AttributeUsage(AttributeTargets.Constructor,AllowMultiple = false)]
    public class ResolveUsingAttribute : Attribute
    {
        public ResolveUsingAttribute()
        {
        }
    }
}
