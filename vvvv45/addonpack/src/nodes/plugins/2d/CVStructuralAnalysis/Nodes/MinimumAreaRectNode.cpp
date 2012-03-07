#include "StdAfx.h"
#include "MinimumAreaRectNode.h"
#include "../Lib/BinSizeUtils.h"
#include "../Lib/CVSeqBuilder.h"
#include "cv.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace VVVV::Utils;

namespace VVVV 
{
	namespace Nodes 
	{
	
		
		MinimumAreaRectNode::MinimumAreaRectNode(void) 
		{
			this->seqbuilder = new CVSeqBuilder();
		}

		void MinimumAreaRectNode::SetPluginHost(IPluginHost ^ Host) 
		{
			array<String ^> ^ arr1d = gcnew array<String ^>(1);
			array<String ^> ^ arr2d = gcnew array<String ^>(2);

			arr1d->SetValue("X",0);
			arr2d->SetValue("X",0);
			arr2d->SetValue("Y",1);

			this->FHost = Host;
			this->FHost->CreateValueInput("Input",2,arr2d,TSliceMode::Dynamic,TPinVisibility::True,this->vInPoints);
			this->vInPoints->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0,0,false,false,false);

			this->FHost->CreateValueInput("Bin Size",1,arr1d,TSliceMode::Dynamic,TPinVisibility::True,this->vInBinSizes);
			this->vInBinSizes->SetSubType(Double::MinValue,Double::MaxValue,1.0,-1.0,false,false,true);

			this->vInPoints->Order = 1;
			this->vInBinSizes->Order = 2;

			this->FHost->CreateValueOutput("Center",2,arr2d,TSliceMode::Dynamic,TPinVisibility::True,this->vOutCenter);
			this->vOutCenter->SetSubType2D(Double::MinValue,Double::MaxValue,0.01,0,0,false,false,false);

			this->FHost->CreateValueOutput("Size",2,arr2d,TSliceMode::Dynamic,TPinVisibility::True,this->vOutSize);
			this->vOutSize->SetSubType2D(0.0,Double::MaxValue,0.01,0,0,false,false,false);

			this->FHost->CreateValueOutput("Angle",1,arr1d,TSliceMode::Dynamic,TPinVisibility::True,this->vOutAngle);
			this->vOutAngle->SetSubType(Double::MinValue,Double::MaxValue,0.01,0.0,false,false,false);

		}

		void MinimumAreaRectNode::Configurate(IPluginConfig^ Input) {}

		void MinimumAreaRectNode::Evaluate(int SpreadMax) 
		{
			if (this->vInPoints->PinIsChanged || this->vInBinSizes->PinIsChanged)
			{
				this->FHost->Log(TLogType::Debug,"Begin");

				double x,y;
				

				this->FHost->Log(TLogType::Debug,"Bin Start");

				List<int>^ bins = BinSizeUtils::CalculateBins(this->FHost, this->vInBinSizes,this->vInPoints->SliceCount);

				this->FHost->Log(TLogType::Debug,"Bin End");

				this->vOutAngle->SliceCount = bins->Count;
				this->vOutCenter->SliceCount = bins->Count;
				this->vOutSize->SliceCount = bins->Count;

				int counter = 0;
				
				//Browse the pre calculated bins
				for (int bin = 0; bin < bins->Count; bin++) 
				{
					int size = bins[bin];
					//Build the sequence here
					seqbuilder->Clear();

					for (int i = 0; i < size; i++) 
					{
						this->vInPoints->GetValue2D(counter,x,y);
						seqbuilder->AddPoint(Convert::ToSingle(x),Convert::ToSingle(y));
						counter++;
					}

					CvBox2D box = cvMinAreaRect2(seqbuilder->points,0);

					this->vOutCenter->SetValue2D(bin,box.center.x,box.center.y);
					this->vOutSize->SetValue2D(bin,box.size.width,box.size.height);
					this->vOutAngle->SetValue(bin,(90.0-box.angle)/360.0);

					seqbuilder->Destroy();

				}

				this->FHost->Log(TLogType::Debug,"End");
			}
		}

	}
}
