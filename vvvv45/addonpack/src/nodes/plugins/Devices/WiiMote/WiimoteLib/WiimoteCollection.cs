//////////////////////////////////////////////////////////////////////////////////
//	WiimoteCollection.cs
//	Managed Wiimote Library
//	Written by Brian Peek (http://www.brianpeek.com/)
//	for MSDN's Coding4Fun (http://msdn.microsoft.com/coding4fun/)
//	Visit http://blogs.msdn.com/coding4fun/archive/2007/03/14/1879033.aspx
//	for more information
//////////////////////////////////////////////////////////////////////////////////

using System.Collections.ObjectModel;

namespace WiimoteLib
{
	/// <summary>
	/// Used to manage multiple Wiimotes
	/// </summary>
	public class WiimoteCollection : Collection<Wiimote>
	{
		/// <summary>
		/// Finds all Wiimotes connected to the system and adds them to the collection
		/// </summary>
		public void FindAllWiimotes()
		{
			Wiimote.FindWiimote(WiimoteFound);
		}

		private bool WiimoteFound(string devicePath)
		{
			this.Add(new Wiimote(devicePath));
			return true;
		}
	}
}
