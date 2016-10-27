using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using VVVV.Utils.Streams;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
    public unsafe interface IFastValuePointerInput : ISynchronizable
    {
        double* Data { get; }
        int Length { get; }
    }

    [ComVisible(false)]
    public unsafe interface IValuePointerInput : ISynchronizable
    {
        double* Data { get; }
        int Length { get; }
    }

    [ComVisible(false)]
    public unsafe interface IColorPointerInput : ISynchronizable
    {
        double* Data { get; }
        int Length { get; }
    }

    [ComVisible(false)]
    public unsafe interface ITransformPointerInput : ISynchronizable
    {
        float* Data { get; }
        int Length { get; }
    }

    [ComVisible(false)]
    public unsafe interface IValuePointerOutput
    {
        double* Data { get; }
        int Length { get; set; }
    }

    [ComVisible(false)]
    public unsafe interface IColorPointerOutput
    {
        double* Data { get; }
        int Length { get; set; }
    }

    [ComVisible(false)]
    public unsafe interface ITransformPointerOutput
    {
        float* Data { get; }
        int Length { get; set; }
    }
}
