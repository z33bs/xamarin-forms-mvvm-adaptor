using System;
using System.Collections.Generic;

namespace XamarinFormsMvvmAdaptor
{
    public interface IMultiNavigation
    {
        Dictionary<string, IMvvmBase> NavigationControllers { get; }
    }
}
