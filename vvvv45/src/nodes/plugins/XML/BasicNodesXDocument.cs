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
                Version = "Document",
                Help = "Casts any type to a type of this category, so be sure the input is of the required type",
                Tags = "convert, as, generic"
                )]
    public class XDocumentCastNode : Cons<XDocument> {}
    
    #endregion SingleValue
    
    #region SpreadOps
	
	[PluginInfo(Name = "Cons",
                Category = "XElement",
                Version = "Document",
                Help = "Concatenates all input spreads to one output spread.",
                Tags = "generic, spreadop"
                )]
    public class XDocumentConsNode : Cons<XDocument> {}
	
	[PluginInfo(Name = "CAR", 
	            Category = "XElement",
	            Version = "Document Bin", 
	            Help = "Splits a given spread into first slice and remainder.", 
	            Tags = "split, generic, spreadop",
	            Author = "woei"
	           )]
	public class XDocumentCARBinNode : CARBin<XDocument> {}
	
	[PluginInfo(Name = "CDR", 
	            Category = "XElement", 
	            Version = "Document Bin", 
	            Help = "Splits a given spread into remainder and last slice.", 
	            Tags = "split, generic, spreadop",
	            Author = "woei"
	           )]
	public class XDocumentCDRBinNode : CDRBin<XDocument> {}
	
	[PluginInfo(Name = "Reverse", 
	            Category = "XElement", 
	            Version = "Document Bin",
	            Help = "Reverses the order of slices in a given spread.",
	            Tags = "invert, generic, spreadop",
	            Author = "woei"
	           )]
	public class XDocumentReverseBinNode : ReverseBin<XDocument> {}

	[PluginInfo(Name = "Shift", 
	            Category = "XElement", 
	            Version = "Document Bin", 
	            Help = "Shifts the slices in a spread upwards by the given phase.", 
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class XDocumentShiftBinNode : ShiftBin<XDocument> {}
	
	[PluginInfo(Name = "SetSlice",
	            Category = "XElement",
	            Version = "Document Bin",
	            Help = "Replaces individual slices of a spread with the given input",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class XDocumentSetSliceNode : SetSlice<XDocument> {}
    
	[PluginInfo(Name = "DeleteSlice",
	            Category = "XElement",
                Version = "Document",
	            Help = "Deletes the slice at the given index.",
	            Tags = "remove, generic, spreadop",
	            Author = "woei"
	           )]
	public class XDocumentDeleteSliceNode : DeleteSlice<XDocument> {}
	
	[PluginInfo(Name = "Select",
                Category = "XElement",
                Version = "Document",
                Help = "Select which slices and how many form the output spread.",
	            Tags = "resample, generic, spreadop"
	           )]
    public class XDocumentSelectNode : Select<XDocument> {}
    
    [PluginInfo(Name = "Select",
                Category = "XElement",
				Version = "Document Bin",				
				Help = "Select the slices which form the new spread.", 
				Tags = "repeat, generic, spreadop",
				Author = "woei"
			)]
    public class XDocumentSelectBinNode : SelectBin<XDocument> {}
    
	[PluginInfo(Name = "Unzip",
                Category = "XElement",
                Version = "Document",
	            Help = "Unzips a spread into multiple spreads.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class XDocumentUnzipNode : Unzip<XDocument> {}
	
	[PluginInfo(Name = "Unzip", 
	            Category = "XElement",
	            Version = "Document Bin",
	            Help = "Unzips a spread into multiple spreads.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class XDocumentUnzipBinNode : Unzip<IInStream<XDocument>> {}
	
	[PluginInfo(Name = "Zip",
                Category = "XElement",
                Version = "Document",
	            Help = "Zips spreads together.", 
	            Tags = "join, generic, spreadop"
	           )]
	public class XDocumentZipNode : Zip<XDocument> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "XElement",
				Version = "Document Bin",	            
	            Help = "Zips spreads together.", 
	            Tags = "join, generic, spreadop"
	           )]
	public class XDocumentZipBinNode : Zip<IInStream<XDocument>> {}
	
    [PluginInfo(Name = "GetSpread",
                Category = "XElement",
                Version = "Document Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class XDocumentGetSpreadNode : GetSpreadAdvanced<XDocument> {}
    
	[PluginInfo(Name = "SetSpread",
	            Category = "XElement",
	            Version = "Document Bin",
	            Help = "Allows to set sub-spreads into a given spread.",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class XDocumentSetSpreadNode : SetSpread<XDocument> {}
    
    [PluginInfo(Name = "Pairwise",
                Category = "XElement",
                Version = "Document",
                Help = "Returns all pairs of successive slices. From an input ABCD returns AB, BC, CD.",
                Tags = "generic, spreadop"
                )]
    public class XDocumentPairwiseNode : Pairwise<XDocument> {}

    [PluginInfo(Name = "SplitAt",
                Category = "XElement",
                Version = "Document",
                Help = "Splits a spread at the given index.",
                Tags = "generic, spreadop"
                )]
    public class XDocumentSplitAtNode : SplitAtNode<XDocument> { }
    
   	#endregion SpreadOps

    #region Collections
    
    [PluginInfo(Name = "Buffer",
                Category = "XElement",
                Version = "Document",
	            Help = "Inserts the input at the given index.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class XDocumentBufferNode : BufferNode<XDocument> {}
    
    [PluginInfo(Name = "Queue",
                Category = "XElement",
                Version = "Document",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO (First In First Out) fashion.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class XDocumentQueueNode : QueueNode<XDocument> {}
	
	[PluginInfo(Name = "RingBuffer",
                Category = "XElement",
                Version = "Document",
	            Help = "Inserts the input at the ringbuffer position.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class XDocumentRingBufferNode : RingBufferNode<XDocument> {}
    
	[PluginInfo(Name = "Store",
                Category = "XElement",
                Version = "Document",
	            Help = "Stores a spread and sets/removes/inserts slices.", 
	            Tags = "add, insert, remove, generic, spreadop, collection",
	            Author = "woei", 
	            AutoEvaluate = true
	           )]
	public class XDocumentStoreNode: Store<XDocument> {}
	
	[PluginInfo(Name = "Stack",
                Category = "XElement",
                Version = "Document",
				Help = "Stack data structure implementation using the LIFO (Last In First Out) paradigm.",
				Tags = "generic, spreadop, collection",
				Author="vux"
				)]
	public class XDocumentStackNode : StackNode<XDocument> {}
	
	#endregion Collections
	
}

