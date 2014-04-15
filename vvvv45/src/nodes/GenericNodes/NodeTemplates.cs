using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Generic;

namespace VVVV.Nodes
{
	//1.) delete this class and do a 'replace all' of REPLACEME_CLASS with the name of your own type
	public class REPLACEME_CLASS {}
	
	//2.) do a 'replace all' for NODECATEGORY to set the category and the class name prefix for all nodes
	
	[PluginInfo(Name = "Buffer",
	            Category = "NODECATEGORY",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class NODECATEGORYBufferNode : BufferNode<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Cons",
                Category = "NODECATEGORY",
                Help = "Concatenates all input spreads to one output spread",
                Tags = ""
                )]
    public class NODECATEGORYConsNode : Cons<REPLACEME_CLASS> {}
    
	[PluginInfo(Name = "DeleteSlice",
	            Category = "NODECATEGORY",
	            Help = "Delete the slice at the given index.",
	            Tags = "remove, filter",
	            Author = "woei"
	           )]
	public class NODECATEGORYDeleteSliceNode : DeleteSlice<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Pairwise",
                Category = "NODECATEGORY",
                Help = "Returns all combinations of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class NODECATEGORYPairwiseNode : Pairwise<REPLACEME_CLASS> {}
    
    [PluginInfo(Name = "Queue",
	            Category = "NODECATEGORY",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class NODECATEGORYQueueNode : QueueNode<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "RingBuffer",
	            Category = "NODECATEGORY",
	            Help = "Inserts the input at the ringbuffer position and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class NODECATEGORYRingBufferNode : RingBufferNode<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Select",
                Category = "NODECATEGORY",
                Help = "Select which slices and how many form the output spread",
	            Tags = "resample"
	           )]
    public class NODECATEGORYSelectNode : Select<REPLACEME_CLASS> {}
    
	[PluginInfo(Name = "SetSlice",
	            Category = "NODECATEGORY",
	            Help = "Replace individual slices of the spread with the given input",
	            Tags = "",
	            Author = "woei"
	           )]
	public class NODECATEGORYSetSliceNode : SetSlice<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Unzip", 
	            Category = "NODECATEGORY", 
	            Help = "Unzips a spread into multiple spreads", 
	            Tags = "spread, split"
	           )]
	public class NODECATEGORYUnzipNode : Unzip<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "NODECATEGORY", 
	            Help = "Zips spreads together", 
	            Tags = "spread, join"
	           )]
	public class NODECATEGORYZipNode : Zip<REPLACEME_CLASS> {}
	
    [PluginInfo(Name = "GetSpread",
                Category = "NODECATEGORY",
                Version = "Advanced",
                Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
                Tags = "",
                Author = "woei")]
    public class NODECATEGORYGetSpreadNode : GetSpreadAdvanced<REPLACEME_CLASS> {}
    
	[PluginInfo(Name = "SetSpread",
	            Category = "NODECATEGORY",
	            Help = "SetSpread with Bin Size",
	            Tags = "",
	            Author = "woei"
	           )]
	public class NODECATEGORYSetSpreadNode : SetSpread<REPLACEME_CLASS> {}
    
    [PluginInfo(Name = "Select", 
				Category = "NODECATEGORY",
				Version = "Bin",				
				Help = "select the slices which and how many form the new spread", 
				Tags = "select, repeat, binsize",
				Author = "woei"
			)]
    public class NODECATEGORYSelectBinNode : SelectBin<REPLACEME_CLASS> {}

	[PluginInfo(Name = "Occurrence", 
	            Category = "NODECATEGORY",
	            Help = "counts the occurrence of equal slices",
	            Tags = "count, occurrence, spectral, spread",
	           	Author = "woei"
	           )]
	public class NODECATEGORYOccurrenceNode: Occurrence<REPLACEME_CLASS> {}
	

	[PluginInfo(Name = "Store", 
	            Category = "NODECATEGORY", 
	            Help = "Stores a spread and sets/removes/inserts slices", 
	            Tags = "spread, set, remove, insert",
	            Author = "woei", 
	            AutoEvaluate = true
	           )]
	public class NODECATEGORYStoreNode: Store<REPLACEME_CLASS> {}
	
	
	[PluginInfo(Name = "Stack",
				Category = "NODECATEGORY",
				Tags = "",
				Author="vux"
				)]
	public class NODECATEGORYStackNode : StackNode<REPLACEME_CLASS> {}
	
	
	[PluginInfo(Name = "CAR", 
	            Category = "NODECATEGORY", 
	            Version = "Bin", 
	            Help = "CAR with bin size", 
	            Author = "woei"
	           )]
	public class NODECATEGORYCARBinNode : CARBin<REPLACEME_CLASS> {}
	
	
	[PluginInfo(Name = "CDR", 
	            Category = "NODECATEGORY", 
	            Version = "Bin", 
	            Help = "CDR with bin size", 
	            Author = "woei"
	           )]
	public class NODECATEGORYCDRBinNode : CDRBin<REPLACEME_CLASS> {}
	
	
	[PluginInfo(Name = "Reverse", 
	            Category = "NODECATEGORY", 
	            Version = "Bin", 
	            Help = "Reverse with bin size",
	            Author = "woei"
	           )]
	public class NODECATEGORYReverseBinNode : ReverseBin<REPLACEME_CLASS> {}
	

	[PluginInfo(Name = "Shift", 
	            Category = "NODECATEGORY", 
	            Version = "Bin", 
	            Help = "Shift with bin size", 
	            Author = "woei"
	           )]
	public class NODECATEGORYShiftBinNode : ShiftBin<REPLACEME_CLASS> {}
}

