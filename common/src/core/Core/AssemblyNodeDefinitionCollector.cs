using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using System.Text.RegularExpressions;

namespace VVVV.Core
{
    internal static class CCIHelpers
    {
        public static string NameSpaceName(this ITypeDefinition type)
        {
            if (type is INamespaceTypeDefinition)
                return (type as INamespaceTypeDefinition).ContainingNamespace.Name.Value; //TypeHelper.GetNamespaceName(
            else if (type is INestedTypeDefinition)
                //todo: check what we need to do for nested types
                return "fullname retrieval not implemented for nested types";
            else
                return "unknown type definition";
        }

        public static string TypeName(this ITypeReference type, bool fullname = false)
        {
            if (fullname)
                return TypeHelper.GetTypeName(type);
            else
                return TypeHelper.GetTypeName(type, NameFormattingOptions.OmitContainingNamespace);
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

        public static bool HasDefaultConstructor(this ITypeDefinition type, MetadataReaderHost host)
        {
            return TypeHelper.GetMethod(type, host.NameTable.Ctor) != Dummy.Method;
        }
    }

    public class AssemblyNodeDefinitionCollector
    {
        // PLAY AROUND WITH THOSE:
        public bool AcceptFunctionNodes = true;
        public bool AcceptFunctorNodes = false;
        public bool AcceptNodesWithoutNodeAttribute = true;        // why not import everything that can be worked with(?)
        public bool AcceptNodesThatHaveRefParams = false;           // not supported yet
        public bool AcceptNodesThatWorkWithUnclonableTypes = true;  // in the best case patch view will take care that only one connection is allowed when values aren't clonable
        public bool AcceptConstructorsAsFunctors = false;           // keep it like that

        private MetadataReaderHost FHost;
        private IName FctorName;

        public AssemblyNodeDefinitionCollector(MetadataReaderHost host)
        {
            FHost = host;
            FctorName = FHost.NameTable.Ctor;
        }

        public static string ParamNameToPinName(string paramname)
        {
            //pin names can have spaces, all individual words starting with an upper case letter
            //inputs and outputs can have same names

            //param names start with a lower case letter, after that are camelcaps
            //out params that need to have the same name as another (in) param, can be named param_Out, resulting in a pin name "Param"
            var pinname = paramname;
            if (pinname.EndsWith("_Out"))
                pinname = pinname.Remove(pinname.LastIndexOf("_Out"));
            
            pinname = Regex.Replace(pinname, "[A-Z]+", match => " "+match.Value);
            pinname = char.ToUpper(pinname[0]) + pinname.Substring(1);
            return pinname;
        }

        public static string FullTypeNameToCategory(string fulltypename)
        {
            if (fulltypename.ToUpper().StartsWith("VVVV.NODES."))
                return fulltypename.Substring(11);

            return "uncategorized";
        }

