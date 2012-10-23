using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core.Viewer
{
    /// <summary>
    /// Base interface for all viewers.
    /// Allows a consumer to set and retrieve the model displayed by this viewer.
    /// </summary>
    public interface IViewer
    {
        /// <summary>
        /// Gets or sets the mapping registry used by this viewer.
        /// </summary>
        /// <remarks>
        /// A registry is usually set only once after calling the constructor of a viewer.
        /// It is used by the viewer to setup a model mapper in order to retrieve the view
        /// layer for the set model object.
        /// </remarks>
        MappingRegistry Registry { get; set; }

        /// <summary>
        /// Gets or sets the model displayed by this viewer.
        /// </summary>
        object Model { get; set; }
    }
}
