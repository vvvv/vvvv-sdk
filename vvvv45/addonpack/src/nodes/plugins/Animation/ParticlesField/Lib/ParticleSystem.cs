using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Lib
{
    public class ParticleSystem
    {
        private LinkedList<Particle> particles = new LinkedList<Particle>();
        private double dtage = 0.01;
        private double dtvelocity = 1.0;
        private Random random = new Random();
        private bool flipx = false;
        private bool flipy = false;

        private int max_particles = 7000;

        public ParticleSystem()
        {

        }

        public ParticleSystem(int maxp)
        {
            this.max_particles = maxp;
        }

        public bool FlipX
        {
            get { return this.flipx; }
            set { this.flipx = value; }
        }

        public bool FlipY
        {
            get { return this.flipy; }
            set { this.flipy = value; }
        }

        public double DtAge
        {
            get { return dtage; }
            set { dtage = value; }
        }

        public double DtVelocity
        {
            get { return dtvelocity; }
            set { dtvelocity = value; }
        }

        public void Reset()
        {
            this.particles.Clear();
        }

        public void AddParticle(double px, double py, double age, double devage, double devx, double devy)
        {
            if (this.particles.Count < this.max_particles)
            {
                Particle p = new Particle(new Random(this.random.Next()),px, py, age, devage, devx, devy);
                this.particles.AddLast(p);
            }
        }

        public List<Particle> Update(IValueFastIn field,int sx, int sy)
        {
            List<Particle> alive = new List<Particle>();

            if (this.particles.Count > 0)
            {
                LinkedListNode<Particle> current = this.particles.First;

                while (current != null)
                {
                    bool err = false;
                    try
                    {
                        int cellx = GetIndexX(current.Value.PositionX, sx);
                        int celly = GetIndexY(current.Value.PositionY, sy);
                        int cell = celly * sx + cellx;
                        double dblvx, dblvy;
                        field.GetValue2D(cell, out dblvx, out dblvy);

                        if (current.Value.Update(this.dtage, this.dtvelocity, dblvx, dblvy))
                        {
                            LinkedListNode<Particle> next = current.Next;
                            current.List.Remove(current);
                            current = next;
                        }
                        else
                        {
                            alive.Add(current.Value);
                            current = current.Next;
                        }

                    }
                    catch
                    {
                        LinkedListNode<Particle> next = current.Next;
                        current.List.Remove(current);
                        current = next;
                    }
                }
            }

            return alive;
        }

        #region Get Indexes
        private int GetIndexX(double x,int sizex)
        {
            if (this.flipx) { x = -x; }
            double stepX = 2.0 / (double)sizex;
            double sx  = (x + 1.0) / stepX;
            return Convert.ToInt32(Math.Truncate(sx));
        }

        private int GetIndexY(double y, int sizey)
        {
            if (this.flipy) { y = -y; }
            double stepY = 2.0 / (double)sizey;
            double sy = (y+1) / stepY;
            return Convert.ToInt32(Math.Truncate(sy));
        }
        #endregion
        
    }
}
