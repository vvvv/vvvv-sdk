using System;
using System.Collections;
using System.Collections.Generic;

using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Input;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	public abstract class ObservableWrapperPin<T> : WrapperPin<T>, IObservableSpread<T>
	{
		private ObservablePin<T> FObservablePin;
		
		/// <summary>
		/// Needs to be set in constructor of extending class.
		/// </summary>
		protected ObservablePin<T> ObservablePin
		{
			get
			{
				return FObservablePin;
			}
			set
			{
				FObservablePin = value;
				FPin = FObservablePin;
			}
		}
		
		public event SpreadChangedEventHander<T> Changed
		{
			add
			{
				FObservablePin.Changed += value;
			}
			remove
			{
				FObservablePin.Changed -= value;
			}
		}
		
		public bool IsChanged
		{
			get
			{
				return FObservablePin.IsChanged;
			}
		}
	}
}