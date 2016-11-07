//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

#region usings
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using ColladaSlimDX.ColladaDocument;
using ColladaSlimDX.ColladaPipeline;
using ColladaSlimDX.ColladaModel;
using ColladaSlimDX.Utils;
using SlimDX;
using SlimDX.Direct3D9;

#endregion usings

//the vvvv node namespace
namespace VVVV.Nodes
{
	[PluginInfo(Name = "ColladaFile",
	            Category = "EX9.Geometry",
	            Version = "",
	            Author = "vvvv group",
	            Help = "Loads a COLLADA *.dae file.",
	            Tags = "dae, read, load")]
	public class PluginColladaLoader: IPluginEvaluate, IDisposable
	{
		#region pins & fields
		
		public enum Axis {Default, X, Y, Z, MinusX, MinusY, MinusZ};
		
		[Input("Filename", IsSingle = true, StringType = StringType.Filename, FileMask = "COLLADA Files (*.dae)|*.dae")]
		protected IDiffSpread<string> FFileNameInput;
		
		[Input("Reload", IsSingle = true, IsBang = true)]
		protected ISpread<bool> FReloadInput;
		
		[Output("COLLADA Model")]
		protected ISpread<Model> FColladaModelOutput;
		
		[Output("Info")]
		protected ISpread<string> FInfoOutput;
		
		[Import]
		protected ILogger FLogger;
		
		// Created in constructor
		private IDiffSpread<CoordinateSystemType> FCsSourceTypeConfig;
		private IDiffSpread<Axis> FUpAxisSourceConfig;
		private IDiffSpread<Axis> FRightAxisSourceConfig;
		private IDiffSpread<double> FMeterSourceConfig;
		private IDiffSpread<CoordinateSystemType> FCsTargetTypeConfig;
		private IDiffSpread<Axis> FUpAxisTargetConfig;
		private IDiffSpread<Axis> FRightAxisTargetConfig;
		private IDiffSpread<double> FMeterTargetConfig;
		
		// Track whether Dispose has been called.
		private bool FDisposed = false;

		private Document FColladaDocument;
		private Model FColladaModel;
		private List<string> FInfo;
		private bool FInfoNeedsUpdate = false;
		private CoordinateSystemType FCsTypeSource;
		private CoordinateSystemType FCsTypeTarget;
		private Axis FUpAxisSource;
		private Axis FUpAxisTarget;
		private Axis FRightAxisSource;
		private Axis FRightAxisTarget;
		private double FDistanceUnitSource = 0;
		private double FDistanceUnitTarget = 0;
		
		#endregion pins & fields
		
		#region constructor/destructor
		[ImportingConstructor]
		public PluginColladaLoader(
			IPluginHost host,
			[Config ("Coordinate system of source", IsSingle = true)]
			IDiffSpread<CoordinateSystemType> csSourceTypeConfig,
			[Config ("Source up axis", IsSingle = true)]
			IDiffSpread<Axis> upAxisSourceConfig,
			[Config ("Source right axis", IsSingle = true)]
			IDiffSpread<Axis> rightAxisSourceConfig,
			[Config ("Source distance unit in meter", IsSingle = true, DefaultValue = 0)]
			IDiffSpread<double> meterSourceConfig,
			[Config ("Coordinate system of target",  IsSingle = true)]
			IDiffSpread<CoordinateSystemType> csTargetTypeConfig,
			[Config ("Target up axis", IsSingle = true)]
			IDiffSpread<Axis> upAxisTargetConfig,
			[Config ("Target right axis", IsSingle = true)]
			IDiffSpread<Axis> rightAxisTargetConfig,
			[Config ("Target distance unit in meter", IsSingle = true, DefaultValue = 0)]
			IDiffSpread<double> meterTargetConfig)
		{
			FInfo = new List<string>();
			
			FCsSourceTypeConfig = csSourceTypeConfig;
			// COLLADA is right handed by default.
			FCsSourceTypeConfig[0] = CoordinateSystemType.RightHanded;
			FUpAxisSourceConfig = upAxisSourceConfig;
			FRightAxisSourceConfig = rightAxisSourceConfig;
			FMeterSourceConfig = meterSourceConfig;
			FCsTargetTypeConfig = csTargetTypeConfig;
			FUpAxisTargetConfig = upAxisTargetConfig;
			FRightAxisTargetConfig = rightAxisTargetConfig;
			FMeterTargetConfig = meterTargetConfig;
			
			FCsSourceTypeConfig.Changed += FCsSourceTypeConfig_Changed;
			FUpAxisSourceConfig.Changed += FUpAxisSourceConfig_Changed;
			FRightAxisSourceConfig.Changed += FRightAxisSourceConfig_Changed;
			FMeterSourceConfig.Changed += FMeterSourceConfig_Changed;
			FCsTargetTypeConfig.Changed += FCsTargetTypeConfig_Changed;
			FUpAxisTargetConfig.Changed += FUpAxisTargetConfig_Changed;
			FRightAxisTargetConfig.Changed += FRightAxisTargetConfig_Changed;
			FMeterTargetConfig.Changed += FMeterTargetConfig_Changed;
		}

