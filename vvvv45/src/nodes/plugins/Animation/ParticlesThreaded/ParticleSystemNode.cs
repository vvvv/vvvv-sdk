#region licence/info

//////project name
//particle system vvvv plugin

//////description
//basic vvvv node plugin template.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;


namespace VVVV.Nodes
{
	public class ThreadedParticleSystemsNode: IPlugin
	{
		//the host
		private IPluginHost FHost;
		
		//input pin declaration
		private IValueIn FInitPositionsIn;
		private IValueIn FVelDirectionIn;
		private IValueIn FVelDeviationIn;
		private IValueIn FAccDirectionIn;
		private IValueIn FAccDeviationIn;
		private IValueIn FMassIn;
		private IValueIn FMassDeviationIn;
		private IValueIn FLifetimeIn;
		private IValueIn FLifetimeDeviationIn;
		private IValueIn FIsInfluencedIn;
		private IValueIn FInfluenceAmountIn;
		private IValueIn FInfluenceIn;
		private IValueIn FdtIn;
		private IValueIn FEmitIn;
		
		//output pin declaration
		private IValueOut FPosOut;
		private IValueOut FAgeOut;
		private IValueOut FHeadingOut;
		private IValueOut FSpreadCountsOut;
		
		double FLastTime = 0;
		ArrayList FParticleSystemsList;
		Random rnd = new Random();
		
		public ThreadedParticleSystemsNode()
		{
			//the nodes constructor
			//nothing to declare
		}
		
		~ThreadedParticleSystemsNode()
		{
			//the nodes destructor
			//nothing to destruct
		}

		//provide node infos
		public static IPluginInfo PluginInfo
		{
			get
			{
				//fill out nodes info
				PluginInfo Info = new PluginInfo();
				Info.Name = "Particles";
				Info.Category = "Spreads";
				Info.Version = "";
				Info.Help = "A spread of particle systems for dual core cpus";
				Info.Tags = "particles, animation";
				Info.Author = "tonfilm";
				Info.Bugs = "";
				Info.Credits = "Base code ported from a processing example by Daniel Shiffman: http://processing.org/learning/examples/multipleparticlesystems.html";
				Info.Warnings = "";
				
				//leave below as is
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
				System.Diagnostics.StackFrame sf = st.GetFrame(0);
				System.Reflection.MethodBase method = sf.GetMethod();
				Info.Namespace = method.DeclaringType.Namespace;
				Info.Class = method.DeclaringType.Name;
				return Info;
				//leave above as is
			}
		}
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;

			//create inputs:
			System.Diagnostics.Debug.WriteLine("Create Pins");
			//origin
			FHost.CreateValueInput("Origin ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FInitPositionsIn);
			System.Diagnostics.Debug.WriteLine("First Pin created");
			
			FInitPositionsIn.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0.5, 0.5, 0.5, false, false, false);
			System.Diagnostics.Debug.WriteLine("subtype set");
			
			//direction
			FHost.CreateValueInput("Direction ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FVelDirectionIn);
			FVelDirectionIn.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0.5, 0.5, 0.5, false, false, false);
			
			FHost.CreateValueInput("Direction Deviation ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FVelDeviationIn);
			FVelDeviationIn.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0.5, 0.5, 0.5, false, false, false);
			
			//acceleration
			FHost.CreateValueInput("Acceleration ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FAccDirectionIn);
			FAccDirectionIn.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0.5, 0.5, 0.5, false, false, false);
			
			FHost.CreateValueInput("Acceleration Deviation ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FAccDeviationIn);
			FAccDeviationIn.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0.5, 0.5, 0.5, false, false, false);
			
			//mass
			FHost.CreateValueInput("Mass", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMassIn);
			FMassIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
			
			FHost.CreateValueInput("Mass Deviation", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FMassDeviationIn);
			FMassDeviationIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
			
			//lifetime
			FHost.CreateValueInput("Lifetime", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FLifetimeIn);
			FLifetimeIn.SetSubType(0, double.MaxValue, 0.01, 1, false, false, false);
			
			FHost.CreateValueInput("Lifetime Deviation", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FLifetimeDeviationIn);
			FLifetimeDeviationIn.SetSubType(0, double.MaxValue, 0.01, 1, false, false, false);
			
			//force interaction
			FHost.CreateValueInput("Is Influenced", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIsInfluencedIn);
			FIsInfluencedIn.SetSubType(0, 1, 1, 0, false, true, false);
			
			FHost.CreateValueInput("Influence Amount", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FInfluenceAmountIn);
			FInfluenceAmountIn.SetSubType(0, 1, 0.01, 0, false, false, false);
			
			FHost.CreateValueInput("Influence", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FInfluenceIn);
			FInfluenceIn.SetSubType(0, 1, 1, 0, false, true, false);
			
			//timebase
			FHost.CreateValueInput("Timestep", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FdtIn);
			FdtIn.SetSubType(double.MinValue, double.MaxValue, 0.001, 0.01, false, false, false);
			
