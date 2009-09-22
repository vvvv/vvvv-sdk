/*
 * Erstellt mit SharpDevelop.
 * Benutzer: Matthias Zauner, Elias
 * Datum: 18.09.2009
 * Zeit: 20:42
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Collections.Generic;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V1;

namespace VVVV.SkeletonInterfaces
{
	/// <summary>
	/// Description of MyClass.
	/// </summary>
	public interface IJoint
	{
		string Name
		{
			set;
			get;
		}
		
		int Id
		{
			set;
			get;
		}
		
		Matrix4x4 BaseTransform
		{
			set;
			get;
		}
		
		Matrix4x4 AnimationTransform
		{
			set;
			get;
		}
		
		IJoint Parent
		{
			get;
			set;
		}
		
		List<IJoint> Children
		{
			get;
		}
		
		
		Vector3D Rotation
		{
			get;
			set;
		}
		
		List<Vector2D> Constraints
		{
			get;
			set;
		}
		
		Matrix4x4 GetCombinedTransform();
		
		void CalculateCombinedTransforms();
		
		void AddChild(IJoint joint);
		
		IJoint DeepCopy();
		
	}
	
	public interface ISkeleton : INodeIOBase
	{
		IJoint Root
		{
			get;
			set;
		}
		
		Dictionary<string, IJoint> JointTable
		{
			get;
		}
		
		void InsertJoint(string parent, IJoint newJoint);
		
		void DeleteJoint(string jointName);
		
		void CalculateCombinedTransforms();
		
		ISkeleton DeepCopy();
		
	}
	
	public class Skeleton : ISkeleton
	{
		private IJoint root;
		public IJoint Root 
		{
			get 
			{
				return root;
			}
			
			set
			{
				root = value;
			}
		}
		
		private Dictionary<string, IJoint> jointTable;
		public Dictionary<string, IJoint> JointTable 
		{
			get 
			{
				return jointTable; 
			}
		}
		
		public Skeleton(IJoint root) 
		{
			this.Root = root;
			jointTable = new Dictionary<string, IJoint>();
		}
		
		public Skeleton()
		{
			jointTable = new Dictionary<string, IJoint>();
		}
		
		public void InsertJoint(string parentName, IJoint joint)
		{
			if (jointTable.ContainsKey(joint.Name))
			{
				throw new Exception("Joint name '" + joint.Name + "' already exists.");
			}
			
			IJoint parent;
			if (jointTable.TryGetValue(parentName, out parent))
			{
				throw new Exception("Unknown parent '" + parentName + "'.");
			}
			
			parent.Children.Add(joint);
			jointTable.Add(joint.Name, joint);
		}
		
		public void DeleteJoint(string jointName)
		{
			IJoint joint;
			if (jointTable.TryGetValue(jointName, out joint))
			{
				throw new Exception("Unknown joint '" + jointName + "'.");
			}
			IJoint parent = joint.Parent;
			parent.Children.Remove(joint);
			jointTable.Remove(joint.Name);
			
		}
		
		public ISkeleton DeepCopy()
		{
			ISkeleton skeleton = new Skeleton(this.Root.DeepCopy());
			AddToJointTable(skeleton.JointTable, skeleton.Root);
			return skeleton;
		}
		
		public void CalculateCombinedTransforms()
		{
			root.CalculateCombinedTransforms();
		}
		
		public void BuildJointTable()
		{
			jointTable = new Dictionary<string, IJoint>();
			AddToJointTable(jointTable, this.Root);
		}
		
		private static void AddToJointTable(Dictionary<string, IJoint> jointTable, IJoint joint)
		{
			jointTable.Add(joint.Name, joint);
			foreach (IJoint j in joint.Children)
			{
				AddToJointTable(jointTable, j);
			}
		}
	}
}
