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
			//*haschanged = true;
			b2ContactPoint* cpoint = new b2ContactPoint();
			cpoint->shape1 = point->shape1;
			cpoint->shape2 = point->shape2;

			cpoint->position.x = point->position.x;
			cpoint->position.y = point->position.y;
			cpoint->normal.x = point->normal.x;
			cpoint->normal.y = point->normal.y;
			contacts->push_back(cpoint);
			newcontacts->push_back(1);
		}
			    
		void Persist(const b2ContactPoint* point)
		{
			b2ContactPoint* cpoint = new b2ContactPoint();
			
			cpoint->shape1 = point->shape1;
			cpoint->shape2 = point->shape2;

			cpoint->position.x = point->position.x;
			cpoint->position.y = point->position.y;
			cpoint->normal.x = point->normal.x;
			cpoint->normal.y = point->normal.y;
			contacts->push_back(cpoint);
			newcontacts->push_back(0);
		}
	    
		void Remove(const b2ContactPoint* point)
		{
			// handle remove point
		}
	    
		void Result(const b2ContactResult* point)
		{
			// handle results
		}
};

