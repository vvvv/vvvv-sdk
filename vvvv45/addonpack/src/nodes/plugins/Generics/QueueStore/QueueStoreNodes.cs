using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
	[PluginInfo(
           Name = "QueueStore", 
           Category = "Spreads", 
           Help = "Stores a series of queues",
           Tags = "append, remove, spreadop, collection",
           Author = "motzi"
    )]
	public class SpreadsQueueStoreNode : QueueStore<double>
    {
        public SpreadsQueueStoreNode() : base(Copier<double>.Immutable) { }
    }
}
