using System;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Nodes
{

	//1.) do a 'replace all' of REPLACEME_CLASS with the name of your own type
	
	//2.) do a 'replace all' for NODECATEGORY to set the category and the class name prefix for all nodes
	
	#region SingleValue
	
	[PluginInfo(Name = "Cast",
                Category = "NODECATEGORY",
                Help = "Casts any type to a type of this category, so be sure the input is of the required type",
                Tags = "convert, as, generic"
                )]
    public class REPLACEME_CLASSCastNode : Cast<REPLACEME_CLASS> {}
    
    #endregion SingleValue
    
    #region SpreadOps
	
	[PluginInfo(Name = "Cons",
                Category = "NODECATEGORY",
                Help = "Concatenates all Input spreads.",
                Tags = "generic, spreadop"
                )]
    public class REPLACEME_CLASSConsNode : Cons<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "CAR", 
	            Category = "NODECATEGORY",
	            Version = "Bin", 
	            Help = "Splits a given spread into first slice and remainder.", 
	            Tags = "split, generic, spreadop",
	            Author = "woei"
	           )]
	public class REPLACEME_CLASSCARBinNode : CARBin<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "CDR", 
	            Category = "NODECATEGORY", 
	            Version = "Bin", 
	            Help = "Splits a given spread into remainder and last slice.", 
	            Tags = "split, generic, spreadop",
	            Author = "woei"
	           )]
	public class REPLACEME_CLASSCDRBinNode : CDRBin<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Reverse", 
	            Category = "NODECATEGORY", 
	            Version = "Bin",
	            Help = "Reverses the order of the slices in the Spread.",
	            Tags = "invert, generic, spreadop",
	            Author = "woei"
	           )]
	public class REPLACEME_CLASSReverseBinNode : ReverseBin<REPLACEME_CLASS> {}

	[PluginInfo(Name = "Shift", 
	            Category = "NODECATEGORY", 
	            Version = "Bin", 
	            Help = "Shifts the slices in the Spread by the given Phase.", 
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class REPLACEME_CLASSShiftBinNode : ShiftBin<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "SetSlice",
	            Category = "NODECATEGORY",
	            Version = "Bin",
	            Help = "Replaces individual slices of a spread with the given input",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class REPLACEME_CLASSSetSliceNode : SetSlice<REPLACEME_CLASS> {}
    
	[PluginInfo(Name = "DeleteSlice",
	            Category = "NODECATEGORY",
	            Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
	            Tags = "remove, generic, spreadop",
	            Author = "woei"
	           )]
	public class REPLACEME_CLASSDeleteSliceNode : DeleteSlice<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Select",
                Category = "NODECATEGORY",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
	            Tags = "repeat, resample, duplicate, spreadop"
	           )]
    public class REPLACEME_CLASSSelectNode : Select<REPLACEME_CLASS> {}
    
    [PluginInfo(Name = "Select", 
				Category = "NODECATEGORY",
				Version = "Bin",				
				Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ", 
				Tags = "repeat, resample, duplicate, spreadop",
				Author = "woei"
			)]
    public class REPLACEME_CLASSSelectBinNode : SelectBin<REPLACEME_CLASS> {}
    
	[PluginInfo(Name = "Unzip", 
	            Category = "NODECATEGORY",
	            Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class REPLACEME_CLASSUnzipNode : Unzip<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Unzip", 
	            Category = "NODECATEGORY",
	            Version = "Bin",
	            Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it. With Bin Size.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class REPLACEME_CLASSUnzipBinNode : Unzip<IInStream<REPLACEME_CLASS>> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "NODECATEGORY",
	            Help = "Interleaves all Input spreads.", 
	            Tags = "interleave, join, generic, spreadop"
	           )]
	public class REPLACEME_CLASSZipNode : Zip<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "NODECATEGORY",
				Version = "Bin",	            
	            Help = "Interleaves all Input spreads.", 
	            Tags = "interleave, join, generic, spreadop"
	           )]
	public class REPLACEME_CLASSZipBinNode : Zip<IInStream<REPLACEME_CLASS>> {}
	
    [PluginInfo(Name = "GetSpread",
                Category = "NODECATEGORY",
                Version = "Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class REPLACEME_CLASSGetSpreadNode : GetSpreadAdvanced<REPLACEME_CLASS> {}
    
	[PluginInfo(Name = "SetSpread",
	            Category = "NODECATEGORY",
	            Version = "Bin",
	            Help = "Allows to set sub-spreads into a given spread.",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class REPLACEME_CLASSSetSpreadNode : SetSpread<REPLACEME_CLASS> {}
    
    [PluginInfo(Name = "Pairwise",
                Category = "NODECATEGORY",
                Help = "Returns all combinations of pairs of successive slices. From an input ABCD returns AB, BC, CD.",
                Tags = "generic, spreadop"
                )]
    public class REPLACEME_CLASSPairwiseNode : Pairwise<REPLACEME_CLASS> {}

    [PluginInfo(Name = "SplitAt",
                Category = "NODECATEGORY",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class REPLACEME_CLASSSplitAtNode : SplitAtNode<REPLACEME_CLASS> { }
    
   	#endregion SpreadOps

    #region Collections
    
    [PluginInfo(Name = "Buffer",
	            Category = "NODECATEGORY",
	            Help = "Inserts the input at the given index.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class REPLACEME_CLASSBufferNode : BufferNode<REPLACEME_CLASS>
    {
        public REPLACEME_CLASSBufferNode() : base(REPLACEME_CLASSCopier.Default) { }
    }
    
    [PluginInfo(Name = "Queue",
	            Category = "NODECATEGORY",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO (First In First Out) fashion.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class REPLACEME_CLASSQueueNode : QueueNode<REPLACEME_CLASS>
    {
        public REPLACEME_CLASSQueueNode() : base(REPLACEME_CLASSCopier.Default) { }
    }
	
	[PluginInfo(Name = "RingBuffer",
	            Category = "NODECATEGORY",
	            Help = "Inserts the input at the ringbuffer position.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class REPLACEME_CLASSRingBufferNode : RingBufferNode<REPLACEME_CLASS>
    {
        public REPLACEME_CLASSRingBufferNode() : base(REPLACEME_CLASSCopier.Default) { }
    }
    
	[PluginInfo(Name = "Store", 
	            Category = "NODECATEGORY", 
	            Help = "Stores a spread and sets/removes/inserts slices.", 
	            Tags = "add, insert, remove, generic, spreadop, collection",
	            Author = "woei", 
	            AutoEvaluate = true
	           )]
	public class REPLACEME_CLASSStoreNode: Store<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Stack",
				Category = "NODECATEGORY",
				Help = "Stack data structure implementation using the LIFO (Last In First Out) paradigm.",
				Tags = "generic, spreadop, collection",
				Author="vux"
				)]
	public class REPLACEME_CLASSStackNode : StackNode<REPLACEME_CLASS> {}

    [PluginInfo(
           Name = "QueueStore",
           Category = "NODECATEGORY",
           Help = "Stores a series of queues",
           Tags = "append, remove, generic, spreadop, collection",
           Author = "motzi"
    )]
    public class REPLACEME_CLASSQueueStoreNodes : QueueStore<REPLACEME_CLASS>
    {
        REPLACEME_CLASSQueueStoreNodes() : base(REPLACEME_CLASSCopier.Default) { }
    }
	
	#endregion Collections
	
}

