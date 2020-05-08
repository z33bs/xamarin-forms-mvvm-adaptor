using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    public class MessagingCenter : IMessagingCenter
    {
        public static IMessagingCenter Instance { get; } = new MessagingCenter();

        class Sender : Tuple<string, Type, Type>
        {
            public Sender(string message, Type senderType, Type argType) : base(message, senderType, argType)
            {
            }
        }

        delegate bool Filter(object sender);

        class MaybeWeakReference
        {
            WeakReference DelegateWeakReference { get; }
            object DelegateStrongReference { get; }

            readonly bool _isStrongReference;

            public MaybeWeakReference(object subscriber, object delegateSource)
            {
                if (subscriber.Equals(delegateSource))
                {
                    // The target is the subscriber; we can use a weakreference
                    DelegateWeakReference = new WeakReference(delegateSource);
                    _isStrongReference = false;
                }
                else
                {
                    DelegateStrongReference = delegateSource;
                    _isStrongReference = true;
                }
            }

            public object Target => _isStrongReference ? DelegateStrongReference : DelegateWeakReference.Target;
            public bool IsAlive => _isStrongReference || DelegateWeakReference.IsAlive;
        }

        class Subscription : Tuple<WeakReference, MaybeWeakReference, MethodInfo>
        {


            public Subscription(object subscriber, object delegateSource, MethodInfo methodInfo)
                : base(new WeakReference(subscriber), new MaybeWeakReference(subscriber, delegateSource), methodInfo)
            {
            }

            public WeakReference Subscriber => Item1;
            MaybeWeakReference DelegateSource => Item2;
            MethodInfo MethodInfo => Item3;

            //Filter Filter => Item4;
            public Action<Exception>? OnException { get; set; } = null;
            public bool IsBlocking { get; set; } = false;
            public object Source { get; set; } = null;
            IViewModelBase viewModel = null;
            public IViewModelBase ViewModel
            {
                get => viewModel;
                set
                {
                    if (value != null)
                        IsBlocking = true;

                    viewModel = value;
                }
            }

            bool _isBusy;
            bool IsBusy
            {
                get
                {
                    if (ViewModel?.IsBusy ?? false)
                        return true;

                    return _isBusy;
                }
                set
                {
                    _isBusy = value;
                    if (ViewModel != null) ViewModel.IsBusy = value;
                }
            }

            public void InvokeCallback(object sender, object args)
            {
                if (IsBusy)
                    return;

                if (IsBlocking)
                    IsBusy = true;
                //Filter filter = sender =>
                //{
                //    var send = (TSender)sender;
                //    return (source == null || send == source);
                //};
                if (Source != null && sender != Source)
                    return;

                //if (MethodInfo.IsStatic)
                //{
                //    MethodInfo.Invoke(null, MethodInfo.GetParameters().Length == 1 ? new[] { sender } : new[] { sender, args });
                //    return;
                //}

                var target = DelegateSource.Target;

                if (target == null)
                {
                    return; // Collected 
                }

                var parameters = MethodInfo.GetParameters().Length == 1 ? new[] { sender } : new[] { sender, args };
                //var parameters = MethodInfo.GetParameters().Any()
                //    ? (MethodInfo.GetParameters().Length == 1
                //        ? new[] { sender }
                //        : new[] { sender, args })
                //    : new object[] { };

                if (MethodInfo.ReturnType != typeof(Task))
                {
                    try
                    {
                        MethodInfo.Invoke(MethodInfo.IsStatic ? null : target, parameters);
                    }
                    catch (Exception ex)
                        when (SafeExecutionHelpers.DefaultExceptionHandler != null
                              || OnException != null)
                    {
                        SafeExecutionHelpers.HandleException(ex, OnException);
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }
                else
                {
                    Task.Run(()=>((Task)MethodInfo
                        .Invoke(MethodInfo.IsStatic
                                    ? null
                                    : target
                                , parameters))
                        .SafeContinueWith(OnException)
                        .ContinueWith(t => IsBusy = false));

                    //((Task)MethodInfo
                    //    .Invoke(MethodInfo.IsStatic
                    //                ? null
                    //                : target
                    //            , parameters))
                    //    .SafeContinueWith(OnException)
                    //    .ContinueWith(t => IsBusy = false);
                }
            }

            public bool CanBeRemoved()
            {
                return !Subscriber.IsAlive || !DelegateSource.IsAlive;
            }
        }

        readonly Dictionary<Sender, List<Subscription>> _subscriptions =
            new Dictionary<Sender, List<Subscription>>();

        public static void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
        {
            Instance.Send(sender, message, args);
        }

        void IMessagingCenter.Send<TSender, TArgs>(TSender sender, string message, TArgs args)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));
            InnerSend(message, typeof(TSender), typeof(TArgs), sender, args);
        }

        public static void Send<TSender>(TSender sender, string message) where TSender : class
        {
            Instance.Send(sender, message);
        }

        void IMessagingCenter.Send<TSender>(TSender sender, string message)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));
            InnerSend(message, typeof(TSender), null, sender, null);
        }

        public static void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, Action<Exception>? onException = null, TSender source = null) where TSender : class
        {
            Instance.Subscribe(subscriber, message, callback, onException, source);
        }

        void IMessagingCenter.Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, Action<Exception>? onException, TSender source)
        {
            ThrowIfNull(subscriber,message, callback);

            var key = new Sender(message, typeof(TSender), typeof(TArgs));
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
                { OnException = onException, Source = source };

            AddSubscription(key, value);
        }

        private static void ThrowIfNull(object subscriber, string message, Delegate callback)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
        }

        public static void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, Action<Exception>? onException = null, TSender source = null) where TSender : class
        {
            Instance.Subscribe(subscriber, message, callback, onException, source);
        }

        void IMessagingCenter.Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, Action<Exception>? onException, TSender source)
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, typeof(TSender), null);
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException, Source = source };

            AddSubscription(key, value);
        }

        public static void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class
        {
            Instance.Unsubscribe<TSender, TArgs>(subscriber, message);
        }

        void IMessagingCenter.Unsubscribe<TSender, TArgs>(object subscriber, string message)
        {
            InnerUnsubscribe(message, typeof(TSender), typeof(TArgs), subscriber);
        }

        public static void Unsubscribe<TSender>(object subscriber, string message) where TSender : class
        {
            Instance.Unsubscribe<TSender>(subscriber, message);
        }

        void IMessagingCenter.Unsubscribe<TSender>(object subscriber, string message)
        {
            InnerUnsubscribe(message, typeof(TSender), null, subscriber);
        }

        void InnerSend(string message, Type senderType, Type argType, object sender, object args)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            //NB to not make a new list, but keep handle
            // on _subscriptions so if unregister while
            // in callback, will be picked up
            List<Subscription> subcriptions;

            var key = new Sender(message, senderType, argType);
            if (_subscriptions.ContainsKey(key))
            {
                subcriptions = _subscriptions[key];
                InvokeCallbacks(sender, args, subcriptions);
            }

            //Process unfiltered version
            key = new Sender(message, null, argType);
            if (_subscriptions.ContainsKey(key))
            {
                subcriptions = _subscriptions[key];
                InvokeCallbacks(sender, args, subcriptions);
            }
        }

        private static void InvokeCallbacks(object sender, object args, List<Subscription> subcriptions)
        {
            if (subcriptions == null || !subcriptions.Any())
                return;

            // ok so this code looks a bit funky but here is the gist of the problem. It is possible that in the course
            // of executing the callbacks for this message someone will subscribe/unsubscribe from the same message in
            // the callback. This would invalidate the enumerator. To work around this we make a copy. However if you unsubscribe 
            // from a message you can fairly reasonably expect that you will therefor not receive a call. To fix this we then
            // check that the item we are about to send the message to actually exists in the live list.
            List<Subscription> subscriptionsCopy = subcriptions.ToList();
            foreach (Subscription subscription in subscriptionsCopy)
            {
                if (subscription.Subscriber.Target != null && subcriptions.Contains(subscription))
                {
                    subscription.InvokeCallback(sender, args);
                }
            }
        }

        private void AddSubscription(Sender key, Subscription value)
        {
            if (_subscriptions.ContainsKey(key))
            {
                _subscriptions[key].Add(value);
            }
            else
            {
                var list = new List<Subscription> { value };
                _subscriptions[key] = list;
            }
        }

        void InnerUnsubscribe(string message, Type senderType, Type argType, object subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var key = new Sender(message, senderType, argType);
            if (!_subscriptions.ContainsKey(key))
                return;
            _subscriptions[key].RemoveAll(sub => sub.CanBeRemoved() || sub.Subscriber.Target == subscriber);
            if (!_subscriptions[key].Any())
                _subscriptions.Remove(key);
        }

        // This is a bit gross; it only exists to support the unit tests in PageTests
        // because the implementations of ActionSheet, Alert, and IsBusy are all very
        // tightly coupled to the MessagingCenter singleton 
        internal static void ClearSubscribers()
        {
            (Instance as MessagingCenter)?._subscriptions.Clear();
        }

        #region Unfiltered Overloads
        public static void UnfilteredSubscribe<TArgs>(object subscriber, string message, Action<object, TArgs> callback, Action<Exception>? onException = null)
            => Instance.UnfilteredSubscribe(subscriber, message, callback, onException);

        void IMessagingCenter.UnfilteredSubscribe<TArgs>(object subscriber, string message, Action<object, TArgs> callback, Action<Exception>? onException)
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, null, typeof(TArgs));
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException };

            AddSubscription(key, value);
        }

        public static void UnfilteredSubscribe(object subscriber, string message, Action<object> callback, Action<Exception>? onException = null)
            => Instance.UnfilteredSubscribe(subscriber, message, callback, onException);

        void IMessagingCenter.UnfilteredSubscribe(object subscriber, string message, Action<object> callback, Action<Exception>? onException)
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, null, null);
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException };

            AddSubscription(key, value);
        }

        //public static void AnonymousSend<TArgs>(string message, TArgs args)
        //{
        //    Instance.AnonymousSend(message, args);
        //}

        //void IMessagingCenter.AnonymousSend<TArgs>(string message, TArgs args)
        //{
        //    InnerSend(message, null, typeof(TArgs), null, args);
        //}

        //public static void AnonymousSend(string message)
        //{
        //    Instance.AnonymousSend(message);
        //}

        //void IMessagingCenter.AnonymousSend(string message)
        //{
        //    InnerSend(message, null, null, null, null);
        //}

        public static void UnfilteredUnsubscribe<TArgs>(object subscriber, string message)
            => Instance.UnfilteredUnsubscribe<TArgs>(subscriber, message);

        void IMessagingCenter.UnfilteredUnsubscribe<TArgs>(object subscriber, string message)
        {
            InnerUnsubscribe(message, null, typeof(TArgs), subscriber);
        }

        public static void UnfilteredUnsubscribe(object subscriber, string message)
            => Instance.UnfilteredUnsubscribe(subscriber, message);

        void IMessagingCenter.UnfilteredUnsubscribe(object subscriber, string message)
        {
            InnerUnsubscribe(message, null, null, subscriber);
        }


        #endregion
        #region Func<Task> overloads
        public static void Subscribe<TSender, TArgs>(object subscriber, string message, Func<TSender, TArgs, Task> asyncCallback, Action<Exception>? onException = null, TSender source = null) where TSender : class
        {
            Instance.Subscribe(subscriber, message, asyncCallback, onException, source);
        }

        void IMessagingCenter.Subscribe<TSender, TArgs>(object subscriber, string message, Func<TSender, TArgs, Task> asyncCallback, Action<Exception>? onException, TSender source)
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, typeof(TSender), typeof(TArgs));
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, Source = source };

            AddSubscription(key, value);
        }

        public static void Subscribe<TSender>(object subscriber, string message, Func<TSender, Task> asyncCallback, Action<Exception>? onException = null, TSender source = null) where TSender : class
        {
            Instance.Subscribe(subscriber, message, asyncCallback, onException, source);
        }

        void IMessagingCenter.Subscribe<TSender>(object subscriber, string message, Func<TSender, Task> asyncCallback, Action<Exception>? onException, TSender source)
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, typeof(TSender), null);
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, Source = source };

            AddSubscription(key, value);
        }

        public static void UnfilteredSubscribe<TArgs>(object subscriber, string message, Func<object, TArgs, Task> asyncCallback, Action<Exception>? onException = null)
    => Instance.UnfilteredSubscribe(subscriber, message, asyncCallback, onException);

        void IMessagingCenter.UnfilteredSubscribe<TArgs>(object subscriber, string message, Func<object, TArgs, Task> asyncCallback, Action<Exception>? onException)
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, null, typeof(TArgs));
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException };

            AddSubscription(key, value);
        }

        public static void UnfilteredSubscribe(object subscriber, string message, Func<object, Task> asyncCallback, Action<Exception>? onException = null)
            => Instance.UnfilteredSubscribe(subscriber, message, asyncCallback, onException);

        void IMessagingCenter.UnfilteredSubscribe(object subscriber, string message, Func<object, Task> asyncCallback, Action<Exception>? onException)
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, null, null);
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException};

            AddSubscription(key, value);
        }


        // Test bool
        public static void UnfilteredSubscribe(object subscriber, string message, Func<object, Task> asyncCallback, Action<Exception>? onException = null, bool isBlocking = false)
            => Instance.UnfilteredSubscribe(subscriber, message, asyncCallback, onException, isBlocking);

        void IMessagingCenter.UnfilteredSubscribe(object subscriber, string message, Func<object, Task> asyncCallback, Action<Exception>? onException, bool isBlocking)
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, null,null);
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, IsBlocking = isBlocking };

            AddSubscription(key, value);
        }

        // Test viewModel
        public static void UnfilteredSubscribe(object subscriber, string message, Func<object, Task> asyncCallback, Action<Exception>? onException = null, IViewModelBase viewModel = null)
            => Instance.UnfilteredSubscribe(subscriber, message, asyncCallback, onException, viewModel);

        void IMessagingCenter.UnfilteredSubscribe(object subscriber, string message, Func<object, Task> asyncCallback, Action<Exception>? onException, IViewModelBase viewModel)
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, null, null);
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, ViewModel = viewModel };

            AddSubscription(key, value);
        }

        #endregion
    }
}
