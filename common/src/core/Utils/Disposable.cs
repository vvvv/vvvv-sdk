
using System;

namespace VVVV.Utils
{
	/// <summary>
	/// Implementation of IDisposable. Override the methods DisposeManaged or DisposeUnmanaged.
	/// </summary>
	public abstract class Disposable: IDisposable
	{
		private bool FDisposed = false;
		public bool Disposed 
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
	   		if (!Disposed)
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
	   		Disposed = true;
		}
	   	
	   	protected virtual void DisposeManaged()
	   	{
	   		
	   	}
	   	
	   	protected virtual void DisposeUnmanaged()
	   	{
	   		
	   	}
	
		// Use C# destructor syntax for finalization code.
		~Disposable()
		{
	  		// Simply call Dispose(false).
	  		Dispose(false);
		}
	}
}
