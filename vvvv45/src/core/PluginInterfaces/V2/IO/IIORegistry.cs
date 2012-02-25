using System;

namespace VVVV.PluginInterfaces.V2
{
	public interface IIORegistry
	{
		void RegisterInput(Type ioType, Func<IIOFactory, InputAttribute, Type, IOHandler> createInputFunc);
		void RegisterOutput(Type ioType, Func<IIOFactory, OutputAttribute, Type, IOHandler> createOutputFunc);	
		void RegisterConfig(Type ioType, Func<IIOFactory, ConfigAttribute, Type, IOHandler> createConfigFunc);
	}
}
