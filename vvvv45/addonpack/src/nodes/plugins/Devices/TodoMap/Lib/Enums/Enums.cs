using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.TodoMap
{
    /// <summary>
    /// Global takeover mode
    /// </summary>
    public enum eTodoGlobalTakeOverMode { Immediate, Pickup,Scale }

    /// <summary>
    /// Local Takeover mode
    /// </summary>
    public enum eTodoLocalTakeOverMode { Parent, Immediate, Pickup, Scale }

    /// <summary>
    /// Local Takeover mode
    /// </summary>
    public enum eTodoLocalFeedBackMode { Parent, False, True }

    /// <summary>
    /// Tweener ease mode
    /// </summary>
    public enum eTweenEaseMode { In, Out, InOut, OutIn }

    /// <summary>
    /// Tweener mode
    /// </summary>
    public enum eTweenMode { Linear, Quadratic, Cubic, Quartic, Quintic, Sinusoidal, Exponential, Circular, Elastic, Back, Bounce }
}
