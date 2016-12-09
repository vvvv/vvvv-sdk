using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace Microsoft.Threading
{
    /// <summary>Provides a SynchronizationContext that's single-threaded.</summary>
    /// <remarks>See http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx </remarks>
    public sealed class SingleThreadSynchronizationContext : SynchronizationContext
    {
        /// <summary>The queue of work items.</summary>
        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> m_queue =
            new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();
        /// <summary>The processing thread.</summary>
        private readonly Thread m_thread = Thread.CurrentThread;

        /// <summary>Dispatches an asynchronous message to the synchronization context.</summary>
        /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public override void Post(SendOrPostCallback d, object state)
        {
            if (d == null) throw new ArgumentNullException("d");
            m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (Thread.CurrentThread != m_thread)
            {
                using (var signal = new System.Threading.ManualResetEventSlim())
                {
                    Post(
                        (s) =>
                        {
                            d(s);
                            signal.Set();
                        }, 
                        state
                    );
                    signal.Wait();
                }
            }
            else
                d(state);
        }

        /// <summary>Runs a loop to process all queued work items.</summary>
        public void RunOnCurrentThread()
        {
            foreach (var workItem in m_queue.GetConsumingEnumerable())
                workItem.Key(workItem.Value);
        }

        public void RunIfOnMainThread()
        {
            if (Thread.CurrentThread == m_thread)
                RunOnCurrentThread();
        }

        public void TryRunOnCurrentThread()
        {
            KeyValuePair<SendOrPostCallback, object> workItem;
            while (m_queue.TryTake(out workItem))
                workItem.Key(workItem.Value);
        }

        public void TryRunIfOnMainThread()
        {
            if (Thread.CurrentThread == m_thread)
                TryRunOnCurrentThread();
        }

        /// <summary>Notifies the context that no more work will arrive.</summary>
        public void Complete() { m_queue.CompleteAdding(); }
    }
}
