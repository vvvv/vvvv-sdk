#region licence/info

//////project name
//Occurrence

//////description
//counts the occurrence of equal slices

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.PluginInterfaces.V2
//VVVV.Utlis.VColor;
//VVVV.Utils.VMath;

//////initial author
//woei

#endregion licence/info

//use what you need
using System;
using System.ComponentModel.Composition;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	[PluginInfo(Name = "Occurrence", 
	            Category = "Value",
	            Help = "Counts the occurrence of equal slices",
	            Tags = "count, spectral, spread",
	           	Author = "woei")]
	public class OccurrenceValue: Occurrence<double>
	{
		[Input("Epsilon", IsSingle = true, MinValue=0, Order = 1)]
    	IDiffSpread<double> FEps;
		
		public override bool Equals(double a, double b)
		{
			return (a >= b-FEps[0] && a <= b+FEps[0]);
		}
		
		public override void Evaluate(int SpreadMax)
		{
			if(FEps.IsChanged)
				eval = true;
			base.Evaluate(SpreadMax);
		} 
	}

    [PluginInfo(Name = "Occurrence",
                Category = "2d",
                Help = "Counts the occurrence of equal slices",
                Tags = "count, spectral, spread",
                Author = "woei")]
    public class OccurrenceVector2D : Occurrence<Vector2D>
    {
        [Input("Epsilon", IsSingle = true, MinValue = 0, Order = 1)]
        IDiffSpread<Vector2D> FEps;

        public override bool Equals(Vector2D a, Vector2D b)
        {
            return  (a.x >= b.x - FEps[0].x && a.x <= b.x + FEps[0].x) &&
                    (a.y >= b.y - FEps[0].y && a.y <= b.y + FEps[0].y);
        }

        public override void Evaluate(int SpreadMax)
        {
            if (FEps.IsChanged)
                eval = true;
            base.Evaluate(SpreadMax);
        }
    }

    [PluginInfo(Name = "Occurrence",
                Category = "3d",
                Help = "Counts the occurrence of equal slices",
                Tags = "count, spectral, spread",
                Author = "woei")]
    public class OccurrenceVector3D : Occurrence<Vector3D>
    {
        [Input("Epsilon", IsSingle = true, MinValue = 0, Order = 1)]
        IDiffSpread<Vector3D> FEps;

        public override bool Equals(Vector3D a, Vector3D b)
        {
            return  (a.x >= b.x - FEps[0].x && a.x <= b.x + FEps[0].x) &&
                    (a.y >= b.y - FEps[0].y && a.y <= b.y + FEps[0].y) &&
                    (a.z >= b.z - FEps[0].z && a.z <= b.z + FEps[0].z);
        }

        public override void Evaluate(int SpreadMax)
        {
            if (FEps.IsChanged)
                eval = true;
            base.Evaluate(SpreadMax);
        }
    }

    [PluginInfo(Name = "Occurrence",
                Category = "4d",
                Help = "Counts the occurrence of equal slices",
                Tags = "count, spectral, spread",
                Author = "woei")]
    public class OccurrenceVector4D : Occurrence<Vector4D>
    {
        [Input("Epsilon", IsSingle = true, MinValue = 0, Order = 1)]
        IDiffSpread<Vector4D> FEps;

        public override bool Equals(Vector4D a, Vector4D b)
        {
            return  (a.x >= b.x - FEps[0].x && a.x <= b.x + FEps[0].x) &&
                    (a.y >= b.y - FEps[0].y && a.y <= b.y + FEps[0].y) &&
                    (a.z >= b.z - FEps[0].z && a.z <= b.z + FEps[0].z) &&
                    (a.w >= b.w - FEps[0].w && a.w <= b.w + FEps[0].w);
        }

        public override void Evaluate(int SpreadMax)
        {
            if (FEps.IsChanged)
                eval = true;
            base.Evaluate(SpreadMax);
        }
    }
	
	[PluginInfo(Name = "Occurrence", 
	            Category = "String",
	            Help = "Counts the occurrence of equal slices",
	            Tags = "count, spectral, spread",
	           	Author = "woei")]
	public class OccurrenceString: Occurrence<string>
	{
		[Input("Case Sensitive", IsSingle = true, Order = 1)]
		IDiffSpread<bool> FCase;
		
		public override bool Equals(string a, string b)
		{
			if (!FCase[0])
			{
				a = a.ToLower();
				b = b.ToLower();
			}
			
			return a == b;
		}
		
		public override void Evaluate(int SpreadMax)
		{
			if(FCase.IsChanged)
				eval = true;
			base.Evaluate(SpreadMax);
		} 
	}
	
	
	[PluginInfo(Name = "Occurrence", 
	            Category = "Color",
	            Help = "Counts the occurrence of equal slices",
	            Tags = "count, spectral, spread",
	           	Author = "woei")]
	public class OccurrenceColor: Occurrence<RGBAColor>
	{
		[Input("Epsilon", IsSingle = true, Order = 1)]
    	IDiffSpread<Vector4D> FEps;
    	
		public override bool Equals(RGBAColor a, RGBAColor b)
		{
			return 	(a.R >= b.R-FEps[0].x && a.R <= b.R+FEps[0].x) &&
					(a.G >= b.G-FEps[0].x && a.G <= b.G+FEps[0].y) &&
					(a.B >= b.B-FEps[0].z && a.B <= b.B+FEps[0].z) &&
					(a.A >= b.A-FEps[0].w && a.A <= b.A+FEps[0].w);
		}
		
		public override void Evaluate(int SpreadMax)
		{
			if(FEps.IsChanged)
				eval = true;
			base.Evaluate(SpreadMax);
		} 
	}
	
	[PluginInfo(Name = "Occurrence", 
	            Category = "Enumerations",
	            Help = "Counts the occurrence of equal slices",
	            Tags = "count, spectral, spread",
	           	Author = "woei")]
	public class OccurrenceEnum: Occurrence<EnumEntry> {}
	
	[PluginInfo(Name = "Occurrence", 
	            Category = "Transform",
	            Help = "Counts the occurrence of equal slices",
	            Tags = "count, spectral, spread",
	           	Author = "woei")]
	public class OccurrenceTransform: Occurrence<Matrix4x4> {}
}
