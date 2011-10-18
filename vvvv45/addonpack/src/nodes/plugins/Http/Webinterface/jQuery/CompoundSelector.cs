using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public class CompoundSelector : JavaScriptObject
	{
		protected List<Selector> FSelectors;
		
		public CompoundSelector(params Selector[] selectors)
		{
			FSelectors = new List<Selector>(selectors);
		}

		public CompoundSelector(List<string> selectors)
		{
			FSelectors = new List<Selector>();
			for (int i = 0; i < selectors.Count; i++)
			{
				FSelectors.Add(new ClassSelector(selectors[i]));
			}
		}

		protected override string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = "'";
			for (int i = 0; i < FSelectors.Count; i++)
			{
				text += FSelectors[i].PValue;
				if (i < FSelectors.Count - 1)
				{
					text += ",";
				}
			}
			text += "'";

			return text;
		}
	}
}