using System;
using System.Collections.Generic;

namespace XamarinFormsMvvmAdaptor
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class MultipleNavigationAttribute : Attribute
    {
        public IList<string> MvvmControllerKeys { get; } = new List<string>();

        public MultipleNavigationAttribute(string key1)
            => MvvmControllerKeys.Add(key1);
        
        public MultipleNavigationAttribute(string key1, string key2) : this(key1)
            => MvvmControllerKeys.Add(key2);
        
        public MultipleNavigationAttribute(string key1, string key2, string key3)
            : this(key1, key2)
                => MvvmControllerKeys.Add(key3);
        
        public MultipleNavigationAttribute(string key1, string key2, string key3, string key4)
            : this(key1, key2, key3)
                => MvvmControllerKeys.Add(key4);

        public MultipleNavigationAttribute(string key1, string key2, string key3, string key4, string key5)
            : this(key1, key2, key3, key4)
                => MvvmControllerKeys.Add(key5);

        public MultipleNavigationAttribute(string key1, string key2, string key3, string key4, string key5, string key6)
            : this(key1, key2, key3, key4, key5)
                => MvvmControllerKeys.Add(key6);

        public MultipleNavigationAttribute(string key1, string key2, string key3, string key4, string key5, string key6, string key7)
            : this(key1, key2, key3, key4, key5, key6)
                => MvvmControllerKeys.Add(key7);

    }
}
