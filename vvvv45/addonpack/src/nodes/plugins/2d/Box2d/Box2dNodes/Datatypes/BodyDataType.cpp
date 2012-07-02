#include "StdAfx.h"
#include "BodyDataType.h"

namespace VVVV 
{
	namespace DataTypes 
	{
		BodyDataType::BodyDataType(void)
		{
			this->m_bodies = new vector<b2Body*>;
		}

		b2Body* BodyDataType::GetSlice(int index)
		{
			return this->m_bodies->at(index % this->m_bodies->size());
		}

		int BodyDataType::Size() 
		{
			return this->m_bodies->size();
		}

		void BodyDataType::Reset() 
		{
			this->m_bodies->clear();
		}

		void BodyDataType::Add(b2Body* body) 
		{
			this->m_bodies->push_back(body);
		}

	}
}

