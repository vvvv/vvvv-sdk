using System;
using System.Collections;

namespace VVVV.Core.Viewer
{
    public class SelectionChangedEventArgs : EventArgs
    {
        public SelectionChangedEventArgs(ISelection selection)
        {
            Selection = selection;
        }
        
        public ISelection Selection
        {
            get;
            private set;
        }
    }
    
    public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs args);
    
    public interface ISelectionProvider
    {
        /// <summary>
        /// The current selection.
        /// </summary>
        ISelection CurrentSelection
        {
            get;
        }
        
        /// <summary>
        /// Fired if current selection changed.
        /// </summary>
        event SelectionChangedEventHandler SelectionChanged;
    }
}
