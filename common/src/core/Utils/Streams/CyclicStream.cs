using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Utils.Streams
{
    public class CyclicStream<T> : IInStream<T>
    {
        private readonly IInStream<T> source;

        public CyclicStream(IInStream<T> source)
        {
            this.source = source;
        }

        public CyclicStreamReader<T> GetReader()
        {
            return new CyclicStreamReader<T>(source);
        }

        IStreamReader<T> IInStream<T>.GetReader()
        {
            return GetReader();
        }

        public int Length
        {
            get { return source.Length; }
        }

        public object Clone()
        {
            return new CyclicStream<T>(source);
        }

        public bool Sync()
        {
            return source.Sync();
        }

        public bool IsChanged
        {
            get { return source.IsChanged; }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            return GetReader();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
