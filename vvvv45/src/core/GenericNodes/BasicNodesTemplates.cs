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
    public class REPLACEME_CLASSCastNode : Cons<REPLACEME_CLASS> {}
    
    #endregion SingleValue
    
    #region SpreadOps
	
	[PluginInfo(Name = "Cons",
                Category = "NODECATEGORY",
                Help = "Concatenates all input spreads to one output spread.",
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
	            Help = "Reverses the order of slices in a given spread.",
	            Tags = "invert, generic, spreadop",
	            Author = "woei"
	           )]
	public class REPLACEME_CLASSReverseBinNode : ReverseBin<REPLACEME_CLASS> {}

	[PluginInfo(Name = "Shift", 
	            Category = "NODECATEGORY", 
	            Version = "Bin", 
	            Help = "Shifts the slices in a spread upwards by the given phase.", 
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
	            Help = "Deletes the slice at the given index.",
	            Tags = "remove, generic, spreadop",
	            Author = "woei"
	           )]
	public class REPLACEME_CLASSDeleteSliceNode : DeleteSlice<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Select",
                Category = "NODECATEGORY",
                Help = "Select which slices and how many form the output spread.",
	            Tags = "resample, generic, spreadop"
	           )]
    public class REPLACEME_CLASSSelectNode : Select<REPLACEME_CLASS> {}
    
    [PluginInfo(Name = "Select", 
				Category = "NODECATEGORY",
				Version = "Bin",				
				Help = "Select the slices which form the new spread.", 
				Tags = "repeat, generic, spreadop",
				Author = "woei"
			)]
    public class REPLACEME_CLASSSelectBinNode : SelectBin<REPLACEME_CLASS> {}
    
	[PluginInfo(Name = "Unzip", 
	            Category = "NODECATEGORY",
	            Help = "Unzips a spread into multiple spreads.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class REPLACEME_CLASSUnzipNode : Unzip<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Unzip", 
	            Category = "NODECATEGORY",
	            Version = "Bin",
	            Help = "Unzips a spread into multiple spreads.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class REPLACEME_CLASSUnzipBinNode : Unzip<IInStream<REPLACEME_CLASS>> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "NODECATEGORY",
	            Help = "Zips spreads together.", 
	            Tags = "join, generic, spreadop"
	           )]
	public class REPLACEME_CLASSZipNode : Zip<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "NODECATEGORY",
				Version = "Bin",	            
	            Help = "Zips spreads together.", 
	            Tags = "join, generic, spreadop"
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
                Help = "Returns all pairs of successive slices. From an input ABCD returns AB, BC, CD.",
                Tags = "generic, spreadop"
                )]
    public class REPLACEME_CLASSPairwiseNode : Pairwise<REPLACEME_CLASS> {}

    [PluginInfo(Name = "SplitAt",
                Category = "NODECATEGORY",
                Help = "Splits a spread at the given index.",
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
	public class REPLACEME_CLASSBufferNode : BufferNode<REPLACEME_CLASS> {}
    
    [PluginInfo(Name = "Queue",
	            Category = "NODECATEGORY",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO (First In First Out) fashion.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class REPLACEME_CLASSQueueNode : QueueNode<REPLACEME_CLASS> {}
	
	[PluginInfo(Name = "RingBuffer",
	            Category = "NODECATEGORY",
	            Help = "Inserts the input at the ringbuffer position.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class REPLACEME_CLASSRingBufferNode : RingBufferNode<REPLACEME_CLASS> {}
    
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
	
	#endregion Collections
	
}

