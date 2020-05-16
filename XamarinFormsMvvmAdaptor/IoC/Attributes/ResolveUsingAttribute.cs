using System;
namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// Attribute to explicitly mark which constructor should be used by the <see cref="Ioc"/>
    /// to instantiate the class
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor,AllowMultiple = false)]
    public class ResolveUsingAttribute : Attribute
    {
        /// <summary>
        /// Attribute to explicitly mark which constructor should be used by the <see cref="Ioc"/>
        /// to instantiate the class
        /// </summary>
        public ResolveUsingAttribute()
        {
        }
    }
}
