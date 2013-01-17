using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VVVV.Core.View.GraphicalEditor
{
	public enum Mouse_Buttons
	{
		Left = 1048576,
		Middle = 4194304,
		Right = 2097152
	}
	
    public interface IHoverable
    {
        void MouseEnter(PointF mousePos);
        void MouseLeave(PointF mousePos, System.TimeSpan timeSinceEnter);
        void MouseHover(PointF mousePos);
    }
    
    public interface IClickable
    {
    	void Click(PointF mousePos, Mouse_Buttons mouseButton);
    	void DoubleClick(PointF mousePos, Mouse_Buttons mouseButton);
    	void MouseDown(PointF mousePos, Mouse_Buttons mouseButton);
    	void MouseUp(PointF mousePos, Mouse_Buttons mouseButton);
    }
    
    public interface IScrollable
    {
    	void MouseDown(PointF mousePos, Mouse_Buttons mouseButton);
    	void MouseMove(PointF mousePos, Mouse_Buttons mouseButton);
    	void MouseUp(PointF mousePos, Mouse_Buttons mouseButton);
    }
}
