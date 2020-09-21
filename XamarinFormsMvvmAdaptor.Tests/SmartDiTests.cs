using System;
using Xunit;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace XamarinFormsMvvmAdaptor.Tests
{
    [Collection("SmartDi")]
    public class NewDiContainerTests
    {
        [Fact]
        public void Playground()
        {
        }

        public abstract class DisposableBase : IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
                => Dispose(true);

            protected void Dispose(bool disposing)
            {
                if (!Disposed)
                {
                    Disposed = true;

                    if (disposing)
                    {
                        DisposeExplicit();
                    }

                    DisposeImplicit();

                    GC.SuppressFinalize(this);
                }
            }

            protected virtual void DisposeExplicit() { }
            protected virtual void DisposeImplicit() { }

            ~DisposableBase()
            {
                Dispose(false);
            }
        }

        class ClassWithStringParameter
        {
            public ClassWithStringParameter(string name)
            {

            }
        }

        interface IService { }
        class MyService : IService { }

        interface IServiceWithTwoImplementations { }
        class ServiceTwo : IServiceWithTwoImplementations { }
        class MockServiceTwo : IServiceWithTwoImplementations { }

        interface IServiceWithNoImplementations { }

        class ConcreteOnly { }

        interface IClassWith3Ctors
        {
            string ConstructorUsed { get; }
            IService Service { get; }
            ConcreteOnly Concrete { get; }
        }
        class ClassWith3Ctors : IClassWith3Ctors
        {
            public string ConstructorUsed { get; }
            public IService Service { get; }
            public ConcreteOnly Concrete { get; }

            public ClassWith3Ctors(IService service)
            { this.Service = service; ConstructorUsed = "(IService service)"; }
            public ClassWith3Ctors() { ConstructorUsed = "()"; }
            internal ClassWith3Ctors(IService service, ConcreteOnly concrete)
            { this.Service = service; this.Concrete = concrete; ConstructorUsed = "(IService service, ConcreteOnly concrete)"; }
        }

        class ClassWithFlaggedCtor
        {
            public IService Service { get; }
            public ConcreteOnly Concrete { get; }

            [ResolveUsing]
            public ClassWithFlaggedCtor(IService service)
            { this.Service = service; }

            public ClassWithFlaggedCtor(IService service, ConcreteOnly concrete)
            { this.Service = service; this.Concrete = concrete; }
        }

        class ClassWith2FlaggedCtors
        {
            public IService Service { get; }
            public ConcreteOnly Concrete { get; }

            [ResolveUsing]
            public ClassWith2FlaggedCtors(IService service)
            { this.Service = service; }

            [ResolveUsing]
            public ClassWith2FlaggedCtors(IService service, ConcreteOnly concrete)
            { this.Service = service; this.Concrete = concrete; }
        }

        class ClassThatsResolvableWithoutRegistering
        {
            public ConcreteOnly Concrete { get; }
            public ClassThatsResolvableWithoutRegistering(ConcreteOnly concrete)
            {
                this.Concrete = concrete;
            }
        }

        class ClassWithKeyedDependency
        {
            public IService Service { get; }
            public ClassWithKeyedDependency()
            {

            }
            public ClassWithKeyedDependency([ResolveNamed("test")]IService service)
            {
                Service = service;
            }
        }

        class ClassThatsUnresolvable
        {
            public ClassThatsUnresolvable()
            {
                throw new Exception("Its not possible to instantiate this class");
            }
        }

        class ClassWithInternalConstructor
        {
            public string ConstructorUsed { get; }

            public ClassWithInternalConstructor()
            {
                ConstructorUsed = "public";
            }

            internal ClassWithInternalConstructor(ConcreteOnly concreteOnly)
            {
                ConstructorUsed = "internal";
            }
        }

        class ClassThatHasToBeRegistered
        {
            public ClassThatHasToBeRegistered(int number)
            {

            }
        }

        class ClassWithoutParamaterlessCtor
        {
            public ClassWithoutParamaterlessCtor(IService service)
            {

            }
        }

        public class ClassThatThrowsOnDisposed : IDisposable
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        interface IClassThatsDisposable
        {

        }
        class ClassThatsDisposable : DisposableBase, IClassThatsDisposable
        {
            private Action _onExplicitDispose;
            private Action _onImplicitDispose;

            public ClassThatsDisposable(Action onExplicitDispose, Action onImplicitDispose)
            {
                _onExplicitDispose = onExplicitDispose;
                _onImplicitDispose = onImplicitDispose;
            }

            protected override void DisposeExplicit()
                => _onExplicitDispose?.DynamicInvoke();
            protected override void DisposeImplicit()
                => _onImplicitDispose?.DynamicInvoke();
        }

        [Fact]
        public void Constructor()
        {
            Assert.IsAssignableFrom<IDiContainer>(new DiContainer());
        }

        [Fact]
        public void InitializeStaticContainer()
        {
            //Not a real test
            DiContainer.Initialize(new ContainerOptions {
                TryResolveUnregistered = false,
                ResolveShouldBubbleUpContainers = false});
        }

        #region MetaObject

        [Fact]
        public void MetaObjectCtor_InstanceNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MetaObject(null));
        }

        [Fact]
        public void MetaObjectCtor_InstanceDelegateNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MetaObject(typeof(ConcreteOnly), LifeCycle.Transient, instanceDelegate: null,null));
        }

        [Fact]
        public void MetaObject_UsingConstructor_TypeHasNoConstructor_Throws()
        {
            Assert.Throws<RegisterException>(() => new MetaObject(typeof(ClassThatsUnresolvable), LifeCycle.Singleton, typeof(MyService)));
        }

        [Fact]
        public void MetaObject_UsingConstructor_NoMatchFound_Throws()
        {
            Assert.Throws<RegisterException>(() => new MetaObject(typeof(ConcreteOnly), LifeCycle.Singleton, typeof(MyService)));
        }

