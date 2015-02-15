using System;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using System.Xml;
using System.Xml.Linq;

namespace VVVV.Nodes
{

	
	#region SingleValue
	
	[PluginInfo(Name = "Cast",
                Category = "XElement",
                Help = "Casts any type to a type of this category, so be sure the input is of the required type",
                Tags = "convert, as, generic"
                )]
    public class XElementCastNode : Cons<XElement> {}
    
    #endregion SingleValue
    
    #region SpreadOps
	
	[PluginInfo(Name = "Cons",
                Category = "XElement",
                Help = "Concatenates all input spreads to one output spread.",
                Tags = "generic, spreadop"
                )]
    public class XElementConsNode : Cons<XElement> {}
	
	[PluginInfo(Name = "CAR", 
	            Category = "XElement",
	            Version = "Bin", 
	            Help = "Splits a given spread into first slice and remainder.", 
	            Tags = "split, generic, spreadop",
	            Author = "woei"
	           )]
	public class XElementCARBinNode : CARBin<XElement> {}
	
	[PluginInfo(Name = "CDR", 
	            Category = "XElement", 
	            Version = "Bin", 
	            Help = "Splits a given spread into remainder and last slice.", 
	            Tags = "split, generic, spreadop",
	            Author = "woei"
	           )]
	public class XElementCDRBinNode : CDRBin<XElement> {}
	
	[PluginInfo(Name = "Reverse", 
	            Category = "XElement", 
	            Version = "Bin",
	            Help = "Reverses the order of slices in a given spread.",
	            Tags = "invert, generic, spreadop",
	            Author = "woei"
	           )]
	public class XElementReverseBinNode : ReverseBin<XElement> {}

	[PluginInfo(Name = "Shift", 
	            Category = "XElement", 
	            Version = "Bin", 
	            Help = "Shifts the slices in a spread upwards by the given phase.", 
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class XElementShiftBinNode : ShiftBin<XElement> {}
	
	[PluginInfo(Name = "SetSlice",
	            Category = "XElement",
	            Version = "Bin",
	            Help = "Replaces individual slices of a spread with the given input",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class XElementSetSliceNode : SetSlice<XElement> {}
    
	[PluginInfo(Name = "DeleteSlice",
	            Category = "XElement",
	            Help = "Deletes the slice at the given index.",
	            Tags = "remove, generic, spreadop",
	            Author = "woei"
	           )]
	public class XElementDeleteSliceNode : DeleteSlice<XElement> {}
	
	[PluginInfo(Name = "Select",
                Category = "XElement",
                Help = "Select which slices and how many form the output spread.",
	            Tags = "resample, generic, spreadop"
	           )]
    public class XElementSelectNode : Select<XElement> {}
    
    [PluginInfo(Name = "Select", 
				Category = "XElement",
				Version = "Bin",				
				Help = "Select the slices which form the new spread.", 
				Tags = "repeat, generic, spreadop",
				Author = "woei"
			)]
    public class XElementSelectBinNode : SelectBin<XElement> {}
    
	[PluginInfo(Name = "Unzip", 
	            Category = "XElement",
	            Help = "Unzips a spread into multiple spreads.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class XElementUnzipNode : Unzip<XElement> {}
	
	[PluginInfo(Name = "Unzip", 
	            Category = "XElement",
	            Version = "Bin",
	            Help = "Unzips a spread into multiple spreads.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class XElementUnzipBinNode : Unzip<IInStream<XElement>> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "XElement",
	            Help = "Zips spreads together.", 
	            Tags = "join, generic, spreadop"
	           )]
	public class XElementZipNode : Zip<XElement> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "XElement",
				Version = "Bin",	            
	            Help = "Zips spreads together.", 
	            Tags = "join, generic, spreadop"
	           )]
	public class XElementZipBinNode : Zip<IInStream<XElement>> {}
	
    [PluginInfo(Name = "GetSpread",
                Category = "XElement",
                Version = "Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class XElementGetSpreadNode : GetSpreadAdvanced<XElement> {}
    
	[PluginInfo(Name = "SetSpread",
	            Category = "XElement",
	            Version = "Bin",
	            Help = "Allows to set sub-spreads into a given spread.",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class XElementSetSpreadNode : SetSpread<XElement> {}
    
    [PluginInfo(Name = "Pairwise",
                Category = "XElement",
                Help = "Returns all pairs of successive slices. From an input ABCD returns AB, BC, CD.",
                Tags = "generic, spreadop"
                )]
    public class XElementPairwiseNode : Pairwise<XElement> {}

    [PluginInfo(Name = "SplitAt",
                Category = "XElement",
                Help = "Splits a spread at the given index.",
                Tags = "generic, spreadop"
                )]
    public class XElementSplitAtNode : SplitAtNode<XElement> { }
    
   	#endregion SpreadOps

    #region Collections
    
    [PluginInfo(Name = "Buffer",
	            Category = "XElement",
	            Help = "Inserts the input at the given index.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class XElementBufferNode : BufferNode<XElement> {}
    
    [PluginInfo(Name = "Queue",
	            Category = "XElement",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO (First In First Out) fashion.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class XElementQueueNode : QueueNode<XElement> {}
	
	[PluginInfo(Name = "RingBuffer",
	            Category = "XElement",
	            Help = "Inserts the input at the ringbuffer position.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class XElementRingBufferNode : RingBufferNode<XElement> {}
    
	[PluginInfo(Name = "Store", 
	            Category = "XElement", 
	            Help = "Stores a spread and sets/removes/inserts slices.", 
	            Tags = "add, insert, remove, generic, spreadop, collection",
	            Author = "woei", 
	            AutoEvaluate = true
	           )]
	public class XElementStoreNode: Store<XElement> {}
	
	[PluginInfo(Name = "Stack",
				Category = "XElement",
				Help = "Stack data structure implementation using the LIFO (Last In First Out) paradigm.",
				Tags = "generic, spreadop, collection",
				Author="vux"
				)]
	public class XElementStackNode : StackNode<XElement> {}
	
	#endregion Collections
	
}

