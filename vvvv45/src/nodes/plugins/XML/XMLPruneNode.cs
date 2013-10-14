using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using VVVV.Core;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;
using VVVV.Utils.Streams;

using VVVV.Core.Logging;
namespace VVVV.Nodes.XML
{
    [PluginInfo(Name = "Prune", Category = "XElement", Version = "", Tags = "xml")]
    public class PruneFixNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        private class QueryInfo
        {
        	public string Name;
        	public IIOContainer<IOutStream<bool>> QueryExistsContainer;
            public IIOContainer<IOutStream<string>> QueryContainer;
        	public IIOContainer<IOutStream<XElement>> FirstContainer;
        	public IIOContainer<IOutStream<IInStream<XElement>>> AllContainer;
            
        	
        	public QueryInfo(string Name, IIOFactory IOFactory)
        	{
        		string pinName = Name;//.Replace("/"," ");
//        		if (pinName.StartsWith("/"))
//        			pinName = pinName.Replace("/","|");
        		this.Name = Name;
        		QueryExistsContainer = IOFactory.CreateIOContainer<IOutStream<bool>>(new OutputAttribute(pinName + " Available") { Visibility = PinVisibility.OnlyInspector});
				QueryExistsContainer.IOObject.Length = 0;
        		QueryContainer = IOFactory.CreateIOContainer<IOutStream<string>>(new OutputAttribute(pinName));
        		QueryContainer.IOObject.Length = 0;
				FirstContainer = IOFactory.CreateIOContainer<IOutStream<XElement>>(new OutputAttribute(pinName + " (Child)"){Visibility = PinVisibility.OnlyInspector});
        		FirstContainer.IOObject.Length = 0;
        		AllContainer = IOFactory.CreateIOContainer<IOutStream<IInStream<XElement>>>(
                    	new OutputAttribute(pinName + " (Children)"){ Visibility = PinVisibility.OnlyInspector, BinVisibility = PinVisibility.OnlyInspector } );
        	
        		AllContainer.IOObject.Length = 0;
        	}
        	
        	public void Dispose()
        	{
        		QueryContainer.Dispose();
        		QueryExistsContainer.Dispose();
        		FirstContainer.Dispose();
        		AllContainer.Dispose();
        	}
        }

    	#region pins
		#pragma warning disable 649, 169
        [Config("Queries", DefaultString = @"NodeA, AttribA", IsSingle = true)]
        public IDiffSpread<string> QueryNamesPin;


        [Input("Element")]
        public IDiffSpread<XElement> Element;

        [Input("XPath", DefaultString = ".", IsSingle = true)]
        public IDiffSpread<string> XPathNamePin;

        [Input("Namespace Resolver", IsSingle = true)]
        public IDiffSpread<IXmlNamespaceResolver> NamespaceResolver;
    	
    	[Output("Elements")]
        public ISpread<ISpread<XElement>> Elements;
    	
    	[Output("Elements Name")]
        public ISpread<string> ElementsName;
    	
        [Import()]
        IIOFactory IOFactory; 
    	
    	[Import()]
        ILogger FLogger; 
		#pragma warning restore
    	#endregion

        #region fields
        Dictionary<string, QueryInfo> FQueries = new Dictionary<string, QueryInfo>();
        private bool ConfigChanged;
        #endregion

        public void OnImportsSatisfied()
        {
            QueryNamesPin.Changed += QueryNamesPin_Changed;
        }

        #region attribute
        void QueryNamesPin_Changed(IDiffSpread<string> spread)
        {
        	try
        	{
            if (spread.SliceCount == 0) return;

            var QueryNames = spread[0].Split(',').Select(s => s.Trim()).Where(s => s.Length > 0);
			
			foreach (var query in QueryNames)
                if (!FQueries.ContainsKey(query))
                	FQueries.Add(query, new QueryInfo(query, IOFactory));

            //remove obsolete pins (ToList is necessary to not read and remove from the same list)
            foreach (var key in FQueries.Keys.Except(QueryNames).ToArray())
            {
            	FQueries[key].Dispose();
                FQueries.Remove(key);
            }
        	ConfigChanged = true;
        		
        	} catch (Exception e)
        	{
        		FLogger.Log(e);
        	}
        }
    	#endregion attribute
        
