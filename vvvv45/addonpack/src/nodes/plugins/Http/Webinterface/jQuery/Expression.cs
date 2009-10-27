using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class Expression : JQuery, IJavaScriptObject
	{
		#region Fields
		protected Queue<ChainableOperation> FChainedOperations;
		#endregion

		public Expression()
        {
            FChainedOperations = new Queue<ChainableOperation>();
			FStatements.Enqueue(this);
        }

		protected void AddMethodCall(String methodName, params object[] arguments)
		{
			FChainedOperations.Enqueue(new MethodCall(methodName, arguments));
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
