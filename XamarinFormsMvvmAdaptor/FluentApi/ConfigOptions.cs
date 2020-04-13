namespace XamarinFormsMvvmAdaptor.FluentApi
{
    public sealed class ConfigOptions
    {
        public ConfigOptions SetViewSuffix(string suffix)
        {
            Settings.ViewSuffix = suffix;
            return this;
        }

        public ConfigOptions SetViewModelSuffix(string suffix)
        {
            Settings.ViewModelSuffix = suffix;
            return this;
        }

        public ConfigOptions SetViewAssemblyQualifiedNamespace<TAnyView>()
        {
            SetViewAssemblyQualifiedNamespace(
                typeof(TAnyView).Namespace,
                typeof(TAnyView).Assembly.FullName);
            return this;
        }

        public ConfigOptions SetViewAssemblyQualifiedNamespace(string namespaceName, string assemblyName)
        {
            Settings.ViewAssemblyName = assemblyName;
            Settings.ViewNamespace = namespaceName;
            return this;
        }

        public ConfigOptions SetViewModelAssemblyQualifiedNamespace<TAnyViewModel>()
        {
            SetViewModelAssemblyQualifiedNamespace(
                typeof(TAnyViewModel).Namespace,
                typeof(TAnyViewModel).Assembly.FullName);
            return this;
        }

        public ConfigOptions SetViewModelAssemblyQualifiedNamespace(string namespaceName, string assemblyName)
        {
            Settings.ViewModelAssemblyName = assemblyName;
            Settings.ViewModelNamespace = namespaceName;
            return this;
        }
    }
}
