using System;
using System.Collections.Generic;
using ColladaSlimDX.ColladaModel;
using VVVV.Utils.VMath;
using VVVV.SkeletonInterfaces;
using VVVV.Utils.SlimDX;

namespace VVVV.Nodes
{
	/// <summary>
	/// Description of BoneWrapper.
	/// </summary>
	public class BoneWrapper : IJoint
	{
		private string FName;
		private int FId;
		private IJoint FParent;
		private List<IJoint> FChildren;
		private List<Vector2D> FConstraints;
		
		private Matrix4x4 FBaseTransform;
		private Matrix4x4 FAnimationTransform;
		private Matrix4x4 FCachedCombinedTransform;
		private Vector3D FCachedTranslation;
		private Vector3D FCachedRotation;
		private Vector3D FCachedScale;
		private bool FDirty;
		
		public BoneWrapper(int id, string name, Matrix4x4 baseTransform)
		{
			FId = id;
			FName = name;
			FChildren = new List<IJoint>();
			
			FBaseTransform = baseTransform;
			FAnimationTransform = VMath.IdentityMatrix;
			FConstraints = new List<Vector2D>();
			FConstraints.Add(new Vector2D(-1.0, 1.0));
			FConstraints.Add(new Vector2D(-1.0, 1.0));
			FConstraints.Add(new Vector2D(-1.0, 1.0));
			SetDirty();
		}
		
		public string Name
		{
			set
			{
				FName = value;
			}
			get
			{
				return FName;
			}
		}
		
		public int Id
		{
			set
			{
				FId = value;
			}
			get
			{
				return FId;
			}
		}
		
		public Matrix4x4 BaseTransform
		{
			set
			{
				FBaseTransform = value;
				SetDirty();
			}
			get
			{
				return FBaseTransform;
			}
		}
		
		public Matrix4x4 AnimationTransform
		{
			set
			{
				FAnimationTransform = value;
				SetDirty();
			}
			get
			{
				return FAnimationTransform;
			}
		}
		
		public IJoint Parent
		{
			get
			{
				return FParent;
			}
			
			set
			{
				FParent = value;
				SetDirty();
			}
		}
		
		public List<IJoint> Children
		{
			get
			{
				return FChildren;
			}
		}
		
		public Vector3D Rotation
		{
			get
			{
				UpdateCachedValues();
				return FCachedRotation;
			}
		}
		
		public Vector3D Translation
		{
			get
			{
				UpdateCachedValues();
				return FCachedTranslation;
			}
		}
		
		public Vector3D Scale
		{
			get
			{
				UpdateCachedValues();
				return FCachedScale;
			}
		}
		
		public List<Vector2D> Constraints
		{
			get
			{
				return FConstraints;
			}
			set
			{
				FConstraints = value;
			}
		}
		
		public Matrix4x4 CombinedTransform
		{
			get
			{
				UpdateCachedValues();
				return FCachedCombinedTransform;
			}
		}
		
		public void CalculateCombinedTransforms()
		{
			UpdateCachedValues();
		}
		
		public void AddChild(IJoint joint)
		{
			joint.Parent = this;
			Children.Add(joint);
		}
		
		public void ClearAll()
		{
			Children.Clear();
		}
		
		public IJoint DeepCopy()
		{
			BoneWrapper copy = new BoneWrapper(Id, Name, new Matrix4x4(BaseTransform));
			copy.AnimationTransform = new Matrix4x4(AnimationTransform);
			
			foreach (IJoint child in Children)
				copy.AddChild(child.DeepCopy());
			
			for (int i = 0; i < 3; i++)
				copy.Constraints[i] = new Vector2D(Constraints[i]);
			
			return copy;
		}
		
		public bool IsDirty()
		{
			return FDirty;
		}
		
		public void SetDirty()
		{
			if (!IsDirty())
			{
				FDirty = true;
				foreach (IJoint joint in Children)
				{
					((BoneWrapper) joint).SetDirty();
				}
			}
		}
		
		private void UpdateCachedValues()
		{
			if (IsDirty())
			{
				AnimationTransform.Decompose(out FCachedScale, out FCachedRotation, out FCachedTranslation);
				if (Parent != null)
					FCachedCombinedTransform = AnimationTransform * BaseTransform * Parent.CombinedTransform;
				else
					FCachedCombinedTransform = AnimationTransform * BaseTransform;
				FDirty = false;
			}
		}
	}
}
