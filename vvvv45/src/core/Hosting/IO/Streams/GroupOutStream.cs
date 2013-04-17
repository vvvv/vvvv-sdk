using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO.Streams
{
    class GroupOutStream<T> : IInStream<IOutStream<T>>, IFlushable, IDisposable
    {
        private readonly MemoryIOStream<IOutStream<T>> FStreams = new MemoryIOStream<IOutStream<T>>(2);
        private readonly List<IIOContainer> FIOContainers = new List<IIOContainer>();
        private readonly IDiffSpread<int> FCountSpread;
        private readonly IIOFactory FFactory;
        private readonly OutputAttribute FOutputAttribute;
        private readonly int FOffsetCounter;
        private static int FInstanceCounter = 1;
        private bool FForceOnNextFlush;
        
        public GroupOutStream(IIOFactory factory, OutputAttribute attribute)
        {
            FFactory = factory;
            FOutputAttribute = attribute;
            //increment instance Counter and store it as pin offset
            FOffsetCounter = FInstanceCounter++;
            
            FCountSpread = factory.CreateIO<IDiffSpread<int>>(
                new ConfigAttribute(FOutputAttribute.Name + " Count")
                {
                    DefaultValue = 2,
                    MinValue = 2
                }
               );
            
            FCountSpread.Changed += HandleCountSpreadChanged;
            FCountSpread.Sync();
        }

        void HandleCountSpreadChanged(IDiffSpread<int> spread)
        {
            int oldCount = FIOContainers.Count;
            int newCount = Math.Max(spread[0], 0);
            
            for (int i = oldCount; i < newCount; i++)
            {
                var attribute = new OutputAttribute(string.Format("{0} {1}", FOutputAttribute.Name, i + 1))
                {
                    IsPinGroup = false,
                    Order = FOutputAttribute.Order + FOffsetCounter * 1000 + i,
                    BinOrder = FOutputAttribute.Order + FOffsetCounter * 1000 + i
                };
                var io = FFactory.CreateIOContainer(typeof(IOutStream<T>), attribute, false);
                FIOContainers.Add(io);
            }
            
            for (int i = oldCount - 1; i >= newCount; i--)
            {
                var io = FIOContainers[i];
                FIOContainers.Remove(io);
                io.Dispose();
            }
            
            FStreams.Length = FIOContainers.Count;
            using (var writer = FStreams.GetWriter())
            {
                foreach (var io in FIOContainers)
                {
                    writer.Write(io.RawIOObject as IOutStream<T>);
                }
            }

            FForceOnNextFlush = true;
        }
        
        public int Length
        {
            get
            {
                return FStreams.Length;
            }
        }
        
        public IStreamReader<IOutStream<T>> GetReader()
        {
            return FStreams.GetReader();
        }
        
        public bool Sync()
        {
            return IsChanged;
        }
        
        public bool IsChanged
        {
            get
            {
                return FStreams.IsChanged;
            }
        }

        public void Flush(bool force = false)
        {
            force |= FForceOnNextFlush;
            FForceOnNextFlush = false;
            FStreams.Flush(force);
            foreach (var stream in this)
            {
                stream.Flush(force);
            }
        }
        
        public object Clone()
        {
            throw new NotImplementedException();
        }
        
        public System.Collections.Generic.IEnumerator<IOutStream<T>> GetEnumerator()
        {
            return GetReader();
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public void Dispose()
        {
            FCountSpread.Changed -= HandleCountSpreadChanged;
            foreach (var container in FIOContainers)
            {
                container.Dispose();
            }
            FIOContainers.Clear();
        }
    }
}
