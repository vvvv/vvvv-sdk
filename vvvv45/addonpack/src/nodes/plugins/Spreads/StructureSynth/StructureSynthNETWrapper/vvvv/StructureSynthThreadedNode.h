#pragma once

using namespace System;
using namespace VVVV::PluginInterfaces::V1;

namespace VVVV 
{
	namespace Nodes
	{
		public ref class StructureSynthThreadedNode
		{
		private:
			IPluginHost^ FHost;

			IStringIn^ vInFormula;
			IValueIn^ vInSeed;


			ITransformOut^ vOutSphere;
			IColorOut^ vOutSphereColor;

			ITransformOut^ vOutBoxes;
			IColorOut^ vOutBoxesColor;

			ITransformOut^ vOutGrid;
			IColorOut^ vOutGridColor;

			IValueOut^ vOutLines;
			IColorOut^ vOutLinesColor;

			IValueOut^ vOutPositions;

			IValueOut^ vOutProcessing;
			IValueOut^ vOutCompleted;
			IStringOut^ vOutMessage;
		public:
			StructureSynthThreadedNode(void);

			static property IPluginInfo^ PluginInfo 
			{
				IPluginInfo^ get() 
				{
					//IPluginInfo^ Info;
					IPluginInfo^ Info = gcnew VVVV::PluginInterfaces::V1::PluginInfo();
					Info->Name = "StructureSynth";
					Info->Category = "Spreads";
					Info->Version = "Threaded";
					Info->Help = "Structure Synth renderer, threaded version";
					Info->Bugs = "";
					Info->Credits = "";
					Info->Warnings = "";
					Info->Author = "vux";
					Info->Tags="LSystem,Generation";

					//leave below as is
					System::Diagnostics::StackTrace^ st = gcnew System::Diagnostics::StackTrace(true);
					System::Diagnostics::StackFrame^ sf = st->GetFrame(0);
					System::Reflection::MethodBase^ method = sf->GetMethod();
					Info->Namespace = method->DeclaringType->Namespace;
					Info->Class = method->DeclaringType->Name;
					return Info;
				}
			}

			virtual void SetPluginHost(IPluginHost^ Host);
			virtual void Configurate(IPluginConfig^ Input);
			virtual void Evaluate(int SpreadMax);
			
			virtual property bool AutoEvaluate {
				bool get() { return false; }
			}
		};
	}
}