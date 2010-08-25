using System;
using System.Collections;
using System.Collections.Generic;

using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
	public abstract class DiffWrapperPin<T> : WrapperPin<T>, IDiffSpread<T>
	{
		private DiffPin<T> FDiffPin;
		
		/// <summary>
		/// Needs to be set in constructor of extending class.
		/// </summary>
		public DiffPin<T> DiffPin
		{
			get
			{
				return FDiffPin;
			}
			set
			{
				FDiffPin = value;
				FPin = FDiffPin;
			}
		}
		
		public event SpreadChangedEventHander<T> Changed
		{
			add
			{
				FDiffPin.Changed += value;
			}
			remove
			{
				FDiffPin.Changed -= value;
			}
		}
		
		public bool IsChanged
		{
			get
			{
				return FDiffPin.IsChanged;
			}
		}
	}
}