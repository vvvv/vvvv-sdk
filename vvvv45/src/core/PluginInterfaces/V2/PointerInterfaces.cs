using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.Streams;

namespace VVVV.PluginInterfaces.V2
{
    public unsafe interface IFastValuePointerInput : ISynchronizable
    {
        double* Data { get; }
        int Length { get; }
    }

    public unsafe interface IValuePointerInput : ISynchronizable
    {
        double* Data { get; }
        int Length { get; }
    }

    public unsafe interface IColorPointerInput : ISynchronizable
    {
        double* Data { get; }
        int Length { get; }
    }

    public unsafe interface ITransformPointerInput : ISynchronizable
    {
        float* Data { get; }
        int Length { get; }
    }

    public unsafe interface IValuePointerOutput
    {
        double* Data { get; }
        int Length { get; set; }
    }

    public unsafe interface IColorPointerOutput
    {
        double* Data { get; }
        int Length { get; set; }
    }

    public unsafe interface ITransformPointerOutput
    {
        float* Data { get; }
        int Length { get; set; }
    }
}
