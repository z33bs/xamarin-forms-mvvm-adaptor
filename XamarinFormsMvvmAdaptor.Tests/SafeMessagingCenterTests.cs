using System;
using System.Globalization;
using XamarinFormsMvvmAdaptor.Helpers;
using Xunit;


namespace XamarinFormsMvvmAdaptor.Tests
{

    public class SafeMessagingCenterTests
    {
        TestSubcriber _subscriber;

        CultureInfo _defaultCulture;
        CultureInfo _defaultUICulture;


        public virtual void Setup()
        {
            _defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            _defaultUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            //Device.PlatformServices = new MockPlatformServices();
        }


        public virtual void TearDown()
        {
            //Device.PlatformServices = null;
            System.Threading.Thread.CurrentThread.CurrentCulture = _defaultCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = _defaultUICulture;
        }

        [Fact]
        public void SingleSubscriber()
        {
            string sentMessage = null;
            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests, string>(this, "SimpleTest", (sender, args) => sentMessage = args);

            SafeMessagingCenter.Send(this, "SimpleTest", "My Message");

            Assert.Equal("My Message", sentMessage);

            SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests, string>(this, "SimpleTest");
        }

        [Fact]
        public void Filter()
        {
            string sentMessage = null;
            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests, string>(this, "SimpleTest", callback:(sender, args) => sentMessage = args, source:this);

            SafeMessagingCenter.Send(new SafeMessagingCenterTests(), "SimpleTest", "My Message");

            Assert.Null(sentMessage);

            SafeMessagingCenter.Send(this, "SimpleTest", "My Message");
            
            Assert.Equal("My Message", sentMessage);

            SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests, string>(this, "SimpleTest");
        }

        [Fact]
        public void MultiSubscriber()
        {
            var sub1 = new object();
            var sub2 = new object();
            string sentMessage1 = null;
            string sentMessage2 = null;
            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests, string>(sub1, "SimpleTest", (sender, args) => sentMessage1 = args);
            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests, string>(sub2, "SimpleTest", (sender, args) => sentMessage2 = args);

            SafeMessagingCenter.Send(this, "SimpleTest", "My Message");

            Assert.Equal("My Message", sentMessage1);
            Assert.Equal("My Message", sentMessage2);

            SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests, string>(sub1, "SimpleTest");
            SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests, string>(sub2, "SimpleTest");
        }

        [Fact]
        public void Unsubscribe()
        {
            string sentMessage = null;
            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests, string>(this, "SimpleTest", (sender, args) => sentMessage = args);
            SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests, string>(this, "SimpleTest");

            SafeMessagingCenter.Send(this, "SimpleTest", "My Message");

            Assert.Null(sentMessage);
        }

        [Fact]
        public void SendWithoutSubscribers()
        {
            var exception = Record.Exception(() => SafeMessagingCenter.Send(this, "SimpleTest", "My Message"));
            Assert.Null(exception);
        }

        [Fact]
        public void NoArgSingleSubscriber()
        {
            bool sentMessage = false;
            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests>(this, "SimpleTest", sender => sentMessage = true);

            SafeMessagingCenter.Send(this, "SimpleTest");

            Assert.True(sentMessage);

            SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests>(this, "SimpleTest");
        }

        [Fact]
        public void NoArgFilter()
        {
            bool sentMessage = false;
            SafeMessagingCenter.Subscribe(this, "SimpleTest", (sender) => sentMessage = true, source:this);

            SafeMessagingCenter.Send(new SafeMessagingCenterTests(), "SimpleTest");

            Assert.False(sentMessage);

            SafeMessagingCenter.Send(this, "SimpleTest");

            Assert.True(sentMessage);

            SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests>(this, "SimpleTest");
        }

