using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VVVV.Utils.VMath;

namespace VVVV.SkeletonInterfaces
{
	[ComVisible(false)]
	public interface IJoint
	{
		string Name
		{
			set;
			get;
		}
		
        /// <summary>
        /// The index in the skinning list. Set to -1 if the joint doesn't take part in skinning.
        /// </summary>
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
		}
		
		Vector3D Translation
		{
			get;
		}
		
		Vector3D Scale
		{
			get;
		}
		
		List<Vector2D> Constraints
		{
			get;
			set;
		}
		
		Matrix4x4 CombinedTransform
		{
			get;
		}
		
		void CalculateCombinedTransforms();
		
		void AddChild(IJoint joint);
		
		void ClearAll();
		
		IJoint DeepCopy();
		
	}
	
	[ComVisible(false)]
	public interface ISkeleton
	{
		IJoint Root
		{
			get;
			set;
		}
		
		Guid Uid
		{
			get;
			set;
		}
		
		Dictionary<string, IJoint> JointTable
		{
			get;
		}
		
		IJoint Selected
		{
			get;
			set;
		}
		
		void InsertJoint(string parent, IJoint newJoint);
		
		void DeleteJoint(string jointName);
		
		void CalculateCombinedTransforms();
		
		void BuildJointTable();
		
		void ClearAll();
		
		ISkeleton DeepCopy();
		
	}
	
	public class SkeletonNodeIO
	{
		private static Guid FGuid;
		public static Guid GUID
		{
			get
			{
				if (FGuid == Guid.Empty)
					FGuid = new Guid("AB312E34-8025-40F2-8241-1958793F3D39");
				return FGuid;
			}
		}
		
		public static string FriendlyName = "Skeleton";
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
		
		private Guid uid;
		public Guid Uid
		{
			get
			{
				return uid;
			}
			set
			{
				uid = value;
			}
		}
		
		public void RenewUid()
		{
			uid = System.Guid.NewGuid();
		}
			
			
		private IJoint selectedJoint;
		public IJoint Selected
		{
			get
			{
				if (selectedJoint!=null)
					return selectedJoint;
				else
					return root;
			}
			set
			{
				selectedJoint = value;
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
			this.RenewUid();
		}
		
		public Skeleton()
		{
			jointTable = new Dictionary<string, IJoint>();
			this.RenewUid();
		}
		
		public void InsertJoint(string parentName, IJoint joint)
		{
			// If we don't have a root accept everything.
			if (Root == null)
			{
				Root = joint;
				jointTable.Add(joint.Name, joint);
				return;
			}
			
			if (jointTable.ContainsKey(joint.Name))
			{
				throw new Exception("Joint name '" + joint.Name + "' already exists.");
			}
			
			IJoint parent;
			if (!jointTable.TryGetValue(parentName, out parent))
			{
				throw new Exception("Unknown parent '" + parentName + "'.");
			}
			
			parent.AddChild(joint);
			jointTable.Add(joint.Name, joint);
		}
		
		public void DeleteJoint(string jointName)
		{
			IJoint joint;
			if (!jointTable.TryGetValue(jointName, out joint))
			{
				throw new Exception("Unknown joint '" + jointName + "'.");
			}
			IJoint parent = joint.Parent;
			parent.Children.Remove(joint);
			jointTable.Remove(joint.Name);
			if (jointTable.Count == 0)
				Root = null;
		}
		
		public ISkeleton DeepCopy()
		{
			ISkeleton skeleton = new Skeleton(this.Root.DeepCopy());
			skeleton.Uid = this.Uid;
			AddToJointTable(skeleton.JointTable, skeleton.Root);
			if (this.Selected!=null && skeleton.JointTable[this.Selected.Name]!=null)
				skeleton.Selected = skeleton.JointTable[this.Selected.Name];
			return skeleton;
		}
		
		public void CalculateCombinedTransforms()
		{
			root.CalculateCombinedTransforms();
		}
		
		public void ClearAll()
		{
			if (Root != null)
				Root.ClearAll();
			Root = null;
			this.jointTable = new Dictionary<string, IJoint>();
			this.BuildJointTable();
			this.RenewUid();
		}
		
		public void BuildJointTable()
		{
			jointTable = new Dictionary<string, IJoint>();
			if (Root != null)
				AddToJointTable(jointTable, this.Root);
			
			// TODO: ID Vergabe anschaun!
			/*
			int currId = 0;
			foreach (KeyValuePair<string, IJoint> pair in jointTable)
			{
				pair.Value.Id = currId;
				currId++;
			}
			*/
		}
		
		private static void AddToJointTable(Dictionary<string, IJoint> jointTable, IJoint joint)
		{
			jointTable.Add(joint.Name, joint);
			for (int i=0; i<joint.Children.Count; i++)
			{
				IJoint j = joint.Children[i];
				AddToJointTable(jointTable, j);
			}
		}
	}
}
