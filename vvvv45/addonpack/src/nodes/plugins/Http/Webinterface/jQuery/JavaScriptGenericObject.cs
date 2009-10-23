using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class JavaScriptGenericObject : JavaScriptObject
	{
		protected Dictionary<string, JavaScriptObject> FJscriptDictionaryObject;
			
		public JavaScriptGenericObject ()
		{
			FJscriptDictionaryObject = new Dictionary<string, JavaScriptObject>();
		}

		public void Set<T>(string key, T value)
		{
			JavaScriptObject jsObject = value as JavaScriptObject;
			if (jsObject == null)
			{
				jsObject = JavaScriptObjectFactory.Create(value);
			}
			FJscriptDictionaryObject[key] = jsObject;
		}

		public override string PScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = "{";
			int dictionaryLength = FJscriptDictionaryObject.Count;
			bool hasInternalLines = dictionaryLength > 0;
			if (breakInternalLines && hasInternalLines)
			{
				text += "\n";
			}
			
			int count = 1;
			foreach (KeyValuePair<string, JavaScriptObject> kvp in FJscriptDictionaryObject)
			{
				if (breakInternalLines)
				{
					for (int i = 0; i < indentSteps + 1; i++)
					{
						text += "\t";
					}
				}
				text += kvp.Key + " : " + kvp.Value.PScript(indentSteps + 1, kvp.Value is JavaScriptAnonymousFunction && breakInternalLines, breakAfter);
				if (count != dictionaryLength)
				{
					text += ",";
					if (!breakInternalLines)
					{
						text += " ";
					}
				}
				if (breakInternalLines)
				{
					text += "\n";
				}
				count++;
			}
			if (breakInternalLines && hasInternalLines)
			{
				for (int i = 0; i < indentSteps; i++)
				{
					text += "\t";
				}
			}
			text += "}";
			return text;
		}
	}
}