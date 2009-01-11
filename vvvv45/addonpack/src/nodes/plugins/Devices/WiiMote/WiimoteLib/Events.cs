//////////////////////////////////////////////////////////////////////////////////
//	Events.cs
//	Managed Wiimote Library
//	Written by Brian Peek (http://www.brianpeek.com/)
//	for MSDN's Coding4Fun (http://msdn.microsoft.com/coding4fun/)
//	Visit http://blogs.msdn.com/coding4fun/archive/2007/03/14/1879033.aspx
//  and http://www.codeplex.com/WiimoteLib
//	for more information
//////////////////////////////////////////////////////////////////////////////////

using System;

namespace WiimoteLib
{
	/// <summary>
	/// Argument sent through the WiimoteExtensionChangedEvent
	/// </summary>
	public class WiimoteExtensionChangedEventArgs: EventArgs
	{
		/// <summary>
		/// The extenstion type inserted or removed
		/// </summary>
		public ExtensionType ExtensionType;
		/// <summary>
		/// Whether the extension was inserted or removed
		/// </summary>
		public bool Inserted;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="type">The extension type inserted or removed</param>
		/// <param name="inserted">Whether the extension was inserted or removed</param>
		public WiimoteExtensionChangedEventArgs(ExtensionType type, bool inserted)
		{
			ExtensionType = type;
			Inserted = inserted;
		}
	}

	/// <summary>
	/// Argument sent through the WiimoteChangedEvent
	/// </summary>
	public class WiimoteChangedEventArgs: EventArgs
	{
		/// <summary>
		/// The current state of the Wiimote and extension controllers
		/// </summary>
		public WiimoteState WiimoteState;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ws">Wiimote state</param>
		public WiimoteChangedEventArgs(WiimoteState ws)
		{
			WiimoteState = ws;
		}
	}
}
