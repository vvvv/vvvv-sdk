using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    /// <summary>
    /// The IO registry is the central place to register new io classes,
    /// like spreads or streams. It's instantiated only once on startup
    /// of vvvv and is used by IO factories to do their work. IO factories
    /// are instantiated per plugin.
    /// </summary>
    [ComVisible(false)]
	public interface IIORegistry
	{
	    /// <summary>
	    /// Register a new IO registry which will be used by IO factories
	    /// to lookup and create custom IO classes.
	    /// </summary>
		void Register(IIORegistry registry, bool first);
		
		/// <summary>
		/// Whether or not this registry can create the IO class as described
		/// by the IO context.
		/// </summary>
		bool CanCreate(IOBuildContext context);
		
		/// <summary>
		/// Creates a new IO container as described by the build context.
		/// </summary>
		IIOContainer CreateIOContainer(IIOFactory factory, IOBuildContext context);
	}
}
