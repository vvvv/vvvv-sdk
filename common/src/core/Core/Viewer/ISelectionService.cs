using System;

namespace VVVV.Core.Viewer
{
    /// <summary>
    /// Gives clients the ability to get or set the current selection provider
    /// of the application and get notified if the selection provider of the
    /// application changed.
    /// </summary>
    public interface ISelectionService
    {
        /// <summary>
        /// The current selection provider.
        /// </summary>
        ISelectionProvider SelectionProvider { get; set; }
        
        /// <summary>
        /// Raised if the current selection provider changed.
        /// </summary>
        event EventHandler<SelectionProviderChangedEventArgs> SelectionProviderChanged;
    }

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
}
