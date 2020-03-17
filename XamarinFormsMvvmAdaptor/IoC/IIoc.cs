using XamarinFormsMvvmAdaptor.FluentApi;

namespace XamarinFormsMvvmAdaptor
{
    public interface IIoc
    {
        IRegisterOptions Register<T>() where T : notnull;
        IInstanceRegisterOptions Register(object concreteInstance);
        T Resolve<T>() where T : notnull;
        bool IsRegistered<T>() where T : notnull;
        void Use3rdPartyContainer(IIocContainer iocContainer);
    }
}
