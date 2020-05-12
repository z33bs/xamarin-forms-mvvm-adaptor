﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <inheritdoc/>
    public class SafeMessagingCenter : ISafeMessagingCenter
    {
        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static ISafeMessagingCenter Instance { get; } = new SafeMessagingCenter();

        class Sender : Tuple<string, Type, Type>
        {
            public Sender(string message, Type senderType, Type argType) : base(message, senderType, argType)
            {
            }
        }

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

            public Action<Exception> OnException { get; set; } = null;
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

                if (Source != null && sender != Source)
                    return;

                var target = DelegateSource.Target;

                if (target == null)
                {
                    return; // Collected 
                }

                var parameters = MethodInfo.GetParameters().Length == 1
                    ? new[] { sender }
                    : new[] { sender, args };

                if (MethodInfo.ReturnType != typeof(Task))
                {
                    try
                    {
                        MethodInfo.Invoke(MethodInfo.IsStatic ? null : target, parameters);
                    }
                    catch (Exception ex)
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
                    Task.Run(() => ((Task)MethodInfo
                        .Invoke(MethodInfo.IsStatic
                                    ? null
                                    : target
                                , parameters))
                        .SafeContinueWith(OnException)
                        .ContinueWith(t => IsBusy = false));

                }
            }

            public bool CanBeRemoved()
            {
                return !Subscriber.IsAlive || !DelegateSource.IsAlive;
            }
        }

        readonly Dictionary<Sender, List<Subscription>> _subscriptions
            = new Dictionary<Sender, List<Subscription>>();

        #region Send
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

        /// <inheritdoc cref="Send{TSender, TArgs}(TSender, string, TArgs)"/>
        public static void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
            => Instance.Send(sender, message, args);
        /// <inheritdoc/>
        void ISafeMessagingCenter.Send<TSender, TArgs>(TSender sender, string message, TArgs args)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));
            InnerSend(message, typeof(TSender), typeof(TArgs), sender, args);
        }

        /// <inheritdoc cref="Send{TSender}(TSender, string)"/>
        public static void Send<TSender>(TSender sender, string message) where TSender : class
            => Instance.Send(sender, message);
        /// <inheritdoc/>
        void ISafeMessagingCenter.Send<TSender>(TSender sender, string message)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));
            InnerSend(message, typeof(TSender), null, sender, null);
        }
        #endregion

        #region Unsubscribe
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

        /// <inheritdoc cref="Unsubscribe{TSender, TArgs}(object, string)"/>
        public static void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class
            => Instance.Unsubscribe<TSender, TArgs>(subscriber, message);
        /// <inheritdoc/>
        void ISafeMessagingCenter.Unsubscribe<TSender, TArgs>(object subscriber, string message)
        {
            InnerUnsubscribe(message, typeof(TSender), typeof(TArgs), subscriber);
        }

        /// <inheritdoc cref="Unsubscribe{TSender}(object, string)"/>
        public static void Unsubscribe<TSender>(object subscriber, string message) where TSender : class
            => Instance.Unsubscribe<TSender>(subscriber, message);
        /// <inheritdoc/>
        void ISafeMessagingCenter.Unsubscribe<TSender>(object subscriber, string message)
        {
            InnerUnsubscribe(message, typeof(TSender), null, subscriber);
        }

        /// <inheritdoc cref="UnsubscribeAny{TArgs}(object, string)"/>
        public static void UnsubscribeAny<TArgs>(object subscriber, string message)
            => Instance.UnsubscribeAny<TArgs>(subscriber, message);
        /// <inheritdoc/>
        void ISafeMessagingCenter.UnsubscribeAny<TArgs>(object subscriber, string message)
        {
            InnerUnsubscribe(message, null, typeof(TArgs), subscriber);
        }

        /// <inheritdoc cref="UnsubscribeAny(object, string)"/>
        public static void UnsubscribeAny(object subscriber, string message)
            => Instance.UnsubscribeAny(subscriber, message);
        /// <inheritdoc/>
        void ISafeMessagingCenter.UnsubscribeAny(object subscriber, string message)
        {
            InnerUnsubscribe(message, null, null, subscriber);
        }
        #endregion


        // This is a bit gross; it only exists to support the unit tests in PageTests
        // because the implementations of ActionSheet, Alert, and IsBusy are all very
        // tightly coupled to the MessagingCenter singleton 
        internal static void ClearSubscribers()
        {
            (Instance as SafeMessagingCenter)?._subscriptions.Clear();
        }

        #region Subscriptions
        #region Actions
        private static void ThrowIfNull(object subscriber, string message, Delegate callback)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
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

        /// <inheritdoc/>
        void ISafeMessagingCenter.Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, Action<Exception> onException, TSender source) where TSender : class
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, typeof(TSender), typeof(TArgs));
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException, Source = source };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, Action<Exception> onException, TSender source) where TSender : class
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, typeof(TSender), null);
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException, Source = source };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.SubscribeAny<TArgs>(object subscriber, string message, Action<object, TArgs> callback, Action<Exception> onException)
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, null, typeof(TArgs));
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.SubscribeAny(object subscriber, string message, Action<object> callback, Action<Exception> onException)
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, null, null);
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, bool isBlocking, Action<Exception> onException, TSender source) where TSender : class
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, typeof(TSender), typeof(TArgs));
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException, Source = source, IsBlocking = isBlocking };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, bool isBlocking, Action<Exception> onException, TSender source) where TSender : class
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, typeof(TSender), null);
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException, Source = source, IsBlocking = isBlocking };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.SubscribeAny<TArgs>(object subscriber, string message, Action<object, TArgs> callback, bool isBlocking, Action<Exception> onException)
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, null, typeof(TArgs));
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException, IsBlocking = isBlocking };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.SubscribeAny(object subscriber, string message, Action<object> callback, bool isBlocking, Action<Exception> onException)
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, null, null);
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException, IsBlocking = isBlocking };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, IViewModelBase viewModel, Action<Exception> onException, TSender source) where TSender : class
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, typeof(TSender), typeof(TArgs));
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException, Source = source, ViewModel = viewModel };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, IViewModelBase viewModel, Action<Exception> onException, TSender source) where TSender : class
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, typeof(TSender), null);
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException, Source = source, ViewModel = viewModel };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.SubscribeAny<TArgs>(object subscriber, string message, Action<object, TArgs> callback, IViewModelBase viewModel, Action<Exception> onException)
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, null, typeof(TArgs));
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException, ViewModel = viewModel };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.SubscribeAny(object subscriber, string message, Action<object> callback, IViewModelBase viewModel, Action<Exception> onException)
        {
            ThrowIfNull(subscriber, message, callback);

            var key = new Sender(message, null, null);
            var value = new Subscription(subscriber, callback.Target, callback.GetMethodInfo())
            { OnException = onException, ViewModel = viewModel };

            AddSubscription(key, value);
        }
        #endregion
        #region Functions
        /// <inheritdoc/>
        void ISafeMessagingCenter.Subscribe<TSender, TArgs>(object subscriber, string message, Func<TSender, TArgs, Task> asyncCallback, Action<Exception> onException, TSender source) where TSender : class
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, typeof(TSender), typeof(TArgs));
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, Source = source };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.Subscribe<TSender>(object subscriber, string message, Func<TSender, Task> asyncCallback, Action<Exception> onException, TSender source) where TSender : class
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, typeof(TSender), null);
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, Source = source };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.SubscribeAny<TArgs>(object subscriber, string message, Func<object, TArgs, Task> asyncCallback, Action<Exception> onException)
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, null, typeof(TArgs));
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.SubscribeAny(object subscriber, string message, Func<object, Task> asyncCallback, Action<Exception> onException)
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, null, null);
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.Subscribe<TSender, TArgs>(object subscriber, string message, Func<TSender, TArgs, Task> asyncCallback, bool isBlocking, Action<Exception> onException, TSender source) where TSender : class
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, typeof(TSender), typeof(TArgs));
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, Source = source, IsBlocking = isBlocking };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.Subscribe<TSender>(object subscriber, string message, Func<TSender, Task> asyncCallback, bool isBlocking, Action<Exception> onException, TSender source) where TSender : class
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, typeof(TSender), null);
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, Source = source, IsBlocking = isBlocking };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.SubscribeAny<TArgs>(object subscriber, string message, Func<object, TArgs, Task> asyncCallback, bool isBlocking, Action<Exception> onException)
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, null, typeof(TArgs));
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, IsBlocking = isBlocking };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.SubscribeAny(object subscriber, string message, Func<object, Task> asyncCallback, bool isBlocking, Action<Exception> onException)
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, null, null);
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, IsBlocking = isBlocking };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.Subscribe<TSender, TArgs>(object subscriber, string message, Func<TSender, TArgs, Task> asyncCallback, IViewModelBase viewModel, Action<Exception> onException, TSender source) where TSender : class
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, typeof(TSender), typeof(TArgs));
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, Source = source, ViewModel = viewModel };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.Subscribe<TSender>(object subscriber, string message, Func<TSender, Task> asyncCallback, IViewModelBase viewModel, Action<Exception> onException, TSender source) where TSender : class
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, typeof(TSender), null);
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, Source = source, ViewModel = viewModel };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.SubscribeAny<TArgs>(object subscriber, string message, Func<object, TArgs, Task> asyncCallback, IViewModelBase viewModel, Action<Exception> onException)
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, null, typeof(TArgs));
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, ViewModel = viewModel };

            AddSubscription(key, value);
        }

        /// <inheritdoc/>
        void ISafeMessagingCenter.SubscribeAny(object subscriber, string message, Func<object, Task> asyncCallback, IViewModelBase viewModel, Action<Exception> onException)
        {
            ThrowIfNull(subscriber, message, asyncCallback);

            var key = new Sender(message, null, null);
            var value = new Subscription(subscriber, asyncCallback.Target, asyncCallback.GetMethodInfo())
            { OnException = onException, ViewModel = viewModel };

            AddSubscription(key, value);
        }
        #endregion
        #endregion

        #region Static Subscriptions
        #region Actions
        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void Subscribe<TSender, TArgs>(
            object subscriber,
            string message,
            Action<TSender, TArgs> callback,
            Action<Exception> onException = null,
            TSender source = null) where TSender : class
            => Instance.Subscribe(subscriber, message, callback, onException, source);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void Subscribe<TSender>(
            object subscriber,
            string message,
            Action<TSender> callback,
            Action<Exception> onException = null,
            TSender source = null) where TSender : class
            => Instance.Subscribe(subscriber, message, callback, onException, source);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void SubscribeAny<TArgs>(
            object subscriber,
            string message,
            Action<object, TArgs> callback,
            Action<Exception> onException = null)
            => Instance.Subscribe(subscriber, message, callback, onException);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void SubscribeAny(
            object subscriber,
            string message,
            Action<object> callback,
            Action<Exception> onException = null)
            => Instance.Subscribe(subscriber, message, callback, onException);

        #region bool isBlocking
        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void Subscribe<TSender, TArgs>(
            object subscriber,
            string message, Action<TSender, TArgs> callback,
            bool isBlocking,
            Action<Exception> onException = null,
            TSender source = null) where TSender : class
            => Instance.Subscribe(subscriber, message, callback,isBlocking, onException, source);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void Subscribe<TSender>(
            object subscriber,
            string message,
            Action<TSender> callback,
            bool isBlocking,
            Action<Exception> onException = null,
            TSender source = null) where TSender : class
            => Instance.Subscribe(subscriber, message, callback, isBlocking, onException, source);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void SubscribeAny<TArgs>(
            object subscriber,
            string message,
            Action<object, TArgs> callback,
            bool isBlocking,
            Action<Exception> onException = null)
            => Instance.Subscribe(subscriber, message, callback, isBlocking, onException);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void SubscribeAny(
            object subscriber,
            string message,
            Action<object> callback,
            bool isBlocking,
            Action<Exception> onException = null)
            => Instance.Subscribe(subscriber, message, callback, isBlocking, onException);


        #endregion

        #region IViewModelBase viewModel
        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void Subscribe<TSender, TArgs>(
            object subscriber,
            string message,
            Action<TSender, TArgs> callback,
            IViewModelBase viewModel,
            Action<Exception> onException = null,
            TSender source = null) where TSender : class
            => Instance.Subscribe(subscriber, message, callback, viewModel, onException, source);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void Subscribe<TSender>(
            object subscriber,
            string message,
            Action<TSender> callback,
            IViewModelBase viewModel,
            Action<Exception> onException = null,
            TSender source = null) where TSender : class
            => Instance.Subscribe(subscriber, message, callback, viewModel, onException, source);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void SubscribeAny<TArgs>(
            object subscriber,
            string message,
            Action<object, TArgs> callback,
            IViewModelBase viewModel,
            Action<Exception> onException = null)
            => Instance.Subscribe(subscriber, message, callback, viewModel, onException);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void SubscribeAny(
            object subscriber,
            string message,
            Action<object> callback,
            IViewModelBase viewModel,
            Action<Exception> onException = null)
            => Instance.Subscribe(subscriber, message, callback, viewModel, onException);
        #endregion

        #endregion
        #region Functions
        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void Subscribe<TSender, TArgs>(
            object subscriber,
            string message,
            Func<TSender, TArgs, Task> asyncCallback,
            Action<Exception> onException = null,
            TSender source = null) where TSender : class
            => Instance.Subscribe(subscriber, message, asyncCallback, onException, source);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void Subscribe<TSender>(
            object subscriber,
            string message,
            Func<TSender, Task> asyncCallback,
            Action<Exception> onException = null,
            TSender source = null) where TSender : class
            => Instance.Subscribe(subscriber, message, asyncCallback, onException, source);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void SubscribeAny<TArgs>(
            object subscriber,
            string message,
            Func<object, TArgs, Task> asyncCallback,
            Action<Exception> onException = null)
            => Instance.Subscribe(subscriber, message, asyncCallback, onException);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void SubscribeAny(
            object subscriber,
            string message,
            Func<object, Task> asyncCallback,
            Action<Exception> onException = null)
            => Instance.Subscribe(subscriber, message, asyncCallback, onException);

        #region bool isBlocking
        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void Subscribe<TSender, TArgs>(
            object subscriber,
            string message,
            Func<TSender, TArgs, Task> asyncCallback,
            bool isBlocking,
            Action<Exception> onException = null,
            TSender source = null) where TSender : class
            => Instance.Subscribe(subscriber, message, asyncCallback, isBlocking, onException, source);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void Subscribe<TSender>(
            object subscriber,
            string message,
            Func<TSender, Task> asyncCallback,
            bool isBlocking,
            Action<Exception> onException = null,
            TSender source = null) where TSender : class
            => Instance.Subscribe(subscriber, message, asyncCallback, isBlocking, onException, source);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void SubscribeAny<TArgs>(
            object subscriber,
            string message,
            Func<object, TArgs, Task> asyncCallback,
            bool isBlocking,
            Action<Exception> onException = null)
            => Instance.Subscribe(subscriber, message, asyncCallback, isBlocking, onException);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void SubscribeAny(
            object subscriber,
            string message,
            Func<object, Task> asyncCallback,
            bool isBlocking,
            Action<Exception> onException = null)
            => Instance.Subscribe(subscriber, message, asyncCallback, isBlocking, onException);


        #endregion

        #region IViewModelBase viewModel
        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void Subscribe<TSender, TArgs>(
            object subscriber,
            string message,
            Func<TSender, TArgs, Task> asyncCallback,
            IViewModelBase viewModel,
            Action<Exception> onException = null,
            TSender source = null) where TSender : class
            => Instance.Subscribe(subscriber, message, asyncCallback, viewModel, onException, source);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void Subscribe<TSender>(
            object subscriber,
            string message,
            Func<TSender, Task> asyncCallback,
            IViewModelBase viewModel,
            Action<Exception> onException = null,
            TSender source = null) where TSender : class
            => Instance.Subscribe(subscriber, message, asyncCallback, viewModel, onException, source);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void SubscribeAny<TArgs>(
            object subscriber,
            string message,
            Func<object, TArgs, Task> asyncCallback,
            IViewModelBase viewModel,
            Action<Exception> onException = null)
            => Instance.Subscribe(subscriber, message, asyncCallback, viewModel, onException);

        /// <inheritdoc cref="Subscribe{TSender, TArgs}(object, string, Action{TSender, TArgs}, Action{Exception}, TSender)"/>
        public static void SubscribeAny(
            object subscriber,
            string message,
            Func<object, Task> asyncCallback,
            IViewModelBase viewModel,
            Action<Exception> onException = null)
            => Instance.Subscribe(subscriber, message, asyncCallback, viewModel, onException);

        #endregion

        #endregion

        #endregion

    }
}
