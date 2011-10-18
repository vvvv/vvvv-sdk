using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public class JavaScriptGenericObject : JavaScriptObject
	{
		protected Dictionary<string, IJavaScriptObject> FJscriptDictionaryObject;
			
		public JavaScriptGenericObject ()
		{
			FJscriptDictionaryObject = new Dictionary<string, IJavaScriptObject>();
		}

		public void Set<T>(string key, T value)
		{
			IJavaScriptObject jsObject = value as IJavaScriptObject;
			if (jsObject == null)
			{
				jsObject = JavaScriptObjectFactory.Create(value);
			}
			FJscriptDictionaryObject[key] = jsObject;
		}

		protected override string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = "{";
			int dictionaryLength = FJscriptDictionaryObject.Count;
			bool hasInternalLines = dictionaryLength > 0;
			if (breakInternalLines && hasInternalLines)
			{
				text += "\n";
			}
			
			int count = 1;
			foreach (KeyValuePair<string, IJavaScriptObject> kvp in FJscriptDictionaryObject)
			{
				if (breakInternalLines)
				{
					for (int i = 0; i < indentSteps + 1; i++)
					{
						text += "\t";
					}
				}
				text += kvp.Key + " : " + kvp.Value.GenerateScript(indentSteps + 1, kvp.Value is JavaScriptAnonymousFunction && breakInternalLines, breakAfter);
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