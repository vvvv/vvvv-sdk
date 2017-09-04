#pragma once

using namespace System;
using namespace System::Threading;
using namespace VVVV::PluginInterfaces::V1;

namespace VVVV 
{
	namespace Nodes
	{
		public ref class StructureSynthRenderThread
		{
		public:
			StructureSynthRenderThread(String^ script,int seed);
			void Start();
			void Stop();
			bool IsRunning() { return isrunning; }
		private:
			void Run();

			bool isrunning;
			Thread^ m_thread;
			String^ script;
			int seed;
		};
	}
}



