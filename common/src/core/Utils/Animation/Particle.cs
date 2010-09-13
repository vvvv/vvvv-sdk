using System;
using System.Diagnostics;
using VVVV.Utils.VMath;

namespace VVVV.Utils.Animation
{
	/// <summary>
	/// A particle class with common data fields.
	/// The Update method is virtual, so it can be overwritten in sub classes.
	/// </summary>
	public class Particle
	{
		public Vector3D Position;
		public Vector3D Velocity;
		public Vector3D Acceleration;
		public double Size;
		public double Mass;
		public double Age { get; protected set; }
		public double StartTime;
		public double LifeTime;
		protected double FCurrentTime;
		
		#region constructors
		
		/// <summary>
		/// Creates a new Particle instance and sets the time.
		/// </summary>
		/// <param name="time">Current system time in seconds.</param>
		/// <param name="lifeTime">Max life time in seconds.</param>
		public Particle(double time, double lifeTime)
		{
			StartTime = time;
			FCurrentTime = time;
			LifeTime = lifeTime;
		}
		
		public Particle(double time, double lifeTime, Vector3D pos, Vector3D vel)
			: this(time, lifeTime)
		{
			Position = pos;
			Velocity = vel;
		}
		
		public Particle(double time, double lifeTime, Vector3D pos, Vector3D vel, Vector3D acc)
			: this(time, lifeTime, pos, vel)
		{
			Acceleration = acc;
		}
		
		public Particle(double time, double lifeTime, Vector3D pos, Vector3D vel, double size)
			: this(time, lifeTime, pos, vel)
		{
			Size = size;
		}
		
		public Particle(double time, double lifeTime, Vector3D pos, Vector3D vel, Vector3D acc, double size)
			: this(time, lifeTime, pos, vel, acc)
		{
			Size = size;
		}
		
		public Particle(double time, double lifeTime, Vector3D pos, Vector3D vel, double size, double mass)
			: this(time, lifeTime, pos, vel, size)
		{
			Mass = mass;
		}
		
		public Particle(double time, double lifeTime, Vector3D pos, Vector3D vel, Vector3D acc, double size, double mass)
			: this(time, lifeTime, pos, vel, acc, size)
		{
			Mass = mass;
		}
		
		#endregion constructors
		
		/// <summary>
		/// Updates the paticle age and position.
		/// </summary>
		/// <param name="time">Current time in seconds.</param>
		/// <returns>False if the particle is dead.</returns>
		public virtual bool Update(double time)
		{
			//calc timings
			var dt = FCurrentTime - time;
			FCurrentTime = time;
			Age = VMath.VMath.Clamp((FCurrentTime - StartTime)/LifeTime, 0, 1);
			
			//cal position
			Velocity += Acceleration * dt;
			Position += Velocity * dt;
			
			//is dead?
			return Age == 1.0;
		}
	}
}
