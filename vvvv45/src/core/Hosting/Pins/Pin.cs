using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using SlimDX;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
	public delegate void PinUpdatedEventHandler<T>(Pin<T> pin);
	
	public abstract class Pin<T> : ISpread<T>, IPluginIOProvider, IDisposable, IPinUpdater
	{
		[Import]
		protected ILogger FLogger;
		protected IPluginHost FHost;
		protected PinAttribute FAttribute;
		protected T[] FData;
		protected int FSliceCount;
		
		public event PinUpdatedEventHandler<T> Updated;
		
		public Pin(IPluginHost host, PinAttribute attribute)
		{
			FHost = host;
			FAttribute = attribute;
		}
		
		/// <summary>
		/// Must be called by subclass at end of constructor.
		/// </summary>
		protected void Initialize(IPluginIO pluginIO)
		{
			PluginIO = pluginIO;
			PluginIO.SetPinUpdater(this);
			PluginIO.Order = FAttribute.Order;
			SliceCount = 1;
		}
		
		public IPluginIO PluginIO
		{
			get;
			private set;
		}
		
		protected virtual void OnUpdated()
		{
			if (Updated != null)
			{
				Updated(this);
			}
		}
		
		public virtual T this[int index]
		{
			get
			{
				return FData[VMath.Zmod(index, FSliceCount)];
			}
			set
			{
				FData[VMath.Zmod(index, FSliceCount)] = value;
			}
		}
		
		public virtual int SliceCount
		{
			get
			{
				return FSliceCount;
			}
			set
			{
				if (FSliceCount != value)
				{
					FData = new T[value];
					
					FSliceCount = value;
				}
			}
		}
		
		//prepare for IPinUpdater
		public virtual void Update()
		{
			OnUpdated();
		}
		
		public virtual void Connect()
		{
			// DO nothing, override in subclass if needed
		}
		
		public virtual void Disconnect()
		{
			// DO nothing, override in subclass is needed
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < SliceCount; i++)
				yield return this[i];
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		protected void LoadDefaultValues(Type type, PinAttribute attribute, out int dimension, out double minValue, out double maxValue, out double stepSize, out bool isInteger)
		{
			dimension = 1;
			minValue = attribute.MinValue;
			maxValue = attribute.MaxValue;
			stepSize = attribute.StepSize;
			isInteger = true;
			
			if (type == typeof(Vector2D))
				dimension = 2;
			else if (type == typeof(Vector3D))
				dimension = 3;
			else if (type == typeof(Vector4D))
				dimension = 4;
			else if (type == typeof(Vector2))
				dimension = 2;
			else if (type == typeof(Vector3))
				dimension = 3;
			else if (type == typeof(Vector4))
				dimension = 4;
			
			//as dimension is set now, check default values
			if(attribute.DefaultValues.Length < dimension)
			{
				var newDefaults = new double[dimension];
				for (int i=0; i<attribute.DefaultValues.Length; i++)
				{
					newDefaults[i] = attribute.DefaultValues[i];
				}
				
				attribute.DefaultValues = newDefaults;
			}
			
			if (attribute.DimensionNames != null)
			{
				if(attribute.DimensionNames.Length < dimension)
				{
					var newNames = new string[dimension];
					for (int i=0; i<dimension; i++)
					{
						if (i < attribute.DimensionNames.Length)
							newNames[i] = attribute.DimensionNames[i];
						else
							newNames[i] = "";
					}
					
					attribute.DimensionNames = newNames;
				}
			}
			
			if (minValue == PinAttribute.DefaultMinValue)
			{
				if (type == typeof(bool))
					minValue = 0.0;
				else if (type == typeof(byte))
					minValue = byte.MinValue;
				else if (type == typeof(sbyte))
					minValue = sbyte.MinValue;
				else if (type == typeof(int))
					minValue = int.MinValue;
				else if (type == typeof(uint))
					minValue = uint.MinValue;
				else if (type == typeof(short))
					minValue = short.MinValue;
				else if (type == typeof(ushort))
					minValue = ushort.MinValue;
				else if (type == typeof(long))
					minValue = long.MinValue;
				else if (type == typeof(ulong))
					minValue = ulong.MinValue;
				else if (type == typeof(float))
					minValue = float.MinValue;
				else if (type == typeof(double))
					minValue = double.MinValue;
			}
			
			if (maxValue == PinAttribute.DefaultMaxValue)
			{
				if (type == typeof(bool))
					maxValue = 1.0;
				else if (type == typeof(byte))
					maxValue = byte.MaxValue;
				else if (type == typeof(sbyte))
					maxValue = sbyte.MaxValue;
				else if (type == typeof(int))
					maxValue = int.MaxValue;
				else if (type == typeof(uint))
					maxValue = uint.MaxValue;
				else if (type == typeof(short))
					maxValue = short.MaxValue;
				else if (type == typeof(ushort))
					maxValue = ushort.MaxValue;
				else if (type == typeof(long))
					maxValue = long.MaxValue;
				else if (type == typeof(ulong))
					maxValue = ulong.MaxValue;
				else if (type == typeof(float))
					maxValue = float.MaxValue;
				else if (type == typeof(double))
					maxValue = double.MaxValue;
			}
			
			if (stepSize == PinAttribute.DefaultStepSize)
			{
				if (type == typeof(float))
					stepSize = 0.01;
				else if (type == typeof(double))
					stepSize = 0.01;
				else if (type == typeof(Vector2D))
					stepSize = 0.01;
				else if (type == typeof(Vector3D))
					stepSize = 0.01;
				else if (type == typeof(Vector4D))
					stepSize = 0.01;
				else if (type == typeof(Vector2))
					stepSize = 0.01;
				else if (type == typeof(Vector3))
					stepSize = 0.01;
				else if (type == typeof(Vector4))
					stepSize = 0.01;
				
				stepSize = attribute.AsInt ? 1.0 : stepSize;
			}
			

			
			if (type == typeof(bool))
				isInteger = false;
			else if (type == typeof(float))
				isInteger = false;
			else if (type == typeof(double))
				isInteger = false;
			else if (type == typeof(Vector2D))
				isInteger = false;
			else if (type == typeof(Vector3D))
				isInteger = false;
			else if (type == typeof(Vector4D))
				isInteger = false;
			else if (type == typeof(Vector2))
				isInteger = false;
			else if (type == typeof(Vector3))
				isInteger = false;
			else if (type == typeof(Vector4))
				isInteger = false;
			
			isInteger = isInteger || attribute.AsInt;
		}
		
		#region IDisposable
		
		// Implement IDisposable.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
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
		private bool FDisposed;
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				// If disposing equals true, dispose all managed
				// and unmanaged resources.
				if(disposing)
				{
					// Dispose managed resources.
					DisposeManaged();
				}

				// Call the appropriate methods to clean up
				// unmanaged resources here.
				// If disposing is false,
				// only the following code is executed.
				DisposeUnmanaged();

				// Note disposing has been done.
				FDisposed = true;
			}
		}
		
		protected virtual void DisposeManaged()
		{
			Updated = null;
		}
		
		protected virtual void DisposeUnmanaged()
		{
			
		}
		
		#endregion
	}
	
	#region Optimize
    
    public static class Optimize
    {
        /// <summary>
        /// FindMulShift finds the multiply and shift to replace a division for a range of
        /// positive integers defined by min and max.
        ///  
        ///   a / n == a * mul >> shift
        /// 
        /// As we know 1/n = 1 * mul >> shift, we can solve mul = (1 << shift) / n
        /// 
        /// </summary>
        /// <param name="max">positive integer, upper limit</param>
        /// <param name="div">divider to replace</param>
        /// <param name="mul">multiply factor</param>
        /// <param name="shift">shift factor (0..63)</param>
        /// <returns>
        /// True if a valid mul and shift value are found. 
        /// If invald both mul and shift are -1
        /// </returns>
        public static bool FindMulShift(long max, long div, out long mul, out int shift)
        {
        	max = Math.Abs(max);
        	div = Math.Abs(div);
        	
            bool found = false;
            mul = -1;
            shift = -1;

            // zero divider error
            if (div == 0) return false;
            
  			// this division would always return 0 from 0..max
  			if (max < div) 
  			{
  				mul = 0;
  				shift = 0;
  				return true;
  			}

            // catch powers of 2
            for (int s = 0; s <= 63; s++)
            {
                if (div == (1L << s))
                {
                    mul = 1;
                    shift = s;
                    return true;
                }
            }

            // start searching for a valid mul/shift pair
            for (shift = 1; shift <= 62; shift++)
            {
            	// shift factor is at least 2log(div), skip others
            	if ((1L << shift) <= div) continue;
            	
                // we calculate a candidate for mul
                mul = (1L << shift) / div + 1;

                // assume it is a good one
                found = true;

                // test if it works for the range 0 .. max
                // Note: takes too much time for large values of max. 
                if (max < 1000000)
                {
	                for (long i = max; i >=1; i--)		// testing large values first fails faster 
	                {
	                	if ((i / div) != ((i * mul) >> shift))
	                    {
	                        found = false;
	                        break;
	                    }
	                }
                }
                else
                {
					// very fast test, no mathematical proof yet but it seems to work well
					// test highest number-pair for which the division must 'jump' correctly
					// test max, to be sure;
					long t = (max/div +1) * div;
					if ((((t-1) / div) != (((t-1) * mul) >> shift)) ||
					    ((t / div) != ((t * mul) >> shift)) ||
					    ((max / div) != ((max * mul) >> shift))
					   )
	                {
	                	found = false;
	                }
                }

                // are we ready?
                if (found)
                {
                	break;
                }
            }
            return found;
        }

    }
    
    #endregion
}
