using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Lib
{
    public class ParticleForce
    {
        protected double px, py, age, devage, vx, vy, prevx, prevy,damp;
        protected bool created;
        public Random random;

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

        public ParticleForce(Random rnd, double positionx, double positiony, double age, double devage, double vx, double vy,double damp)
        {
            this.random = rnd;
            this.px = positionx;
            this.py = positiony;       
            this.created = true;
            this.devage = (random.NextDouble() * devage);
            this.age = age;
            this.vx = vx;
            this.vy = vy;
            this.damp = damp;

        }

        public bool Update(double dtage,double dtvel,double fx, double fy)
        {
            if (!this.created)
            {
                this.age -= (dtage + (this.devage * dtage));

                this.prevx = this.px;
                this.prevy = this.py;


                this.vx += fx * dtvel;
                this.vy += fy * dtvel;
                this.vx *= damp;
                this.vy *= damp;
               

                this.px += vx * dtvel;
                this.py += vy * dtvel;
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