			//create new particles
			FHost.CreateValueInput("Emit", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FEmitIn);
			FEmitIn.SetSubType(0, 1, 1, 0, false, true, false);


			//create outputs
			FHost.CreateValueOutput("Positions ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FPosOut);
			FPosOut.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			
			FHost.CreateValueOutput("Heading ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FHeadingOut);
			FHeadingOut.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			
			FHost.CreateValueOutput("Age", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FAgeOut);
			FAgeOut.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			
			FHost.CreateValueOutput("Spread Counts", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSpreadCountsOut);
			FSpreadCountsOut.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, true);

			
			FParticleSystemsList = new ArrayList();
			
		}
		
		
		
		
		public void Evaluate(int SpreadMax)
		{
			
			//calc input spreadcount
			int inputSpreadCount = SpreadMax;
			
			//create or delete systems
			int diff = inputSpreadCount - FParticleSystemsList.Count;
			if (diff > 0)
			{
				for (int i=0; i<diff; i++)
				{
					FParticleSystemsList.Add(new ParticleSystem());
				}
			}
			else if (diff < 0)
			{
				for (int i=0; i< -diff; i++)
				{
					FParticleSystemsList.RemoveAt(FParticleSystemsList.Count-1-i);
				}
			}
			
			//update 3D parameters
			int slice;
			if (   FInitPositionsIn.PinIsChanged
			    || FVelDirectionIn.PinIsChanged
			    || FVelDeviationIn.PinIsChanged
			    || FAccDirectionIn.PinIsChanged
			    || FAccDeviationIn.PinIsChanged)
			{
				
				for (slice = 0; slice < inputSpreadCount; slice++)
				{
					ParticleSystem ps = (ParticleSystem) FParticleSystemsList[slice];
					
					double x, y, z;
					
					//update origins
					FInitPositionsIn.GetValue3D(slice, out x, out y, out z);
					ps.origin = new Vector3D(x, y, z);
					
					//update directions
					FVelDirectionIn.GetValue3D(slice, out x, out y, out z);
					ps.direction = new Vector3D(x, y, z);
					
					//update deviation
					FVelDeviationIn.GetValue3D(slice, out x, out y, out z);
					ps.deviation = new Vector3D(x, y, z);
					
					//update acceleration deviation
					FAccDirectionIn.GetValue3D(slice, out x, out y, out z);
					ps.accDirection = new Vector3D(x, y, z);
					
					//update acceleration deviation
					FAccDeviationIn.GetValue3D(slice, out x, out y, out z);
					ps.accDeviation = new Vector3D(x, y, z);
				}
			}
			
			//update single parameters
			if (   FMassIn.PinIsChanged
			    || FMassDeviationIn.PinIsChanged
			    || FLifetimeIn.PinIsChanged
			    || FLifetimeDeviationIn.PinIsChanged
			    || FIsInfluencedIn.PinIsChanged
			    || FInfluenceIn.PinIsChanged
			    || FInfluenceAmountIn.PinIsChanged
			    || FdtIn.PinIsChanged)
			{
				for (slice = 0; slice < inputSpreadCount; slice++)
				{
					ParticleSystem ps = (ParticleSystem) FParticleSystemsList[slice];
					
					double mass, massDeviation;
					double lifetimeIn, lifetimeDeviation;
					double isInfluenced, influenceAmount, influences;
					double dt;
					
					FMassIn.GetValue(slice, out mass);
					FMassDeviationIn.GetValue(slice, out massDeviation);
					FLifetimeIn.GetValue(slice, out lifetimeIn);
					FLifetimeDeviationIn.GetValue(slice, out lifetimeDeviation);
					FIsInfluencedIn.GetValue(slice, out isInfluenced);
					FInfluenceAmountIn.GetValue(slice, out influenceAmount);
					FInfluenceIn.GetValue(slice, out influences);
					FdtIn.GetValue(slice, out dt);
					
					ps.mass = mass;
					ps.massDeviation = massDeviation;
					ps.lifetime = lifetimeIn;
					ps.lifetimeDeviation = lifetimeDeviation;
					ps.influences = (influences >= 0.5);
					ps.isInfluenced = (isInfluenced >= 0.5);
					ps.influenceAmount = influenceAmount;
					ps.dt = dt;
					
				}
			}
			
			//force calculation
			UpdateForce();

			
			// Cycle through all particle systems, run them and get particle counts
			FSpreadCountsOut.SliceCount = FParticleSystemsList.Count;
			int outcount = 0;
			slice = 0;
			for (slice = 0; slice < inputSpreadCount; slice++)
			{
				
				ParticleSystem ps = (ParticleSystem) FParticleSystemsList[slice];
				
				//add new particle ?
				double emit;
				FEmitIn.GetValue(slice, out emit);
				
				if (emit >= 0.5){
					ps.addParticle();
				}
				
				//update system
				double time;
				FHost.GetCurrentTime(out time);
				ps.run(0.1);
				FLastTime = time;
				
				//check particle count
				outcount += ps.particles.Count;
				FSpreadCountsOut.SetValue(slice, ps.particles.Count);
			}
			
			
			//write output to pins
			FPosOut.SliceCount = outcount;
			FAgeOut.SliceCount = outcount;
			FHeadingOut.SliceCount = outcount;
			
			slice = 0;
			for (int i = 0; i < inputSpreadCount; i++)
			{
				ParticleSystem ps = (ParticleSystem) FParticleSystemsList[i];
				int pcount = ps.particles.Count;
				
				for (int j = 0; j < pcount; j++)
				{
					Particle p = (Particle) ps.particles[j];
					
					FPosOut.SetValue3D(slice, p.loc.x, p.loc.y, p.loc.z);
					FHeadingOut.SetValue3D(slice, p.vel.x, p.vel.y, p.vel.z);
					FAgeOut.SetValue(slice, 1-p.age());
					slice++;
				}
			}

		}
		