        // method called each frame in vvvv
        public void Evaluate(int SpreadMax)
        {
            //if (Element.SliceCount == 0) return;

        	if (!Element.IsChanged && !ConfigChanged && !XPathNamePin.IsChanged) return;

            Elements.SliceCount = Element.SliceCount;
        	
        	
            for (int i = 0; i < Element.SliceCount; i++)
            {
            	try
            	{
            		var element = Element[i];

            		if (!string.IsNullOrEmpty(XPathNamePin[0]) && element != null)
                		Elements[i] = element.XPathSelectElements(XPathNamePin[0], NamespaceResolver[0]).ToSpread();
	            	else 
	            		Elements[i] = new Spread<XElement>(0);
            	}
            	catch (Exception e)
            	{
            		Elements[i] = new Spread<XElement>(0);
            		FLogger.Log(e);
            	}
            }
        	
            var allElements = Elements.SelectMany(spread => spread).ToArray();

        	// set slicecount to all element hits
            ElementsName.SliceCount = allElements.Length;
        	
        	#region process names
        	int j = 0;
        	foreach (var element in allElements)
        	{
        		ElementsName[j] = element.Name.ToString();
	            j++;
        	}
        	#endregion process names

            #region process queries
            foreach (var query in FQueries.Values)
            {
            	var queryExistsPin = query.QueryExistsContainer.IOObject;
            	var queryPin = query.QueryContainer.IOObject;
            	var firstPin = query.FirstContainer.IOObject;
            	var allPin = query.AllContainer.IOObject;
            
            	queryExistsPin.Length = allElements.Length;
            	queryPin.Length = allElements.Length;
              	firstPin.Length = allElements.Length;
            	allPin.Length = allElements.Length;
            	
            	using (var queryExistsWriter = queryExistsPin.GetWriter())
            	using (var queryWriter = queryPin.GetWriter())
            	using (var firstWriter = firstPin.GetWriter())
            	using (var allWriter = allPin.GetWriter())
            	{
            		foreach (var element in allElements)
            		{
            			XObject xobj = null;
            			IEnumerable<XObject> xobjEnumerable = null;
            			try
            			{
	            			var node = (IEnumerable)element.XPathEvaluate(query.Name, NamespaceResolver[0]);
	            			xobjEnumerable = node.Cast<XObject>();
	            			xobj = xobjEnumerable.FirstOrDefault();
            			}
            			catch (XPathException xe)
            			{
            				FLogger.Log(xe);
            			}
            			if (xobj != null)
            			{
	            			if (xobj.NodeType == XmlNodeType.Attribute)
	            			{
	            				var attr = xobj as XAttribute;
	            				queryExistsWriter.Write(true);
	            		   		queryWriter.Write(attr.Value);
	            				firstWriter.Write(new XElement("NIL"));
	                			allWriter.Write(new MemoryIOStream<XElement>(0));
	            			}
	            			else
	            			{
	            				var el = xobj as XElement;
	            				queryExistsWriter.Write(true);
	            		   		queryWriter.Write(el.Value);
	            				firstWriter.Write(el);
	            				allWriter.Write(xobjEnumerable.Cast<XElement>().ToStream());
	            			}
            			}
	                    else
            			{
            				queryWriter.Write("");
            				queryExistsWriter.Write(false);
            				firstWriter.Write(new XElement("NIL"));
	                		allWriter.Write(new MemoryIOStream<XElement>(0));
            			}
            		}
            	}
            }
        	#endregion process queries
        	
            ConfigChanged = false;
        }
    }
}
