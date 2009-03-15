#include "StdAfx.h"
#include "WorldDataType.h"

namespace VVVV 
{
	namespace DataTypes 
	{
		WorldDataType::WorldDataType(void)
		{
			this->isvalid = false;
			this->enabled = false;
			this->mWorld = nullptr;
			this->Reset = false;
		}

		bool WorldDataType::GetIsValid() 
		{
			return this->isvalid;
		}

		void WorldDataType::SetIsValid(bool value)
		{
			this->isvalid = value;
		}
		bool WorldDataType::GetIsEnabled()
		{
			return this->enabled;
		}

		void WorldDataType::SetIsEnabled(bool value)
		{
			this->enabled = value;
		}

		b2World* WorldDataType::GetWorld() 
		{
			return this->mWorld;
		}

		void WorldDataType::SetWorld(b2World* world) 
		{
			this->mWorld = world;
		}		
	}
}
