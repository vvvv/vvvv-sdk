using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

/// <summary>
/// Contains helper tools for multithreaded developement.
/// </summary>
namespace VVVV.Utils.Concurrent
{
	/// <summary>
	/// Implementation of BlockingQueue based on
	/// http://stackoverflow.com/questions/530211/creating-a-blocking-queuet-in-net
	/// </summary>
	[ComVisible(false)]
	public class BlockingQueue<T>
	{
	    private readonly Queue<T> FQueue = new Queue<T>();
	    private bool FClosing;
	    
	    /// <summary>
	    /// Puts an item in the queue.
	    /// </summary>
	    public void Enqueue(T item)
	    {
	        lock (FQueue)
	        {
	            FQueue.Enqueue(item);
                // Wake up any blocked takers.
                Monitor.PulseAll(FQueue);
	        }
	    }
	    
	    /// <summary>
	    /// Takes an item from the queue. Blocks if no item is available.
	    /// Returns false if queue was closed.
	    /// </summary>
	    public bool TryDequeue(out T value)
	    {
	        lock (FQueue)
	        {
	        	// Block as long as there's no item available.
	            while (FQueue.Count == 0)
	            {
	            	if (FClosing)
	            	{
	            		value = default(T);
	            		return false;
	            	}
	            	Monitor.Wait(FQueue);
	            }
	            
	            value = FQueue.Dequeue();
	            return true;
	        }
	    }
	    
	    /// <summary>
	    /// Wakes up all pending dequeues.
	    /// </summary>
	    public void Close()
	    {
	    	lock(FQueue)
	    	{
	    		FClosing = true;
	    		Monitor.PulseAll(FQueue);
	    	}
	    }
	    
	    
	    /// <summary>
	    /// Gets the number of elements contained in the queue.
	    /// </summary>
	    public int Count
	    {
	    	get
	    	{
	    		lock(FQueue)
	    		{
	    			return FQueue.Count;
	    		}
	    	}
	    }
	}
}
