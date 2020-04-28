using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.Tests
{
    /// <summary>
    /// A TaskScheduler to be used for unit testing. 
	/// The class allows to execute new scheduled tasks 
	/// on the same thread as a unit test.
    /// </summary>
    public class DeterministicTaskScheduler : TaskScheduler
    {
        private readonly List<Task> scheduledTasks = new List<Task>();

        #region TaskScheduler overrides
        public override int MaximumConcurrencyLevel => 1;

        protected override IEnumerable<Task> GetScheduledTasks() => scheduledTasks;
        protected override void QueueTask(Task task) => scheduledTasks.Add(task);
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            scheduledTasks.Add(task);
            return false;
        }
        #endregion

        readonly bool shouldThrowExceptions;

        public DeterministicTaskScheduler(bool shouldThrowExceptions = true) : base()
        {
            this.shouldThrowExceptions = shouldThrowExceptions;
        }

        public List<Exception> Exceptions { get; } = new List<Exception>();

        /// <summary>
		/// Runs only the currently scheduled tasks.
		/// </summary>
		public void RunPendingTasks()
        {
            foreach (var task in scheduledTasks.ToArray())
            {
                TryExecuteTask(task);
                try
                {
                    task.GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Exceptions.Add(ex);
                    if(shouldThrowExceptions)
                        throw ex;
                }
                finally
                {
                    scheduledTasks.Remove(task);
                }
            }
        }

        /// <summary>
        /// Rununs all tasks until no more scheduled tasks are left.
        /// If a pending task schedules an additional task it will also be executed.
        /// </summary>
        public void RunTasksUntilIdle()
        {
            while (scheduledTasks.Any())
            {
                RunPendingTasks();
            }
        }
    }
}
