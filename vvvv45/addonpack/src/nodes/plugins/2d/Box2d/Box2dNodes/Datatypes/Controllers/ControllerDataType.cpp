#include "StdAfx.h"
#include "ControllerDataType.h"

namespace VVVV 
{
	namespace DataTypes 
	{
		ControllerDataType::ControllerDataType(void)
		{
			this->m_controllers = new vector<b2Controller*>;	
		}

		b2Controller* ControllerDataType::GetSlice(int index)
		{
			return this->m_controllers->at(index % this->m_controllers->size());
		}

		int ControllerDataType::Size() 
		{
			return this->m_controllers->size();
		}

		void ControllerDataType::Reset() 
		{
			this->m_controllers->clear();
		}

		void ControllerDataType::Add(b2Controller* ctrl) 
		{
			this->m_controllers->push_back(ctrl);
		}

	}
}