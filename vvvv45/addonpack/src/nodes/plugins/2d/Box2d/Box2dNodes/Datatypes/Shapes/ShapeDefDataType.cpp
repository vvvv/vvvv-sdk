#include "StdAfx.h"
#include "ShapeDefDataType.h"

namespace VVVV 
{
	namespace DataTypes 
	{
		ShapeDefDataType::ShapeDefDataType(void)
		{
			this->m_shapes = new vector<b2ShapeDef*>;
		}

		b2ShapeDef* ShapeDefDataType::GetSlice(int index)
		{
			return this->m_shapes->at(index % this->m_shapes->size());
		}

		void ShapeDefDataType::Reset() 
		{
			this->m_shapes->clear();
		}

		b2PolygonDef* ShapeDefDataType::AddPolygon() 
		{
			b2PolygonDef* shapedef = new b2PolygonDef;
			this->m_shapes->push_back(shapedef);
			return shapedef;
		}

		b2CircleDef* ShapeDefDataType::AddCircle() 
		{
			b2CircleDef* shapedef = new b2CircleDef;
			this->m_shapes->push_back(shapedef);
			return shapedef;
		}

	}
}