		// Implementing IDisposable's Dispose method.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
		public void Dispose()
		{
			Dispose(true);
			// Take yourself off the Finalization queue
			// to prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		protected virtual void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					// Dispose managed resources.
					FColladaDocument = null;
					FColladaModel = null;
					FInfo = null;
					
					FCsSourceTypeConfig.Changed -= FCsSourceTypeConfig_Changed;
					FUpAxisSourceConfig.Changed -= FUpAxisSourceConfig_Changed;
					FRightAxisSourceConfig.Changed -= FRightAxisSourceConfig_Changed;
					FMeterSourceConfig.Changed -= FMeterSourceConfig_Changed;
					FCsTargetTypeConfig.Changed -= FCsTargetTypeConfig_Changed;
					FUpAxisTargetConfig.Changed -= FUpAxisTargetConfig_Changed;
					FRightAxisTargetConfig.Changed -= FRightAxisTargetConfig_Changed;
					FMeterTargetConfig.Changed -= FMeterTargetConfig_Changed;
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				
				// Note that this is not thread safe.
				// Another thread could start disposing the object
				// after the managed resources are disposed,
				// but before the disposed flag is set to true.
				// If thread safety is necessary, it must be
				// implemented by the client.
			}
			FDisposed = true;
		}

		// Use C# destructor syntax for finalization code.
		// This destructor will run only if the Dispose method
		// does not get called.
		// It gives your base class the opportunity to finalize.
		// Do not provide destructors in types derived from this class.
		~PluginColladaLoader()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
		}
		#endregion constructor/destructor
		
		#region IDiffSpread.Changed event handlers
		
		void FCsSourceTypeConfig_Changed(IDiffSpread<CoordinateSystemType> spread)
		{
			FCsTypeSource = FCsSourceTypeConfig[0];
			ConfigurateModel();
		}
		
		void FUpAxisSourceConfig_Changed(IDiffSpread<Axis> spread)
		{
			FUpAxisSource = FUpAxisSourceConfig[0];
			ConfigurateModel();
		}
		
		void FRightAxisSourceConfig_Changed(IDiffSpread<Axis> spread)
		{
			FRightAxisSource = FRightAxisSourceConfig[0];
			ConfigurateModel();
		}
		
		void FMeterSourceConfig_Changed(IDiffSpread<double> spread)
		{
			FDistanceUnitSource = FMeterSourceConfig[0];
			ConfigurateModel();
		}
		
		void FCsTargetTypeConfig_Changed(IDiffSpread<CoordinateSystemType> spread)
		{
			FCsTypeTarget = FCsTargetTypeConfig[0];
			ConfigurateModel();
		}
		
		void FUpAxisTargetConfig_Changed(IDiffSpread<Axis> spread)
		{
			FUpAxisTarget = FUpAxisTargetConfig[0];
			ConfigurateModel();
		}
		
		void FRightAxisTargetConfig_Changed(IDiffSpread<Axis> spread)
		{
			FRightAxisTarget = FRightAxisTargetConfig[0];
			ConfigurateModel();
		}
		
		void FMeterTargetConfig_Changed(IDiffSpread<double> spread)
		{
			FDistanceUnitTarget = FMeterTargetConfig[0];
			ConfigurateModel();
		}
		
		#endregion
		
		private void ConfigurateModel()
		{
			if (FColladaDocument == null) return;
			if (FColladaModel == null) return;
			
			CoordinateSystem csSource = new CoordinateSystem(FColladaDocument.CoordinateSystem);
			csSource.Type = FCsTypeSource;
			CoordinateSystem csTarget = new CoordinateSystem(CoordinateSystemType.LeftHanded);
			csTarget.Type = FCsTypeTarget;
			
			if (FUpAxisSource > 0)
				csSource.Up = GetVectorForAxis(FUpAxisSource);
			if (FUpAxisTarget > 0)
				csTarget.Up = GetVectorForAxis(FUpAxisTarget);
			if (FRightAxisSource > 0)
				csSource.Right = GetVectorForAxis(FRightAxisSource);
			if (FRightAxisTarget > 0)
				csTarget.Right = GetVectorForAxis(FRightAxisTarget);
			
			if (FDistanceUnitSource > 0)
				csSource.Meter = (float) FDistanceUnitSource;
			if (FDistanceUnitTarget > 0)
				csTarget.Meter = (float) FDistanceUnitTarget;
			
			FColladaModel.CoordinateSystemSource = csSource;
			FColladaModel.CoordinateSystemTarget = csTarget;
			
			// Ensure pin is changed.
			FColladaModelOutput[0] = FColladaModel;
			
			GenerateInfoStrings();
		}
		
		private Vector3 GetVectorForAxis(Axis axis)
		{
			switch (axis)
			{
					case Axis.X: return new Vector3(1f, 0f, 0f);
					case Axis.Y: return new Vector3(0f, 1f, 0f);
					case Axis.Z: return new Vector3(0f, 0f, 1f);
					case Axis.MinusX: return new Vector3(-1f, 0f, 0f);
					case Axis.MinusY: return new Vector3(0f, -1f, 0f);
					default: return new Vector3(0f, 0f, -1f);
			}
		}
		
		private string VectorToString(Vector3 v)
		{
			if (v.X == 1) return "X";
			if (v.Y == 1) return "Y";
			if (v.Z == 1) return "Z";
			if (v.X == -1) return "-X";
			if (v.Y == -1) return "-Y";
			return "-Z";
		}
		
		private void GenerateInfoStrings()
		{
			FInfoNeedsUpdate = true;
			FInfo.Clear();
			
			if (FColladaModel == null)
			{
				return;
			}
			
			FInfo.Add("Source up axis: " + VectorToString(FColladaModel.CoordinateSystemSource.Up));
			FInfo.Add("Source right axis: " + VectorToString(FColladaModel.CoordinateSystemSource.Right));
			FInfo.Add("Source in axis: " + VectorToString(FColladaModel.CoordinateSystemSource.Inward));
			FInfo.Add("Source distance unit in meter: " + FColladaModel.CoordinateSystemSource.Meter);
			FInfo.Add("Target up axis: " + VectorToString(FColladaModel.CoordinateSystemTarget.Up));
			FInfo.Add("Target right axis: " + VectorToString(FColladaModel.CoordinateSystemTarget.Right));
			FInfo.Add("Target in axis: " + VectorToString(FColladaModel.CoordinateSystemTarget.Inward));
			FInfo.Add("Target distance unit in meter: " + FColladaModel.CoordinateSystemTarget.Meter);
		}
		
		public void Evaluate(int SpreadMax)
		{
		    COLLADAUtil.Logger = new LoggerWrapper(FLogger);
		    
			//if any of the inputs has changed
			//recompute the outputs
			if (FFileNameInput.IsChanged || FReloadInput[0])
			{
				FColladaDocument = null;
				FColladaModel = null;
				FColladaModelOutput.SliceCount = 0;
				FInfoOutput.SliceCount = 0;
				
				string filename = FFileNameInput[0];
				if (string.IsNullOrEmpty(filename) || !File.Exists(filename)) return;
				
				FLogger.Log(LogType.Message, "Loading " + filename);
				try
				{
					FColladaDocument = new Document(filename);
					Conditioner.ConvexTriangulator(FColladaDocument);
					// not necessary anymore
					//Conditioner.Reindexor(colladaDocument);
					FColladaModel = new Model(FColladaDocument);
					
					FColladaModelOutput.SliceCount = 1;
					FColladaModelOutput[0] = FColladaModel;
					ConfigurateModel();
					
					FLogger.Log(LogType.Message, filename + " loaded.");
				}
				catch (Exception e)
				{
					FLogger.Log(e);
				}
			}
			
			if (FInfoNeedsUpdate)
			{
				FInfoNeedsUpdate = false;
				FInfoOutput.SliceCount = FInfo.Count;
				for (int i = 0; i< FInfo.Count; i++)
					FInfoOutput[i] = FInfo[i];
			}
		}
	}
}
