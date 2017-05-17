using System;
using System.Collections;
using System.Collections.Generic;

using VVVV.Utils.VMath;

namespace VVVV.Nodes.Lindenmayer
{
	public class TLindenmayer
	{
		#region field declaration
		private string FAxiom;
		
		private int FDepth;
		private int FSeed;

		private double FBranchLength;
		private double FBranchLengthDeviation;
		private double FAngle;
		private double FAngleDeviation;
				
		public string Axiom
		{
			set{FAxiom = value;}
		}
		
		public int Depth
		{
			set{FDepth = value;}
		}
		
		public int Seed
		{
			set{FSeed = value;}
		}
		
		public double BranchLength
		{
			set{FBranchLength = value;}
		}
		
		public double BranchLengthDeviation
		{
			set{FBranchLengthDeviation = value;}
		}
		
		public double Angle
		{
			set{FAngle = value;}
		}
		
		public double AngleDeviation
		{
			set{FAngleDeviation = value;}
		}
		
		public int BranchCount
		{
			get {return FTransforms.Count;}
		}
		
		public ArrayList Transforms
		{
			get {return FTransforms;}
		}

		public ArrayList TransformsG
		{
			get {return FTransformsG;}
		}
		
		public ArrayList Level
		{
			get {return FLevels;}
		}
		
		public ArrayList GAtFSlice
		{
			get {return FGAtFSlice;}
		}


		public ArrayList ProductionsF
		{
			get {return FProductionsF;}
		}
		
		public ArrayList ProductionsG
		{
			get {return FProductionsG;}
		}
		
        private double FRotY, FRotZ = 0;
        private double FCurrentX, FCurrentY, FCurrentZ = 0;
        private Matrix4x4 FCurrentTransform;
        private int FCurrentBranchCount = 0;
        
        private ArrayList FProductionsF = new ArrayList();
        private ArrayList FProductionsG = new ArrayList();
        private ArrayList FTransforms = new ArrayList();
        private ArrayList FTransformsG = new ArrayList();
        private ArrayList FLevels = new ArrayList();
        private ArrayList FGAtFSlice = new ArrayList();
        
        #endregion field declaration
		
		public TLindenmayer()
		{
		}
		
		public void Evaluate()
		{
			FRotY = -Math.PI / 2;
        	FRotZ = 0;
        	FCurrentX = FCurrentY = FCurrentZ = 0;
        	
        	FCurrentTransform = VMath.IdentityMatrix;
        	FCurrentBranchCount = 0;

        	Random rnd = new Random(FSeed);
        	
        	FTransforms.Clear();
        	FTransformsG.Clear();
        	FLevels.Clear();
        	FGAtFSlice.Clear();
        	
        	//start recursion
        	Recurse(FAxiom, rnd, FDepth, FBranchLength, FBranchLengthDeviation, FAngle, FAngleDeviation);
		}
		
		void Recurse(string Start, Random Rnd, int Depth, double Length, double LengthDev, double Angle, double AngleDev)
		{
			char c;
			
			Matrix4x4 FTempTransform = VMath.IdentityMatrix;
			double TempX = 0;
			double TempY = 0;
			double TempZ = 0;
			double TempRotY = 0;
			double TempRotZ = 0;
			int TempBranchCount = 0;
			
			if (Depth == 0)
				return;
			
			Depth -= 1;
			
			for (int i=0; i<Start.Length; i++)
			{
				c = Start[i];
				
				switch(c)
				{
					case 'F':
						{
							FCurrentBranchCount += 1;
							
							string production = "";
							if (FProductionsF.Count > 0)
							{
								production = (string) FProductionsF[Rnd.Next(FProductionsF.Count)];
								Recurse(production, Rnd, Depth, Length, LengthDev, Angle, AngleDev);
							}
							
							if (Depth == 0)
							{
								double r = Rnd.Next(-360, 360) / 360.0 * Math.PI;
								
								double yaw = FRotY + FRotY * r * AngleDev;
								double pitch = FRotZ + FRotZ * r * AngleDev;
								
								double ys = Math.Sin(yaw);
								double yc = Math.Cos(yaw);
								double ps = Math.Sin(pitch);
								double pc = Math.Cos(pitch);
								
								r = Rnd.Next(100) / 50.0 - 1;
								double len = Math.Abs(Length + Length * r * LengthDev);
								
								FCurrentX -= ys * pc * len;
								FCurrentY += ps * len;
								FCurrentZ -= yc * pc * len;
								
								FTransforms.Add(VMath.Scale(0.1, len, 0.1) * VMath.Translate(0, -len/2, 0) * 
								                VMath.Rotate(-(yaw+Math.PI*0.5), 0, -pitch) * VMath.Translate(FCurrentY, FCurrentX, FCurrentZ));
								FLevels.Add(FCurrentBranchCount);
							}
							break;
						}
					case 'G':
						{
							string production = "";
							if (FProductionsG.Count > 0)
							{
								System.Diagnostics.Debug.WriteLine("here: " +FProductionsG[0] + "-");
								production = (string) FProductionsG[Rnd.Next(FProductionsG.Count)];
								Recurse(production, Rnd, Depth, Length, LengthDev, Angle, AngleDev);
							}
							
							if (Depth == 0)
							{
								double r = Rnd.NextDouble();
							
								if (r>0.5)
								  r = -Math.PI / 2;
								else
								  r = Math.PI / 2;
								
								double yaw = 0; //FRotY + FRotY;
								double pitch = FRotZ;// + FRotZ;
								FTransformsG.Add(VMath.Rotate(-yaw, 0, -pitch+r) * VMath.Translate(FCurrentY, FCurrentX, FCurrentZ));
								FGAtFSlice.Add(FTransforms.Count);
							}
							
							break;
						}
					case '-':
						{
							FRotZ -= Angle * 2 * Math.PI;
							break;
						}
					case '+':
						{
							FRotZ += Angle * 2 * Math.PI;
							break;
						}
					case '/':
						{
							FRotY -= Angle * 2 * Math.PI;
							break;
						}
					case '\\':
						{
							FRotY += Angle * 2 * Math.PI;
							break;
						}
					case '[':
						{
							FTempTransform = FCurrentTransform;
							TempX = FCurrentX;
							TempY = FCurrentY;
							TempZ = FCurrentZ;
							TempRotY = FRotY;
							TempRotZ = FRotZ;
							TempBranchCount = FCurrentBranchCount;
							break;
						}
					case ']':
						{
							FCurrentTransform = FTempTransform;
							FCurrentX = TempX;
							FCurrentY = TempY;
							FCurrentZ = TempZ;
							FRotY = TempRotY;
							FRotZ = TempRotZ;
							FCurrentBranchCount = TempBranchCount;
							break;
						}
				}
			}
		}
	}
}
