using System.Drawing;
using System.Drawing.Drawing2D;

namespace VVVV.Nodes.Timeliner
{
	public delegate void TransformationChangedHandler(double Translation, double Scaling);
	
	public sealed class TLTransformer
	{
		private double GTimeScaleAt, GTimeScaleAtUT = 0;
		public double GTimeScale = 50;
		public double GTimeTranslate = 0;
		public Matrix GTransformation = new Matrix((float) 50, 0, 0, 1, 0, 0 );
		public Matrix GScale = new Matrix((float) 50, 0, 0, 1, 0, 0 );
		public event TransformationChangedHandler OnTransformationChanged;
		
		public void TranslateTime(double DeltaTime)
		{	
			GTimeTranslate += (DeltaTime / GTimeScale);
		}
		
		public void ScaleTime(double X, double ScaleAtTime)
		{		
			GTimeScale *= X;
			
			GTimeScaleAt = ScaleAtTime - TimelinerPlugin.FHeaderWidth;
			GTimeScaleAtUT = UnScalePoint(new PointF((float) GTimeScaleAt, 0)).X;
		}
		
		public double XPosToTime(int X)
		{
			return UnTransformPoint(new PointF(X, 0)).X;
		}
		
		public double XPosToTime(float X)
		{
			return UnTransformPoint(new PointF(X, 0)).X;
		}
		
		public void ApplyTransformation()
		{
			GTransformation.Reset();
			GTransformation.Translate((float) GTimeScaleAt, 0);
			GTransformation.Scale((float) GTimeScale, 1);
			GTransformation.Translate((float) -GTimeScaleAtUT, 0);
			GTransformation.Translate((float) GTimeTranslate, 0);
			
			
			GScale.Reset();
			GScale.Translate((float) GTimeScaleAt, 0);
			GScale.Scale((float) GTimeScale, 1);
			GScale.Translate((float) -GTimeScaleAtUT, 0);
			
			OnTransformationChanged(-UnTransformPoint(new PointF(0,0)).X, GTimeScale);
		}
		
		
		public PointF UnTransformPoint(PointF In)
		{
			PointF[] p = new PointF[1];
			p[0] = In;
				
			Matrix m = GTransformation.Clone();
			m.Invert();
			m.TransformPoints(p);

			return p[0];
		}
		
		public PointF TransformPoint(PointF In)
		{
			PointF[] p = new PointF[1];
			p[0] = In;
				
			GTransformation.TransformPoints(p);

			return p[0];
		}
		
		//should not need a matrix for this!!!?
		private PointF UnScalePoint(PointF In)
		{
			PointF[] p = new PointF[1];
			p[0] = In;
				
			Matrix m = GScale.Clone();
			m.Invert();
			m.TransformPoints(p);

			return p[0];
		}
		

	}
}
