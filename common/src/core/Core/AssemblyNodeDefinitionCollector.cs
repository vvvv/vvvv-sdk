using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;

namespace VVVV.Core
{
    internal static class CCIHelpers
    {
        public static string NameSpaceName(this ITypeDefinition type)
        {
            if (type is INamespaceTypeDefinition)
                return (type as INamespaceTypeDefinition).ContainingNamespace.Name.Value;
            else if (type is INestedTypeDefinition)
                //todo: check what we need to do for nested types
                return "fullname retrieval not implemented for nested types";
            else
                return "unknown type definition";
        }

        public static string TypeName(this ITypeReference type, bool fullname = false)
        {
            return type.ResolvedType.TypeName(fullname);
        }

        public static string TypeName(this ITypeDefinition type, bool fullname = false)
        {
            if (type is INamedTypeDefinition)
                if (fullname)
                    // find a fast way to retrive the full name: NamespaceName.TypeName. do we need to add those manually?
                    return type.NameSpaceName() + "." + (type as INamedTypeDefinition).Name.Value;
                else
                    return (type as INamedTypeDefinition).Name.Value;
            else
                return null;
        }

        public static bool ValueIsString(this IMetadataNamedArgument arg)
        {
            return ((arg != null) && (arg is IMetadataConstant) && ((arg.ArgumentValue as IMetadataConstant).Value is string));
        }

        public static string Value(this IMetadataNamedArgument arg)
        {
            return ((arg.ArgumentValue as IMetadataConstant).Value as string);
        }
        public static bool ValueIsBool(this IMetadataNamedArgument arg)
        {
            return ((arg != null) && (arg is IMetadataConstant) && ((arg.ArgumentValue as IMetadataConstant).Value is bool));
        }

        public static bool ValueAsBool(this IMetadataNamedArgument arg)
        {
            return (bool)(arg.ArgumentValue as IMetadataConstant).Value;
        }
        public static IMetadataNamedArgument GetArgument(this ICustomAttribute attribute, string attributename)
        {
            return (attribute != null) ? attribute.NamedArguments.FirstOrDefault(arg => arg.ArgumentName.Value == attributename) : null;
        }
    }

    public class AssemblyNodeDefinitionCollector
    {
        public bool NodeAttributesNeedToBeSet = true;        
        public bool SearchForFunctionNodes = true;
        public bool SearchForFunctorNodes = true;


        public static string ParamNameToPinName(string paramname)
        {
            //pin names can have spaces, all individual word starting with a upper case letter
            //inputs and outputs can have same names

            //param names start with a lower case letter, after that are camelcaps
            //out params that need to have the same name as another (in) param, can be named param_Out, resulting in a pin name "Param"

            //todo: automatically convert:
            //- remove an "_Out" at the end of the string
            //- put a space in front of every first uppercase letter in a row
            //- uppercase the first letter
            return paramname;
        }

        public static string FullTypeNameToCategory(string fulltypename)
        {
            //todo:
            //if uppercase(fulltypename) starts with "VVVV.NODES." remove that 
            //return the rest of the string

            //optionally just set category to "unsorted"
            return "unsorted";
        }

        public static string FullTypeNameToVersion(string fulltypename, INodeDefinition node)
        {
            // in case of an unsorted category we need to ensure a unique systemname otherwise 
            // (by putting the name of the containing type into the version)
            if (node.Category == "unsorted")
                return fulltypename;
            else
                return "";
        }

        public static string ExtractXMLHelpSnippet(IMethodDefinition method)
        {
            return "extracting methods xml help snippet not implemented yet. no help argument in node attribute found.";
        }

        public IEnumerable<IDataflowPinDefinition> Collect(IParameterDefinition param, INodeDefinition node)
        {
            var pinattribute = param.Attributes.FirstOrDefault(attribute => attribute.Type.TypeName() == "PinAttribute");

            // todo: ref params should result in both in and output pins(?), for now only input is created

            DataflowPinDefinition pin;
            if (param.IsOut)
            {
                var output = new OutputPinDefinition();
                pin = output;
            }
            else
            {
                var input = new InputPinDefinition();
                
                input.HasDefaultValue = param.HasDefaultValue;
                if (input.HasDefaultValue)
                    input.DefaultValue = param.DefaultValue;
                
                var strikearg = pinattribute.GetArgument("StrikedOutByDefault");
                input.StrikedOutByDefault = strikearg.ValueIsBool() ? strikearg.ValueAsBool() : false;
                
                pin = input;
            }

            var namearg = pinattribute.GetArgument("Name");
            pin.Name = namearg.ValueIsString() ? namearg.Value() : ParamNameToPinName(param.Name.Value);

            pin.NameInTextualCode = param.Name.Value;

            pin.Type = param.Type;

            pin.Node = node as IDataflowNodeDefinition;

            yield return pin;
        }

