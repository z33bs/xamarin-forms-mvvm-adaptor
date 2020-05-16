using System.ComponentModel;

//todo Now that have EditorBrowsable can keep in same namespace
namespace XamarinFormsMvvmAdaptor.FluentApi
{
    /// <summary>
    /// Plumbing for Fluent Api
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ConfigOptions
    {
        /// <summary>
        /// Plumbing for Fluent Api
        /// </summary>
        public ConfigOptions SetViewSuffix(string suffix)
        {
            Settings.ViewSuffix = suffix;
            return this;
        }

        /// <summary>
        /// Plumbing for Fluent Api
        /// </summary>
        public ConfigOptions SetViewModelSuffix(string suffix)
        {
            Settings.ViewModelSuffix = suffix;
            return this;
        }

        /// <summary>
        /// Plumbing for Fluent Api
        /// </summary>
        public ConfigOptions SetViewAssemblyQualifiedNamespace<TAnyView>()
        {
            SetViewAssemblyQualifiedNamespace(
                typeof(TAnyView).Namespace,
                typeof(TAnyView).Assembly.FullName);
            return this;
        }

        /// <summary>
        /// Plumbing for Fluent Api
        /// </summary>
        public ConfigOptions SetViewAssemblyQualifiedNamespace(string namespaceName, string assemblyName)
        {
            Settings.ViewAssemblyName = assemblyName;
            Settings.ViewNamespace = namespaceName;
            return this;
        }

        /// <summary>
        /// Plumbing for Fluent Api
        /// </summary>
        public ConfigOptions SetViewModelAssemblyQualifiedNamespace<TAnyViewModel>()
        {
            SetViewModelAssemblyQualifiedNamespace(
                typeof(TAnyViewModel).Namespace,
                typeof(TAnyViewModel).Assembly.FullName);
            return this;
        }

        /// <summary>
        /// Plumbing for Fluent Api
        /// </summary>
        public ConfigOptions SetViewModelAssemblyQualifiedNamespace(string namespaceName, string assemblyName)
        {
            Settings.ViewModelAssemblyName = assemblyName;
            Settings.ViewModelNamespace = namespaceName;
            return this;
        }
    }
}
