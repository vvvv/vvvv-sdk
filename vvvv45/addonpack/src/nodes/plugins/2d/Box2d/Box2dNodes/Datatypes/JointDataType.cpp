#include "StdAfx.h"
#include "JointDataType.h"

namespace VVVV 
{
	namespace DataTypes 
	{
		JointDataType::JointDataType(void)
		{
			this->m_joints = new vector<b2Joint*>;
		}

		b2Joint* JointDataType::GetSlice(int index)
		{
			return this->m_joints->at(index % this->m_joints->size());
		}

		int JointDataType::Size() 
		{
			return this->m_joints->size();
		}

		void JointDataType::Reset() 
		{
			this->m_joints->clear();
		}

		void JointDataType::Add(b2Joint* body) 
		{
			this->m_joints->push_back(body);
		}

	}
}