using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Utils.VMath;

namespace VVVV.TodoMap.Lib
{

	public partial class TodoTweenMapper
	{
		#region Set Delegate
		public void SetDelegate()
		{
			if (this.tweenmode == eTweenMode.Linear)
			{
				this.tweener = this.Linear;
			}

			if (this.tweenmode == eTweenMode.Back)
			{
				if (this.easemode == eTweenEaseMode.In)
				{
					this.tweener = Tweener.BackEaseIn;
				}
				if (this.easemode == eTweenEaseMode.InOut)
				{
					this.tweener = Tweener.BackEaseInOut;
				}
				if (this.easemode == eTweenEaseMode.Out)
				{
					this.tweener = Tweener.BackEaseOut;
				}
				if (this.easemode == eTweenEaseMode.OutIn)
				{
					this.tweener = Tweener.BackEaseOutIn;
				}
			}

			if (this.tweenmode == eTweenMode.Bounce)
			{
				if (this.easemode == eTweenEaseMode.In)
				{
					this.tweener = Tweener.BounceEaseIn;
				}
				if (this.easemode == eTweenEaseMode.InOut)
				{
					this.tweener = Tweener.BounceEaseInOut;
				}
				if (this.easemode == eTweenEaseMode.Out)
				{
					this.tweener = Tweener.BounceEaseOut;
				}
				if (this.easemode == eTweenEaseMode.OutIn)
				{
					this.tweener = Tweener.BounceEaseOutIn;
				}
			}

			if (this.tweenmode == eTweenMode.Circular)
			{
				if (this.easemode == eTweenEaseMode.In)
				{
					this.tweener = Tweener.CircularEaseIn;
				}
				if (this.easemode == eTweenEaseMode.InOut)
				{
					this.tweener = Tweener.CircularEaseInOut;
				}
				if (this.easemode == eTweenEaseMode.Out)
				{
					this.tweener = Tweener.CircularEaseOut;
				}
				if (this.easemode == eTweenEaseMode.OutIn)
				{
					this.tweener = Tweener.CircularEaseOutIn;
				}
			}


			if (this.tweenmode == eTweenMode.Cubic)
			{
				if (this.easemode == eTweenEaseMode.In)
				{
					this.tweener = Tweener.CubicEaseIn;
				}
				if (this.easemode == eTweenEaseMode.InOut)
				{
					this.tweener = Tweener.CubicEaseInOut;
				}
				if (this.easemode == eTweenEaseMode.Out)
				{
					this.tweener = Tweener.CubicEaseOut;
				}
				if (this.easemode == eTweenEaseMode.OutIn)
				{
					this.tweener = Tweener.CubicEaseOutIn;
				}
			}

			if (this.tweenmode == eTweenMode.Elastic)
			{
				if (this.easemode == eTweenEaseMode.In)
				{
					this.tweener = Tweener.ElasticEaseIn;
				}
				if (this.easemode == eTweenEaseMode.InOut)
				{
					this.tweener = Tweener.ElasticEaseInOut;
				}
				if (this.easemode == eTweenEaseMode.Out)
				{
					this.tweener = Tweener.ElasticEaseOut;
				}
				if (this.easemode == eTweenEaseMode.OutIn)
				{
					this.tweener = Tweener.ElasticEaseOutIn;
				}
			}

			if (this.tweenmode == eTweenMode.Exponential)
			{
				if (this.easemode == eTweenEaseMode.In)
				{
					this.tweener = Tweener.ExponentialEaseIn;
				}
				if (this.easemode == eTweenEaseMode.InOut)
				{
					this.tweener = Tweener.ExponentialEaseInOut;
				}
				if (this.easemode == eTweenEaseMode.Out)
				{
					this.tweener = Tweener.ExponentialEaseOut;
				}
				if (this.easemode == eTweenEaseMode.OutIn)
				{
					this.tweener = Tweener.ExponentialEaseOutIn;
				}
			}

			if (this.tweenmode == eTweenMode.Quartic)
			{
				if (this.easemode == eTweenEaseMode.In)
				{
					this.tweener = Tweener.QuarticEaseIn;
				}
				if (this.easemode == eTweenEaseMode.InOut)
				{
					this.tweener = Tweener.QuarticEaseInOut;
				}
				if (this.easemode == eTweenEaseMode.Out)
				{
					this.tweener = Tweener.QuarticEaseOut;
				}
				if (this.easemode == eTweenEaseMode.OutIn)
				{
					this.tweener = Tweener.QuarticEaseOutIn;
				}
			}

			if (this.tweenmode == eTweenMode.Quintic)
			{
				if (this.easemode == eTweenEaseMode.In)
				{
					this.tweener = Tweener.QuinticEaseIn;
				}
				if (this.easemode == eTweenEaseMode.InOut)
				{
					this.tweener = Tweener.QuinticEaseInOut;
				}
				if (this.easemode == eTweenEaseMode.Out)
				{
					this.tweener = Tweener.QuinticEaseOut;
				}
				if (this.easemode == eTweenEaseMode.OutIn)
				{
					this.tweener = Tweener.QuinticEaseOutIn;
				}
			}

			if (this.tweenmode == eTweenMode.Sinusoidal)
			{
				if (this.easemode == eTweenEaseMode.In)
				{
					this.tweener = Tweener.SinusoidalEaseIn;
				}
				if (this.easemode == eTweenEaseMode.InOut)
				{
					this.tweener = Tweener.SinusoidalEaseInOut;
				}
				if (this.easemode == eTweenEaseMode.Out)
				{
					this.tweener = Tweener.SinusoidalEaseOut;
				}
				if (this.easemode == eTweenEaseMode.OutIn)
				{
					this.tweener = Tweener.SinusoidalEaseOutIn;
				}
			}

			if (this.tweenmode == eTweenMode.Quadratic)
			{
				if (this.easemode == eTweenEaseMode.In)
				{
					this.tweener = Tweener.QuadEaseIn;
				}
				if (this.easemode == eTweenEaseMode.Out)
				{
					this.tweener = Tweener.QuadEaseOut;
				}
				if (this.easemode == eTweenEaseMode.InOut)
				{
					this.tweener = Tweener.QuadEaseInOut;
				}
				if (this.easemode == eTweenEaseMode.OutIn)
				{
					this.tweener = Tweener.QuadEaseOutIn;
				}
			}


		}
		#endregion
	}
}
