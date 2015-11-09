	/**
	 * Multiple Particle Systems
	 * by Daniel Shiffman.
	 * 
	 * Click the mouse to generate a burst of particles
	 * at mouse location.
	 * 
	 * Each burst is one instance of a particle system
	 * with Particles and CrazyParticles (a subclass of Particle)
	 * Note use of Inheritance and Polymorphism here.
	 * 
	 * Created 2 May 2005
	 */





//
// Welcome, this is a basic vvvv node plugin template.
// Copy this an rename it, to write your own plugin node.
//
//

//use what you need
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;


namespace VVVV.Nodes
{
	public class ParticleSystem {
	
		public ArrayList particles;    // An arraylist for all the particles
		public Vector3D origin;        // An origin point for where particles are birthed
		public Vector3D direction;
		public Vector3D deviation;
		public Vector3D accDirection;
		public Vector3D accDeviation;
		public double mass = 1;
		public double massDeviation = 0;
		public double lifetime = 1;
		public double lifetimeDeviation = 0;
		public bool isInfluenced = false;
		public bool influences = false;
		public double influenceAmount = 0;
		public double dt = 0.1;
		public Random rnd;
	
		public ParticleSystem(int num, Vector3D v) {
			
			particles = new ArrayList();            // Initialize the arraylist
			rnd = new Random();
			origin = v; 						// Store the origin point
			direction = new Vector3D();
			deviation = new Vector3D();
			accDirection = new Vector3D();
			accDeviation = new Vector3D();
			for (int i = 0; i < num; i++) {
				particles.Add(new Particle(origin, randomDir(), randomDir(), (int) rnd.NextDouble()*50));    // Add "num" amount of particles to the arraylist
			}
		}
		
		public ParticleSystem() {
			
			particles = new ArrayList();              // Initialize the arraylist
			rnd = new Random();
			origin = new Vector3D();
			direction = new Vector3D();
			deviation = new Vector3D();
			accDirection = new Vector3D();
			accDeviation = new Vector3D();
		}
	
	
		public void run(double timediff) {
			
			// Cycle through the ArrayList backwards b/c we are deleting
			//Particle[] pArray = (Particle[])particles.ToArray();
			
			for (int i = particles.Count-1; i >= 0; i--) {
				Particle p = (Particle) particles[i];
				
				if (isInfluenced){
					p.update(dt, influenceAmount);
				} else {
					p.update(dt);
				}
				
				//remove if dead
				if (p.dead()){
					particles.RemoveAt(i);
				}
				
			}
		}
	
		public void addParticle() {
			particles.Add(new Particle(origin, randomDir(), randomDir(), randNr(mass, massDeviation), randNr(lifetime, lifetimeDeviation) ));
		}
	
		public void addParticle(Particle p) {
			particles.Add(p);
		}
	
		// A method to test if the particle system still has particles
		public bool dead() {
			return particles.Count == 0;
		}
		
		//make a random vector
		private Vector3D randomDir()
		{
			return new Vector3D(randNr(direction.x, deviation.x), randNr(direction.y, deviation.y), randNr(direction.z, deviation.z));
		}
		
		//make a random number
		private double randNr(double a, double dev)
		{
			return a + (rnd.NextDouble()*dev - dev*0.5);
		}
	
	}
	
	
}

