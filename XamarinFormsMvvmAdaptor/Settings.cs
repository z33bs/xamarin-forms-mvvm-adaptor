namespace XamarinFormsMvvmAdaptor
{
    internal static class Settings
    {
        const string DEFAULT_VM_NAMESPACE = "ViewModels";
        const string DEFAULT_V_NAMESPACE = "Views";
        const string DEFAULT_VM_SUFFIX = "ViewModel";
        const string DEFAULT_V_SUFFIX = "Page";

        //Used internally when using default settings. When overridden
        //will used full namespace instead
        internal static string ViewModelSubNamespace { get; set; } = DEFAULT_VM_NAMESPACE;
        internal static string ViewSubNamespace { get; set; } = DEFAULT_V_NAMESPACE;

        //null means no override
        internal static string ViewSuffix { get; set; } = DEFAULT_V_SUFFIX;
        internal static string ViewAssemblyName { get; set; } = null;
        internal static string ViewNamespace { get; set; } = null;

        internal static string ViewModelSuffix { get; set; } = DEFAULT_VM_SUFFIX;
        internal static string ViewModelAssemblyName { get; set; } = null;
        internal static string ViewModelNamespace { get; set; } = null;
    }
}
