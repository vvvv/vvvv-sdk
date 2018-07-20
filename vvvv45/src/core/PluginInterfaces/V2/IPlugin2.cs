using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;

using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.Core.Model;

using VVVV.PluginInterfaces.V1;

/// <summary>
/// Version 2 of the VVVV PluginInterface.
///
/// To convert this to a typelib make sure AssemblyInfo.cs states: ComVisible(true).
/// Then on a commandline type:
/// <c>regasm _PluginInterfaces.dll /tlb</c>
/// This generates and registers the typelib which can then be imported e.g. via Delphi:Components:Import Component:Import Typelib
/// </summary>
namespace VVVV.PluginInterfaces.V2
{
	#region basic interfaces
	/// <summary>
	/// Basic interface to provide a plugin with an Evaluate function.
	/// </summary>
	[Guid("5BDEF445-5734-427C-BD9C-A69809277799"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginEvaluate: IPluginBase
	{
		/// <summary>
		/// Called by the PluginHost once per frame. This is where the plugin calculates and sets the SliceCounts and Values
		/// of its outputs depending on the values of its current inputs.
		/// </summary>
		/// <param name="SpreadMax">The maximum SliceCount of all of the plugins inputs, which would typically be used
		/// to adjust the SliceCounts of all outputs accordingly.</param>
		void Evaluate(int SpreadMax);
	}
	
	/// <summary>
	/// Optional interface to provide a plugin with the possibility to prohibit its own deletion.
	/// </summary>
	[Guid("E66B6358-4850-41D3-B4F1-1C09F0557DD0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IQueryDelete
	{
		/// <summary>
		/// Called by the PluginHost when the plugin is about to be deleted. The plugin can return FALSE to prohibit the deletion. 
		/// </summary>
		bool DeleteMe();
	}

    [Guid("C0EB9C47-DB39-4DC6-9B6D-032348E6F7EA"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IVLHost
    {
        void CloseActiveTab(out bool windowIsGone);
        bool OpenDocument(string filename);
    }

    /// <summary>
    /// Implement this interface on your gui-plugin if the Mouse (System Window)
    /// or Keyboard (System Window) nodes should output data for it.
    /// </summary>
    [Guid("E8C47417-6146-472B-BCE5-A9550AA30C3A"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUserInputWindow
    {
        IntPtr InputWindowHandle { get; }
    }
    
    /// <summary>
    /// Implement this interface on your gui-plugin to specify an initial background color.
    /// </summary>
    [Guid("419E642A-8779-46F8-8175-A42DB7DCA539"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBackgroundColor
    {
        RGBAColor BackgroundColor { get; }
    }

    /// <summary>
    /// Implement this interface on your gui plugin if this has a notion of a projection space.
    /// You can use the aspect ratio of your window or viewport to do the math or have an explicit aspect ratio transform input.
    /// </summary>
    [Guid("9FFDA595-CB41-4B60-9132-2E907A777D94"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IProjectionSpace
    {
        /// <summary>
        /// Transforms a position in pixels into 
        /// * a position in normalized projection space ((bottom, left = -1, -1) .. (top, right = 1, 1)) 
        /// * a position in projection space (typically one that respects the aspect ratio of the window)
        /// so you might need to "undo" the last 2 or 3 transformations in that chain:
        ///             World T.          View T.          Proj T.          AspectR. T.           Crop T.        Viewport Placement
        /// Object Space  -->  World Space  -->  View Space  -->  PROJ SPACE  -->  NORM PROJ SPACE  -->  Viewport Space  -->  Pixel Space
        /// </summary>
        void MapFromPixels(Point inPixels, out Vector2D inNormalizedProjection, out Vector2D inProjection);
    }

    /// <summary>
    /// Implement this interface on your gui plugin if you a notion of a projection space.
    /// You can use the aspect ratio of your window to do the math or have an explicit aspect ratio transform input.
    /// </summary>
    [Guid("60BC056A-518B-4015-B0DF-4C2E8A0FA04A"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IProjectionSpace2
    {
        /// <summary>
        /// Transforms a position in pixels into 
        /// * a position in normalized projection space ((bottom, left = -1, -1) .. (top, right = 1, 1)) 
        /// * a position in projection space (typically one that respects the aspect ratio of the window)
        /// so you might need to "undo" the last 2 or 3 transformations in that chain:         
        ///             World T.          View T.          Proj T.          AspectR. T.           Crop T.        Viewport Placement
        /// Object Space  -->  World Space  -->  View Space  -->  PROJ SPACE  -->  NORM PROJ SPACE  -->  Viewport Space  -->  Pixel Space
        /// </summary>
        void MapFromPixels(int xInPix, int yInPix,
            out double xInNormalizedProjection, out double yInNormalizedProjection, out double xInProjection, out double yInProjection);
    }
    #endregion basic interfaces
}
