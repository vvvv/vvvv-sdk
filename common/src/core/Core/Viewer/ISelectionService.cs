using System;

namespace VVVV.Core.Viewer
{
    public class SelectionProviderChangedEventArgs : EventArgs
    {
        public SelectionProviderChangedEventArgs(ISelectionProvider provider)
        {
            SelectionProvider = provider;
        }
        
        public ISelectionProvider SelectionProvider
        {
            get;
            private set;
        }
    }
    
    public delegate void SelectionProviderChangedEventHandler(object sender, SelectionProviderChangedEventArgs args);
    
    public interface ISelectionService
    {
        /// <summary>
        /// The current selection provider.
        /// </summary>
        ISelectionProvider SelectionProvider
        {
            get;
            set;
        }
        
        /// <summary>
        /// Fired if the current selection provider changed.
        /// </summary>
        event SelectionProviderChangedEventHandler SelectionProviderChanged;
    }
}
