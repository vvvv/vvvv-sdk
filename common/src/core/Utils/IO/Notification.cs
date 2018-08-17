using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Utils.IO
{
    public interface INotification
    {
        object Sender { get; }

        INotification WithSender(object sender);
    }

    public abstract class Notification : INotification
    {
        public readonly object Sender;

        public Notification()
        {
        }

        public Notification(object sender)
        {
            Sender = sender;
        }

        object INotification.Sender => Sender;

        public virtual INotification WithSender(object sender)
        {
            throw new NotImplementedException(nameof(WithSender));
        }
    }
}
