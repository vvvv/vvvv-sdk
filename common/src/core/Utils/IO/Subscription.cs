using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Reactive;

namespace VVVV.Utils.IO
{
    public class Subscription<TSource, TNotification> : IDisposable
        where TSource : class
    {
        private readonly Action<TSource, TNotification> FOnNext;
        private readonly IScheduler FScheduler;
        private TSource FSource;
        private Func<TSource, IObservable<TNotification>> FSelector;
        private IDisposable FSubscription;

        public Subscription(Func<TSource, IObservable<TNotification>> selector, Action<TSource, TNotification> onNext, IScheduler scheduler = null)
        {
            FSelector = selector;
            FOnNext = onNext;
            FScheduler = scheduler;
        }

        public bool Update(TSource source)
        {
            if (source != FSource)
            {
                Unsubscribe();
                FSource = source;
                Subscribe();
                return true;
            }
            return false;
        }

        public void UpdateSelector(Func<TSource, IObservable<TNotification>> selector)
        {
            Unsubscribe();
            FSelector = selector;
            Subscribe();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (FSource != null)
            {
                var notifications = FSelector(FSource);
                if (FScheduler != null)
                    notifications = notifications.ObserveOn(FScheduler);
                FSubscription = notifications.Subscribe(n => FOnNext(FSource, n));
            }
        }

        private void Unsubscribe()
        {
            if (FSubscription != null)
            {
                FSubscription.Dispose();
                FSubscription = null;
            }
        }
    }

    public class Subscription2<TSource, TNotification> : IDisposable
        where TSource : class
    {
        private readonly Queue<TNotification> FNotifications = new Queue<TNotification>();
        private readonly Func<TSource, IObservable<TNotification>> FSelector;
        private TSource FSource;
        private IEnumerator<IList<TNotification>> FEnumerator;

        public Subscription2(Func<TSource, IObservable<TNotification>> selector)
        {
            FSelector = selector;
        }

        public IEnumerable<TNotification> ConsumeNext(TSource source)
        {
            UpdateSource(source);
            if (FNotifications.Count > 0)
                yield return FNotifications.Dequeue();
        }

        public IEnumerable<TNotification> ConsumeAll(TSource source)
        {
            UpdateSource(source);
            try
            {
                while (FNotifications.Count > 0)
                    yield return FNotifications.Dequeue();
            }
            finally
            {
                FNotifications.Clear();
            }
        }

        private void UpdateSource(TSource source)
        {
            if (FEnumerator != null && FEnumerator.MoveNext())
                foreach (var item in FEnumerator.Current)
                    FNotifications.Enqueue(item);
            if (source != FSource)
            {
                Unsubscribe();
                FSource = source;
                if (FSource != null)
                    FEnumerator = FSelector(FSource).Chunkify()
                        .GetEnumerator();
            }
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (FEnumerator != null)
            {
                FEnumerator.Dispose();
                FEnumerator = null;
            }
        }
    }
}
