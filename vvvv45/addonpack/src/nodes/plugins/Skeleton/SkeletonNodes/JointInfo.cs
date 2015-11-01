using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V1;
using VVVV.SkeletonInterfaces;

namespace VVVV.Nodes
{
	[Serializable]
	public class JointInfo : IJoint
	{
		private string name;
		public string shortname;
		public int index;
		public List<IJoint> children;
		public IJoint parent;
		public Matrix4x4 baseTransform;
		public Matrix4x4 animationTransform;
		public Vector3D rotation;
		public Matrix4x4 preTransform;
		public System.Guid guid;
		public List<Vector2D> constraints; 
		
		public IPluginHost FHost;
		
		public string Name{
			set { this.name = value+guid.ToString(); this.shortname = value; }
			get { return this.shortname; }
		}
		
		public JointInfo(string n)
		{
			guid = System.Guid.NewGuid();
			name = n+guid.ToString();
			index = 0;
			shortname = n;
			children = new List<IJoint>();
			baseTransform = new Matrix4x4(VMath.IdentityMatrix);
			animationTransform = new Matrix4x4(VMath.IdentityMatrix);
			rotation = new Vector3D(0);
			preTransform = new Matrix4x4(VMath.IdentityMatrix);
			constraints = new List<Vector2D>();
			constraints.Add(new Vector2D(-1.0, 1.0));
			constraints.Add(new Vector2D(-1.0, 1.0));
			constraints.Add(new Vector2D(-1.0, 1.0));
		}
		
		public JointInfo()
		{
			guid = System.Guid.NewGuid();
			name = guid.ToString();
			index = 0;
			shortname = "";
			children = new List<IJoint>();
			baseTransform = new Matrix4x4(VMath.IdentityMatrix);
			animationTransform = new Matrix4x4(VMath.IdentityMatrix);
			rotation = new Vector3D(0);
			preTransform = new Matrix4x4(VMath.IdentityMatrix);
			constraints = new List<Vector2D>();
			constraints.Add(new Vector2D(-1.0, 1.0));
			constraints.Add(new Vector2D(-1.0, 1.0));
			constraints.Add(new Vector2D(-1.0, 1.0));
		}
		
		public void setTransform(Matrix4x4 t)
		{
			baseTransform = t;
		}
		
		public void setTransform2(Matrix4x4 t)
		{
			animationTransform = t;
		}
		
		public string calculatePreTransform(Matrix4x4 parentTransform)
		{
			string ret = "";
			this.preTransform = parentTransform;
			IEnumerator childrenEnum = this.children.GetEnumerator();
			while (childrenEnum.MoveNext())
			{
				ret += ((JointInfo)childrenEnum.Current).calculatePreTransform(CombinedTransform);
			}
			return ret;
		}
		
		public string print()
		{
			string ret = this.name+" [";
			for (int i=0; i<children.Count; i++)
			{
				ret += (string)children[i].Name+"-";
			}
			ret+="]";
			return ret;
		}
		
		public JointInfo clone()
		{
			return clone(null);
		}
		
		public JointInfo clone(JointInfo parent)
		{
			JointInfo ret = new JointInfo();
			ret.guid = new System.Guid(this.guid.ToString());
			ret.Name = this.Name;
			ret.index = this.index;
			ret.parent = parent;
			
			for (int i=0; i<=15; i++)
			{
				ret.baseTransform[i] = this.baseTransform[i];
				ret.animationTransform[i] = this.animationTransform[i];
				ret.preTransform[i] = this.preTransform[i];
			}
			
			for (int i=0; i<3; i++)
			{
				ret.rotation[i] = this.rotation[i];
			}
			
			ret.constraints.Clear();
			for (int i=0; i<3; i++)
			{
				Vector2D v = new Vector2D(this.constraints[i].x, this.constraints[i].y);
				ret.constraints.Add(v);
			}
			
			JointInfo child;
			for (int i=0; i<this.children.Count; i++)
			{
				child = ((JointInfo)this.children[i]).clone(ret);
				ret.children.Add(child);
			}
			
			return ret;
		}
		
		// interfaces implementations inherited for IJoint
		
		public int Id{
			get
			{
				return index;
			}
			set
			{
				index = value;
			}
		}
		
		public List<IJoint> Children{
			get
			{
				return children;
			}
			set
			{
				children = value;
			}
		}
		
		
		public IJoint Parent{
			get
			{
				return parent;
			}
			set
			{
				if (parent!=null)
					parent.Children.Remove(this);
				parent = value;
				parent.Children.Add(this);
			}
		}
		
		public Vector3D Rotation
		{
			get
			{
				Vector3D translation = new Vector3D(0);
				Vector3D rot = new Vector3D(0);
				Vector3D scale = new Vector3D(0);
				//VSlimDXUtils.Decompose(this.AnimationTransform, out scale, out rot, out translation);
				return rot;
			}
		}
		
		public Vector3D Translation
		{
			get
			{
				return new Vector3D(0);
			}
		}
		
		public Vector3D Scale
		{
			get
			{
				return new Vector3D(0);
			}
		}
		
		public List<Vector2D> Constraints
		{
			get
			{
				return this.constraints;
			}
			set
			{
				constraints = value;
			}
		}
		
		public Matrix4x4 AnimationTransform
		{
			get
			{
				return animationTransform;
			}
			set
			{
				animationTransform = value;
			}
		}
		
		public Matrix4x4 BaseTransform
		{
			get
			{
				return baseTransform;
			}
			set
			{
				baseTransform = value;
			}
		}
		
		public Matrix4x4 CombinedTransform
		{
			get
			{
				return this.AnimationTransform * this.BaseTransform * this.preTransform;
			}
		}
		
		public void CalculateCombinedTransforms()
		{
			this.calculatePreTransform(VMath.IdentityMatrix);
		}
		
		public void CalculateCombinedTransforms(Matrix4x4 pre)
		{
			this.calculatePreTransform(pre);
		}
		
		
		public void AddChild(IJoint joint)
		{
			joint.Parent = this;
		}
		
		public void ClearAll()
		{
			this.children = new List<IJoint>();
		}
		
		public IJoint DeepCopy()
		{
			IJoint copy = this.clone();
			return copy;
		}

		/*public Matrix4x4 ConstrainedRotation
		{
			get
			{
				return VMath.Rotate(VMath.Map(this.Rotation.x, -1.0, 1.0, 2*Math.PI*this.Constraints[0].x, 2*Math.PI*this.Constraints[0].y, TMapMode.Clamp),
						            VMath.Map(this.Rotation.y, -1.0, 1.0, 2*Math.PI*this.Constraints[1].x, 2*Math.PI*this.Constraints[1].y, TMapMode.Clamp),
						            VMath.Map(this.Rotation.z, -1.0, 1.0, 2*Math.PI*this.Constraints[2].x, 2*Math.PI*this.Constraints[2].y, TMapMode.Clamp));
			}
		}
		*/
	}
}
