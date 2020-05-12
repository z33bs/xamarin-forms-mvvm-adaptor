﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// For unit testing and mocking of <see cref="SafeTaskExtensions"/>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ISafeTask
    {
        /// <summary>
        /// For unit testing and mocking of <see cref="SafeTaskExtensions"/>
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Task SafeContinueWith<TException>(
            Task task, Action<TException> onException,
            TaskScheduler scheduler = null)
            where TException : Exception;
    }
}