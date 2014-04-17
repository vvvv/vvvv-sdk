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
    public class NODECATEGORYCastNode : Cons<REPLACEME_CLASS> {}
    
    #endregion SingleValue
    
    #region SpreadOps
	
	[PluginInfo(Name = "Cons",
                Category = "NODECATEGORY",
                Help = "Concatenates all input spreads to one output spread",
                Tags = "generic, spreadop"
                )]
    public class NODECATEGORYConsNode : Cons<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "CAR", 
	            Category = "NODECATEGORY",
	            Version = "Bin", 
	            Help = "CAR with bin size", 
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class NODECATEGORYCARBinNode : CARBin<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "CDR", 
	            Category = "NODECATEGORY", 
	            Version = "Bin", 
	            Help = "CDR with bin size", 
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class NODECATEGORYCDRBinNode : CDRBin<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Reverse", 
	            Category = "NODECATEGORY", 
	            Version = "Bin",
	            Help = "Reverse with bin size",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class NODECATEGORYReverseBinNode : ReverseBin<REPLACEME_CLASS> {}

	[PluginInfo(Name = "Shift", 
	            Category = "NODECATEGORY", 
	            Version = "Bin", 
	            Help = "Shift with bin size", 
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class NODECATEGORYShiftBinNode : ShiftBin<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "SetSlice",
	            Category = "NODECATEGORY",
	            Version = "Bin",
	            Help = "Replace individual slices of the spread with the given input",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class NODECATEGORYSetSliceNode : SetSlice<REPLACEME_CLASS> {}
    
	[PluginInfo(Name = "DeleteSlice",
	            Category = "NODECATEGORY",
	            Help = "Delete the slice at the given index.",
	            Tags = "remove, filter, generic, spreadop",
	            Author = "woei"
	           )]
	public class NODECATEGORYDeleteSliceNode : DeleteSlice<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Select",
                Category = "NODECATEGORY",
                Help = "Select which slices and how many form the output spread",
	            Tags = "resample, generic, spreadop"
	           )]
    public class NODECATEGORYSelectNode : Select<REPLACEME_CLASS> {}
    
    [PluginInfo(Name = "Select", 
				Category = "NODECATEGORY",
				Version = "Bin",				
				Help = "select the slices which and how many form the new spread", 
				Tags = "select, repeat, binsize, generic, spreadop",
				Author = "woei"
			)]
    public class NODECATEGORYSelectBinNode : SelectBin<REPLACEME_CLASS> {}
    
	[PluginInfo(Name = "Unzip", 
	            Category = "NODECATEGORY",
	            Help = "Unzips a spread into multiple spreads", 
	            Tags = "spread, split, generic, spreadop"
	           )]
	public class NODECATEGORYUnzipNode : Unzip<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Unzip", 
	            Category = "NODECATEGORY",
	            Version = "Bin",
	            Help = "Unzips a spread into multiple spreads", 
	            Tags = "spread, split, generic, spreadop"
	           )]
	public class NODECATEGORYUnzipBinNode : Unzip<IInStream<REPLACEME_CLASS>> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "NODECATEGORY",
	            Help = "Zips spreads together", 
	            Tags = "spread, join, generic, spreadop"
	           )]
	public class NODECATEGORYZipNode : Zip<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "NODECATEGORY",
				Version = "Bin",	            
	            Help = "Zips spreads together", 
	            Tags = "spread, join, generic, spreadop"
	           )]
	public class NODECATEGORYZipBinNode : Zip<IInStream<REPLACEME_CLASS>> {}
	
    [PluginInfo(Name = "GetSpread",
                Category = "NODECATEGORY",
                Version = "Advanced",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class NODECATEGORYGetSpreadNode : GetSpreadAdvanced<REPLACEME_CLASS> {}
    
	[PluginInfo(Name = "SetSpread",
	            Category = "NODECATEGORY",
	            Help = "SetSpread with Bin Size",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class NODECATEGORYSetSpreadNode : SetSpread<REPLACEME_CLASS> {}
    
    [PluginInfo(Name = "Pairwise",
                Category = "NODECATEGORY",
                Help = "Returns all pairs of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = "generic, spreadop"
                )]
    public class NODECATEGORYPairwiseNode : Pairwise<REPLACEME_CLASS> {}
    
   	#endregion SpreadOps

    #region Collections
    
    [PluginInfo(Name = "Buffer",
	            Category = "NODECATEGORY",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class NODECATEGORYBufferNode : BufferNode<REPLACEME_CLASS> {}
    
    [PluginInfo(Name = "Queue",
	            Category = "NODECATEGORY",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class NODECATEGORYQueueNode : QueueNode<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "RingBuffer",
	            Category = "NODECATEGORY",
	            Help = "Inserts the input at the ringbuffer position and returns the whole Buffer as spread",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class NODECATEGORYRingBufferNode : RingBufferNode<REPLACEME_CLASS> {}
    
	[PluginInfo(Name = "Store", 
	            Category = "NODECATEGORY", 
	            Help = "Stores a spread and sets/removes/inserts slices", 
	            Tags = "set, remove, insert, generic, spreadop, collection",
	            Author = "woei", 
	            AutoEvaluate = true
	           )]
	public class NODECATEGORYStoreNode: Store<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Stack",
				Category = "NODECATEGORY",
				Tags = "generic, spreadop, collection",
				Author="vux"
				)]
	public class NODECATEGORYStackNode : StackNode<REPLACEME_CLASS> {}
	
	#endregion Collections
	
}

