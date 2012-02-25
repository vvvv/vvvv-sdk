using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes
{
	public unsafe class ValueInterleaveNode : IPlugin, IDisposable
	{
		#region Plugin Info
		public static IPluginInfo PluginInfo
		{
			get
			{
				IPluginInfo Info = new PluginInfo();
				Info.Name = "Vector";							//use CamelCaps and no spaces
				Info.Category = "Spreads";						//try to use an existing one
				Info.Version = "Join";						//versions are optional. leave blank if not needed
				Info.Help = "Vector (nd) Join";
				Info.Bugs = "";
				Info.Credits = "";								//give credits to thirdparty code used
				Info.Warnings = "";
				Info.Author = "vux";

				//leave below as is
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
				System.Diagnostics.StackFrame sf = st.GetFrame(0);
				System.Reflection.MethodBase method = sf.GetMethod();
				Info.Namespace = method.DeclaringType.Namespace;
				Info.Class = method.DeclaringType.Name;
				return Info;
				//leave above as is
			}
		}
		#endregion

		#region Fields
		private IPluginHost FHost;

		private IValueConfig FPinCfgInputCount;
		private List<IValueFastIn> FPinInputList = new List<IValueFastIn>();
		//private List<Tuple<IntPtr, IntPtr>> FInputPointerList = new List<Tuple<IntPtr, IntPtr>>();
		private double**[] FPInData;
		private int*[] FPInLength;

		private IValueOut FPinOutput;
		private double** FPPData;
		#endregion

		#region Auto Evaluate
		public bool AutoEvaluate
		{
			get { return false; }
		}
		#endregion

		#region Set Plugin Host
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			this.FHost = Host;

			this.FHost.CreateValueConfig("Input Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinCfgInputCount);
			this.FPinCfgInputCount.SetSubType(2, double.MaxValue, 1, 2, false, false, true);
			Configurate(this.FPinCfgInputCount);

			this.FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutput);
			this.FPinOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			this.FPinOutput.GetValuePointer(out FPPData);
		}
		#endregion

		#region Configurate
		public void Configurate(IPluginConfig Input)
		{
			if (Input == this.FPinCfgInputCount)
			{
				double dblcount;
				this.FPinCfgInputCount.GetValue(0, out dblcount);

				//Always 2 inputs minimum
				dblcount = Math.Max(dblcount, 2);

				if (dblcount < this.FPinInputList.Count)
				{
					//Remove pins, as value is lower
					while (this.FPinInputList.Count > dblcount)
					{
						this.FHost.DeletePin(this.FPinInputList[this.FPinInputList.Count - 1]);
						//this.FInputPointerList.RemoveAt(this.FPinInputList.Count - 1);
						this.FPinInputList.RemoveAt(this.FPinInputList.Count - 1);
					}
				}

				if (dblcount > this.FPinInputList.Count)
				{
					//Add new pins, as value is bigger
					while (this.FPinInputList.Count < dblcount)
					{
						IValueFastIn pin;
						//                        double** ppData;
						//                        int* pLength;

						int index = this.FPinInputList.Count + 1;

						this.FHost.CreateValueFastInput("Input " + index, 1, null, TSliceMode.Dynamic, TPinVisibility.True, out pin);
						pin.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
//						pin.GetValuePointer(out pLength, out ppData);
						this.FPinInputList.Add(pin);
						//this.FInputPointerList.Add(Tuple.Create(new IntPtr(pLength), new IntPtr(ppData)));
					}
				}
				
				FPInData = new double**[FPinInputList.Count];
				FPInLength = new int*[FPinInputList.Count];
				for (int i = 0; i < FPInData.Length; i++)
				{
					double** ppData;
					int* pLength;
					FPinInputList[i].GetValuePointer(out pLength, out ppData);
					FPInData[i] = ppData;
					FPInLength[i] = pLength;
				}
			}
		}
		#endregion

		#region Evaluate
		public void Evaluate(int SpreadMax)
		{

			if (SpreadMax > 0)
			{
				int outcount = SpreadMax * this.FPinInputList.Count;
				this.FPinOutput.SliceCount = outcount;

				double*[] ptrs = new double*[this.FPinInputList.Count];
				int[] cnts = new int[this.FPinInputList.Count];
				int vcount = this.FPinInputList.Count;

				for (int i = 0; i < this.FPinInputList.Count; i++)
				{
					this.FPinInputList[i].GetValuePointer(out cnts[i], out ptrs[i]);
//					var tuple = this.FInputPointerList[i];
//					cnts[i] = Marshal.ReadInt32(tuple.Item1);
//					ptrs[i] = (double*) Marshal.ReadIntPtr(tuple.Item2);
					cnts[i] = *FPInLength[i];
					ptrs[i] = *FPInData[i];
				}

				double* outptr = *FPPData;

				for (int i = 0; i < SpreadMax; i++)
				{
					for (int j = 0; j < vcount; j++)
					{
						*outptr = ptrs[j][i % cnts[j]];
						outptr++;
					}
				}
			}
			else
			{
				this.FPinOutput.SliceCount = 0;
			}
		}
		#endregion

		#region Dispose
		public void Dispose()
		{
		}
		#endregion
	}
	
	
}
