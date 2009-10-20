using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	class JavaScriptGenericObject : JavaScriptObject
	{
		protected Dictionary<String, JavaScriptObject> FJscriptDictionaryObject;
		protected ScriptBlock FScriptBlock;
			
		public JavaScriptGenericObject ()
		{
			FJscriptDictionaryObject = new Dictionary<string, JavaScriptObject>();
			FScriptBlock = new ScriptBlock(0, false);
		}

		public JavaScriptGenericObject(ScriptBlock scriptBlock)
		{
			FJscriptDictionaryObject = new Dictionary<string, JavaScriptObject>();
			FScriptBlock = scriptBlock;
		}

		public void Set(string key, JavaScriptObject value)
		{
			FJscriptDictionaryObject.Add(key, value);
		}

		public override string PScript
		{	
			get
			{
				string text = "{";
				int dictionaryLength = FJscriptDictionaryObject.Count;
				int count = 1;
				foreach (KeyValuePair<string, JavaScriptObject> kvp in FJscriptDictionaryObject)
				{
					text += kvp.Key + " : " + kvp.Value.PScript;
					if (count != dictionaryLength)
					{
						text += ",";
						if (!FScriptBlock.PDoBreakInternalLines)
						{
							text += " ";
						}
					}
					if (FScriptBlock.PDoBreakInternalLines)
					{
						text += "\n";
					}
					count++;
				}
				text += "}";
				return text;
			}
		}
	}
}