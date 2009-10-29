using System;
using System.Collections.Generic;
using System.Text;

using VVVV.Webinterface.jQuery;

namespace VVVV.Nodes.HttpGUI
{
	public class JQueryNodeIOData
	{
		private JQueryExpression FExpression;
		private JQueryNodeIOData FUpstreamJQueryData;

		public JQueryNodeIOData UpstreamJQueryData
		{
			get { return FUpstreamJQueryData; }
			set { FUpstreamJQueryData = value; }
		}

		public JQueryExpression Expression
		{
			get { return FExpression; }
		}

		private JQueryNodeIOData()
		{
			FUpstreamJQueryData = null;
		}
		
		public JQueryNodeIOData(JQueryExpression expression) : this()
		{
			FExpression = expression;
		}

		public JQueryExpression BuildChain()
		{
			JQueryExpression upstreamExpression;
			if (FUpstreamJQueryData != null)
			{
				upstreamExpression = FUpstreamJQueryData.BuildChain();
			}
			else
			{
				upstreamExpression = new JQueryExpression();
			}

			return FExpression.Chain(upstreamExpression);
		}
	}
}
