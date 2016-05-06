#pragma once

public class ContactListener : public b2ContactListener
{
	private:
		std::vector<b2ContactPoint*>* contacts;
		std::vector<double>* newcontacts;
	public:
		ContactListener(std::vector<b2ContactPoint*>* contacts,std::vector<double>* newcontacts) 
		{
			this->contacts = contacts;
			this->newcontacts = newcontacts;
		}
				
		~ContactListener(void) 
		{

		}
							
		void Add(const b2ContactPoint* point)
		{
			int idx = -1;
			for (int i = 0; i < contacts->size();i++)
			{
				uint32 u1 = point->id.key;
				uint32 u2 = contacts->at(i)->id.key;

				if (u1 == u2)
				{
					idx = i;
				}
			}

			b2ContactPoint* cpoint;
			if (idx > -1)
			{
				cpoint = contacts->at(idx);
			}
			else
			{
				cpoint = new b2ContactPoint();
			}

			cpoint->shape1 = point->shape1;
			cpoint->shape2 = point->shape2;

			cpoint->position.x = point->position.x;
			cpoint->position.y = point->position.y;
			cpoint->normal.x = point->normal.x;
			cpoint->normal.y = point->normal.y;

			if (idx == -1)
			{
				contacts->push_back(cpoint);
				newcontacts->push_back(1);
			}

		}
			    
		void Persist(const b2ContactPoint* point)
		{
			int idx = -1;
			for (int i = 0; i < contacts->size();i++)
			{
				uint32 u1 = point->id.key;
				uint32 u2 = contacts->at(i)->id.key;

				if (u1 == u2)
				{
					idx = i;
				}
			}

			b2ContactPoint* cpoint;
			if (idx > -1)
			{
				cpoint = contacts->at(idx);
			}
			else
			{
				cpoint = new b2ContactPoint();
			}

			cpoint->shape1 = point->shape1;
			cpoint->shape2 = point->shape2;

			cpoint->position.x = point->position.x;
			cpoint->position.y = point->position.y;
			cpoint->normal.x = point->normal.x;
			cpoint->normal.y = point->normal.y;

			if (idx == -1)
			{
				contacts->push_back(cpoint);
				newcontacts->push_back(1);
			}
		}
	    
		void Remove(const b2ContactPoint* point)
		{		
			int idx = -1;
			for (int i = 0; i < contacts->size();i++)
			{
				uint32 u1 = point->id.key;
				uint32 u2 = contacts->at(i)->id.key;

				if (u1 == u2)
				{
					idx = i;
				}
			}

			if (idx > -1)
			{
				contacts->erase (contacts->begin()+idx);
				newcontacts->erase(newcontacts->begin()+idx);
			}
			//contacts->remove(point);
			//contacts->erase(5);
			// handle remove point
		}
	    
		void Result(const b2ContactResult* point)
		{
			// handle results
		}
};

