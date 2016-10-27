using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Lib
{
    public class ParticleSystem3d
    {
        private LinkedList<Particle3d> particles = new LinkedList<Particle3d>();
        private double dtage = 0.01;
        private double dtvelocity = 1.0;
        private Random random = new Random();

        private int max_particles = 7000;

        public ParticleSystem3d()
        {

        }

        public ParticleSystem3d(int maxp)
        {
            this.max_particles = maxp;
        }

        public void Reset()
        {
            this.particles.Clear();
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

        public void AddParticle(double px, double py, double pz, double age, double devage, double devx, double devy, double devz)
        {
            if (this.particles.Count < this.max_particles)
            {
                Particle3d p = new Particle3d(new Random(this.random.Next()), px, py, pz, age, devage, devx, devy, devz);
                this.particles.AddLast(p);
            }
        }

        public List<Particle3d> Update(IValueFastIn field, int sx, int sy, int sz)
        {
            List<Particle3d> alive = new List<Particle3d>();

            if (this.particles.Count > 0)
            {
                LinkedListNode<Particle3d> current = this.particles.First;

                while (current != null)
                {
                    try
                    {
                        int cellx = GetIndexX(current.Value.PositionX, sx);
                        int celly = GetIndexY(current.Value.PositionY, sy);
                        int cellz = GetIndexZ(current.Value.PositionZ, sz);

                        int cell = celly * sx + cellx + cellz * sx * sy;
                        double dblvx, dblvy, dblvz;
                        field.GetValue3D(cell, out dblvx, out dblvy, out dblvz);
                        //return ((i) + (_NX + 2) * (j)); 

                        if (current.Value.Update(this.dtage, this.dtvelocity, dblvx, dblvy, dblvz))
                        {
                            LinkedListNode<Particle3d> next = current.Next;
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
                        LinkedListNode<Particle3d> next = current.Next;
                        current.List.Remove(current);
                        current = next;
                    }
                }
            }

            return alive;
        }

        #region Get Indexes
        private int GetIndexX(double x, int sizex)
        {
            double stepX = 2.0 / (double)sizex;
            double sx = (x + 1) / stepX;
            return Convert.ToInt32(Math.Truncate(sx));
        }

        private int GetIndexY(double y, int sizey)
        {
            double stepY = 2.0 / (double)sizey;
            double sy = (y + 1) / stepY;
            return Convert.ToInt32(Math.Truncate(sy));
        }

        private int GetIndexZ(double z, int sizez)
        {
            double stepZ = 2.0 / (double)sizez;
            double sz = (z + 1) / stepZ;
            return Convert.ToInt32(Math.Truncate(sz));
        }
        #endregion

    }

}
