using System;
using System.Collections.Generic;

namespace XamarinFormsMvvmAdaptor
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class MultipleNavigationAttribute : Attribute
    {
        public IList<string> MvvmControllerKeys { get; } = new List<string>();
        public MultipleNavigationAttribute(string key1)
        {
            MvvmControllerKeys.Add(key1);
        }
        public MultipleNavigationAttribute(string key1, string key2) : this(key1)
        {
            MvvmControllerKeys.Add(key2);
        }
    }
}
