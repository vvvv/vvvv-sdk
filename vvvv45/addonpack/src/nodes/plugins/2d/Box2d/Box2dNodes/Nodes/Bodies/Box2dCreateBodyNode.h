#pragma once
#include "../../DataTypes/WorldDataType.h"
#include "../../DataTypes/Shapes/ShapeDefDataType.h"
#include "../../DataTypes/BodyDataType.h"

using namespace VVVV::DataTypes;

namespace VVVV 
{
	namespace Nodes 
	{
		ref class Box2dCreateBodyNode : IPlugin,IPluginConnections
		{
		public:
			Box2dCreateBodyNode(void);

			static property IPluginInfo^ PluginInfo 
				{
					IPluginInfo^ get() 
					{
						//IPluginInfo^ Info;
						IPluginInfo^ Info = gcnew VVVV::PluginInterfaces::V1::PluginInfo();
						Info->Name = "CreateBody";
						Info->Category = "Box2d";
						Info->Version = "";
						Info->Help = "Creates box2d body";
						Info->Bugs = "";
						Info->Credits = "Box2d";
						Info->Warnings = "";
						Info->Author = "vux";
						Info->Tags="Physics,2d,Collision";

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
			virtual void ConnectPin(IPluginIO^ Pin);
			virtual void DisconnectPin(IPluginIO^ Pin);
			
			virtual property bool AutoEvaluate 
			{
				bool get() { return true; }
			}

		private:
			IPluginHost^ FHost;

			INodeIn^ vInWorld;
			INodeIn^ vInShapes;

			IValueIn^ vInPosition;
			IValueIn^ vInAngle;
			IValueIn^ vInVelocity;
			IValueIn^ vInAngularVelocity;
			IValueIn^ vInLinearDamping;
			IValueIn^ vInAngularDamping;
			IValueIn^ vInIsBullet;
			IValueIn^ vInFixedRotation;
			IValueIn^ vInShapeCount;
			IStringIn^ vInCustom;
			IValueIn^ vInDoCreate;

			WorldDataType^ mWorld;
			ShapeDefDataType^ mShapes;

			BodyDataType^ mBodies;
			//std::vector<b2Body*>* createdbodies;

			IValueOut^ vOutCanCreate;
			INodeOut^ vOutBodies;

		};
	}
}
