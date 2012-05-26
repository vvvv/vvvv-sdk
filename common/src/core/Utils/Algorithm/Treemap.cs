using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VVVV.Utils.VMath;


namespace VVVV.Utils.Algorithm
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Rect
	{
		public double X;
		public double Y;
		public double Width;
		public double Height;
		
		public Rect(double x, double y, double width, double height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
		
		public Vector2D Center
		{
			get
			{
				return new Vector2D(X + Width * 0.5, -(Y + Height * 0.5));
			}
		}
		
		public Size Size
		{
			get
			{
				return new Size(Width, Height);
			}
		}
		
		public static bool operator ==(Rect r1, Rect r2)
        {
            return r1.X == r2.X && r1.Y == r2.X && r1.Width == r2.Width && r1.Height == r2.Height;
        }

        public static bool operator !=(Rect r1, Rect r2)
        {
            return r1.X != r2.X || r1.Y != r2.X || r1.Width != r2.Width || r1.Height != r2.Height;
        }
		
		public override bool Equals(object obj)
        {
            return (obj is Rect) && Equals((Rect)obj);
        }
		
		public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked {
                hashCode += 1000000007 * X.GetHashCode();
                hashCode += 1000000009 * Y.GetHashCode();
            	hashCode += 1000000011 * Width.GetHashCode();
                hashCode += 1000000013 * Height.GetHashCode();
            }
            return hashCode;
        }
		
        public bool Equals(Rect other)
        {
            return this == other;
        }
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct Size
	{
		public double Width;
		public double Height;
		
		public Size(double width, double height)
		{
			Width = width;
			Height = height;
		}
		
		public static implicit operator Vector2D(Size s)  // implicit digit to byte conversion operator
		{
			return new Vector2D(s.Width, s.Height);
		}
	}
	
	public class Node : IComparer <Node> 
	{
		public string Name;
		
		// The weight
		public double    Size;
		public double    Value;
		
		// Children of this node
		public List<Node> Children;
		
		// Used during layout:
		public double Area;
		public Rect   Rect;
		
		public int Compare (Node a, Node b)
		{
			var n = Math.Sign(b.Size - a.Size);
			
			if (n != 0)
				return n;
			return string.Compare (b.Name, a.Name);
		}
		
		public Node Clone ()
		{
			return new Node (this);
		}
		
		public Node ()
		{
			Children = new List<Node> ();
		}
		
		public Node (int n)
		{
			Children = new List<Node> (n);
		}
		
		public Node (Node re)
		{
			Name = re.Name;
			Size = re.Size;
			Area = re.Area;
			Rect = re.Rect;
			
			Children = new List<Node> (re.Children.Count);
			foreach (Node rec in re.Children)
			Children.Add (rec);
			
		}
	}
	
	public class Treemap
	{
		public Node FRoot;
		Rect FRegion;
		Treemap FActiveChild;
		
		public Treemap (Node source)
		{
			this.FRoot = source.Clone ();
			Sort (FRoot);
		}
		
		public Size Arrange(Size finalSize)
		{
			
			Rect newRegion = new Rect (0, 0, finalSize.Width, finalSize.Height);
			if (newRegion != FRegion && newRegion.Width > 0 && newRegion.Height > 0) 
			{
				Treemap t = this, child;
				
				while (true)
				{
					child = t.FActiveChild;
					if (child == null)
					{
						t.SetRegion (newRegion);
						break;
					}
					t = child;
				}
			}
			
			return finalSize;
			
		}
		
		public void SetRegion (Rect newRegion)
		{
			FRegion = newRegion;
			Squarify (FRegion, FRoot.Children);
		}
		
		const int PADX = 0;
		const int PADY = 0;

		// Render a child
		void Clicked (Node n)
		{
			Treemap c = new Treemap (n);
			
			Size ns = new Size(FRegion.Width, FRegion.Height);
			c.Arrange(ns);
			FActiveChild = c;
		}
		
		public void Back()
		{
			Treemap last = this, child = FActiveChild;
			
			while (child != null && child.FActiveChild != null) 
			{
				last = child;
				child = child.FActiveChild;
			}
			if (child != null) 
			{
				Rect childRegion = child.FRegion;
				
				last.FActiveChild = null;
				
				// In case layout changed while we were rendering the child
				if (childRegion != FRegion)
					SetRegion (childRegion);
			}
		}
		
		public static double GetShortestSide (Rect r)
		{
			return Math.Min (r.Width, r.Height);
		}
		
		static void Squarify (Rect emptyArea, List<Node> children)
		{
			double fullArea = 0;
			foreach (Node child in children)
			{
				fullArea += child.Size;
			}
			
			double area = emptyArea.Width * emptyArea.Height;
			foreach (Node child in children)
			{
				child.Area = (area * child.Size / fullArea);
			}
			
			Squarify (emptyArea, children, new List<Node> (), GetShortestSide (emptyArea));
			
			foreach (Node child in children)
			{
				if (child.Area < 9000 || child.Children.Count == 0)
				{
					//Console.WriteLine ("Passing on this {0} {1} {2}", child.Area, child.Children, child.Children.Count);
					//continue;
				}
				
				Squarify (child.Rect, child.Children);
			}
		}
		
		static void Squarify (Rect emptyArea, List<Node> children, List<Node> row, double w)
		{
			if (children.Count == 0)
			{
				AddRowToLayout (emptyArea, row);
				return;
			}
			
			Node head = children [0];
			
			List<Node> row_plus_head = new List<Node> (row);
			row_plus_head.Add (head);
			
			double worst1 = Worst (row, w);
			double worst2 = Worst (row_plus_head, w);
			
			if (row.Count == 0 || worst1 > worst2)
			{
				List<Node> children_tail = new List<Node> (children);
				children_tail.RemoveAt (0);
				Squarify (emptyArea, children_tail, row_plus_head, w);
			} 
			else 
			{
				emptyArea = AddRowToLayout (emptyArea, row);
				Squarify (emptyArea, children, new List<Node>(), GetShortestSide (emptyArea));
			}
		}
		
		static double Worst (List<Node> row, double sideLength)
		{
			if (row.Count == 0)
			return 0;
			
			double maxArea = 0, minArea = double.MaxValue;
			double totalArea  = 0;
			foreach (Node n in row)
			{
				maxArea = Math.Max (maxArea, n.Area);
				minArea = Math.Min (minArea, n.Area);
				totalArea += n.Area;
			}
			
			if (minArea == double.MaxValue)
			minArea = 0;
			
			double v1 = (sideLength * sideLength * maxArea) / (totalArea * totalArea);
			double v2 = (totalArea * totalArea) / (sideLength * sideLength * minArea);
			
			return Math.Max (v1, v2);
		}
		
		static Rect AddRowToLayout (Rect emptyArea, List<Node> row)
		{
			Rect result;
			double areaUsed = 0;
			foreach (Node n in row)
				areaUsed += n.Area;
			
			if (emptyArea.Width > emptyArea.Height)
			{
				double w = areaUsed / emptyArea.Height;
				result = new Rect (emptyArea.X + w, emptyArea.Y, Math.Max (0, emptyArea.Width - w), emptyArea.Height);
				
				double y = emptyArea.Y;
				foreach (Node n in row)
				{
					double h = n.Area * emptyArea.Height / areaUsed;
					n.Rect = new Rect (emptyArea.X, y, w, h);
					y += h;
				}
			} 
			else 
			{
				double h = areaUsed / emptyArea.Width;
				result = new Rect (emptyArea.X, emptyArea.Y + h, emptyArea.Width, Math.Max (0, emptyArea.Height - h));
				
				double x = emptyArea.X;
				foreach (Node n in row)
				{
					double w = n.Area * emptyArea.Width / areaUsed;
					n.Rect = new Rect (x, emptyArea.Y, w, h);
					x += w;
				}
			}
			
			return result;
		}
		
		static void Sort (Node n)
		{
			n.Children.Sort (n);
			foreach (Node child in n.Children)
				Sort (child);
		}
	}
}