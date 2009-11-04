using System;
using System.Collections.Generic;
using System.Text;

using VVVV.Webinterface.jQuery;
using VVVV.Nodes.Http.BaseNodes;

namespace VVVV.Nodes.Http.BaseNodes
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
