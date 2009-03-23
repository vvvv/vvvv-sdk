#include "../Lib/CVSeqBuilder.h"

#pragma once

using namespace System;
using namespace VVVV::PluginInterfaces::V1;

namespace VVVV {
	namespace Nodes {
	ref class FitEllipseNode  :IPlugin
	{
	public:
		FitEllipseNode(void);

		virtual void SetPluginHost(IPluginHost^ Host);
		virtual void Configurate(IPluginConfig^ Input);
		virtual void Evaluate(int SpreadMax);
			
		virtual property bool AutoEvaluate { bool get() { return false; } }

		static property IPluginInfo^ PluginInfo 
			{
				IPluginInfo^ get() 
				{
					//IPluginInfo^ Info;
					IPluginInfo^ Info = gcnew VVVV::PluginInterfaces::V1::PluginInfo();
					Info->Name = "FitEllipse";
					Info->Category = "2d";
					Info->Version = "OpenCV";
					Info->Help = "Ellipse that fits best a Point Spread";
					Info->Bugs = "";
					Info->Credits = "";
					Info->Warnings = "";
					Info->Author = "vux";
					Info->Tags="Geometry,Analysis";

					//leave below as is
					System::Diagnostics::StackTrace^ st = gcnew System::Diagnostics::StackTrace(true);
					System::Diagnostics::StackFrame^ sf = st->GetFrame(0);
					System::Reflection::MethodBase^ method = sf->GetMethod();
					Info->Namespace = method->DeclaringType->Namespace;
					Info->Class = method->DeclaringType->Name;
					return Info;
				}
			}


	private:
		IPluginHost^ FHost;
		IValueIn^ vInPoints;
		IValueIn^ vInBinSizes;
		
		IValueOut^ vOutCenter;
		IValueOut^ vOutSize;
		IValueOut^ vOutAngle;

		CVSeqBuilder* seqbuilder;

	};

	}
}
