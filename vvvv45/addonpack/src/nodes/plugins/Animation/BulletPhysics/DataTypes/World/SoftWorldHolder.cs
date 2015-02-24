using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using BulletSharp.SoftBody;
using VVVV.Internals.Bullet;

namespace VVVV.DataTypes.Bullet
{
	public delegate void WorldResetDelegate();
	public delegate void RigidBodyDeletedDelegate(RigidBody rb,int id);
	public delegate void SoftBodyDeletedDelegate(SoftBody rb, int id);
	public delegate void ConstraintDeletedDelegate(TypedConstraint tc, int id);

	//Just to make it easier to manage than having million of stuff in
	//World node. Can also easily switch broadphases
	public class BulletRigidSoftWorld
	{
		private DefaultCollisionConfiguration collisionConfiguration;
		private CollisionDispatcher dispatcher;
		private SequentialImpulseConstraintSolver solver;
		private BroadphaseInterface overlappingPairCache;
		private SoftRigidDynamicsWorld dynamicsWorld;
		private SoftBodyWorldInfo worldInfo;

		private int bodyindex;
		private int cstindex;

		protected bool enabled;
		protected float gx, gy, gz;
		protected float ts;
		protected int iter;

		public event RigidBodyDeletedDelegate RigidBodyDeleted;
		public event SoftBodyDeletedDelegate SoftBodyDeleted;
		public event ConstraintDeletedDelegate ConstraintDeleted;
		public event WorldResetDelegate WorldHasReset;

		#region Rigid Registry
		//Cache so we speed up process
		private List<RigidBody> bodies = new List<RigidBody>(); 

		public void Register(RigidBody body)
		{
			this.World.AddRigidBody(body);
			this.bodies.Add(body);
		}

		public void Unregister(RigidBody body)
		{
			this.bodies.Remove(body);
			this.World.RemoveRigidBody(body);
			body.Dispose();
		}

		public List<RigidBody> RigidBodies
		{
			get { return this.bodies; }
		}
		#endregion

		#region Soft Registry
		//Cache so we speed up process
		private List<SoftBody> softbodies = new List<SoftBody>();

		public void Register(SoftBody body)
		{
			this.World.AddSoftBody(body);
			this.softbodies.Add(body);
		}

		public void Unregister(SoftBody body)
		{
			this.softbodies.Remove(body);
			this.World.RemoveCollisionObject(body);
		}

		public List<SoftBody> SoftBodies
		{
			get { return this.softbodies; }
		}
		#endregion

		#region Rigid Registry
		//Cache so we speed up process
		private List<TypedConstraint> constraints = new List<TypedConstraint>();

		public void Register(TypedConstraint cst, bool collideconnected)
		{
			this.World.AddConstraint(cst, !collideconnected);
			this.constraints.Add(cst);
		}

		public void Register(TypedConstraint cst)
		{
			this.Register(cst, true);
		}

		public void Unregister(TypedConstraint cst)
		{
			this.constraints.Remove(cst);
			this.World.RemoveConstraint(cst);
			cst.Dispose();
		}

		public List<TypedConstraint> Constraints
		{
			get { return this.constraints; }
		}
		#endregion

		#region Creation
		private bool created = false;
		public bool Created { get { return this.created; } }

		public void Create()
		{
			if (created)
			{
				this.Destroy();
			}

			this.bodyindex = 0;
			this.cstindex = 0;

			collisionConfiguration = new SoftBodyRigidBodyCollisionConfiguration();
			dispatcher = new CollisionDispatcher(collisionConfiguration);
			solver = new SequentialImpulseConstraintSolver();
			overlappingPairCache = new DbvtBroadphase();
			dynamicsWorld = new SoftRigidDynamicsWorld(dispatcher, overlappingPairCache, solver, collisionConfiguration);
			worldInfo = new SoftBodyWorldInfo();
			worldInfo.Gravity = dynamicsWorld.Gravity;
			worldInfo.Broadphase = overlappingPairCache;
			worldInfo.Dispatcher = dispatcher;
			worldInfo.SparseSdf.Initialize();
			this.created = true;

			if (this.WorldHasReset != null)
			{
				this.WorldHasReset();
			}
		}
		#endregion

