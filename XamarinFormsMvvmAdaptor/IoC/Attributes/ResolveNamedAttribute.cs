using System;
namespace XamarinFormsMvvmAdaptor
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ResolveNamedAttribute : Attribute
    {
        public string Key { get; }
        public ResolveNamedAttribute(string key)
        {
            Key = key;
        }
    }
}
