using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V1
{
	/// <summary>
	/// Allows the WindowSwitcher to communicate back to the host
	/// </summary>
	[Guid("41CC97F3-106E-4DC9-AA74-E50C0B5694DD"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWindowSwitcher
	{
		/// <summary>
		/// Called by the WindowSwitcherHost to hand itself over to the WindowSwitcher.
		/// </summary>
		/// <param name="Host">Interface to the WindowSwitcherHost.</param>
		void SetWindowSwitcherHost(IWindowSwitcherHost host);
		void Initialize(IWindow currentWindow, out int width, out int height);
		void Up();
		void Down();
	}
	
	/// <summary>
	/// Allows the WindowSwitcher to communicate back to the host
	/// </summary>
	[Guid("A14BBFDE-9B91-430B-B098-FD8E2DC7D60B"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWindowSwitcherHost
	{
		void HideMe(IWindow window);
	}	
}
