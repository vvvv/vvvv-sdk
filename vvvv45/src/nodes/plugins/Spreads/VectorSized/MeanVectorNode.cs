#region usings
using System;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Mean", Category = "Spectral", Version = "Vector", Help = "Mean (Spectral) with vector size", Author = "woei")]
	#endregion PluginInfo
	public class MeanVectorNode : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<double> spread = new VecBinSpread<double>();
        
        #pragma warning disable 649
        [Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Output("Output")]
		IOutStream<double> FOutput;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FVec.Sync();
			FBin.Sync();
			
			if (FInput.IsChanged || FVec.IsChanged || FBin.IsChanged)
			{
				if (FInput.Length>0 && FVec.Length>0 && FBin.Length>0)
				{
					int vecSize = Math.Max(1,FVec.GetReader().Read());
                    spread.Sync(FInput,vecSize,FBin);

                    if (spread.ItemCount == 0)
                        FOutput.Length = 0;
                    else
                    {
                        FOutput.Length = spread.Count * vecSize;
                        using (var dataWriter = FOutput.GetWriter())
                            for (int b = 0; b < spread.Count; b++)
                                for (int v = 0; v < vecSize; v++)
                                    dataWriter.Write(spread.GetBinColumn(b, v).DefaultIfEmpty(0).Average(), 1);
                    }
				}
				else
					FOutput.Length = 0;
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "MeanSquare", Category = "Spectral", Version = "Vector", Help = "MeanSquare (Spectral) with vector size", Author = "woei")]
	#endregion PluginInfo
	public class MeanSquareVectorNode : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<double> spread = new VecBinSpread<double>();
        
        #pragma warning disable 649
        [Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Output("Output")]
		IOutStream<double> FOutput;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FVec.Sync();
			FBin.Sync();
			
			if (FInput.IsChanged || FVec.IsChanged || FBin.IsChanged)
			{
				if (FInput.Length>0 && FVec.Length>0 && FBin.Length>0)
				{
					int vecSize = Math.Max(1,FVec.GetReader().Read());
                    spread.Sync(FInput,vecSize,FBin); 
					
					if (spread.ItemCount==0)
						FOutput.Length=0;
					else
					{
						FOutput.Length = spread.Count*vecSize;
						using (var dataWriter = FOutput.GetWriter())
							for (int b = 0; b < spread.Count; b++)
								for (int v = 0; v < vecSize; v++)
									dataWriter.Write(spread.GetBinColumn(b,v).Select(f => f*f).Average(),1);
					}
				}
				else
					FOutput.Length = 0;
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "RootMeanSquare", Category = "Spectral", Version = "Vector", Help = "RootMeanSquare (Spectral) with vector size", Author = "woei")]
	#endregion PluginInfo
	public class RootMeanSquareVectorNode : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<double> spread = new VecBinSpread<double>();

        #pragma warning disable 649
        [Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Output("Output")]
		IOutStream<double> FOutput;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FVec.Sync();
			FBin.Sync();
			
			if (FInput.IsChanged || FVec.IsChanged || FBin.IsChanged)
			{
				if (FInput.Length>0 && FVec.Length>0 && FBin.Length>0)
				{
					int vecSize = Math.Max(1,FVec.GetReader().Read());
                    spread.Sync(FInput,vecSize,FBin); 
					
					if (spread.ItemCount==0)
						FOutput.Length=0;
					else
					{
						FOutput.Length = spread.Count*vecSize;
						using (var dataWriter = FOutput.GetWriter())
							for (int b = 0; b < spread.Count; b++)
								for (int v = 0; v < vecSize; v++)
									dataWriter.Write(Math.Pow(spread.GetBinColumn(b,v).Select(f => f*f).Average(),0.5),1);
					}
				}
				else
					FOutput.Length = 0;
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "GeometricMean", Category = "Spectral", Version = "Vector", Help = "GeometricMean (Spectral) with vector size", Author = "woei")]
	#endregion PluginInfo
	public class GeometricMeanVectorNode : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<double> spread = new VecBinSpread<double>();

        #pragma warning disable 649
        [Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Output("Output")]
		IOutStream<double> FOutput;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FVec.Sync();
			FBin.Sync();
			
			if (FInput.IsChanged || FVec.IsChanged || FBin.IsChanged)
			{
				if (FInput.Length>0 && FVec.Length>0 && FBin.Length>0)
				{
					int vecSize = Math.Max(1,FVec.GetReader().Read());
                    spread.Sync(FInput,vecSize,FBin); 
					
					if (spread.ItemCount==0)
						FOutput.Length=0;
					else
					{
						FOutput.Length = spread.Count*vecSize;
						using (var dataWriter = FOutput.GetWriter())
							for (int b = 0; b < spread.Count; b++)
								for (int v = 0; v < vecSize; v++)
									dataWriter.Write(Math.Pow(spread.GetBinColumn(b,v).Aggregate((work,next) => work*next),1/(double)(spread[b].Length/vecSize)),1);
					}
				}
				else
					FOutput.Length = 0;
			}
		}
	}
	#region PluginInfo
	[PluginInfo(Name = "HarmonicMean", Category = "Spectral", Version = "Vector", Help = "HarmonicMean (Spectral) with vector size", Author = "woei")]
	#endregion PluginInfo
	public class HarmonicMeanVectorNode : IPluginEvaluate
	{
        #region fields & pins
        readonly VecBinSpread<double> spread = new VecBinSpread<double>();

        #pragma warning disable 649
        [Input("Input", CheckIfChanged = true, AutoValidate = false)]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false)]
		IInStream<int> FBin;
		
		[Output("Output")]
		IOutStream<double> FOutput;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInput.Sync(); 
			FVec.Sync();
			FBin.Sync();
			
			if (FInput.IsChanged || FVec.IsChanged || FBin.IsChanged)
			{
				if (FVec.Length>0 && FVec.Length>0 && FBin.Length>0)
				{
					int vecSize = Math.Max(1,FVec.GetReader().Read());
                    spread.Sync(FInput,vecSize,FBin);
					
					if (spread.ItemCount==0)
						FOutput.Length=0;
					else
					{
						FOutput.Length = spread.Count*vecSize;
						using (var dataWriter = FOutput.GetWriter())
							for (int b = 0; b < spread.Count; b++)
								for (int v = 0; v < vecSize; v++)
									dataWriter.Write((spread[b].Length/vecSize)/spread.GetBinColumn(b,v).Select(f => 1/f).Sum(),1);
					}
				}
				else
					FOutput.Length = 0;
			}
		}
	}
}