        public static string FullTypeNameToVersion(string fulltypename, INodeDefinition node)
        {
            // in case of an unsorted category we need to ensure a unique systemname otherwise
            // (by putting the name of the containing type into the version)
            if (node.Category == "uncategorized")
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

            // todo: ref params should result in both in and output pins or in a ref pin(?), for now only input is created

            DataflowPinDefinition pin;
            if (param.IsOut)
            {
                var output = new OutputPinDefinition();
                pin = output;
            }
            else
            {
                var input = new InputPinDefinition();
                
                input.ParameterDefinition = param;

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

        private IEnumerable<IDataflowPinDefinition> ReturnValueToOutputDefinition(DataflowNodeDefinition node)
        {
            if (node.MethodDefinition.Type.TypeCode != PrimitiveTypeCode.Void)
            {
                var pin = new OutputPinDefinition();

                var pinattribute = node.MethodDefinition.ReturnValueAttributes.FirstOrDefault(attribute => attribute.Type.TypeName() == "PinAttribute");
                var namearg = pinattribute.GetArgument("Name");

                var returnvaluename =
                    namearg.ValueIsString() ? namearg.Value() :
                    (node.MethodDefinition.ReturnValueName != Dummy.Name ? node.MethodDefinition.ReturnValueName.Value : null);

                pin.Name = returnvaluename != null ? returnvaluename : "Output";

                pin.NameInTextualCode = returnvaluename != null ? returnvaluename : "";

                pin.Type = node.MethodDefinition.Type;

                pin.Node = node as IDataflowNodeDefinition;

                yield return pin;
            }
        }

        public void CollectInputsAndOutpus(IMethodDefinition methodDefinition, DataflowNodeDefinition node)
        {
            IEnumerable<IDataflowPinDefinition> stdpindefs =
                from param in methodDefinition.Parameters
                from pindef in Collect(param, node)
                select pindef;

            IEnumerable<IDataflowPinDefinition> allpindefs =
                stdpindefs.Concat(ReturnValueToOutputDefinition(node)).ToArray(); // cache before looking at it twice

            node.Inputs = allpindefs.Where(pindef => pindef is IInputPinDefinition).Cast<IInputPinDefinition>();
            node.Outputs = allpindefs.Where(pindef => pindef is IOutputPinDefinition).Cast<IOutputPinDefinition>();
        }

        public IEnumerable<INodeDefinition> Collect(IMethodDefinition methodDefinition)
        {
            if (!AcceptConstructorsAsFunctors && methodDefinition.IsConstructor)
                yield break;

            if (!AcceptNodesThatHaveRefParams && methodDefinition.Parameters.Any(param => param.IsByReference))
                yield break;

            if (!AcceptNodesThatWorkWithUnclonableTypes && !(
                methodDefinition.Type.IsClonable() &&
                methodDefinition.Parameters.Any(param => param.Type.IsClonable()))
               )
                yield break;

            var nodeattribute = methodDefinition.Attributes.FirstOrDefault(attribute => attribute.Type.TypeName() == "NodeAttribute");

            if (AcceptNodesWithoutNodeAttribute || nodeattribute != null)
            {
                DataflowNodeDefinition node;
                if (methodDefinition.IsStatic)
                    node = new FunctionNodeDefinition();
                else
                    node = new FunctorNodeDefinition()
                {
                    StateType = methodDefinition.ContainingTypeDefinition
                };

                node.MethodDefinition = methodDefinition;

                var namearg = nodeattribute.GetArgument("Name");
                node.Name = namearg.ValueIsString() ? namearg.Value() : methodDefinition.Name.Value;

                var categoryarg = nodeattribute.GetArgument("Category");
                node.Category = categoryarg.ValueIsString() ? categoryarg.Value() : FullTypeNameToCategory(methodDefinition.ContainingTypeDefinition.TypeName(true));

                var versionarg = nodeattribute.GetArgument("Version");
                node.Version = versionarg.ValueIsString() ? versionarg.Value() : FullTypeNameToVersion(methodDefinition.ContainingTypeDefinition.TypeName(true), node);

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

                node.Filename = methodDefinition.Locations.First().Document.Location; // how could any methoddefinition exist in 2 locations?

                CollectInputsAndOutpus(methodDefinition, node);

                yield return node;
            }
        }
        
        public IEnumerable<INodeDefinition> Collect(IAssembly assembly)
        {
            return
                from type in assembly.GetAllTypes()
                where
                    (!type.IsInterface) &&
                    (
                        (type.IsStatic && AcceptFunctionNodes) ||
                        (!type.IsStatic && (AcceptFunctionNodes || AcceptFunctorNodes))
                    )
                let functortype =
                    (type.IsValueType || type.HasDefaultConstructor(FHost)) &&
                    (AcceptNodesThatWorkWithUnclonableTypes || type.IsClonable())
                from method in type.Methods
                where
                    (method.IsStatic && AcceptFunctionNodes) ||
                    (!method.IsStatic && AcceptFunctorNodes && functortype)
                from node in Collect(method)
                select node;
        }

    }
}
