
using System;
using System.ComponentModel.Composition;

namespace VVVV.Hosting.Pins
{
	public class OutputAttribute : PinAttribute
	{
		public OutputAttribute(string name)
			:base(name)
		{
		}
	}
}
