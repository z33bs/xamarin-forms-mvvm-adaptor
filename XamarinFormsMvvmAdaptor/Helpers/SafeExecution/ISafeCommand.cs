﻿using System.Threading.Tasks;
using System.Windows.Input;

namespace XamarinFormsMvvmAdaptor.Helpers
{
    /// <summary>
    /// Safe implementation of the <see cref="ICommand"/> interface
    /// </summary>
    public interface ISafeCommand : ICommand
    {
        /// <summary>
        /// Raises the CanExecuteChanged event.
        /// </summary>
        void RaiseCanExecuteChanged();

        /// <summary>
        /// Useful for Unit Tests. Executes the Action/Function delegate
        /// only without "Safe" features (onException, isBlocking, etc).
        /// </summary>
        Task RawExecuteAsync(object parameter);
    }
}