		public void UpdateForce()
		{
			
			List<Particle> influencedSystems = new List<Particle>();
			List<Particle> influencingSystems = new List<Particle>();
			
			//add particles to the lists
			foreach (ParticleSystem ps in FParticleSystemsList)
			{
				if (ps.isInfluenced && ps.influences)
				{
					foreach (Particle p in ps.particles)
					{
						influencedSystems.Add(p);
						influencingSystems.Add(p);
					}
				}
				else if (ps.isInfluenced && !ps.influences)
				{
					foreach (Particle p in ps.particles)
					{
						influencedSystems.Add(p);
					}
				}
				else if (!ps.isInfluenced && ps.influences)
				{
					foreach (Particle p in ps.particles)
					{
						influencingSystems.Add(p);
					}
				}
			}
			
			//convert lists to arrays and run calculation
			
			int halfLength = (int)Math.Ceiling(influencedSystems.Count * 0.5);
			Particle[] toUpdate1 = new Particle[halfLength];
			Particle[] toUpdate2 = new Particle[influencedSystems.Count-halfLength];
			Particle[] influence = influencingSystems.ToArray();
			
			//copy first dataset
			for (int i=0; i<halfLength; i++) 
			{
				toUpdate1[i] = influencedSystems[i];
			
			}
			//copy second dataset
			for (int i=halfLength; i<influencedSystems.Count; i++) 
			{
				toUpdate2[i-halfLength] = influencedSystems[i];
			}
			
			//calc common variables
			Vector3D center = new Vector3D(0);
			Vector3D velo = new Vector3D(0);
			
			for (int j=0; j<influence.Length; j++)
			{
				center += influence[j].loc;
				velo += influence[j].vel;
			}
			
			center /= influence.Length;
			velo /= influence.Length;
			
			//set up and start the threads
			//the thread constructor can take a function delegate 
			//parameterized by one object, here its CalcForce(object oin)			
			Thread t1 = new Thread(CalcForce);
			Thread t2 = new Thread(CalcForce);
			
			//start the threads and pass the parameters as an object array, 
			//which is an object and therefore accepted as parameter
			t1.Start(new object[] {toUpdate1, influence, center, velo});			
			t2.Start(new object[] {toUpdate2, influence, center, velo});
			
			//wait for the threads to be finished
			t1.Join();
			t2.Join();
			
		}
		
		//threaded function
		private void CalcForce(object oin)
		{
			//split paramters		
			object[] o = (object[])oin;
			Particle[] toUpdate = (Particle[]) o[0];
			Particle[] influence = (Particle[]) o[1];
			Vector3D center = (Vector3D) o[2];
			Vector3D velo = (Vector3D) o[3];
			
			Vector3D force;
			Vector3D diff;
			
			for (int i=0; i<toUpdate.Length; i++)
			{
				force = new Vector3D(0);
				for (int j=0; j<influence.Length; j++)
				{
					if (!toUpdate[i].Equals(influence[j]))
					{
						diff = toUpdate[i].loc - influence[j].loc;
						
						double factor = 0;
						if ( diff*diff < 3 )
						{
							//factor = -toUpdate[i].mass/Math.Pow(!diff, 3);
							factor = toUpdate[i].mass;// * !diff;
							force = force - diff * factor;//* influence[j].age();
							//force = force - (b.position - bJ.position)
						}
					}
				}
				
				//set normalized force
				toUpdate[i].force = (force / Math.Max(influence.Length, 1)) + (center - toUpdate[i].loc)*0.000 + (velo - toUpdate[i].vel);
			}		
		
		}
		
		//configuration functions:
		public bool AutoEvaluate
		{
			//return true if this node needs to calculate every frame even if nobody asks for its output
			get {return false;}
		}
		
		public void Configurate(IPluginConfig Input)
		{
			//nothing to configure in this plugin
			//only used in conjunction with inputs of type cmpdConfigurate
		}
		
		
		

	}
	


}

