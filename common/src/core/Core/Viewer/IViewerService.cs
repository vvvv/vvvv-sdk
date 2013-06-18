using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core.Viewer
{
    /// <summary>
    /// Provides the ability to get or set the active viewer of the application
    /// and get notified if the active viewer of the application changed.
    /// </summary>
    public interface IViewerService
    {
        /// <summary>
        /// Gets or sets the active viewer of the application.
        /// </summary>
        IViewer ActiveViewer { get; set; }

        /// <summary>
        /// Raised if the active viewer of the application changed.
        /// </summary>
        event EventHandler<ActiveViewerChangedEventArgs> ActiveViewerChanged;
    }

    public class ActiveViewerChangedEventArgs : EventArgs
    {
        public ActiveViewerChangedEventArgs(IViewer activeViewer)
        {
            ActiveViewer = activeViewer;
        }

        public IViewer ActiveViewer { get; private set; }
    }
}
