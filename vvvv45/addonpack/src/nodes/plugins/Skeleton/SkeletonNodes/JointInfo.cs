/*
 * Erstellt mit SharpDevelop.
 * Benutzer: matthias
 * Datum: 08.01.2009
 * Zeit: 23:08
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VVVV.Utils.VMath;
using System.Globalization;
using VVVV.PluginInterfaces.V1;
using VVVV.SkeletonInterfaces;

namespace VVVV.Nodes
{
	/// <summary>
	/// Description of JointInfo.
	/// </summary>
	/// 
	[Serializable]
	public class JointInfo : IJoint
	{
		
		private string name;
		public string shortname;
		public int index;
		public List<IJoint> children;
		public IJoint parent;
		public Matrix4x4 world2Joint;
		public Matrix4x4 transform;
		public Matrix4x4 transform2;
		public Vector3D rotation;
		public Hashtable skinWeights;
		public Vector3D worldPos;
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
			skinWeights = new Hashtable();
			transform = new Matrix4x4(VMath.IdentityMatrix);
			transform2 = new Matrix4x4(VMath.IdentityMatrix);
			rotation = new Vector3D(0);
			world2Joint = new Matrix4x4(VMath.IdentityMatrix);
			worldPos = new Vector3D(0);
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
			skinWeights = new Hashtable();
			transform = new Matrix4x4(VMath.IdentityMatrix);
			transform2 = new Matrix4x4(VMath.IdentityMatrix);
			rotation = new Vector3D(0);
			world2Joint = new Matrix4x4(VMath.IdentityMatrix);
			worldPos = new Vector3D(0);
			preTransform = new Matrix4x4(VMath.IdentityMatrix);
			constraints = new List<Vector2D>();
			constraints.Add(new Vector2D(-1.0, 1.0));
			constraints.Add(new Vector2D(-1.0, 1.0));
			constraints.Add(new Vector2D(-1.0, 1.0));
		}
		

		
		public void setTransform(Matrix4x4 t)
		{
			transform = t;
		}
		
		public void setTransform2(Matrix4x4 t)
		{
			transform2 = t;
		}
		
		public string calculatePreTransform(Matrix4x4 parentTransform, Matrix4x4 parentWorld2Joint)
		{
			string ret = "";
			this.preTransform = parentTransform;
			this.world2Joint = parentWorld2Joint* VMath.Inverse(this.transform);
			ret = this.name+"\n======\n";
			ret += this.transform.ToString()+"\n";
			ret += this.world2Joint.ToString()+"\n-----\n";
			IEnumerator childrenEnum = this.children.GetEnumerator();
			while (childrenEnum.MoveNext())
			{
				ret += ((JointInfo)childrenEnum.Current).calculatePreTransform(CombinedTransform, this.world2Joint);
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
		
		public JointInfo clone(bool includeSkinWeights)
		{
			JointInfo ret = new JointInfo();
			ret.guid = new System.Guid(this.guid.ToString());
			ret.Name = this.Name;
			ret.index = this.index;
			JointInfo child;
			for (int i=0; i<=15; i++)
			{
				ret.transform[i] = this.transform[i];
				ret.transform2[i] = this.transform2[i];
				ret.world2Joint[i] = this.world2Joint[i];
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
			
			for (int i=0; i<3; i++)
			{
				ret.worldPos[i] = this.worldPos[i];
			}
			for (int i=0; i<this.children.Count; i++)
			{
				child = ((JointInfo)this.children[i]).clone(includeSkinWeights);
				ret.children.Add(child);
			}
			if (includeSkinWeights)
			{
				IDictionaryEnumerator weightEnum = skinWeights.GetEnumerator();
				while (weightEnum.MoveNext())
				{
					ret.skinWeights.Add((int)weightEnum.Key, (double)weightEnum.Value);
				}
			}
			return ret;
		}
		
		public JointInfo clone()
		{
			return this.clone(true);
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
				return this.rotation;
			}
			set
			{
				rotation = value;
			}
		}
		
		public Vector3D Translation
		{
			get
			{
				return new Vector3D();
			}
		}
		
		public Vector3D Scale
		{
			get
			{
				return new Vector3D(1.0, 1.0, 1.0);
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
				return transform2;
			}
			set
			{
				transform2 = value;
			}
		}
		
		public Matrix4x4 BaseTransform
		{
			get
			{
				return transform;
			}
			set
			{
				transform = value;
			}
		}
		
		public Matrix4x4 CombinedTransform
		{
			get
			{
				return ConstrainedRotation * this.transform2*this.transform*this.preTransform;
			}
		}
		
		public void CalculateCombinedTransforms()
		{
			this.calculatePreTransform(VMath.IdentityMatrix, VMath.IdentityMatrix);
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
			return this.clone();
		}
		
		public Matrix4x4 ConstrainedRotation
		{
			get
			{
				return VMath.Rotate(VMath.Map(this.Rotation.x, -1.0, 1.0, 2*Math.PI*this.Constraints[0].x, 2*Math.PI*this.Constraints[0].y, TMapMode.Clamp),
						            VMath.Map(this.Rotation.y, -1.0, 1.0, 2*Math.PI*this.Constraints[1].x, 2*Math.PI*this.Constraints[1].y, TMapMode.Clamp),
						            VMath.Map(this.Rotation.z, -1.0, 1.0, 2*Math.PI*this.Constraints[2].x, 2*Math.PI*this.Constraints[2].y, TMapMode.Clamp));
			}
		}
		
	}
	
}
