using System;
using System.Collections.Generic;

namespace XamarinFormsMvvmAdaptor
{
    public sealed class MultiNavigation : IMultiNavigation
    {
        private MultiNavigation()
        {
        }
        static readonly Lazy<MultiNavigation> instance
            = new Lazy<MultiNavigation>(() => new MultiNavigation());
        public static MultiNavigation Instance => instance.Value;

        public Dictionary<string, INavController> NavigationControllers { get; } = new Dictionary<string, INavController>();
        
    }


}
