#include "StdAfx.h"
#include "GroundDataType.h"

namespace VVVV 
{
	namespace DataTypes 
	{
		GroundDataType::GroundDataType(void)
		{
			this->m_isvalid = false;
			this->m_ground = nullptr;
		}

		b2Body* GroundDataType::GetGround()
		{
			return this->m_ground;
		}

		bool GroundDataType::IsValid()
		{
			return this->m_isvalid;
		}

		void GroundDataType::SetIsValid(bool value)
		{
			this->m_isvalid = value;
		}

		void GroundDataType::SetGround(b2Body* ground)
		{
			this->m_ground = ground;
		}
	}
}

