using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Text;

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

        public void Update(TSource source)
        {
            if (source != FSource)
            {
                Unsubscribe();
                FSource = source;
                Subscribe();
            }
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
}
