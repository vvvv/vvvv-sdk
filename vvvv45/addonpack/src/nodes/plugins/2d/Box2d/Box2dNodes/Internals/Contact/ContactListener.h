#pragma once

public class ContactListener : public b2ContactListener
{
	private:
		std::vector<b2ContactPoint*>* contacts;
	public:
		ContactListener(std::vector<b2ContactPoint*>* contacts) 
		{
			this->contacts = contacts;
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
			contacts->push_back(cpoint);
		}
			    
		void Persist(const b2ContactPoint* point)
		{
			// handle persist point
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

