namespace VVVV.Nodes

open System
open System.ComponentModel.Composition
open VVVV.PluginInterfaces.V2
open VVVV.Utils.VColor
open VVVV.Utils.VMath
open VVVV.Core.Logging

//the node class
[<PluginInfo(Name = "Template", 
    Category = "Value", 
    Version = "F#", 
    Help = "Offers a basic F# code layout to start from when writing a vvvv plugin", 
    Tags = "functional")>]
type FSharpTemplateNode() =

    //inherit from obj to get access to base identifier
    inherit obj()

    //pins
    [<Input("Input")>]
    let mutable myInput:ISpread<double> = null

    [<Output("Output")>]
    let mutable myOutput:ISpread<double> = null

    //imports
    [<Import>]
    let mutable fLogger:ILogger = null


    //evaluate function
    let evaluate spreadMax = 
        
        let pinvals = seq {for i in 0..spreadMax-1 -> myInput.Item i }

        myOutput.SliceCount <- spreadMax

        let sliceFunc s = s * 2.0
        let setOutput i s = myOutput.Item i <- s

        pinvals
        |> Seq.map sliceFunc
        |> Seq.iteri setOutput
        
    //implement the IPluginEvaluate interface
    interface IPluginEvaluate with 
        member this.Evaluate(spreadMax) = evaluate spreadMax
  
        
