using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Lib
{
    public class Particle3d
    {
        private double px, py, pz, age, devage, devvelx, devvely, devvelz, prevx, prevy,prevz;
        private bool created;
        public Random random;
        

        public Particle3d(Random rnd,double positionx, double positiony,double positionz, double age, double devage, double devx, double devy,double devz)
        {
            this.random = rnd;
            this.px = positionx;
            this.py = positiony;
            this.pz = positionz;
            this.prevx = this.px;
            this.prevy = this.py;
            this.prevz = this.pz;
            this.age = age;
            this.created = true;
            this.devage = (random.NextDouble() * devage);

            this.devvelx = (random.NextDouble() * devx) - (devx * 0.5);
            this.devvely = (random.NextDouble() * devy) - (devy * 0.5);
            this.devvelz = (random.NextDouble() * devz) - (devz * 0.5);
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

        public double PositionZ
        {
            get { return pz; }
            set { pz = value; }
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

        public double PreviousZ
        {
            get { return prevz; }
            set { prevz = value; }
        }

        public double Age
        {
            get { return age; }
        }

        public bool Update(double dtage,double dtvel,double vx, double vy,double vz)
        {
            if (!this.created)
            {
                this.age -= (dtage + (this.devage * dtage));

                this.prevx = this.px;
                this.prevy = this.py;
                this.prevz = this.pz;
                this.px += vx * dtvel + devvelx;
                this.py += vy * dtvel + this.devvely;
                this.pz += vz * dtvel + this.devvelz;
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
