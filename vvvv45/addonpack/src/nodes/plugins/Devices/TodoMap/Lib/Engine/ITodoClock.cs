using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.TodoMap.Lib
{
    /// <summary>
    /// Global clock interface
    /// </summary>
    public interface ITodoClock : IDisposable
    {
        void Start();
        void Stop();

        double Time { get; }
    }
}
