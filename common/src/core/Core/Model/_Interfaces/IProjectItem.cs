using System;

namespace VVVV.Core.Model
{
    /// <summary>
    /// A IProjectItem belongs to a IProject.
    /// </summary>
    public interface IProjectItem : IDisposable
    {
        /// <summary>
        /// The IProject this IProjectItem belongs to. This property is set
        /// by IProject after a project item has been added to it.
        /// </summary>
        IProject Project
        {
            get;
            set;
        }
        
        bool CanBeCompiled
        {
        	get;
        }

        event EventHandler Disposed;
    }
}
