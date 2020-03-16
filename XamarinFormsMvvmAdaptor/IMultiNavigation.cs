using System;
using System.Collections.Generic;

namespace XamarinFormsMvvmAdaptor
{
    public interface IMultiNavigation
    {
        Dictionary<string, INavController> NavigationControllers { get; }
    }
}