        [Fact]
        public void NoArgMultiSubscriber()
        {
            var sub1 = new object();
            var sub2 = new object();
            bool sentMessage1 = false;
            bool sentMessage2 = false;
            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests>(sub1, "SimpleTest", (sender) => sentMessage1 = true);
            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests>(sub2, "SimpleTest", (sender) => sentMessage2 = true);

            SafeMessagingCenter.Send(this, "SimpleTest");

            Assert.True(sentMessage1);
            Assert.True(sentMessage2);

            SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests>(sub1, "SimpleTest");
            SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests>(sub2, "SimpleTest");
        }

        [Fact]
        public void NoArgUnsubscribe()
        {
            bool sentMessage = false;
            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests>(this, "SimpleTest", (sender) => sentMessage = true);
            SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests>(this, "SimpleTest");

            SafeMessagingCenter.Send(this, "SimpleTest", "My Message");

            Assert.False(sentMessage);
        }

        [Fact]
        public void NoArgSendWithoutSubscribers()
        {
            var exception = Record.Exception(() => SafeMessagingCenter.Send(this, "SimpleTest"));
            Assert.Null(exception);
        }

        [Fact]
        public void ThrowOnNullArgs()
        {
            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Subscribe<SafeMessagingCenterTests, string>(null, "Foo", (sender, args) => { }));
            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Subscribe<SafeMessagingCenterTests, string>(this, null, (sender, args) => { }));
            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Subscribe<SafeMessagingCenterTests, string>(this, "Foo", null));

            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Subscribe<SafeMessagingCenterTests>(null, "Foo", (sender) => { }));
            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Subscribe<SafeMessagingCenterTests>(this, null, (sender) => { }));
            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Subscribe(this, "Foo",(Action<SafeMessagingCenterTests>)null));

            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Send<SafeMessagingCenterTests, string>(null, "Foo", "Bar"));
            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Send<SafeMessagingCenterTests, string>(this, null, "Bar"));

            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Send<SafeMessagingCenterTests>(null, "Foo"));
            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Send<SafeMessagingCenterTests>(this, null));

            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests>(null, "Foo"));
            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests>(this, null));

            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests, string>(null, "Foo"));
            Assert.Throws<ArgumentNullException>(() => SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests, string>(this, null));
        }

        [Fact]
        public void UnsubscribeInCallback()
        {
            int messageCount = 0;

            var subscriber1 = new object();
            var subscriber2 = new object();

            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests>(subscriber1, "SimpleTest", (sender) =>
            {
                messageCount++;
                SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests>(subscriber2, "SimpleTest");
            });

            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests>(subscriber2, "SimpleTest", (sender) =>
            {
                messageCount++;
                SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests>(subscriber1, "SimpleTest");
            });

            SafeMessagingCenter.Send(this, "SimpleTest");

            Assert.Equal(1, messageCount);
        }

        [Fact]
        public void SubscriberShouldBeCollected()
        {
            new Action(() =>
            {
                var subscriber = new TestSubcriber();
                SafeMessagingCenter.Subscribe<TestPublisher>(subscriber, "test", p => Assert.True(false)); //ie Assert.Fail
            })();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var pub = new TestPublisher();
            pub.Test(); // Assert.Fail() shouldn't be called, because the TestSubcriber object should have ben GCed
        }

        [Fact]
        public void ShouldBeCollectedIfCallbackTargetIsSubscriber()
        {
            WeakReference wr = null;

            new Action(() =>
            {
                var subscriber = new TestSubcriber();

                wr = new WeakReference(subscriber);

                subscriber.SubscribeToTestMessages();
            })();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var pub = new TestPublisher();
            pub.Test();

            Assert.False(wr.IsAlive); // The Action target and subscriber were the same object, so both could be collected
        }

        [Fact]
        public void NotCollectedIfSubscriberIsNotTheCallbackTarget()
        {
            WeakReference wr = null;

            new Action(() =>
            {
                var subscriber = new TestSubcriber();

                wr = new WeakReference(subscriber);

                // This creates a closure, so the callback target is not 'subscriber', but an instancce of a compiler generated class 
                // So MC has to keep a strong reference to it, and 'subscriber' won't be collectable
                SafeMessagingCenter.Subscribe<TestPublisher>(subscriber, "test", p => subscriber.SetSuccess());
            })();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.True(wr.IsAlive); // The closure in Subscribe should be keeping the subscriber alive
            Assert.NotNull(wr.Target as TestSubcriber);

            Assert.False(((TestSubcriber)wr.Target).Successful);

            var pub = new TestPublisher();
            pub.Test();

            Assert.True(((TestSubcriber)wr.Target).Successful);  // Since it's still alive, the subscriber should still have received the message and updated the property
        }

        //todo Test fails - why?
        //[Fact]
        //public void SubscriberCollectableAfterUnsubscribeEvenIfHeldByClosure()
        //{
        //	WeakReference wr = null;

        //	new Action(() =>
        //	{
        //		var subscriber = new TestSubcriber();

        //		wr = new WeakReference(subscriber);

        //		MessagingCenter.Subscribe<TestPublisher>(subscriber, "test", p => subscriber.SetSuccess());
        //	})();

        //	Assert.NotNull(wr.Target as TestSubcriber);

        //	MessagingCenter.Unsubscribe<TestPublisher>(wr.Target, "test");

        //	GC.Collect();
        //	GC.WaitForPendingFinalizers();

        //	Assert.False(wr.IsAlive); // The Action target and subscriber were the same object, so both could be collected
        //}

        [Fact]
        public void StaticCallback()
        {
            int i = 4;

            _subscriber = new TestSubcriber(); // Using a class member so it doesn't get optimized away in Release build

            SafeMessagingCenter.Subscribe<TestPublisher>(_subscriber, "test", p => MessagingCenterTestsCallbackSource.Increment(ref i));

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var pub = new TestPublisher();
            pub.Test();

            Assert.True(i == 5, "The static method should have incremented 'i'");
        }

        [Fact]
        public void NothingShouldBeCollected()
        {
            var success = false;

            _subscriber = new TestSubcriber(); // Using a class member so it doesn't get optimized away in Release build

            var source = new MessagingCenterTestsCallbackSource();
            SafeMessagingCenter.Subscribe<TestPublisher>(_subscriber, "test", p => source.SuccessCallback(ref success));

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var pub = new TestPublisher();
            pub.Test();

            Assert.True(success); // TestCallbackSource.SuccessCallback() should be invoked to make success == true
        }

        [Fact]
        public void MultipleSubscribersOfTheSameClass()
        {
            var sub1 = new object();
            var sub2 = new object();

            string args2 = null;

            const string message = "message";

            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests, string>(sub1, message, (sender, args) => { });
            SafeMessagingCenter.Subscribe<SafeMessagingCenterTests, string>(sub2, message, (sender, args) => args2 = args);
            SafeMessagingCenter.Unsubscribe<SafeMessagingCenterTests, string>(sub1, message);

            SafeMessagingCenter.Send(this, message, "Testing");
            Assert.Equal("Testing", args2); //unsubscribing sub1 should not unsubscribe sub2
        }

        class TestSubcriber
        {
            public void SetSuccess()
            {
                Successful = true;
            }

            public bool Successful { get; private set; }

            public void SubscribeToTestMessages()
            {
                SafeMessagingCenter.Subscribe<TestPublisher>(this, "test", p => SetSuccess());
            }
        }

        class TestPublisher
        {
            public void Test()
            {
                SafeMessagingCenter.Send(this, "test");
            }
        }

        public class MessagingCenterTestsCallbackSource
        {
            public void SuccessCallback(ref bool success)
            {
                success = true;
            }

            public static void Increment(ref int i)
            {
                i = i + 1;
            }
        }

        //[Fact] //This is a demonstration of what a test with a fake/mock/substitute IMessagingCenter might look like
        //public void TestMessagingCenterSubstitute()
        //{
        //    var mc = new FakeMessagingCenter();

        //    // In the real world, you'd construct this with `new ComponentWithMessagingDependency(MessagingCenter.Instance);`
        //    var component = new ComponentWithMessagingDependency(mc);
        //    component.DoAThing();

        //    Assert.True(mc.WasSubscribeCalled, "ComponentWithMessagingDependency should have subscribed in its constructor");
        //    Assert.True(mc.WasSendCalled, "The DoAThing method should send a message");
        //}

        class ComponentWithMessagingDependency
        {
            readonly ISafeMessagingCenter _messagingCenter;

            public ComponentWithMessagingDependency(ISafeMessagingCenter messagingCenter)
            {
                _messagingCenter = messagingCenter;
                _messagingCenter.Subscribe<ComponentWithMessagingDependency>(this, "test", dependency => Console.WriteLine("test"));
            }

            public void DoAThing()
            {
                _messagingCenter.Send(this, "test");
            }
        }

        //internal class FakeMessagingCenter : IMessagingCenter
        //{
        //    public bool WasSubscribeCalled { get; private set; }
        //    public bool WasSendCalled { get; private set; }

        //    public void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
        //    {
        //        WasSendCalled = true;
        //    }

        //    public void Send<TSender>(TSender sender, string message) where TSender : class
        //    {
        //        WasSendCalled = true;
        //    }

        //    public void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, Action<Exception>? onException, TSender source = default(TSender)) where TSender : class
        //    {
        //        WasSubscribeCalled = true;
        //    }

        //    public void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, Action<Exception>? onException, TSender source = default(TSender)) where TSender : class
        //    {
        //        WasSubscribeCalled = true;
        //    }

        //    public void Subscribe<TSender, TArgs>(object subscriber, string message, Func<TSender, TArgs, Task> asyncCallback, Action<Exception> onException, TSender source) where TSender : class
        //    {
        //        WasSubscribeCalled = true;
        //    }

        //    public void Subscribe<TSender>(object subscriber, string message, Func<TSender, Task> asyncCallback, Action<Exception> onException, TSender source) where TSender : class
        //    {
        //        WasSubscribeCalled = true;
        //    }

        //    public void SubscribeAny<TArgs>(object subscriber, string message, Action<object, TArgs> callback, Action<Exception>? onException)
        //    {
        //        WasSubscribeCalled = true;
        //    }

        //    public void SubscribeAny(object subscriber, string message, Action<object> callback, Action<Exception>? onException)
        //    {
        //        WasSubscribeCalled = true;
        //    }

        //    public void UnfilteredSubscribe<TArgs>(object subscriber, string message, Func<object, TArgs, Task> callback, Action<Exception> onException = null)
        //    {
        //        WasSubscribeCalled = true;
        //    }

        //    public void UnfilteredSubscribe(object subscriber, string message, Func<object, Task> callback, Action<Exception> onException = null)
        //    {
        //        WasSubscribeCalled = true;
        //    }

        //    public void UnfilteredSubscribe(object subscriber, string message, Func<object, Task> callback, Action<Exception> onException = null, bool isBlocking = false)
        //    {
        //        WasSubscribeCalled = true;
        //    }

        //    public void UnfilteredSubscribe(object subscriber, string message, Func<object, Task> callback, Action<Exception> onException = null, IViewModelBase viewModel = null)
        //    {
        //        WasSubscribeCalled = true;
        //    }

        //    public void UnsubscribeAny<TArgs>(object subscriber, string message)
        //    {

        //    }

        //    public void UnsubscribeAny(object subscriber, string message)
        //    {

        //    }

        //    public void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class
        //    {

        //    }

        //    public void Unsubscribe<TSender>(object subscriber, string message) where TSender : class
        //    {

        //    }
        //}
    }
}