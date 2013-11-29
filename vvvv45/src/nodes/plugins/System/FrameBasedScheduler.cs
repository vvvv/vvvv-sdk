using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text;

namespace VVVV.Nodes.Input
{
    public enum ScheduleMode
    {
        Enqueue,
        Discard
    }

    class FrameBasedScheduler : IScheduler
    {
        private readonly SchedulerQueue<uint> FQueue = new SchedulerQueue<uint>(4);

        public DateTimeOffset Now
        {
            get;
            private set;
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            throw new NotSupportedException();
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            throw new NotSupportedException();
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            var scheduledItem = new ScheduledItem<uint, TState>(this, state, action, CurrentFrame);
            FQueue.Enqueue(scheduledItem);
            return Disposable.Create(scheduledItem.Cancel);
        }

        public uint CurrentFrame { get; private set; }

        public void Run(ScheduleMode mode = ScheduleMode.Enqueue)
        {
            var currentFrame = CurrentFrame;
            switch (mode)
            {
                case ScheduleMode.Enqueue:
                    CurrentFrame++;
                    Run(currentFrame);
                    break;
                case ScheduleMode.Discard:
                    Run(currentFrame);
                    CurrentFrame++;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void Run(uint frame)
        {
            while (FQueue.Count > 0)
            {
                var nextWorkItem = FQueue.Peek();
                if (nextWorkItem.DueTime <= frame)
                {
                    var workItem = FQueue.Dequeue();
                    if (!workItem.IsCanceled)
                        workItem.Invoke();
                }
                else
                    break;
            }
        }
    }
}
