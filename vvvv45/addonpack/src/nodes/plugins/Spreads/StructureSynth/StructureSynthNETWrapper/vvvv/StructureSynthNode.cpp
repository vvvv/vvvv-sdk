#include "stdafx.h"
#include "StructureSynthNode.h"
#include "../StructureSynth/Parser/Tokenizer.h"
#include "../StructureSynth/Parser/Preprocessor.h"
#include "../StructureSynth/Parser/EisenParser.h"
#include "../StructureSynth/Model/Builder.h"
#include "../StructureSynth/Model/Ruleset.h"
#include "../StructureSynth/Model/Rendering/ListRenderer.h"
#include "../StructureSynth/Model/RandomStreams.h"

#include <QString>
#include <QClipBoard>
#include <QApplication>

using namespace System;
using namespace VVVV::PluginInterfaces::V1;
using namespace VVVV::Utils::VMath;
using namespace VVVV::Utils::VColor;
using namespace StructureSynth::Parser;
using namespace StructureSynth::Model;
using namespace StructureSynth::Model::Rendering;
using namespace System::Runtime::InteropServices;

namespace MyNodes
{
	void StructureSynthNode::SetPluginHost(IPluginHost ^ Host) 
	{
		this->FHost = Host;
		this->FHost->CreateStringInput("Input",TSliceMode::Single,TPinVisibility::True,this->vInFormula);
		this->vInFormula->SetSubType("{ x 6} sphere",false);

		this->FHost ->CreateValueInput("Random Seed",1,nullptr,TSliceMode::Single,TPinVisibility::True,this->vInSeed);
		this->vInSeed->SetSubType(1,Double::MaxValue,0.01,1,false,false,true);

		this->FHost->CreateTransformOutput("Sphere",TSliceMode::Dynamic,TPinVisibility::True,this->vOutSphere);
		this->FHost->CreateColorOutput("Sphere Color",TSliceMode::Dynamic,TPinVisibility::True,this->vOutSphereColor);

		this->FHost->CreateTransformOutput("Box",TSliceMode::Dynamic,TPinVisibility::True,this->vOutBoxes);
		this->FHost->CreateColorOutput("Box Color",TSliceMode::Dynamic,TPinVisibility::True,this->vOutBoxesColor);

		this->FHost->CreateTransformOutput("Grid",TSliceMode::Dynamic,TPinVisibility::True,this->vOutGrid);
		this->FHost->CreateColorOutput("Grid Color",TSliceMode::Dynamic,TPinVisibility::True,this->vOutGridColor);

		this->FHost->CreateValueOutput("Lines",3,nullptr,TSliceMode::Dynamic,TPinVisibility::True,this->vOutLines);
		this->FHost->CreateColorOutput("Lines Color",TSliceMode::Dynamic,TPinVisibility::True,this->vOutLinesColor);

		this->FHost->CreateValueOutput("Triangle Positions",3,nullptr,TSliceMode::Dynamic,TPinVisibility::True,this->vOutPositions);
		//this->FHost->CreateValueOutput("Grid Color",TSliceMode::Dynamic,TPinVisibility::True,this->vOutGridColor);

		
		this->FHost->CreateValueOutput("Points",3,nullptr,TSliceMode::Dynamic,TPinVisibility::True,this->vOutPoints);
		this->FHost->CreateColorOutput("Points Color",TSliceMode::Dynamic,TPinVisibility::True,this->vOutPointsColor);

		this->FHost->CreateStringOutput("Message",TSliceMode::Single,TPinVisibility::True,this->vOutMessage);

		this->vOutSphere->Order = 0;
		this->vOutSphereColor->Order = 1;
		this->vOutBoxes->Order = 2;
		this->vOutBoxesColor->Order = 3;
		this->vOutGrid->Order = 4;
		this->vOutGridColor->Order = 5;
		this->vOutLines->Order = 6;
		this->vOutLinesColor->Order = 7;
		this->vOutPositions->Order = 8;
		this->vOutPoints->Order = 9;
		this->vOutPointsColor->Order = 10;
		this->vOutMessage->Order = 11;
	}

	void StructureSynthNode::Configurate(IPluginConfig^ Input) 
	{
		
	}

