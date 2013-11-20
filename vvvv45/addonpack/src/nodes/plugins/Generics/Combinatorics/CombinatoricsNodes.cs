#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;

using Combinatorics;
using Combinatorics.Collections;
#endregion usings

namespace VVVV.Nodes
{

    public class CombinatoricsBaseNode<T> : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input", Order = 0)]
        protected IDiffSpread<T> FInput;

        [Input("With Repetition", IsSingle = true, IsToggle = true, DefaultBoolean = false, Order =2 )]
        protected IDiffSpread<bool> FWithRepetition;

        [Output("Output")]
        protected ISpread<T> FOutput;

        [Output("Count")]
        protected ISpread<int> FCount;

        protected List<T> buffer = new List<T>();
        protected GenerateOption go = new GenerateOption();

        #endregion fields & pins

        public virtual void Evaluate(int SpreadMax)
        {
        }
    }

    public class PermutationsNode<T> : CombinatoricsBaseNode<T>
	{
        override public void Evaluate(int SpreadMax)
		{

            if (FInput.IsChanged || FWithRepetition.IsChanged)
            {
                buffer.Clear();

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    buffer.Add(FInput[i]);
                }

                if (FWithRepetition[0])
                    go = GenerateOption.WithRepetition;
                else
                    go = GenerateOption.WithoutRepetition;

                Permutations<T> permutations = new Permutations<T>(buffer, go);

                FOutput.SliceCount = 0;
                foreach (IList<T> p in permutations)
                {
                    FOutput.AddRange(p);
                }

                FCount.SliceCount = 1;
                FCount[0] = (int)permutations.Count;

            }
		}
	}

    #region Permutations Spreads Node
    [PluginInfo(Name = "Permutations", 
                Category = "Spreads", 
                Version = "Combinatorics", 
                Help = "calculates permutations of given input", 
                Tags = "combinatorics, permutation",
                Author = "bjo:rn",
                Credits = "http://www.codeproject.com/Members/Adrian-Akison")]
    
    public class PermutationsSpreads : PermutationsNode<double>
    {
    }
    #endregion Permutations Spreads Node

    #region Permutations String Node
    [PluginInfo(Name = "Permutations",
                Category = "String",
                Version = "Combinatorics",
                Help = "calculates permutations of given input",
                Tags = "combinatorics, permutation",
                Author = "bjo:rn",
                Credits = "http://www.codeproject.com/Members/Adrian-Akison")]
   
    public class PermutationsString : PermutationsNode<string>
    {
    }
    #endregion Permutations String Node

    #region Permutations Color Node
    //Doesn't work for now:
    //Unable to cast object of type 'VVVV.Utils.VColor.RGBAColor' to type 'System.IComparable`1[VVVV.Utils.VColor.RGBAColor]'

    //[PluginInfo(Name = "Permutations",
    //            Category = "Color",
    //            Version = "Combinatorics",
    //            Help = "calculates permutations of given input",
    //            Tags = "combinatorics, permutation",
    //            Author = "bjo:rn",
    //            Credits = "http://www.codeproject.com/Members/Adrian-Akison")]

    //public class PermutationsColor : PermutationsNode<RGBAColor>
    //{
    //}
    #endregion  Permutations Color Node

    public class CombinationsNode<T> : CombinatoricsBaseNode<T>
    {
        #region fields & pins
        [Input("Subset Element Count", IsSingle = true, DefaultValue = 1, MinValue = 1, Order = 1 )]
        protected IDiffSpread<int> FSubsetElementCount;
        #endregion fields & pins


        override public void Evaluate(int SpreadMax)
        {

            if (FInput.IsChanged || FSubsetElementCount.IsChanged || FWithRepetition.IsChanged)
            {
                buffer.Clear();

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    buffer.Add(FInput[i]);
                }

                if (FWithRepetition[0])
                    go = GenerateOption.WithRepetition;
                else
                    go = GenerateOption.WithoutRepetition;

                //int lowerIndex = Math.Min(FSubsetElementCount[0], FInput.SliceCount);

                Combinations<T> combinations = new Combinations<T>(buffer, FSubsetElementCount[0], go);

                FOutput.SliceCount = 0;
                foreach (IList<T> c in combinations)
                {
                    FOutput.AddRange(c);
                }

                FCount.SliceCount = 1;
                FCount[0] = (int)combinations.Count;

            }
        }
    }

    #region Combinations Spreads Node
    [PluginInfo(Name = "Combinations",
                Category = "Spreads",
                Version = "Combinatorics",
                Help = "calculates combinations of given input",
                Tags = "combinatorics, combinations",
                Author = "bjo:rn",
                Credits = "http://www.codeproject.com/Members/Adrian-Akison")]

    public class CombinationsSpreads : CombinationsNode<double>
    {
    }
    #endregion Combinations Spreads Node

    #region Combinations String Node
    [PluginInfo(Name = "Combinations",
                Category = "String",
                Version = "Combinatorics",
                Help = "calculates combinations of given input",
                Tags = "combinatorics, combinations",
                Author = "bjo:rn",
                Credits = "http://www.codeproject.com/Members/Adrian-Akison")]

    public class CombinationsString : CombinationsNode<string>
    {
    }
    #endregion Combinations String Node

    #region Combinations Color Node
    [PluginInfo(Name = "Combinations",
                Category = "Color",
                Version = "Combinatorics",
                Help = "calculates combinations of given input",
                Tags = "combinatorics, combinations",
                Author = "bjo:rn",
                Credits = "http://www.codeproject.com/Members/Adrian-Akison")]

    public class CombinationsColor : CombinationsNode<RGBAColor>
    {
    }
    #endregion Combinations Color Node

    public class VariationsNode<T> : CombinatoricsBaseNode<T>
    {
        #region fields & pins
        [Input("Subset Element Count", IsSingle = true, DefaultValue = 1, MinValue = 1, Order = 1)]
        protected IDiffSpread<int> FSubsetElementCount;
        #endregion fields & pins

        override public void Evaluate(int SpreadMax)
        {

            if (FInput.IsChanged || FSubsetElementCount.IsChanged || FWithRepetition.IsChanged)
            {
                buffer.Clear();

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    buffer.Add(FInput[i]);
                }

                if (FWithRepetition[0])
                    go = GenerateOption.WithRepetition;
                else
                    go = GenerateOption.WithoutRepetition;

                //int lowerIndex = Math.Min(FSubsetElementCount[0], FInput.SliceCount);

                Variations<T> variations = new Variations<T>(buffer, FSubsetElementCount[0], go);

                FOutput.SliceCount = 0;
                foreach (IList<T> v in variations)
                {
                    FOutput.AddRange(v);
                }

                FCount.SliceCount = 1;
                FCount[0] = (int)variations.Count;

            }
        }
    }

    #region Variations Spreads Node
    [PluginInfo(Name = "Variations",
                Category = "Spreads",
                Version = "Combinatorics",
                Help = "calculates variations of given input",
                Tags = "combinatorics, variations",
                Author = "bjo:rn",
                Credits = "http://www.codeproject.com/Members/Adrian-Akison")]

    public class VariationsSpreads : VariationsNode<double>
    {
    }
    #endregion Variations Spreads Node

    #region Variations String Node
    [PluginInfo(Name = "Variations",
                Category = "String",
                Version = "Combinatorics",
                Help = "calculates Variations of given input",
                Tags = "combinatorics, variations",
                Author = "bjo:rn",
                Credits = "http://www.codeproject.com/Members/Adrian-Akison")]

    public class VariationsString : VariationsNode<string>
    {
    }
    #endregion Variations String Node

    #region Variations Color Node
    [PluginInfo(Name = "Variations",
                Category = "Color",
                Version = "Combinatorics",
                Help = "calculates variations of given input",
                Tags = "combinatorics, variations",
                Author = "bjo:rn",
                Credits = "http://www.codeproject.com/Members/Adrian-Akison")]

    public class VariationsColor : VariationsNode<RGBAColor>
    {
    }
    #endregion Variations Color Node

}