        public void CollectInputsAndOutpus(IMethodDefinition methodDefinition, DataflowNodeDefinition node)
        {
            IEnumerable<IDataflowPinDefinition> allpindefs =
                from param in methodDefinition.Parameters
                from pindef in Collect(param, node)                
                select pindef;

            node.Inputs = allpindefs.Where(pindef => pindef is IInputPinDefinition).Cast<IInputPinDefinition>();
            node.Outputs = allpindefs.Where(pindef => pindef is IOutputPinDefinition).Cast<IOutputPinDefinition>();
        }

        public IEnumerable<INodeDefinition> Collect(IMethodDefinition methodDefinition)
        {
            var nodeattribute = methodDefinition.Attributes.FirstOrDefault(attribute => attribute.Type.TypeName() == "NodeAttribute"); 
            
            if (!NodeAttributesNeedToBeSet || nodeattribute!=null)
            {
                DataflowNodeDefinition node;
                if (methodDefinition.IsStatic)
                    node = new FunctionNodeDefinition();
                else
                    //todo: only create functor nodes if the containing type has a default constructor
                    if (methodDefinition.ContainingTypeDefinition.IsValueType /*|| methodDefinition.ContainingTypeDefinition.*/ )
                        node = new FunctorNodeDefinition()
                        {
                            StateType = methodDefinition.ContainingTypeDefinition
                        };
                    else
                        yield break;
                    
                //is there any way of instancing the attribute? that ease the access of the properties - think Attribute.GetCustomAttribute(

                var namearg = nodeattribute.GetArgument("Name");
                node.Name = namearg.ValueIsString() ? namearg.Value() : methodDefinition.Name.Value;

                var categoryarg = nodeattribute.GetArgument("Category");
                node.Category = categoryarg.ValueIsString() ? categoryarg.Value() : FullTypeNameToCategory(methodDefinition.Type.TypeName());

                var versionarg = nodeattribute.GetArgument("Version");
                node.Version = versionarg.ValueIsString() ? versionarg.Value() : FullTypeNameToVersion(methodDefinition.Type.TypeName(), node);

                var helparg = nodeattribute.GetArgument("Help");
                node.Help = helparg.ValueIsString() ? helparg.Value() : ExtractXMLHelpSnippet(methodDefinition);

                var tagargs = nodeattribute.GetArgument("Tags");
                node.Tags = tagargs.ValueIsString() ? tagargs.Value() : ""; //is there a similar concept in .net?
                
                var authorarg = nodeattribute.GetArgument("Author");
                node.Author = authorarg.ValueIsString() ? authorarg.Value() : ""; //todo: get the author of the assemby!!!!

                var creditsarg = nodeattribute.GetArgument("Credits");
                node.Credits = creditsarg.ValueIsString() ? creditsarg.Value() : ""; //is there a similar concept in .net?

                var bugsarg = nodeattribute.GetArgument("Bugs");
                node.Bugs = bugsarg.ValueIsString() ? bugsarg.Value() : ""; //is there a similar concept in .net?

                var warningsarg = nodeattribute.GetArgument("Warnings");
                node.Warnings = warningsarg.ValueIsString() ? warningsarg.Value() : ""; //is there a similar concept in .net?

                node.NameInTextualCode = methodDefinition.Name.Value;

                node.Namespace = methodDefinition.ContainingTypeDefinition.NameSpaceName();

                node.MethodDefinition = methodDefinition;

                //todo: add return value as output pin if not void

                //todo: set filename property of node definition
                
                CollectInputsAndOutpus(methodDefinition, node);

                yield return node;
            }            
        }
        
        public IEnumerable<INodeDefinition> Collect(IAssembly assembly)
        {
            return
                from type in assembly.GetAllTypes()
                where
                    (type.IsStatic && SearchForFunctionNodes) ||
                    (!type.IsStatic && (SearchForFunctionNodes || SearchForFunctorNodes))
                from method in type.Methods
                where
                    (method.IsStatic && SearchForFunctionNodes) ||
                    (!method.IsStatic && SearchForFunctorNodes)
                from node in Collect(method)
                select node;
        }
    }
}
