using System;

namespace VVVV.Core.Model
{
	/// <summary>
	/// All data model objects should derive from this class if they should
	/// be marshaled by reference in .NET Remoting.
	/// This class acts as layer for possible future changes and additions,
	/// which could affect all model objects.
	/// 
	/// This class also implements IDisposable and provides subclasses with
	/// the methods DisposeManaged and DisposeUnmanaged to override.
	/// </summary>
    public abstract class RemotableObject : IDisposable //MarshalByRefObject, IDisposable
	{
		private bool FDisposed = false;
		public bool IsDisposed 
		{
			get 
			{
				return FDisposed; 
			}
			private set
			{
				FDisposed = value;
			}
		}

	   	public void Dispose() 
	   	{
	     	Dispose(true);
	     	GC.SuppressFinalize(this); 
	   	}
	
	   	protected void Dispose(bool disposing) 
	   	{
	   		if (!IsDisposed)
	   		{
		      	if (disposing) 
		      	{
		         	// Free other state (managed objects).
		         	DisposeManaged();
		      	}
		  		// Free your own state (unmanaged objects).
		  		// Set large fields to null.
		  		DisposeUnmanaged();
	   		}
	   		IsDisposed = true;
		}
	   	
	   	/// <summary>
	   	/// Dispose managed resources. 
	   	/// If overwritten make sure to call base.DisposeManaged().
	   	/// </summary>
	   	protected virtual void DisposeManaged()
	   	{
	   		
	   	}
	   	
	   	/// <summary>
	   	/// Dispose unmanaged resources. 
	   	/// If overwritten make sure to call base.DisposeUnmanaged().
	   	/// </summary>
	   	protected virtual void DisposeUnmanaged()
	   	{
	   		
	   	}
	
		// Use C# destructor syntax for finalization code.
		~RemotableObject()
		{
	  		// Simply call Dispose(false).
	  		Dispose(false);
		}
	}
}
