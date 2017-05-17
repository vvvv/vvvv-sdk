using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Lib
{
    public class Particle
    {
        protected double px, py, age, devage,devvelx,devvely,prevx,prevy;
        protected bool created;
        public Random random;
        

        public Particle(Random rnd,double positionx, double positiony, double age, double devage, double devx, double devy)
        {
            this.random = rnd;
            this.px = positionx;
            this.py = positiony;
            this.prevx = this.px;
            this.prevy = this.py;        
            this.created = true;
            this.devage = (random.NextDouble() * devage);
            this.age = age;
            this.devvelx = (random.NextDouble() * devx) - (devx * 0.5);
            this.devvely = (random.NextDouble() * devy) - (devy * 0.5);

        }

        public double PositionX
        {
            get { return px; }
            set { px = value; }
        }

        public double PositionY
        {
            get { return py; }
            set { py = value; }
        }

        public double PreviousX
        {
            get { return prevx; }
            set { prevx = value; }
        }

        public double PreviousY
        {
            get { return prevy; }
            set { prevy = value; }
        }

        public double Age
        {
            get { return age; }
        }

        public virtual bool Update(double dtage,double dtvel,double vx, double vy)
        {
            if (!this.created)
            {
                this.age -= (dtage + (this.devage * dtage));

                this.prevx = this.px;
                this.prevy = this.py;
                this.px += vx * dtvel + devvelx;
                this.py += vy * dtvel + this.devvely;
                return this.age <= 0.0;
            }
            else
            {
                this.created = false;
                return false;
            }
        }
    }
}
