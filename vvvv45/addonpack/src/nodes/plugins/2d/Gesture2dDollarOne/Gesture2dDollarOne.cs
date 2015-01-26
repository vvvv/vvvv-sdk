#region license
/**
 * The $1 Unistroke Recognizer (VVVV Version)
 * VVVV port from C# example by motzi, 2015
 * The port uses code from the original example code found here:
 * http://depts.washington.edu/aimgroup/proj/dollar/index.html
 * 
 * The new-BSD license from the original license (see below) applies.
**/
/**
 * The $1 Unistroke Recognizer (C# version)
 *
 *		Jacob O. Wobbrock, Ph.D.
 * 		The Information School
 *		University of Washington
 *		Mary Gates Hall, Box 352840
 *		Seattle, WA 98195-2840
 *		wobbrock@u.washington.edu
 *
 *		Andrew D. Wilson, Ph.D.
 *		Microsoft Research
 *		One Microsoft Way
 *		Redmond, WA 98052
 *		awilson@microsoft.com
 *
 *		Yang Li, Ph.D.
 *		Department of Computer Science and Engineering
 * 		University of Washington
 *		The Allen Center, Box 352350
 *		Seattle, WA 98195-2840
 * 		yangli@cs.washington.edu
 *
 * The Protractor enhancement was published by Yang Li and programmed here by 
 * Jacob O. Wobbrock.
 *
 *	Li, Y. (2010). Protractor: A fast and accurate gesture 
 *	  recognizer. Proceedings of the ACM Conference on Human 
 *	  Factors in Computing Systems (CHI '10). Atlanta, Georgia
 *	  (April 10-15, 2010). New York: ACM Press, pp. 2169-2172.
 * 
 * This software is distributed under the "New BSD License" agreement:
 * 
 * Copyright (c) 2007-2011, Jacob O. Wobbrock, Andrew D. Wilson and Yang Li.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 *    * Neither the names of the University of Washington nor Microsoft,
 *      nor the names of its contributors may be used to endorse or promote 
 *      products derived from this software without specific prior written
 *      permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS
 * IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Jacob O. Wobbrock OR Andrew D. Wilson
 * OR Yang Li BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
**/
#endregion license

#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{

	#region PluginInfo
	[PluginInfo(Name = "Resample", 
		Category = "2d Vector Distancebased", 
		Help = "Resamples a Spread of 2d Vectors according to a specified distance between points", 
		Tags = "linear,interpolation",
		Author = "motzi",
		Credits = "Jacob Wobbrock"
	)]
	#endregion PluginInfo
	
	public class Vector2dDistanceResampleNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<ISpread<Vector2D>> FPoints;
		
		[Input("StepSize", DefaultValue = 1.0)]
		public ISpread<double> FStep;
		
		[Input("MinStepSize", DefaultValue = 0.01)]
		public ISpread<double> FMinStep;

		[Output("Output")]
		public ISpread<ISpread<Vector2D>> FResampled;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			
			if (FPoints.SliceCount == 0 || FStep.SliceCount == 0 || FMinStep.SliceCount == 0)
			{	
				FResampled.SliceCount = 0;
				return;
			}
			
			FResampled.SliceCount = FPoints.SliceCount;
			
			for(int i=0; i<FPoints.SliceCount; i++)
			{
				if(FPoints[i].SliceCount == 0)
				{
					FResampled[i].SliceCount = 1;
					FResampled[i][0] = new Vector2D();
					break;
				}
				
				// security mesure to not allow stepsize of zero
				double px = FMinStep[i];
				if (FStep[i] != 0)
					px = FStep[i];
				
				
				IList<Vector2D> srcPts = new List<Vector2D>(FPoints[i]);  // copy source points so we can insert into them
				IList<Vector2D> dstPts = new List<Vector2D>();  // create the destination points that we return
	            dstPts.Add(srcPts[0]); // add the first source point as the first destination point
				
				double D = 0.0; // accumulated distance
	            
				for (int j = 1; j < srcPts.Count; j++)
	            {
	                Vector2D pt1 = srcPts[j - 1];
	                Vector2D pt2 = srcPts[j];
	
	                double d = VMath.Dist(pt1, pt2); // distance in space
	                if ((D + d) >= px) // has enough space been traversed in the last step?
	                {
	                    double qx = pt1.x + ((px - D) / d) * (pt2.x - pt1.x); // interpolate position
	                    double qy = pt1.y + ((px - D) / d) * (pt2.y - pt1.y); // interpolate position
	                    Vector2D q = new Vector2D(qx, qy); 
	                    dstPts.Add(q); // append new point 'q'
	                    srcPts.Insert(j, q); // insert 'q' at position j in points s.t. 'q' will be the next i
	                    D = 0.0;
	                }
	                else
	                {
	                    D += d; // accumulator
	                }
	            }
	            
				// unless px divides evenly into the path length (not likely), we will have some accumulation
	            // left over, so just add the last point as the last point, which will not be the full interval.
	            if (D > 0.000001)
	            {
	                dstPts.Add(srcPts[srcPts.Count - 1]);
	            }
				
	            FResampled[i].AssignFrom(dstPts);
				//FResampled.Insert(0, dstPts.ToSpread());
			}
		}
	}
	
	
	#region PluginInfo
	[PluginInfo(Name = "Vectorize", 
		Category = "2d Vector", 
		Help = "Normalizes a set of 2d vectors", 
		Tags = "$1, DollarOne",
		Author = "motzi",
		Credits = "Jacob Wobbrock, Yang Li"
	)]
	#endregion PluginInfo
	public class SquareNormalizeNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<ISpread<Vector2D>> FInput;
		
		[Input("Squarify")]
		public ISpread<bool> FSquare;
		
		[Output("Output")]
		public ISpread<ISpread<Vector2D>> FOutput;
		