	void StructureSynthNode::Evaluate(int SpreadMax) 
	{
		if (this->vInFormula->PinIsChanged || this->vInSeed->PinIsChanged) 
		{
			String^ input;
			double seed;
			this->vInFormula->GetString(0,input);
			this->vInSeed->GetValue(0,seed);

			char* chrinput = (char*)(void*)Marshal::StringToHGlobalAnsi(input);
			QString qinput(chrinput);

			//srand(Convert::ToInt32(seed));
			RandomStreams::SetSeed(Convert::ToInt32(seed));
			
			try 
			{
				
				ListRenderer rendering;
				
				rendering.begin();
				//Tokenizer tok(Preprocessor::Process(qinput));
				Preprocessor* p = new Preprocessor();
				//p->Process(qinput);
				Tokenizer tok(p->Process(qinput));

				EisenParser e(&tok);
				RuleSet* rs = e.parseRuleset();
				rs->resolveNames();
				rs->dumpInfo();
				Builder b(&rendering, rs);
				
				b.build();
				rendering.end();

				int spherecount = rendering.spheres_center.count();
				this->vOutSphere->SliceCount = spherecount;
				this->vOutSphereColor->SliceCount = spherecount;
				for (int i = 0; i<spherecount;i++) 
				{
					SyntopiaCore::Math::Vector3f t = rendering.spheres_center.at(i);
					float s = rendering.spheres_radius.at(i) * 2;
					//VMath::

					Matrix4x4 mat = VMath::Translate(t.x(),t.y(),t.z()) * VMath::Scale(s,s,s);

					this->vOutSphere->SetMatrix(i,mat);

					VRGBAColor color = rendering.spheres_color.at(i);
					this->vOutSphereColor->SetColor(i,RGBAColor(color.r,color.g,color.b,color.a));
				}

				int boxcount = rendering.boxes.count();
				this->vOutBoxes->SliceCount = boxcount;
				this->vOutBoxesColor->SliceCount = boxcount;
				for (int i = 0; i<boxcount;i++) 
				{
					SyntopiaCore::Extras::VBox box = rendering.boxes.at(i);
					//float centerx = (box.dir1.x()/2.0f)+0.5f-box.dir1.x();
					//float centerz = (box.dir1.z()/2.0f)+0.5f-box.dir3.z();
					Matrix4x4 mat = Matrix4x4(box.dir1.x(),box.dir1.y(),box.dir1.z(),0.0,box.dir2.x(),box.dir2.y(),box.dir2.z(),0.0,box.dir3.x(),box.dir3.y(),box.dir3.z(),0.0,box.base.x(),box.base.y(),box.base.z(),1.0);
					//Matrix4x4 mat = Matrix4x4(box.dir1.x(),box.dir2.x(),box.dir3.x(),box.base.x(),box.dir1.y(),box.dir2.y(),box.dir3.y(),box.base.y(),box.dir1.z(),box.dir2.z(),box.dir3.z(),box.base.z(),0.0,0.0,0.0,1.0);
					this->vOutBoxes->SetMatrix(i,mat);
					VRGBAColor color = rendering.boxes_color.at(i);
					this->vOutBoxesColor->SetColor(i,RGBAColor(color.r,color.g,color.b,color.a));
				}


				int gridcount = rendering.grids.count();
				this->vOutGrid->SliceCount = gridcount;
				this->vOutGridColor->SliceCount = gridcount;
				for (int i = 0; i<gridcount;i++) 
				{

					SyntopiaCore::Extras::VBox box = rendering.grids.at(i);
					Matrix4x4 mat = Matrix4x4(box.dir1.x(),box.dir1.y(),box.dir1.z(),0.0,box.dir2.x(),box.dir2.y(),box.dir2.z(),0.0,box.dir3.x(),box.dir3.y(),box.dir3.z(),0.0,box.base.x(),box.base.y(),box.base.z(),1.0);
					//Matrix4x4 mat = Matrix4x4(box.dir1.x(),box.dir2.x(),box.dir3.x(),box.base.x(),box.dir1.y(),box.dir2.y(),box.dir3.y(),box.base.y(),box.dir1.z(),box.dir2.z(),box.dir3.z(),box.base.z(),0.0,0.0,0.0,1.0);
					this->vOutGrid->SetMatrix(i,mat);
					VRGBAColor color = rendering.grids_color.at(i);
					this->vOutGridColor->SetColor(i,RGBAColor(color.r,color.g,color.b,color.a));
				}

				int linecount = rendering.lines.count();
				this->vOutLines->SliceCount = linecount * 2;
				this->vOutLinesColor->SliceCount = linecount;
				for (int i = 0; i < linecount;i++) 
				{
					VLine line = rendering.lines.at(i);
					VRGBAColor color = rendering.lines_color.at(i);

					this->vOutLines->SetValue3D(i*2,line.v1.x(),line.v1.y(),line.v1.z());
					this->vOutLines->SetValue3D(i*2+1,line.v2.x(),line.v2.y(),line.v2.z());

					this->vOutLinesColor->SetColor(i,RGBAColor(color.r,color.g,color.b,color.a));
				}

				int tricount = rendering.triangles.count();
				this->vOutPositions->SliceCount = tricount * 3;
				for (int i = 0; i < tricount;i++) 
				{
					VTriangle tri = rendering.triangles.at(i);
					//VRGBAColor color = rendering.lines_color.at(i);

					this->vOutPositions->SetValue3D(i*3,tri.v1.x(),tri.v1.y(),tri.v1.z());
					this->vOutPositions->SetValue3D(i*3+1,tri.v2.x(),tri.v2.y(),tri.v2.z());
					this->vOutPositions->SetValue3D(i*3+2,tri.v3.x(),tri.v3.y(),tri.v3.z());
				}

				int ptcount = rendering.points.count();
				this->vOutPoints->SliceCount = ptcount;
				this->vOutPointsColor->SliceCount = ptcount;
				for (int i = 0; i < ptcount;i++) 
				{
					Vector3f tri = rendering.points.at(i);
					VRGBAColor color = rendering.points_color.at(i);

					this->vOutPoints->SetValue3D(i,tri.x(),tri.y(),tri.z());
					this->vOutPointsColor->SetColor(i,RGBAColor(color.r,color.g,color.b,color.a));
				}

				delete p;

				this->vOutMessage->SetString(0,"OK");
			} 
			catch (ParseError error) 
			{
				this->vOutMessage->SetString(0,"Error");
			}
			


		}
	}	
}