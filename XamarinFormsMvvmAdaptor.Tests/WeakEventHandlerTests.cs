using System;
using Xunit;
using System.ComponentModel;
using XamarinFormsMvvmAdaptor.Helpers;

namespace XamarinFormsMvvmAdaptor.Tests
{
    public class WeakEventHandlerTests
    {
        #region Example

        public class Alarm
        {
            public event PropertyChangedEventHandler Beeped;

            public void Beep()
            {
                var handler = Beeped;
                if (handler != null) handler(this, new PropertyChangedEventArgs("Beep!"));
            }
        }

        public class Sleepy
        {
            private readonly Alarm _alarm;
            private int _snoozeCount;

            public Sleepy(Alarm alarm)
            {
                _alarm = alarm;
                _alarm.Beeped += new WeakEventHandler<PropertyChangedEventArgs>(Alarm_Beeped).Handler;
            }

            private void Alarm_Beeped(object sender, PropertyChangedEventArgs e)
            {
                _snoozeCount++;
            }

            public int SnoozeCount
            {
                get { return _snoozeCount; }
            }
        }

        #endregion

        [Fact]
        public void ShouldHandleEventWhenBothReferencesAreAlive()
        {
            var alarm = new Alarm();
            var sleepy = new Sleepy(alarm);
            alarm.Beep();
            alarm.Beep();

            Assert.Equal(2, sleepy.SnoozeCount);
        }

        [Fact]
        public void ShouldAllowSubscriberReferenceToBeCollected()
        {
            var alarm = new Alarm();
            var sleepyReference = null as WeakReference;
            new Action(() =>
            {
                // Run this in a delegate to that the local variable gets garbage collected
                var sleepy = new Sleepy(alarm);
                alarm.Beep();
                alarm.Beep();
                Assert.Equal(2, sleepy.SnoozeCount);
                sleepyReference = new WeakReference(sleepy);
            })();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.Null(sleepyReference.Target);
        }

        [Fact]
        public void SubscriberShouldNotBeUnsubscribedUntilCollection()
        {
            var alarm = new Alarm();
            var sleepy = new Sleepy(alarm);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            alarm.Beep();
            alarm.Beep();
            Assert.Equal(2, sleepy.SnoozeCount);
        }
    }
}

