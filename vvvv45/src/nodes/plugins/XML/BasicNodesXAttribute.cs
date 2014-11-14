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
                Version = "Attribute",
                Help = "Casts any type to a type of this category, so be sure the input is of the required type",
                Tags = "convert, as, generic"
                )]
    public class XAttributeCastNode : Cons<XAttribute> {}
    
    #endregion SingleValue
    
    #region SpreadOps
	
	[PluginInfo(Name = "Cons",
                Category = "XElement",
                Version = "Attribute",
                Help = "Concatenates all input spreads to one output spread.",
                Tags = "generic, spreadop"
                )]
    public class XAttributeConsNode : Cons<XAttribute> {}
	
	[PluginInfo(Name = "CAR", 
	            Category = "XElement",
	            Version = "Attribute Bin", 
	            Help = "Splits a given spread into first slice and remainder.", 
	            Tags = "split, generic, spreadop",
	            Author = "woei"
	           )]
	public class XAttributeCARBinNode : CARBin<XAttribute> {}
	
	[PluginInfo(Name = "CDR", 
	            Category = "XElement", 
	            Version = "Attribute Bin", 
	            Help = "Splits a given spread into remainder and last slice.", 
	            Tags = "split, generic, spreadop",
	            Author = "woei"
	           )]
	public class XAttributeCDRBinNode : CDRBin<XAttribute> {}
	
	[PluginInfo(Name = "Reverse", 
	            Category = "XElement", 
	            Version = "Attribute Bin",
	            Help = "Reverses the order of slices in a given spread.",
	            Tags = "invert, generic, spreadop",
	            Author = "woei"
	           )]
	public class XAttributeReverseBinNode : ReverseBin<XAttribute> {}

	[PluginInfo(Name = "Shift", 
	            Category = "XElement", 
	            Version = "Attribute Bin", 
	            Help = "Shifts the slices in a spread upwards by the given phase.", 
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class XAttributeShiftBinNode : ShiftBin<XAttribute> {}
	
	[PluginInfo(Name = "SetSlice",
	            Category = "XElement",
	            Version = "Attribute Bin",
	            Help = "Replaces individual slices of a spread with the given input",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class XAttributeSetSliceNode : SetSlice<XAttribute> {}
    
	[PluginInfo(Name = "DeleteSlice",
	            Category = "XElement",
                Version = "Attribute",
	            Help = "Deletes the slice at the given index.",
	            Tags = "remove, generic, spreadop",
	            Author = "woei"
	           )]
	public class XAttributeDeleteSliceNode : DeleteSlice<XAttribute> {}
	
	[PluginInfo(Name = "Select",
                Category = "XElement",
                Version = "Attribute",
                Help = "Select which slices and how many form the output spread.",
	            Tags = "resample, generic, spreadop"
	           )]
    public class XAttributeSelectNode : Select<XAttribute> {}
    
    [PluginInfo(Name = "Select", 
				Category = "XElement",
				Version = "Attribute Bin",				
				Help = "Select the slices which form the new spread.", 
				Tags = "repeat, generic, spreadop",
				Author = "woei"
			)]
    public class XAttributeSelectBinNode : SelectBin<XAttribute> {}
    
	[PluginInfo(Name = "Unzip", 
	            Category = "XElement",
                Version = "Attribute",
	            Help = "Unzips a spread into multiple spreads.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class XAttributeUnzipNode : Unzip<XAttribute> {}
	
	[PluginInfo(Name = "Unzip", 
	            Category = "XElement",
	            Version = "Attribute Bin",
	            Help = "Unzips a spread into multiple spreads.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class XAttributeUnzipBinNode : Unzip<IInStream<XAttribute>> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "XElement",
                Version = "Attribute",
	            Help = "Zips spreads together.", 
	            Tags = "join, generic, spreadop"
	           )]
	public class XAttributeZipNode : Zip<XAttribute> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "XElement",
				Version = "Attribute Bin",	            
	            Help = "Zips spreads together.", 
	            Tags = "join, generic, spreadop"
	           )]
	public class XAttributeZipBinNode : Zip<IInStream<XAttribute>> {}
	
    [PluginInfo(Name = "GetSpread",
                Category = "XElement",
                Version = "Attribute Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class XAttributeGetSpreadNode : GetSpreadAdvanced<XAttribute> {}
    
	[PluginInfo(Name = "SetSpread",
	            Category = "XElement",
	            Version = "Attribute Bin",
	            Help = "Allows to set sub-spreads into a given spread.",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class XAttributeSetSpreadNode : SetSpread<XAttribute> {}
    
    [PluginInfo(Name = "Pairwise",
                Category = "XElement",
                Version = "Attribute",
                Help = "Returns all pairs of successive slices. From an input ABCD returns AB, BC, CD.",
                Tags = "generic, spreadop"
                )]
    public class XAttributePairwiseNode : Pairwise<XAttribute> {}

    [PluginInfo(Name = "SplitAt",
                Category = "XElement",
                Version = "Attribute",
                Help = "Splits a spread at the given index.",
                Tags = "generic, spreadop"
                )]
    public class XAttributeSplitAtNode : SplitAtNode<XAttribute> { }
    
   	#endregion SpreadOps

    #region Collections
    
    [PluginInfo(Name = "Buffer",
	            Category = "XElement",
                Version = "Attribute",
	            Help = "Inserts the input at the given index.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class XAttributeBufferNode : BufferNode<XAttribute> {}
    
    [PluginInfo(Name = "Queue",
	            Category = "XElement",
                Version = "Attribute",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO (First In First Out) fashion.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class XAttributeQueueNode : QueueNode<XAttribute> {}
	
	[PluginInfo(Name = "RingBuffer",
	            Category = "XElement",
                Version = "Attribute",
	            Help = "Inserts the input at the ringbuffer position.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class XAttributeRingBufferNode : RingBufferNode<XAttribute> {}
    
	[PluginInfo(Name = "Store", 
	            Category = "XElement",
                Version = "Attribute",
	            Help = "Stores a spread and sets/removes/inserts slices.", 
	            Tags = "add, insert, remove, generic, spreadop, collection",
	            Author = "woei", 
	            AutoEvaluate = true
	           )]
	public class XAttributeStoreNode: Store<XAttribute> {}
	
	[PluginInfo(Name = "Stack",
				Category = "XElement",
                Version = "Attribute",
				Help = "Stack data structure implementation using the LIFO (Last In First Out) paradigm.",
				Tags = "generic, spreadop, collection",
				Author="vux"
				)]
	public class XAttributeStackNode : StackNode<XAttribute> {}
	
	#endregion Collections
	
}

