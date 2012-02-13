#include "StdAfx.h"
#include "ShapeDataType.h"

namespace VVVV 
{
	namespace DataTypes 
	{
		ShapeDataType::ShapeDataType(void)
		{
			this->m_shapes = new vector<b2Fixture*>;
		}

		b2Fixture* ShapeDataType::GetSlice(int index)
		{
			return this->m_shapes->at(index % this->m_shapes->size());
		}

		void ShapeDataType::Reset() 
		{
			this->m_shapes->clear();
		}

		void ShapeDataType::Add(b2Fixture* shape) 
		{
			this->m_shapes->push_back(shape);
		}

		int ShapeDataType::Count()
		{
			return this->m_shapes->size();
		}

	}
}