//		[Import()]
//		public ILogger FLogger;
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInput.SliceCount == 0 || FInput[0].SliceCount == 0)
			{
				FOutput.SliceCount = 0;
				return;
			}	
			
			FOutput.SliceCount = FInput.SliceCount;
			
			for(int i=0; i<FInput.SliceCount; i++)
			{
				List<Vector2D> originals = new List<Vector2D>(FInput[i]);
				List<Vector2D> points = new List<Vector2D>();
				Vector2D centroid = Centroid(originals);
				
				double radians = Angle(centroid, originals[0], false);
	            points = RotatePoints(originals, centroid, -radians);
	            if(FSquare[i])
					points = ScaleTo(points, new Vector2D(1,1));
	            points = TranslateTo(points, new Vector2D(0,0));
	            List<Vector2D> vector = Vectorize(points); // candidate's vector representation
				
				FOutput[i].AssignFrom(vector);
			}
		}
		
		private Vector2D Centroid(List<Vector2D> points)
		{
			double wxsum = 0.0; // weighted sum of x values
            double wysum = 0.0; // weighted sum of y values
			
            for (int i = 0; i < points.Count; i++)
            {
                wxsum += points[i].x;
                wysum += points[i].y;
            }
            return new Vector2D(wxsum/points.Count, wysum/points.Count);
		}
		
		private double Angle(Vector2D start, Vector2D end, bool positiveOnly)
        {
            double radians = 0.0;
            if (start.x != end.x)
            {
                radians = Math.Atan2(end.y - start.y, end.x - start.x);
            }
            else // pure vertical movement
            {
                if (end.y < start.y)
                    radians = -Math.PI / 2.0; // -90 degrees is straight up
                else if (end.y > start.y)
                    radians = +Math.PI / 2.0; // 90 degrees is straight down
            }
            if (positiveOnly && radians < 0.0)
            {
                radians += Math.PI * 2.0;
            }
            return radians;
        }
		
		private List<Vector2D> RotatePoints(List<Vector2D> points, Vector2D c, double radians)
		{
			List<Vector2D> rPoints = new List<Vector2D>();
			
			foreach (Vector2D p in points)
			{
				//TODO: check!
				Vector2D q = new Vector2D();
				q.x = (p.x - c.x) * Math.Cos(radians) - (p.y - c.y) * Math.Sin(radians) + c.x;
           		q.y = (p.x - c.x) * Math.Sin(radians) + (p.y - c.y) * Math.Cos(radians) + c.y;
				
				rPoints.Add(q);
			}
			return rPoints;
		}
		
		private List<Vector2D> ScaleTo(List<Vector2D> points, Vector2D size)
		{
			List<Vector2D> newPoints = new List<Vector2D>(points.Count);
            Vector2D r = FindBoundingBox(points);
            for (int i = 0; i < points.Count; i++)
            {
                Vector2D p = points[i];
                if (r.x != 0.0)
                    p.x *= (size.x / r.x);
                if (r.y != 0.0)
                    p.y *= (size.y / r.y);
                newPoints.Add(p);
            }
            return newPoints;
		}
		
		private Vector2D FindBoundingBox(List<Vector2D> points)
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
		
			foreach (Vector2D p in points)
			{
				if (p.x < minX)
					minX = p.x;
				if (p.x > maxX)
					maxX = p.x;
			
				if (p.y < minY)
					minY = p.y;
				if (p.y > maxY)
					maxY = p.y;
			}
		
			return new Vector2D(maxX - minX, maxY - minY);
        }
		
		private List<Vector2D> TranslateTo(List<Vector2D> points, Vector2D toPt)
        {
            List<Vector2D> newPoints = new List<Vector2D>(points.Count);
            
            Vector2D ctr = Centroid(points);
        	
            for (int i = 0; i < points.Count; i++)
            {
                Vector2D p = points[i];
                p.x += (toPt.x - ctr.x);
                p.y += (toPt.y - ctr.y);
                newPoints.Add(p);
            }
        	
            return newPoints;
        }
		
		private List<Vector2D> Vectorize(List<Vector2D> points)
        {
            double sum = 0.0;
            List<Vector2D> vector = new List<Vector2D>();
            for (int i = 0; i < points.Count; i++)
            {
                sum += points[i].x * points[i].x + points[i].y * points[i].y;
            }
            double magnitude = Math.Sqrt(sum);
            for (int i = 0; i < points.Count; i++)
            {
                vector.Add( new Vector2D(points[i].x / magnitude, points[i].y / magnitude));
            }
        	
            return vector;
        }
		
	}
	
	#region PluginInfo
	[PluginInfo(Name = "OptimalCosineDistance",
	            Category = "2d Vector Spectral",
	            Help = "Calculates the minimal angular distance between two sets of 2d vectors",
	            Tags = "Protractor,$1,DollarOne",
				Author = "motzi",
				Credits = "Yiang Li, Jacob Wobbrock"
	)]
	#endregion PluginInfo
	public class CosineDistanceNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input1")]
        public ISpread<ISpread<Vector2D>> FInput;
		
		[Input("Input2")]
        public ISpread<ISpread<Vector2D>> FGesture;
		
		[Output("Distance")]
        public ISpread<double> FOutput;

		[Output("Angle")]
        public ISpread<double> FAngle;
		
		[Import()]
        public ILogger FLogger;
		#endregion fields & pins
 
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{ 
			if (FInput.SliceCount == 0 || FGesture.SliceCount == 0 ||
				FInput[0].SliceCount < 1 || FGesture[0].SliceCount < 1 )
			{
				FOutput.SliceCount = 0;
				return;
			}
			
			FOutput.SliceCount = FAngle.SliceCount = (FGesture.SliceCount > FInput.SliceCount ? 
														FGesture.SliceCount : FInput.SliceCount);
			
			for(int i=0; i<FOutput.SliceCount; i++)
			{
				double[] result = OptimalCosineDistance(new List<Vector2D>(FGesture[i]), new List<Vector2D>(FInput[i]));
				FOutput[i] = result[0];
				FAngle[i] = result[1];
			}
			
			//double[] result = OptimalCosineDistance(new List<double>(FGesture), new List<double>(FInput));
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
		
		private double[] OptimalCosineDistance(List<Vector2D> v1, List<Vector2D> v2)
        {
            double a = 0.0;
            double b = 0.0;
            for (int i = 0; i < Math.Min(v1.Count, v2.Count); i++)
            {
                a += v1[i].x * v2[i].x + v1[i].y * v2[i].y;
                b += v1[i].x * v2[i].y - v1[i].y * v2[i].x;
            }
        	
            double angle = Math.Atan(b / a);
        	double distance = Math.Acos(a * Math.Cos(angle) + b * Math.Sin(angle));

        	return new double[2] {distance, VMath.RadToCyc*angle};
        }
	}
}
