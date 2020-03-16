using XamarinFormsMvvmAdaptor.FluentGrammar;

namespace XamarinFormsMvvmAdaptor
{
    public interface IIoC
    {
        ICanAddCondition Register<TConcrete>();
        ICanAddAsType RegisterInstance(object concreteInstance);
        T Resolve<T>() where T : notnull;
        bool IsRegistered<T>() where T : notnull;
        void Use3rdPartyIoc(IIocContainer iocContainer);
    }
}
