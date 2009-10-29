using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public abstract class Expression : JQuery, IJavaScriptObject
	{
		#region Fields
		protected List<ChainableOperation> FChainedOperations;
		#endregion

		public IEnumerable<ChainableOperation> ChainedOperations
		{
			get { return FChainedOperations; }
		}
	

		public Expression()
        {
            FChainedOperations = new List<ChainableOperation>();
			FStatements.Enqueue(this);
        }

		protected void AddMethodCall(String methodName, params object[] arguments)
		{
			FChainedOperations.Add(new MethodCall(methodName, arguments));
		}

		public virtual new string PScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = GetObjectScript(indentSteps, breakInternalLines, breakAfter);
			foreach (ChainableOperation op in FChainedOperations)
			{
				text += op.PScript(indentSteps, breakInternalLines, breakAfter);
			}
			return text;
		}

		protected abstract string GetObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter);
	}
}
