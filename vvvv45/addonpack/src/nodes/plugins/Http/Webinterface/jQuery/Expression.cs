using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
	public abstract class Expression : JQuery, IJavaScriptObject
	{
		#region Fields
		protected List<ChainableOperation> FChainedOperations;
		protected bool FDoInclude;		
		#endregion
		
		public bool DoInclude
		{
			get { return FDoInclude; }
			set { FDoInclude = value; }
		}

		public Expression()
        {
			FDoInclude = true;
			FChainedOperations = new List<ChainableOperation>();
			FStatements.Add(this);
        }

		public void AddChainedMethodCall(String methodName, params object[] arguments)
		{
			FChainedOperations.Add(new MethodCall(methodName, arguments));
		}

		public void AddChainedOperation(ChainableOperation op)
		{
			FChainedOperations.Add(op);
		}

		public virtual new string GenerateScript(int indentSteps, bool breakInternalLines, bool breakAfter)
		{
			string text = GenerateObjectScript(indentSteps, breakInternalLines, breakAfter);
			foreach (ChainableOperation op in FChainedOperations)
			{
				if (op.DoInclude)
				{
					text += op.GenerateScript(indentSteps, breakInternalLines, breakAfter);
				}
			}
			return text;
		}

		protected abstract string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter);
	}
}
