#pragma once

using namespace VVVV::PluginInterfaces::V1;
using namespace System::Collections::Generic;
using namespace System;

namespace VVVV 
{
	namespace Utils 
	{
		ref class BinSizeUtils
		{
			public:
				BinSizeUtils(void);

				static List<int>^ BinSizeUtils::CalculateBins(IPluginHost^ Host, IValueIn^ BinInputPin, int SpreadMax)
				{
					double dbin = 0;
					List<int>^ result = gcnew List<int>();

					int spreadindex = 0;
					bool end = false;

					int binidx = 0;

					Host->Log(TLogType::Debug,"Start");

					while (!end) 
					{
						BinInputPin->GetValue(binidx,dbin);
						Host->Log(TLogType::Debug,"Bin : " + Convert::ToString(dbin));
						Host->Log(TLogType::Debug,"Spread Max : " + Convert::ToString(SpreadMax));
						int bin = ConvertBinSize(SpreadMax, Convert::ToInt32(dbin));
						result->Add(bin);
						Host->Log(TLogType::Debug,"New bin : " + Convert::ToString(bin));

						spreadindex += bin;
						
						Host->Log(TLogType::Debug,"SpreadIndex : " + Convert::ToString(spreadindex));
						binidx++;

						//When we reach the end of spread
						if (spreadindex >= SpreadMax)
						{
							//We just add the remainder bin
							while ((binidx % BinInputPin->SliceCount) != 0)
							{
								BinInputPin->GetValue(binidx,dbin);
								result->Add(ConvertBinSize(SpreadMax, Convert::ToInt32(dbin)));
								binidx++;
							}

							end = true;
						}
					}
					return result;
				}

			private:			
				static int BinSizeUtils::ConvertBinSize(int SpreadMax, int bin)
				{
					if (bin < 0)
					{
						int newbin = SpreadMax / Math::Abs(bin);

						if (SpreadMax % bin != 0)
						{
							newbin++;
						}
						return newbin;
					}
					else
					{
						return bin;
					}
				}

		};
	}
}
