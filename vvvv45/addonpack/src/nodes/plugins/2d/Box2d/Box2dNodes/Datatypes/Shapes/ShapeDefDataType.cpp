#include "StdAfx.h"
#include "ShapeDefDataType.h"

namespace VVVV 
{
	namespace DataTypes 
	{
		ShapeDefDataType::ShapeDefDataType(void)
		{
			this->m_shapes = new vector<b2FixtureDef*>;
			this->m_custom = gcnew List<String^>();
		}

		ShapeDefDataType::~ShapeDefDataType(void)
		{
			this->Reset();
			delete this->m_shapes;
		}

		b2FixtureDef* ShapeDefDataType::GetSlice(int index)
		{
			return this->m_shapes->at(index % this->m_shapes->size());
		}

		String^ ShapeDefDataType::GetCustom(int index)
		{
			return this->m_custom[index % this->m_custom->Count];
		}

		void ShapeDefDataType::Reset() 
		{
			for (int i = 0; i < this->m_shapes->size(); i++)
			{
				b2FixtureDef* shape = this->m_shapes->at(i);
				delete shape;
			}
			this->m_shapes->clear();
			this->m_custom->Clear();
		}

		void ShapeDefDataType::AddCustom(String^ cust) 
		{
			this->m_custom->Add(cust);
		}

		b2FixtureDef* ShapeDefDataType::AddPolygon() 
		{
			b2FixtureDef* f = new b2FixtureDef();
			//b2PolygonShape* shapedef = new b2PolygonShape;

			//f->shape = shapedef;
			this->m_shapes->push_back(f);
			return f;
		}

		b2FixtureDef* ShapeDefDataType::AddCircle() 
		{
			b2FixtureDef* f = new b2FixtureDef();
			//b2CircleShape* shapedef = new b2CircleShape;

			//f->shape = shapedef;
			this->m_shapes->push_back(f);
			return f;
		}

		//b2EdgeChainDef* ShapeDefDataType::AddEdgeChain() 
		//{
		//	b2EdgeChainDef* shapedef = new b2EdgeChainDef;
		//	this->m_shapes->push_back(shapedef);
		//	return shapedef;
		//}
	}
}
