using System;

namespace VVVV.Core.Model
{
    /// <summary>
    /// A class implementing IPersistent can be loaded from and saved to
    /// and arbitrary location, thus making it persistent.
    /// The location is defined with the Location property.
    /// </summary>
    /// <remarks>
    /// A persistent object can be in four different states:
    /// (1) Not loaded and not dirty
    /// (2) Not loaded and dirty
    /// (3) Loaded and not dirty
    /// (4) Loaded and dirty
    /// </remarks>
    public interface IPersistent : IDisposable
    {
        /// <summary>
        /// The Location this object should be loaded from or saved to.
        /// </summary>
        Uri Location
        {
            get;
        }
        
        /// <summary>
        /// Determines if this object needs to be saved.
        /// </summary>
        bool IsDirty 
        {
            get;
        }
        
        /// <summary>
        /// Determines if this object is loaded into memory.
        /// </summary>
        bool IsLoaded
        {
        	get;
        }
        
        /// <summary>
        /// Determines if this object is read only. If true the Save method
        /// should fail.
        /// </summary>
        bool IsReadOnly
        {
        	get;
        	set;
        }
        
        /// <summary>
        /// Loads this object from Location to memory.
        /// </summary>
        /// <remarks>
        /// After this call the object should be in state (3).
        /// </remarks>
        void Load();
        
        /// <summary>
        /// Unloads this object.
        /// </summary>
        /// <remarks>
        /// After this call the object should be in state (1).
        /// </remarks>
        void Unload();
        
        /// <summary>
        /// The Loaded event occurs if this object was loaded from Location into memory.
        /// </summary>
        event EventHandler Loaded;
        
        /// <summary>
        /// The Unloaded event occurs if this object was unloaded.
        /// </summary>
        event EventHandler Unloaded;
        
        /// <summary>
        /// Saves this object from memory to Location.
        /// </summary>
        /// <remarks>
        /// After this call the object should be in state (3).
        /// </remarks>
        void Save();
        
        /// <summary>
        /// The Saved event occurs if this object was saved from memory to Location.
        /// </summary>
        event EventHandler Saved;
        
        /// <summary>
        /// Saves this object to a new location.
        /// </summary>
        /// <param name="newLocation">The new location to save this document to.</param>
        void SaveTo(Uri newLocation);
        
        /// <summary>
        /// The Disposed event occurs after Dispose() has been called on this object.
        /// </summary>
        event EventHandler Disposed;
    }
    
    public static class PersistentExtensionMethods
    {
    	/// <summary>
    	/// Returns the relative path from the specified persistent to this IPersistent.
    	/// Example: Foo\Bar\ThisDocument.txt
    	/// </summary>
    	public static string GetRelativePath(this IPersistent peristent1, IPersistent persistent2)
        {
    		var persistent2Dir = persistent2.Location.GetLocalDir() + "\\";
    		var relativePath = new Uri(persistent2Dir).MakeRelativeUri(peristent1.Location).ToString();
            return Uri.UnescapeDataString(relativePath).Replace('/', '\\');
        }
    }
}
