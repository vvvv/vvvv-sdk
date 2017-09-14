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
                Help = "Concatenates all Input spreads.",
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
	            Help = "Reverses the order of the slices in the Spread. With Bin Size.",
	            Tags = "invert, generic, spreadop",
	            Author = "woei"
	           )]
	public class XDocumentReverseBinNode : ReverseBin<XDocument> {}

	[PluginInfo(Name = "Shift", 
	            Category = "XElement", 
	            Version = "Document Bin", 
	            Help = "Shifts the slices in the Spread by the given Phase. With Bin Size.", 
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class XDocumentShiftBinNode : ShiftBin<XDocument> {}
	
	[PluginInfo(Name = "SetSlice",
	            Category = "XElement",
	            Version = "Document Bin",
	            Help = "Replaces slices in the Spread that are addressed by the Index pin, with the given Input.",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class XDocumentSetSliceNode : SetSlice<XDocument> {}
    
	[PluginInfo(Name = "DeleteSlice",
	            Category = "XElement",
                Version = "Document",
	            Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
	            Tags = "remove, generic, spreadop",
	            Author = "woei"
	           )]
	public class XDocumentDeleteSliceNode : DeleteSlice<XDocument> {}
	
	[PluginInfo(Name = "Select",
                Category = "XElement",
                Version = "Document",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted.",
	            Tags = "repeat, resample, duplicate, spreadop"
	           )]
    public class XDocumentSelectNode : Select<XDocument> {}
    
    [PluginInfo(Name = "Select",
                Category = "XElement",
				Version = "Document Bin",				
				Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. With Bin Size.", 
				Tags = "repeat, resample, duplicate, spreadop",
				Author = "woei"
			)]
    public class XDocumentSelectBinNode : SelectBin<XDocument> {}
    
	[PluginInfo(Name = "Unzip",
                Category = "XElement",
                Version = "Document",
	            Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class XDocumentUnzipNode : Unzip<XDocument> {}
	
	[PluginInfo(Name = "Unzip", 
	            Category = "XElement",
	            Version = "Document Bin",
	            Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it. With Bin Size.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class XDocumentUnzipBinNode : Unzip<IInStream<XDocument>> {}
	
	[PluginInfo(Name = "Zip",
                Category = "XElement",
                Version = "Document",
	            Help = "Interleaves all Input spreads.", 
	            Tags = "interleave, join, generic, spreadop"
	           )]
	public class XDocumentZipNode : Zip<XDocument> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "XElement",
				Version = "Document Bin",	            
	            Help = "Interleaves all Input spreads.", 
	            Tags = "interleave, join, generic, spreadop"
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
                Help = "Returns all combinations of pairs of successive slices. From an input ABCD returns AB, BC, CD.",
                Tags = "generic, spreadop"
                )]
    public class XDocumentPairwiseNode : Pairwise<XDocument> {}

    [PluginInfo(Name = "SplitAt",
                Category = "XElement",
                Version = "Document",
                Help = "Splits the Input spread in two at the specified Index.",
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
	public class XDocumentBufferNode : BufferNode<XDocument>
    {
        public XDocumentBufferNode() : base(XDocumentCopier.Default) { }
    }
    
    [PluginInfo(Name = "Queue",
                Category = "XElement",
                Version = "Document",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO (First In First Out) fashion.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class XDocumentQueueNode : QueueNode<XDocument>
    {
        public XDocumentQueueNode() : base(XDocumentCopier.Default) { }
    }
	
	[PluginInfo(Name = "RingBuffer",
                Category = "XElement",
                Version = "Document",
	            Help = "Inserts the input at the ringbuffer position.",
	            Tags = "generic, spreadop, collection",
	            AutoEvaluate = true
	           )]
	public class XDocumentRingBufferNode : RingBufferNode<XDocument>
    {
        public XDocumentRingBufferNode() : base(XDocumentCopier.Default) { }
    }

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