		public int GetNewBodyId()
		{
			this.bodyindex++;
			return this.bodyindex;
		}

		public int GetNewConstraintId()
		{
			this.cstindex++;
			return this.cstindex;
		}

		#region Process Deletion
		internal void ProcessDelete()
		{
            List<RigidBody> todelete = new List<RigidBody>();
            List<int> deleteid = new List<int>();

            int cnt = this.RigidBodies.Count;
            for (int i = 0; i < cnt; i++)
            {
                RigidBody body = this.RigidBodies[i];
                BodyCustomData bd = (BodyCustomData)body.UserObject;
                bd.Created = false;
                if (bd.MarkedForDeletion)
                {
                    todelete.Add(body);
                    deleteid.Add(bd.Id);
                }
            }

            for (int i = 0; i < todelete.Count; i++)
            {
                RigidBody body = todelete[i];
                if (this.RigidBodyDeleted != null)
                {
                    this.RigidBodyDeleted(body, deleteid[i]);
                }
                this.Unregister(body);
            }

			cnt = this.SoftBodies.Count;
			for (int i = 0; i < cnt; i++)
			{
				SoftBody body = this.SoftBodies[i];
				BodyCustomData bd = (BodyCustomData)body.UserObject;
				bd.Created = false;
				if (bd.MarkedForDeletion)
				{
					if (this.SoftBodyDeleted != null)
					{
						this.SoftBodyDeleted(body, bd.Id);
					}
					this.Unregister(body);
				}
			}

			cnt = this.Constraints.Count;
			for (int i = 0; i < cnt; i++)
			{
				TypedConstraint cst = this.constraints[i];
				ConstraintCustomData cd = (ConstraintCustomData)cst.UserObject;
				cd.Created = false;
				if (cd.MarkedForDeletion)
				{
					if (this.ConstraintDeleted != null)
					{
						this.ConstraintDeleted(cst, cd.Id);
					}
					this.Unregister(cst);
				}
			}
		}
		#endregion

		#region Info
		public SoftBodyWorldInfo WorldInfo
		{
			get { return worldInfo; }
			set { worldInfo = value; }
		}

		public SoftRigidDynamicsWorld World
		{
			get { return this.dynamicsWorld; }
			set { this.dynamicsWorld = value; }
		}

		public int ObjectCount
		{
			get { return this.dynamicsWorld.NumCollisionObjects; }
		}
		#endregion

		#region Gravity/Enabled/Ans Step Stuff
		public void SetGravity(float x, float y, float z)
		{
			this.gx = x;
			this.gy = y;
			this.gz = z;
			this.dynamicsWorld.Gravity = new Vector3(this.gx, this.gy, this.gz);
		}

		public bool Enabled
		{
			set { this.enabled = value; }
			get { return this.enabled; }
		}

		public float TimeStep
		{
			set { this.ts = value; }
		}

		public int Iterations
		{
			set { this.iter = value; }
		}

		public void Step()
		{
			if (this.enabled)
			{
				this.dynamicsWorld.StepSimulation(this.ts, this.iter);
			}
		}
		#endregion

		#region Destroy
		public void Destroy()
		{

			dynamicsWorld.Dispose();
			solver.Dispose();
			overlappingPairCache.Dispose();
			dispatcher.Dispose();
			collisionConfiguration.Dispose();
			foreach (RigidBody rb in this.bodies)
			{
				rb.Dispose();
			}
			foreach (SoftBody sb in this.softbodies)
			{
				sb.Dispose();
			}
			foreach (TypedConstraint tc in this.constraints)
			{
				tc.Dispose();
			}

			this.bodies.Clear();
			this.softbodies.Clear();
			this.constraints.Clear();
			this.created = false;
		}
		#endregion

	}
}
