
//
// Welcome, this is a basic vvvv node plugin template.
// Copy this an rename it, to write your own plugin node.
//
//

// A simple Particle class

//use what you need
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;


namespace VVVV.Nodes
{
	public class Particle {
		
		public Vector3D loc;
		public Vector3D vel;
		public Vector3D acc;
		public Vector3D force;
		public double mass;
		double timer;
		double lifetime;
	
		// One constructor
		public Particle(Vector3D a, Vector3D v, Vector3D l, double r_) {
			
			force = new Vector3D(0);
			acc = a;
			vel = v;
			loc = l;
			mass = r_;
			lifetime =  100;
			timer = 100;
	
		}
		
		// Another constructor (the one we are using here)
		public Particle(Vector3D l, Vector3D v, Vector3D a, double mass, double lifetime) {
			
			force = new Vector3D(0);
			loc = l;
		
			vel = v;
			acc = a;
			this.mass = mass;
			this.lifetime = lifetime;
			this.timer = lifetime;
		}
	
		// Method to update location
		public void update(double dt) {
			
			vel += acc * dt;
			loc += vel;
			timer-= dt;
		}
		
		public void update(double dt, double amount) {
			
			vel += VMath.Lerp(acc, force, amount) * dt;
			loc += vel;
			timer -= dt;
		}
		
		//get the age 0..1
		public double age() {
			return 1 - Math.Max((double)timer, 0)/(double)lifetime;
		}
		
		// Is the particle still useful?
		public bool dead() {
			return (timer <= 0);
		}
	}
	
	
	// A class to describe a group of Particles
	// An ArrayList is used to manage the list of Particles
}