//#if DEBUG
//        [Fact]
//        public void MetaObject_RegisteredTransient_SetInstance_Throws()
//        {
//            var metaObject = new MetaObject(typeof(ConcreteOnly), LifeCycle.Transient);
//            Assert.Throws<Exception>(() => metaObject.Instance = new ConcreteOnly());

//        }
//#endif

        #endregion

        #region Registration
        #region internal

        [Fact]
        public void InnerRegister_ConcreteTypeIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            DiContainer.Instance.InternalRegister(
                new ConcurrentDictionary<Tuple<Type, string>, MetaObject>(),
                typeof(IService),
                null,
                new MetaObject(concreteType: null, //We need this at a minimum to justify registration 
                               LifeCycle.Transient)
                ));
        }

        #endregion
        #region Register<ConcreteType>()
        [Fact]
        public void StaticRegisterConcreteType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<MyService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.Register<MyService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
        }

        [Fact]
        public void StaticRegisterConcreteType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.Register<MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);
        }

        #endregion
        #region Register<ConcreteType,ResolvedType>()
        [Fact]
        public void StaticRegisterResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<IService, MyService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.Register<IService, MyService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));
        }

        [Fact]
        public void StaticRegisterResolvedType_Default_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<IService, MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedType_Default_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.Register<IService, MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);
        }

        #endregion
        #region Register<ConcreteType>(string key)
        [Fact]
        public void StaticRegisterConcreteTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<MyService>("test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void StaticRegisterWithCtorParametersWithKey_ResolvesWithSelectedCtor()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<ClassWith3Ctors>("test",typeof(IService));

            Assert.Equal("(IService service)", DiContainer.Resolve<ClassWith3Ctors>("test").ConstructorUsed);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.Register<MyService>("test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));
        }

        #endregion
        #region Register<ConcreteType,ResolvedType>(string key)
        [Fact]
        public void StaticRegisterResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<IService, MyService>("test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.Register<IService, MyService>("test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));
        }

        [Fact]
        public void StaticRegisterResolvedWithCtorParameters_ResolvesWithSelectedCtor()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<IClassWith3Ctors, ClassWith3Ctors>(typeof(IService));

            Assert.Equal("(IService service)", DiContainer.Resolve<IClassWith3Ctors>().ConstructorUsed);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void StaticRegisterResolvedWithCtorParametersWithKey_ResolvesWithSelectedCtor()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<IClassWith3Ctors, ClassWith3Ctors>("test",typeof(IService));

            Assert.Equal("(IService service)", DiContainer.Resolve<IClassWith3Ctors>("test").ConstructorUsed);

            DiContainer.ResetContainer();
        }

        #endregion

        #region RegisterInstance
        [Fact]
        public void StaticRegisterInstance_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.RegisterInstance(new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterInstance_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.RegisterInstance(new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
        }

        [Fact]
        public void StaticRegisterInstanceWithResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.RegisterInstance<IService>(new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterInstanceWithResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.RegisterInstance<IService>(new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));
        }

        [Fact]
        public void StaticRegisterInstanceWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.RegisterInstance(new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterInstanceWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.RegisterInstance(new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));
        }

        [Fact]
        public void StaticRegisterInstanceWithResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.RegisterInstance<IService>(new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterInstanceWithResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.RegisterInstance<IService>(new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));
        }


        #endregion

        #region RegisterExpression

        [Fact]
        public void StaticRegisterExpression_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.RegisterExplicit<MyService,MyService>(c => new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterExpression_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.RegisterExplicit<MyService,MyService>(c => new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
        }

        [Fact]
        public void StaticRegisterExpressionWithResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.RegisterExplicit<IService,MyService>(c => new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterExpressionWithResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.RegisterExplicit<IService,MyService>(c => new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));
        }

        [Fact]
        public void StaticRegisterExpressionWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.RegisterExplicit<MyService,MyService>(c => new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterExpressionWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.RegisterExplicit<MyService,MyService>(c => new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));
        }

        [Fact]
        public void StaticRegisterExpressionWithResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.RegisterExplicit<IService,MyService>(c => new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterExpressionWithResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.RegisterExplicit<IService,MyService>(c => new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));
        }

        #endregion

        #region RegisterType
        [Fact]
        public void StaticRegisterType_Defaults_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.RegisterType(typeof(MyService));

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterType_Defaults_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.RegisterType(typeof(MyService));

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
        }

        [Fact]
        public void StaticRegisterType_ResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.RegisterType(typeof(MyService), typeof(IService));

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterType_ResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.RegisterType(typeof(MyService), typeof(IService));

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));
        }

        [Fact]
        public void StaticRegisterType_ResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.RegisterType(typeof(MyService), typeof(IService), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterType_ResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.RegisterType(typeof(MyService), typeof(IService), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));
        }

        [Fact]
        public void StaticRegisterType_WithConstructorParams_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.RegisterType(typeof(ClassWith3Ctors), null, null, typeof(IService), typeof(ConcreteOnly));

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(ClassWith3Ctors), null)));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterType_WithConstructorParams_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.RegisterType(typeof(ClassWith3Ctors), null, null, typeof(IService), typeof(ConcreteOnly));

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(ClassWith3Ctors), null)));
        }
        #endregion

        #region RegisterOptions
        [Fact]
        public void StaticRegisterConcreteType_OptionsSingleInstance_RegistersAsSingleInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer
                .Register<MyService>()
                .SingleInstance();

            Assert.Equal(LifeCycle.Singleton, mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType_OptionsSingleInstance_RegistersAsSingleInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container
                .Register<MyService>()
                .SingleInstance();

            Assert.Equal(LifeCycle.Singleton, mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);
        }

        [Fact]
        public void StaticRegisterResolvedType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer
                .Register<IService, MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container
                .Register<IService, MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);
        }

        [Fact]
        public void StaticRegisterConcreteType_UsingConstructorOption_ResolvesSpecifiedConstructor()
        {
            DiContainer.Register<IService, MyService>();

            DiContainer
                .Register<ClassWith3Ctors>(typeof(IService));

            var resolved = DiContainer.Resolve<ClassWith3Ctors>();
            Assert.Equal("(IService service)", resolved.ConstructorUsed);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType__UsingConstructorOption_ResolvesSpecifiedConstructor()
        {
            IDiContainer container = new DiContainer();
            container.Register<IService, MyService>();

            container
                .Register<ClassWith3Ctors>(typeof(IService));

            var resolved = container.Resolve<ClassWith3Ctors>();
            Assert.Equal("(IService service)", resolved.ConstructorUsed);
        }

        [Fact]
        public void StaticRegisterConcreteType_UsingConstructorOption_WrongCtor_ThrowsRegistrationException()
        {
            Assert.Throws<RegisterException>(() =>
                DiContainer
                    .Register<ClassWith3Ctors>(typeof(ConcreteOnly)));

            DiContainer.ResetContainer();
        }


        [Fact]
        public void StaticRegisterConcreteType_UsingConstructorOption_ResolvesInternalConstructor()
        {
            DiContainer
                .Register<ClassWithInternalConstructor>(typeof(ConcreteOnly));

            var resolved = DiContainer.Resolve<ClassWithInternalConstructor>();
            Assert.Equal("internal", resolved.ConstructorUsed);

            DiContainer.ResetContainer();
        }

        #endregion
        #endregion

        #region Resolve
        #region Internal

        [Fact]
        public void GetConstructorParams_gt1Constructor_ReturnsFromGreediestPublic()
        {
            //ClassWith3Ctors has 3 constructors, two public () & (IService)
            //and one internal (IService, ConcreteOnly)

            //Expect to ignore internal, but take greediest public ctor
            var exepectedParamters
                = typeof(ClassWith3Ctors)
                    .GetConstructor(new Type[] { typeof(IService) });//.GetParameters();

            var metaObj = new MetaObject(typeof(object), LifeCycle.Singleton);

            var parameters
                = metaObj.
                    GetBestConstructor(typeof(ClassWith3Ctors));

            Assert.Equal(exepectedParamters, parameters);
        }

        [Fact]
        public void GetConstructorParams_gt1Ctor_FlaggedCtor_ReturnsFlaggedNotGreediest()
        {
            //ClassWithFlaggedCtor has 2 public constructors
            //(IService) - Flagged
            //(IService, ConcreteOnly)

            //Expect to pick flagged ctor
            var exepectedParamters
                = typeof(ClassWithFlaggedCtor)
                    .GetConstructor(new Type[] { typeof(IService) });//.GetParameters();

            var metaObj = new MetaObject(typeof(object), LifeCycle.Singleton);

            var parameters
                = metaObj.
                    GetBestConstructor(typeof(ClassWithFlaggedCtor));

            Assert.Equal(exepectedParamters, parameters);
        }

        [Fact]
        public void GetConstructorParams_gt1Ctor_2FlaggedCtors_ThrowsResolveException()
        {
            var exepectedParamters
                = typeof(ClassWith2FlaggedCtors)
                    .GetConstructor(new Type[] { typeof(IService) }).GetParameters();

            var metaObj = new MetaObject(typeof(object), LifeCycle.Singleton);

            Assert.Throws<ResolveException>(() => metaObj.
                                GetBestConstructor(typeof(ClassWith2FlaggedCtors)));
        }

        [Fact]
        public void GetMetaObject_IsGenericType_NotConstructedGeneric_Throws()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            var container = new DiContainer();
            Assert.Throws<ResolveException>(()=>container.GetMetaObject(mock, typeof(IEnumerable<int>), null));
        }

        [Fact]
        public void GetMetaObject_UnregisteredInterface_Gt1Implementations_Throws()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            var container = new DiContainer();
            Assert.Throws<ResolveException>(() => container.GetMetaObject(mock, typeof(IServiceWithTwoImplementations), null));
        }

        [Fact]
        public void GetMetaObject_UnregisteredInterface_NoImplementations_Throws()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            var container = new DiContainer();
            Assert.Throws<ResolveException>(() => container.GetMetaObject(mock, typeof(IServiceWithNoImplementations), null));
        }

        [Fact]
        public void GetEnumerableException_Nothing_Registered_Throws()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            var container = new DiContainer();
            Assert.Throws<ResolveException>(()=>container.GetEnumerableExpression(mock, typeof(IEnumerable<IService>)));
            
        }

        [Fact]
        public void MakeNewExpression_ConstructorCacheNull_Throws()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            var container = new DiContainer();
            var metaObject = new MetaObject(new MyService());
            Assert.Null(metaObject.ConstructorCache);
            Assert.Throws<Exception>(() => container.MakeNewExpression(mock, metaObject));
        }

        #endregion

        [Fact]
        public void StaticResolve_Unregistered_Works()
        {
            var obj = DiContainer.Resolve<ClassThatsResolvableWithoutRegistering>();

            Assert.IsType<ClassThatsResolvableWithoutRegistering>(obj);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void Resolve_Unregistered_Works()
        {
            IDiContainer container = new DiContainer();
            var obj = container.Resolve<ClassThatsResolvableWithoutRegistering>();

            Assert.IsType<ClassThatsResolvableWithoutRegistering>(obj);

        }

        [Fact]
        public void StaticResolve_IsStrictModeTrue_Unregistered_Throws()
        {
            DiContainer.Initialize(o => o.TryResolveUnregistered = false);
            Assert.Throws<ResolveException>(
                () => DiContainer.Resolve<ClassThatsResolvableWithoutRegistering>());
        }

        [Fact]
        public void StaticResolve_KeyedDependency_Works()
        {
            DiContainer.Register<IService, MyService>("test");
            DiContainer.Register<ClassWithKeyedDependency>();
            var obj = DiContainer.Resolve<ClassWithKeyedDependency>();

            Assert.IsType<ClassWithKeyedDependency>(obj);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void StaticResolve_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            DiContainer.RegisterInstance(instance);
            var resolved = DiContainer.Resolve<MyService>();
            Assert.Equal(instance, resolved); //exactly same object returned

            DiContainer.ResetContainer();
        }

        [Fact]
        public void StaticResolve_InterfaceThatsNotRegistered_Works()
        {
            Assert.IsAssignableFrom<IService>(DiContainer.Resolve<IService>());

            DiContainer.ResetContainer();
        }


        //Silly test - just a throw in the constructor
        //[Fact]
        //public void StaticResolve_UnregisteredUnresolvable2_Throws()
        //{
        //    Assert.Throws<ResolveException>(
        //        () => DiContainer.Resolve<ClassThatsUnresolvable>());
        //}

        //Now have explicit child container
        //[Fact]
        //public void ResolveFromContainerInstance_RegisteredInStaticContainer_ResolvesFromStaticContainer()
        //{
        //    var registeredObject = new ClassThatHasToBeRegistered(3);
        //    DiContainer.RegisterInstance(registeredObject);

        //    IDiContainer container = new DiContainer();
        //    var resolved = container.Resolve<ClassThatHasToBeRegistered>();

        //    Assert.Equal(registeredObject, resolved);

        //    DiContainer.ResetContainer();
        //}


        [Fact]
        public void StaticResolve_Interface_ThrowsResolutionException()
        {
            Assert.Throws<RegisterException>(() => DiContainer.Register<IService>());
            DiContainer.ResetContainer();
        }

        [Fact]
        public void StaticResolveWithKey_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            DiContainer.RegisterInstance(instance, "test");
            var resolved = DiContainer.Resolve<MyService>("test");
            Assert.Equal(instance, resolved); //exactly same object returned

            DiContainer.ResetContainer();
        }

        [Fact]
        public void ResolveWithKey_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            IDiContainer container = new DiContainer();
            container.RegisterInstance(instance, "test");
            var resolved = container.Resolve<MyService>("test");
            Assert.Equal(instance, resolved); //exactly same object returned
        }

        [Fact]
        public void StaticResolveofSingleInstance_SecondResolve_ReturnsSavedInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<IService, MyService>().SingleInstance();

            Assert.Null(mock[new Tuple<Type, string>(typeof(IService), null)].Instance);

            //first resolve
            var first = DiContainer.Resolve<IService>();

            Assert.NotNull(mock[new Tuple<Type, string>(typeof(IService), null)].Instance);

            //second resolve
            var second = DiContainer.Resolve<IService>();

            Assert.Equal(first, second);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void StaticResolveofSingleInstance_SecondResolve_IsSameObject()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            var singleton = new MyService();
            DiContainer.RegisterInstance<IService>(singleton);

            //first resolve
            Assert.Equal(singleton, DiContainer.Resolve<IService>());

            //second resolve
            //first resolve
            Assert.Equal(singleton, DiContainer.Resolve<IService>());

            DiContainer.ResetContainer();
        }

        #region Resolve from RegisterExpression
        [Fact]
        public void StaticResolve_Expression_ReturnsInstance()
        {
            DiContainer.Register<IService, MyService>();
            DiContainer.RegisterExplicit<ClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(DiContainer.Resolve<IService>()));

            var resolved = DiContainer.Resolve<ClassWith3Ctors>();
            Assert.IsType<ClassWith3Ctors>(resolved);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void Resolve_Expression_ReturnsInstance()
        {
            IDiContainer container = new DiContainer();
            container.Register<IService, MyService>();
            container.RegisterExplicit<ClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(c.Resolve<IService>()));

            var resolved = container.Resolve<ClassWith3Ctors>();
            Assert.IsType<ClassWith3Ctors>(resolved);
        }

        [Fact]
        public void StaticResolve_ExpressionWithResolveType_ReturnsInstance()
        {
            DiContainer.Register<IService, MyService>();
            DiContainer.RegisterExplicit<IClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(DiContainer.Resolve<IService>()));

            var resolved = DiContainer.Resolve<IClassWith3Ctors>();
            Assert.IsType<ClassWith3Ctors>(resolved);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void Resolve_ExpressionWithResolveType_ReturnsInstance()
        {
            IDiContainer container = new DiContainer();
            container.Register<IService, MyService>();
            container.RegisterExplicit<IClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(c.Resolve<IService>()));

            var resolved = container.Resolve<IClassWith3Ctors>();
            Assert.IsType<ClassWith3Ctors>(resolved);
        }


        [Fact]
        public void StaticResolve_ExpressionWithKey_ReturnsInstance()
        {
            DiContainer.Register<IService, MyService>();
            DiContainer.RegisterExplicit<ClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(DiContainer.Resolve<IService>()), "test");

            var resolved = DiContainer.Resolve<ClassWith3Ctors>("test");
            Assert.IsType<ClassWith3Ctors>(resolved);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void Resolve_ExpressionWithKey_ReturnsInstance()
        {
            IDiContainer container = new DiContainer();
            container.Register<IService, MyService>();
            container.RegisterExplicit<ClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(c.Resolve<IService>()), "test");

            var resolved = container.Resolve<ClassWith3Ctors>("test");
            Assert.IsType<ClassWith3Ctors>(resolved);
        }

        [Fact]
        public void StaticResolve_ExpressionWithResolveTypeWithKey_ReturnsInstance()
        {
            DiContainer.Register<IService, MyService>();
            DiContainer.RegisterExplicit<IClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(DiContainer.Resolve<IService>()), "test");

            var resolved = DiContainer.Resolve<IClassWith3Ctors>("test");
            Assert.IsType<ClassWith3Ctors>(resolved);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void Resolve_ExpressionWithResolveTypeWithKey_ReturnsInstance()
        {
            IDiContainer container = new DiContainer();
            container.Register<IService, MyService>();
            container.RegisterExplicit<IClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(c.Resolve<IService>()), "test");

            var resolved = container.Resolve<IClassWith3Ctors>("test");
            Assert.IsType<ClassWith3Ctors>(resolved);
        }

        #endregion

        #region Resolve(Type) overloads
        [Fact]
        public void StaticResolveType_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            DiContainer.RegisterInstance(instance);
            var resolved = DiContainer.Resolve(typeof(MyService));
            Assert.Equal(instance, resolved); //exactly same object returned

            DiContainer.ResetContainer();
        }

        [Fact]
        public void ResolveType_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            IDiContainer container = new DiContainer();
            container.RegisterInstance(instance);
            var resolved = container.Resolve(typeof(MyService));
            Assert.Equal(instance, resolved); //exactly same object returned
        }

        [Fact]
        public void StaticResolveTypeWithKey_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            DiContainer.RegisterInstance(instance, "test");
            var resolved = DiContainer.Resolve(typeof(MyService), "test");
            Assert.Equal(instance, resolved); //exactly same object returned

            DiContainer.ResetContainer();
        }

        [Fact]
        public void ResolveTypeWithKey_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            IDiContainer container = new DiContainer();
            container.RegisterInstance(instance, "test");
            var resolved = container.Resolve(typeof(MyService), "test");
            Assert.Equal(instance, resolved); //exactly same object returned
        }

        #endregion

        #endregion

        #region Unregister
        [Fact]
        public void StaticUnregister_NotRegistered_ThrowsResolutionExceptoin()
        {
            Assert.Throws<ResolveException>(() => DiContainer.Unregister<ConcreteOnly>());
        }

        [Fact]
        public void StaticUnregister_ExceptionWhileDisposing_ThrowsException()
        {
            //Ensure MetaObject.Instance is set
            DiContainer.Register<ClassThatThrowsOnDisposed>().SingleInstance();

            DiContainer.Resolve<ClassThatThrowsOnDisposed>();

            Assert.Throws<Exception>(() => DiContainer.Unregister<ClassThatThrowsOnDisposed>());

            DiContainer.ResetContainer();
        }

        [Fact]
        public void StaticUnregister_Registered_IsRemoved()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<ConcreteOnly>();

            Assert.NotEmpty(mock);

            DiContainer.Unregister<ConcreteOnly>();

            Assert.Empty(mock);

            DiContainer.ResetContainer();
        }
        [Fact]
        public void Unregister_Registered_IsRemoved()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.Register<ConcreteOnly>();

            Assert.NotEmpty(mock);

            container.Unregister<ConcreteOnly>();

            Assert.Empty(mock);
        }
        [Fact]
        public void StaticUnregisterWithKey_Registered_IsRemoved()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<ConcreteOnly>("test");

            Assert.NotEmpty(mock);

            DiContainer.Unregister<ConcreteOnly>("test");

            Assert.Empty(mock);

            DiContainer.ResetContainer();
        }
        [Fact]
        public void UnregisterWithKey_Registered_IsRemoved()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.Register<ConcreteOnly>("test");

            Assert.NotEmpty(mock);

            container.Unregister<ConcreteOnly>("test");

            Assert.Empty(mock);
        }
        [Fact]
        public void StaticUnregisterWithKey_NotRegistered_ThrowsResolutionException()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<ConcreteOnly>("test");

            Assert.NotEmpty(mock);

            Assert.Throws<ResolveException>(
                () => DiContainer.Unregister<ConcreteOnly>("wrongKey"));

            DiContainer.ResetContainer();
        }

        [Fact]
        public void StaticUnregister_Registered_DisposeCalled()
        {
            bool wasDisposedExplicitly = false;
            var disposable = new ClassThatsDisposable(() => wasDisposedExplicitly = true, () => { });
            DiContainer.RegisterInstance<IClassThatsDisposable>(disposable);
            DiContainer.Resolve<IClassThatsDisposable>();

            DiContainer.Unregister<IClassThatsDisposable>();

            Assert.True(wasDisposedExplicitly);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void StaticUnregisterAll()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            DiContainer.SetContainer(mock);

            DiContainer.Register<MyService>();
            DiContainer.Register<ConcreteOnly>();
            DiContainer.Register<ClassWith3Ctors>();

            Assert.Equal(3, mock.Count);

            DiContainer.UnregisterAll();

            Assert.Empty(mock);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void UnregisterAll()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.Register<MyService>();
            container.Register<ConcreteOnly>();
            container.Register<ClassWith3Ctors>();

            Assert.Equal(3, mock.Count);

            container.UnregisterAll();

            Assert.Empty(mock);
        }

        [Fact]
        public void Dispose()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IDiContainer container = new DiContainer(mock, ContainerOptions.Default);

            container.Register<MyService>();
            container.Register<ConcreteOnly>();
            container.Register<ClassWith3Ctors>();

            Assert.Equal(3, mock.Count);

            container.Dispose();

            Assert.Empty(mock);
        }

        #endregion

        #region Exceptions
        [Fact]
        public void StaticRegisterConcreteType_Duplicate_ThrowsRegistrationException()
        {
            DiContainer.Register<MyService>();
            Assert.Throws<RegisterException>(() => DiContainer.Register<MyService>());
        }

        [Fact]
        public void StaticRegisterConcreteTypeWithKey_Duplicate_ThrowsRegistrationException()
        {
            DiContainer.Register<MyService>("test");
            Assert.Throws<RegisterException>(() => DiContainer.Register<MyService>("test"));
        }

        [Fact]
        public void StaticResolveClassWithInternalConstructor_ResolvesPublicEvenThoughInternalIsGreediest()
        {
            DiContainer
                .Register<ClassWithInternalConstructor>();

            var resolved = DiContainer.Resolve<ClassWithInternalConstructor>();
            Assert.Equal("public", resolved.ConstructorUsed);

            DiContainer.ResetContainer();
        }



        #endregion

    }
}
