using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Utils.VMath;

namespace VVVV.TodoMap.Lib
{
	public delegate double TweenerDunctionDelegate(double input);

	public partial class TodoTweenMapper
	{
		private eTweenEaseMode easemode = eTweenEaseMode.In;
		private eTweenMode tweenmode = eTweenMode.Linear;

		public double MinValue { get; set; }
		public double MaxValue { get; set; }
		public bool Reverse { get; set; }

		public eTweenEaseMode EaseMode
		{
			get { return this.easemode; }
			set
			{
				this.easemode = value;
				this.SetDelegate();
			}
		}

		public eTweenMode TweenMode
		{
			get { return this.tweenmode; }
			set
			{
				this.tweenmode = value;
				this.SetDelegate();
			}
		}

		private double Linear(double input)
		{
			return input;
		}

		private TweenerDunctionDelegate tweener;

		public TodoTweenMapper()
		{
			this.MinValue = 0;
			this.MaxValue = 1;
			this.Reverse = false;
			this.tweener = this.Linear;
		}

		public double GetValue(double val)
		{
			if (this.Reverse)
			{
				val = 1.0 -val;
			}
			return VMath.Map(this.tweener(val), 0, 1, this.MinValue, this.MaxValue, TMapMode.Clamp);
		}
	}
}
