using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;
using VVVV.Utils.Streams;
using VL.Lang.Symbols;
using VL.Lang.Platforms.CIL;
using VL.Lang.Platforms.CIL.Cci;

namespace VVVV.VL.Hosting
{
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using EvaluateDelegateType = System.Action<int, MemoryIOStream<object>, IInStream[], IOutStream[]>;
    class EvaluateMethodBuilder
    {
        private readonly Host FVlHost;
        private readonly HostEnvironment FHost;
        private readonly NameProvider FNameProvider;
        private readonly CilCompilation FCompilation;
        private readonly DocSymbols FScope;
        private readonly bool FReturnValueIsState;

        private ExpressionBuilder FExpressionBuilder;
        private Dictionary<ITypeReference, ILocalDefinition> FArrays;
        private Dictionary<IParameterDefinition, int> FArrayOffsets;
        private List<IParameterDefinition> FParameters;
        private IMethodDefinition FMethodToCall;
        private ITypeReference FInstanceType;
        private IParameterDefinition FInstanceStreamParameter;
        private ILocalDefinition FInstanceReader;
        private ILocalDefinition FInstanceArray;

        public EvaluateMethodBuilder(Host vlHost, CilCompilation compilation, DocSymbols scope, bool returnValueIsState)
        {
            FVlHost = vlHost;
            FCompilation = compilation;
            FHost = compilation.CciHost;
            FNameProvider = new NameProvider(FHost.NameTable);
            FScope = scope;
            FReturnValueIsState = returnValueIsState;
        }

        public Task<EvaluateDelegateType> BuildAsync(IMethodReference methodToCall) => Task.Run(() => Build(methodToCall));

        public EvaluateDelegateType Build(IMethodReference methodToCall)
        {
            FMethodToCall = methodToCall.ResolvedMethod;
            FParameters = FMethodToCall.Parameters.ToList();
            if (FMethodToCall.Type.TypeCode != PrimitiveTypeCode.Void)
            {
                // Create a fake output parameter for return value so we can treat it uniformly
                FParameters.Add(new ParameterDefinition() {
                    Name = FHost.NameTable.GetNameFor("return"),
                    Type = FReturnValueIsState ? FHost.PlatformType.SystemObject : FMethodToCall.Type,
                    IsByReference = true,
                    IsOut = true
                });
            }
            var evaluateMethod = CreateEvaluateMethod();
            var dynamicMethod = FCompilation.Load(evaluateMethod, false);
            //FCompilation.Save();
            return dynamicMethod.CreateDelegate(typeof(EvaluateDelegateType)) as EvaluateDelegateType;
        }

        private IMethodDefinition CreateEvaluateMethod()
        {
            var nonGenericInStreamType = FVlHost.UtilsAssembly.ResolvedAssembly
                .GetTypeDefinition(typeof(IInStream));
            var nonGenericInStreamArrayType = nonGenericInStreamType.MakeArrayType(FHost.InternFactory);
            var nonGenericOutStreamType = FVlHost.UtilsAssembly.ResolvedAssembly
                .GetTypeDefinition(typeof(IOutStream));
            var nonGenericOutStreamArrayType = nonGenericOutStreamType.MakeArrayType(FHost.InternFactory);
            var evaluateMethod = new MethodDefinition()
            {
                CallingConvention = CallingConvention.Standard,
                InternFactory = FHost.InternFactory,
                IsCil = true,
                IsStatic = true,
                IsVirtual = false,
                Name = FNameProvider.GetUniqueName("Evaluate"),
                Type = FHost.PlatformType.SystemVoid,
                Parameters = new List<IParameterDefinition>(4),
                Visibility = TypeMemberVisibility.Public
            };
            var maxLengthParameter = new ParameterDefinition()
            {
                ContainingSignature = evaluateMethod,
                Index = 0,
                Name = FNameProvider.GetUniqueName("maxLength"),
                Type = FHost.PlatformType.SystemInt32
            };
            evaluateMethod.Parameters.Add(maxLengthParameter);
            var instanceStreamParameter = new ParameterDefinition()
            {
                ContainingSignature = evaluateMethod,
                Index = 1,
                Name = FNameProvider.GetUniqueName("instanceStream"),
                Type = GetInstanceStreamType(FHost.PlatformType.SystemObject)
            };
            evaluateMethod.Parameters.Add(instanceStreamParameter);
            var inputStreamsParameter = new ParameterDefinition()
            {
                ContainingSignature = evaluateMethod,
                Index = 2,
                Name = FNameProvider.GetUniqueName("inputStreams"),
                Type = nonGenericInStreamArrayType
            };
            evaluateMethod.Parameters.Add(inputStreamsParameter);
            var outputStreamsParameter = new ParameterDefinition()
            {
                ContainingSignature = evaluateMethod,
                Index = 3,
                Name = FNameProvider.GetUniqueName("outputStreams"),
                Type = nonGenericOutStreamArrayType
            };
            evaluateMethod.Parameters.Add(outputStreamsParameter);

            var methodBlock = new BlockStatement();
            var methodBody = new SourceMethodBody(FHost, null)
            {
                MethodDefinition = evaluateMethod,
                LocalsAreZeroed = true,
                Block = methodBlock
            };
            evaluateMethod.Body = methodBody;

            FExpressionBuilder = new ExpressionBuilder(FHost, evaluateMethod, null, FCompilation, FScope, ImmutableDictionary<ITypeParameterSymbol, IGenericParameter>.Empty, FNameProvider);

            Emit_EvaluateBody(methodBlock, evaluateMethod);

            return evaluateMethod;
        }

        private ITypeReference GetStreamType(System.Type streamReflectionType, ITypeReference elementType)
        {
            var streamType = FVlHost.UtilsAssembly.ResolvedAssembly
                .GetTypeDefinition(streamReflectionType);
            return streamType.MakeGenericType(FHost.InternFactory, elementType);
        }

        private ITypeReference GetInputStreamType(ITypeReference elementType)
        {
            return GetStreamType(typeof(IInStream<>), elementType);
        }

        private ITypeReference GetOutputStreamType(ITypeReference elementType)
        {
            return GetStreamType(typeof(IOutStream<>), elementType);
        }

        private ITypeReference GetInstanceStreamType(ITypeReference elementType)
        {
            return GetStreamType(typeof(MemoryIOStream<>), elementType);
        }
        
        private void Emit_EvaluateBody(BlockStatement block, MethodDefinition evaluateMethod)
        {
            var nameTable = FHost.NameTable;
            var utilsAssembly = FVlHost.UtilsAssembly.ResolvedAssembly;
            var genericStreamReader = utilsAssembly.GetTypeDefinition(typeof(VVVV.Utils.Streams.IStreamReader<>));
            var genericStreamWriter = utilsAssembly.GetTypeDefinition(typeof(VVVV.Utils.Streams.IStreamWriter<>));

            // Retrieve the in and output streams
            var maxLength = evaluateMethod.Parameters[0];
            var instanceStreamParameter = evaluateMethod.Parameters[1];
            var inputStreamsParameter = evaluateMethod.Parameters[2];
            var outputStreamsParameter = evaluateMethod.Parameters[3];
            // Instance stream
            FInstanceStreamParameter = default(IParameterDefinition);
            FInstanceType = default(ITypeReference);
            if (!FMethodToCall.IsStatic)
            {
                FInstanceStreamParameter = instanceStreamParameter;
                FInstanceType = FMethodToCall.ContainingType;
            }
            // Input streams
            var inputLocals = new List<ILocalDefinition>();
            foreach (var parameter in FParameters)
            {
                if (parameter.IsOut()) break;
                var streamType = GetInputStreamType(parameter.Type);
                var local = evaluateMethod.DefineLocal(streamType, FNameProvider.GetUniqueName(parameter.Name + "InStream"));
                block.Statements.Add(
                    new LocalDeclarationStatement()
                    {
                        LocalVariable = local,
                        InitialValue = GetStreamArrayIndexerExpression(inputStreamsParameter, inputLocals.Count, streamType)
                    });
                inputLocals.Add(local);
            }
            // Output streams
            var outputLocals = new List<ILocalDefinition>();
            foreach (var parameter in FParameters)
            {
                if (!parameter.IsOut()) continue;
                var streamType = GetOutputStreamType(parameter.Type);
                var local = evaluateMethod.DefineLocal(streamType, FNameProvider.GetUniqueName(parameter.Name + "OutStream"));
                block.Statements.Add(
                    new LocalDeclarationStatement()
                    {
                        LocalVariable = local,
                        InitialValue = FReturnValueIsState && IsFakeOutParameter(parameter)
                            ? new BoundExpression() { Definition = FInstanceStreamParameter, Type = FInstanceStreamParameter.Type }
                            : GetStreamArrayIndexerExpression(outputStreamsParameter, outputLocals.Count, streamType)
                    });
                outputLocals.Add(local);
            }

            if (FReturnValueIsState)
                Emit_SetLengthOfOutputs(block, maxLength, outputLocals.SkipLast(1));
            else
                Emit_SetLengthOfOutputs(block, maxLength, outputLocals);

            var inputParameters = FParameters.Where(p => !p.IsOut()).ToArray();
            var outputParameters = FParameters.Where(p => p.IsOut()).ToArray();

            var readers = Emit_CreateReaders(block, evaluateMethod, inputParameters, inputLocals);
            if (FInstanceStreamParameter != null)
            {
                var streamExpr = new BoundExpression() { Definition = FInstanceStreamParameter, Type = FInstanceStreamParameter.Type };
                FInstanceReader = Emit_CreateReader(block, evaluateMethod, streamExpr, FHost.PlatformType.SystemObject, "state", false);
            }
            var writers = Emit_CreateWriters(block, evaluateMethod, outputParameters, outputLocals);
            var localResults = Emit_CreateLocalResults(block, evaluateMethod, outputParameters);

            var tryBlock = new BlockStatement();

            {
                // In case spread count is one, go for single Read/Write methods,
                // as other route has more overhead.

                // TRUE block (one slice)
                var trueBlock = new BlockStatement();
                {
                    Emit_MethodCall(
                        trueBlock,
                        FMethodToCall,
                        new CompileTimeConstant()
                        {
                            Type = FHost.PlatformType.SystemInt32,
                            Value = 0
                        },
                        localResults,
                        parameter =>
                        {
                            var reader = readers[parameter];
                            var readMethod = reader.Type.ResolvedType.GetMethodDefinition(
                                nameTable.GetNameFor("Read"),
                                FHost.PlatformType.SystemInt32
                               );
                            // Cache in local to avoid double read calls due to null check for reference types
                            var resultLocal = evaluateMethod.DefineLocal(readMethod.Type, FNameProvider.GetUniqueName(parameter.Name + "Result"));
                            trueBlock.Statements.Add(
                                new LocalDeclarationStatement()
                                {
                                    LocalVariable = resultLocal,
                                    InitialValue = new MethodCall()
                                    {
                                        Arguments = new List<IExpression>()
                                        {
                                            new CompileTimeConstant()
                                            {
                                                Type = readMethod.Parameters.Last().Type,
                                                Value = readMethod.Parameters.Last().DefaultValue.Value
                                            }
                                        },
                                        IsVirtualCall = true,
                                        MethodToCall = readMethod,
                                        ThisArgument = new BoundExpression()
                                        {
                                            Definition = reader,
                                            Type = reader.Type
                                        },
                                        Type = readMethod.Type
                                    }
                                });
                            return new BoundExpression()
                            {
                                Definition = resultLocal,
                                Type = resultLocal.Type
                            };
                        },
                        () =>
                            // instanceReader<object>.Read(1) as INSTANCE_TYPE
                        new CastIfPossible()
                        {
                            ValueToCast = new MethodCall()
                            {
                                Arguments = new List<IExpression>()
                                {
                                    new CompileTimeConstant()
                                    {
                                        Type = FHost.PlatformType.SystemInt32,
                                        Value = 1
                                    }
                                },
                                IsVirtualCall = true,
                                MethodToCall = FInstanceReader.Type.ResolvedType.GetMethodDefinition(
                                    FHost.NameTable.GetNameFor("Read"),
                                    FHost.PlatformType.SystemInt32
                                   ),
                                ThisArgument = new BoundExpression()
                                {
                                    Definition = FInstanceReader,
                                    Type = FInstanceReader.Type
                                },
                                Type = FHost.PlatformType.SystemObject
                            },
                            TargetType = FInstanceType,
                            Type = FInstanceType
                        }
                       );
                    Emit_WriteLocalResults(trueBlock, outputParameters, writers, localResults);
                }

                // FALSE block (more than one slice)
                var falseBlock = new BlockStatement();
                {
                    FArrays = Emit_CreateArrays(falseBlock, evaluateMethod);

                    if (FInstanceStreamParameter != null)
                        FInstanceArray = Emit_CreateArray(falseBlock, evaluateMethod, FHost.PlatformType.SystemObject, StreamUtils.BUFFER_SIZE, "state");

                    // Get offsets for in and outputs
                    FArrayOffsets = GetArrayOffsets(FArrays);

                    // try block
                    var falseTryBlock = new BlockStatement();

                    Emit_OuterLoop(
                        falseTryBlock,
                        evaluateMethod,
                        FMethodToCall,
                        maxLength,
                        inputParameters,
                        outputParameters,
                        readers,
                        writers,
                        localResults
                       );

                    // finally block

                    // Put all arrays back into pool
                    var falseFinallyBlock = new BlockStatement();

                    foreach (ILocalDefinition array in FArrays.Values)
                    {
                        Emit_PutMemory(falseFinallyBlock, array);
                    }

                    if (FInstanceArray != null)
                    {
                        Emit_PutMemory(falseFinallyBlock, FInstanceArray);
                    }

                    var falseTryCatchFinallyStatement = new TryCatchFinallyStatement()
                    {
                        FinallyBody = falseFinallyBlock,
                        TryBody = falseTryBlock
                    };

                    falseBlock.Statements.Add(falseTryCatchFinallyStatement);
                }

                tryBlock.Statements.Add(
                    new ConditionalStatement()
                    {
                        Condition = new Equality()
                        {
                            LeftOperand = new BoundExpression()
                            {
                                Definition = maxLength,
                                Type = maxLength.Type
                            },
                            RightOperand = new CompileTimeConstant()
                            {
                                Type = FHost.PlatformType.SystemInt32,
                                Value = 1
                            }
                        },
                        FalseBranch = falseBlock,
                        TrueBranch = trueBlock
                    }
                   );
            }

            var finallyBlock = new BlockStatement();
            {
                var readersAndWriters = readers.Values.Concat(writers.Values);
                Emit_DisposeCalls(finallyBlock, readersAndWriters);
                if (FInstanceReader != null)
                {
                    Emit_DisposeCalls(finallyBlock, IteratorHelper.GetSingletonEnumerable(FInstanceReader));
                }
            }

            if (finallyBlock.Statements.Count > 0)
            {
                block.Statements.Add(
                    new TryCatchFinallyStatement()
                    {
                        TryBody = tryBlock,
                        FinallyBody = finallyBlock
                    }
                   );
            }
            else
                block.Statements.Add(tryBlock);
            
            block.Statements.Add(new ReturnStatement());
        }

        private IExpression GetStreamArrayIndexerExpression(IParameterDefinition streamsParameter, int index, ITypeReference targetType)
        {
            var streamsParameterType = streamsParameter.Type as IArrayTypeReference;
            return new CastIfPossible()
            {
                ValueToCast = new ArrayIndexer()
                {
                    IndexedObject = new BoundExpression()
                    {
                        Definition = streamsParameter,
                        Type = streamsParameter.Type
                    },
                    Indices = new List<IExpression>(1)
                    {
                        new CompileTimeConstant()
                        {
                            Value = index,
                            Type = FHost.PlatformType.SystemInt32
                        }
                    },
                    Type = streamsParameterType.ElementType
                },
                TargetType = targetType,
                Type = targetType
            };
        }
        
        private void Emit_SetLengthOfOutputs(
            BlockStatement block,
            IParameterDefinition maxLength,
            IEnumerable<ILocalDefinition> outputStreams
           )
        {
            foreach (var outputLocal in outputStreams)
            {
                var lengthProperty = outputLocal.Type.ResolvedType.GetPropertyDefinition(FHost.NameTable.GetNameFor("Length"));
                var setLengthCall = new MethodCall()
                {
                    MethodToCall = lengthProperty.Setter,
                    ThisArgument = new BoundExpression()
                    {
                        Definition = outputLocal, 
                        Type = outputLocal.Type
                    },
                    IsStaticCall = false,
                    IsVirtualCall = true,
                    Arguments = new List<IExpression>()
                    {
                        new BoundExpression()
                        {
                            Definition = maxLength,
                            Type = maxLength.Type
                        }
                    },
                    Type = lengthProperty.Setter.Type
                };
                
                block.Statements.Add(new ExpressionStatement() { Expression = setLengthCall });
            }
        }

        private Dictionary<IParameterDefinition, ILocalDefinition> Emit_CreateReaders(
            BlockStatement block,
            IMethodDefinition evaluateMethod,
            IEnumerable<IParameterDefinition> parameters,
            List<ILocalDefinition> inputStreams
           )
        {
            // Create a reader for each input stream
            var inputReaders = new Dictionary<IParameterDefinition, ILocalDefinition>();
            var needCyclicReader = inputStreams.Count > 1;
            var i = 0;
            foreach (var parameter in parameters)
            {
                var inputLocal = inputStreams[i++];
                var streamExpr = new BoundExpression() { Definition = inputLocal, Type = inputLocal.Type };
                inputReaders[parameter] = Emit_CreateReader(block, evaluateMethod, streamExpr, parameter.Type, parameter.Name.Value, needCyclicReader);
            }
            return inputReaders;
        }
        
        private ILocalDefinition Emit_CreateReader(BlockStatement block, IMethodDefinition evaluateMethod, IExpression streamExpression, ITypeReference elementType, string name, bool cyclic)
        {
            var utilsAssembly = FVlHost.UtilsAssembly.ResolvedAssembly;
            var genericStreamReader = utilsAssembly.GetTypeDefinition(typeof(IStreamReader<>));
            var streamUtils = utilsAssembly.GetTypeDefinition(typeof(StreamUtils));
            var genericGetCyclicReaderMethod = streamUtils.GetMethodDefinition(FHost.NameTable.GetNameFor("GetCyclicReader"));
            var readerType = genericStreamReader.MakeGenericType(FHost.InternFactory, elementType);
            var reader = evaluateMethod.DefineLocal(
                readerType,
                FNameProvider.GetUniqueName(name + "Reader")
               );
            
            MethodCall getReaderMethodCall = null;
            if (cyclic)
            {
                var getCyclicReaderMethod = genericGetCyclicReaderMethod.MakeGenericMethod(FHost.InternFactory, elementType);
                getReaderMethodCall = new MethodCall()
                {
                    Arguments = new List<IExpression>()
                    {
                        streamExpression
                    },
                    IsStaticCall = true,
                    MethodToCall = getCyclicReaderMethod,
                    Type = getCyclicReaderMethod.Type
                };
            }
            else
            {
                var getReaderMethod = utilsAssembly
                    .GetTypeDefinition(typeof(IInStream<>))
                    .MakeGenericType(FHost.InternFactory, elementType)
                    .ResolvedType
                    .GetMethodDefinition(FHost.NameTable.GetNameFor("GetReader"));
                getReaderMethodCall = new MethodCall()
                {
                    IsVirtualCall = true,
                    MethodToCall = getReaderMethod,
                    ThisArgument = streamExpression,
                    Type = getReaderMethod.Type
                };
            }
            
            block.Statements.Add(
                new LocalDeclarationStatement()
                {
                    LocalVariable = reader,
                    InitialValue = getReaderMethodCall
                }
               );
            return reader;
        }

        private Dictionary<IParameterDefinition, ILocalDefinition> Emit_CreateWriters(
            BlockStatement block,
            IMethodDefinition evaluateMethod,
            IEnumerable<IParameterDefinition> parameters,
            List<ILocalDefinition> outputStreams
           )
        {
            // Create a writer for each output stream
            var nameTable = FHost.NameTable;
            var utilsAssembly = FVlHost.UtilsAssembly.ResolvedAssembly;
            var genericStreamWriter = utilsAssembly.GetTypeDefinition(typeof(IStreamWriter<>));
            var outputWriters = new Dictionary<IParameterDefinition, ILocalDefinition>();
            var i = 0;
            foreach (var parameter in parameters)
            {
                var outputLocal = outputStreams[i++];
                var outputWriterType = genericStreamWriter.MakeGenericType(FHost.InternFactory, parameter.Type);
                var outputWriter = evaluateMethod.DefineLocal(
                    outputWriterType,
                    nameTable.GetNameFor(string.Format("{0}Writer", parameter.Name.Value))
                   );
                var getWriterMethod = outputLocal.Type.ResolvedType.GetMethodDefinition(nameTable.GetNameFor("GetWriter"));
                var getWriterMethodCall = new MethodCall()
                {
                    IsVirtualCall = true,
                    MethodToCall = getWriterMethod,
                    ThisArgument = new BoundExpression()
                    {
                        Definition = outputLocal,
                        Type = outputLocal.Type
                    },
                    Type = getWriterMethod.Type,
                };

                block.Statements.Add(
                    new LocalDeclarationStatement()
                    {
                        LocalVariable = outputWriter,
                        InitialValue = getWriterMethodCall
                    }
                   );

                outputWriters[parameter] = outputWriter;
            }
            return outputWriters;
        }

        private Dictionary<IParameterDefinition, ILocalDefinition> Emit_CreateLocalResults(
            BlockStatement block,
            IMethodDefinition evaluateMethod,
            IEnumerable<IParameterDefinition> parameters
           )
        {
            // Create local result variables for each output
            var nameTable = FHost.NameTable;
            var locals = new Dictionary<IParameterDefinition, ILocalDefinition>();
            foreach (var parameter in parameters)
            {
                var local = evaluateMethod.DefineLocal(parameter.Type, nameTable.GetNameFor(string.Format("{0}Result", parameter.Name.Value)));
                block.Statements.Add(
                    new LocalDeclarationStatement()
                    {
                        LocalVariable = local
                    }
                   );
                
                locals[parameter] = local;
            }
            return locals;
        }
        
        private void Emit_WriteLocalResults(
            BlockStatement block,
            IEnumerable<IParameterDefinition> outputParameters,
            Dictionary<IParameterDefinition, ILocalDefinition> outputWriters,
            Dictionary<IParameterDefinition, ILocalDefinition> localResults
           )
        {
            // writer.Write(localResult, 1)
            var nameTable = FHost.NameTable;
            foreach (var parameter in outputParameters)
            {
                var writer = outputWriters[parameter];
                var localResult = localResults[parameter];
                var writeMethod = writer.Type.ResolvedType.GetMethodDefinition(
                    nameTable.GetNameFor("Write"),
                    parameter.Type,
                    FHost.PlatformType.SystemInt32
                   );
                block.Statements.Add(
                    new ExpressionStatement()
                    {
                        Expression = new MethodCall()
                        {
                            Arguments = new List<IExpression>()
                            {
                                new BoundExpression()
                                {
                                    Definition = localResult,
                                    Type = localResult.Type
                                },
                                new CompileTimeConstant()
                                {
                                    Type = writeMethod.Parameters.Last().Type,
                                    Value = writeMethod.Parameters.Last().DefaultValue.Value
                                }
                            },
                            IsVirtualCall = true,
                            MethodToCall = writeMethod,
                            ThisArgument = new BoundExpression()
                            {
                                Definition = writer,
                                Type = writer.Type
                            },
                            Type = FHost.PlatformType.SystemVoid
                        }
                    }
                   );
            }
        }
        
        private Dictionary<ITypeReference, ILocalDefinition> Emit_CreateArrays(BlockStatement block, IMethodDefinition methodDefinition)
        {
            var nameTable = FHost.NameTable;
            var arrays = new Dictionary<ITypeReference, ILocalDefinition>(CciComparers.TypeComparer);
            var arraySizes = new Dictionary<ITypeReference, int>(CciComparers.TypeComparer);
            // Compute array size
            var types = FParameters.Select(p => p.Type).ToList();
            foreach (var parameterType in types)
            {
                if (!arraySizes.ContainsKey(parameterType))
                    arraySizes[parameterType] = 0;
                arraySizes[parameterType] += StreamUtils.BUFFER_SIZE;
            }
            // Allocate arrays
            foreach (var parameterType in arraySizes.Keys)
            {
                arrays[parameterType] = Emit_CreateArray(
                    block,
                    methodDefinition,
                    parameterType,
                    arraySizes[parameterType],
                    TypeHelper.GetTypeName(parameterType, NameFormattingOptions.UseTypeKeywords)
                   );;
            }
            return arrays;
        }

        private ILocalDefinition Emit_CreateArray(BlockStatement block, IMethodDefinition methodDefinition, ITypeReference elementType, int arraySize, string name)
        {
            var utilsAssembly = FVlHost.UtilsAssembly.ResolvedAssembly;
            var genericMemoryPoolType = utilsAssembly.GetTypeDefinition(typeof(MemoryPool<>));
            var array = methodDefinition.DefineLocal(
                Vector45.GetVector(elementType, FHost.InternFactory),
                FNameProvider.GetUniqueName(name + "Array"));
            var memoryPoolType = genericMemoryPoolType.MakeGenericType(FHost.InternFactory, elementType);
            var getArrayMethod = memoryPoolType.ResolvedType.GetMethodDefinition(FHost.NameTable.GetNameFor("GetArray"));
            var getArrayMethodCall = new MethodCall()
            {
                Arguments = new List<IExpression>()
                {
                    new CompileTimeConstant()
                    {
                        Type = FHost.PlatformType.SystemInt32,
                        Value = arraySize
                    }
                },
                IsStaticCall = true,
                MethodToCall = getArrayMethod,
                Type = getArrayMethod.Type
            };

            block.Statements.Add(
                new LocalDeclarationStatement()
                {
                    LocalVariable = array,
                    InitialValue = getArrayMethodCall
                }
               );
            return array;
        }
        
        private void Emit_PutMemory(BlockStatement block, ILocalDefinition array)
        {
            var utilsAssembly = FVlHost.UtilsAssembly.ResolvedAssembly;
            var genericMemoryPool = utilsAssembly.GetTypeDefinition(typeof(MemoryPool<>));
            var arrayType = array.Type as Microsoft.Cci.Immutable.Vector;
            var memoryPool = genericMemoryPool.MakeGenericType(FHost.InternFactory, arrayType.ElementType);
            var putArrayMethod = memoryPool.ResolvedType.GetMethodDefinition(FHost.NameTable.GetNameFor("PutArray"));
            block.Statements.Add(
                new ExpressionStatement()
                {
                    Expression = new MethodCall()
                    {
                        Arguments = new List<IExpression>()
                        {
                            new BoundExpression()
                            {
                                Definition = array,
                                Type = array.Type
                            }
                        },
                        IsStaticCall = true,
                        MethodToCall = putArrayMethod,
                        Type = FHost.PlatformType.SystemVoid
                    }
                }
               );
        }
        
        private void Emit_OuterLoop(
            BlockStatement block,
            IMethodDefinition evaluateMethod,
            IMethodReference methodToCall,
            IParameterDefinition maxLength,
            IEnumerable<IParameterDefinition> inputs,
            IEnumerable<IParameterDefinition> outputs,
            Dictionary<IParameterDefinition, ILocalDefinition> readers,
            Dictionary<IParameterDefinition, ILocalDefinition> writers,
            Dictionary<IParameterDefinition, ILocalDefinition> localResults
           )
        {
            var nameTable = FHost.NameTable;
            var numSlicesToRead = evaluateMethod.DefineLocal(maxLength.Type, nameTable.GetNameFor("numSlicesToRead"));
            block.Statements.Add(
                new LocalDeclarationStatement()
                {
                    InitialValue = new BoundExpression()
                    {
                        Definition = maxLength,
                        Type = maxLength.Type
                    },
                    LocalVariable = numSlicesToRead
                }
               );
            
            var loopBlock = new BlockStatement();
            
            Emit_OuterLoopBody(
                loopBlock,
                evaluateMethod,
                methodToCall,
                numSlicesToRead,
                maxLength,
                inputs,
                outputs,
                readers,
                writers,
                localResults);
            
            block.Statements.Add(
                new WhileDoStatement()
                {
                    Condition = new GreaterThan()
                    {
                        LeftOperand = new BoundExpression()
                        {
                            Definition = numSlicesToRead,
                            Type = numSlicesToRead.Type
                        },
                        RightOperand = new CompileTimeConstant()
                        {
                            Type = numSlicesToRead.Type,
                            Value = 0
                        },
                        Type = FHost.PlatformType.SystemBoolean
                    },
                    Body = loopBlock
                }
               );
        }
        
        private void Emit_OuterLoopBody(
            BlockStatement block,
            IMethodDefinition evaluateMethod,
            IMethodReference methodToCall,
            ILocalDefinition numSlicesToRead,
            IParameterDefinition maxLength,
            IEnumerable<IParameterDefinition> inputParameters,
            IEnumerable<IParameterDefinition> outputParameters,
            Dictionary<IParameterDefinition, ILocalDefinition> readers,
            Dictionary<IParameterDefinition, ILocalDefinition> writers,
            Dictionary<IParameterDefinition, ILocalDefinition> localResults
           )
        {
            // int blockSize = Math.Min(StreamUtils.BUFFER_SIZE, numSlicesToRead)
            var nameTable = FHost.NameTable;
            var blockSize = evaluateMethod.DefineLocal(FHost.PlatformType.SystemInt32, nameTable.GetNameFor("blockSize"));
            var minMethod = FHost.CoreAssembly.ResolvedAssembly
                .GetTypeDefinition(typeof(Math))
                .GetMethodDefinition(
                    nameTable.GetNameFor("Min"),
                    blockSize.Type,
                    blockSize.Type
                   );
            
            block.Statements.Add(
                new LocalDeclarationStatement()
                {
                    InitialValue = new MethodCall()
                    {
                        Arguments = new List<IExpression>()
                        {
                            new CompileTimeConstant()
                            {
                                Type = FHost.PlatformType.SystemInt32,
                                Value = StreamUtils.BUFFER_SIZE
                            },
                            new BoundExpression()
                            {
                                Definition = numSlicesToRead,
                                Type = numSlicesToRead.Type
                            }
                        },
                        IsStaticCall = true,
                        MethodToCall = minMethod,
                        Type = minMethod.Type
                    },
                    LocalVariable = blockSize
                }
               );
            
            // Call Read methods
            foreach (var parameter in inputParameters)
            {
                var reader = readers[parameter];
                var array = FArrays[parameter.Type];
                var arrayOffset = FArrayOffsets[parameter];
                Emit_ReadArray(block, reader, array, arrayOffset, blockSize);
            }
            
            if (FInstanceArray != null)
            {
                Emit_ReadArray(block, FInstanceReader, FInstanceArray, 0, blockSize);
            }
            
            // Call the actual method chunkSize times
            var innerLoopBlock = new BlockStatement();
            var iterator = evaluateMethod.DefineLocal(FHost.PlatformType.SystemInt32, nameTable.GetNameFor("i"));
            
            // someResult = MethodToCall(doubleArray[i + offset], ..., out otherResult, ...)
            Emit_MethodCall(
                innerLoopBlock,
                methodToCall,
                new Addition()
                {
                    LeftOperand = new Subtraction()
                    {
                        LeftOperand = new BoundExpression()
                        {
                            Definition = maxLength,
                            Type = maxLength.Type
                        },
                        RightOperand = new BoundExpression()
                        {
                            Definition = numSlicesToRead,
                            Type = numSlicesToRead.Type
                        }
                    },
                    RightOperand = new BoundExpression()
                    {
                        Definition = iterator,
                        Type = iterator.Type
                    }
                },
                localResults,
                (parameter) =>
                {
                    var inputArray = FArrays[parameter.Type];
                    var inputArrayOffset = FArrayOffsets[parameter];
                    IExpression index = new BoundExpression() { Definition = iterator, Type = iterator.Type };
                    if (inputArrayOffset > 0)
                    {
                        index = new Addition()
                        {
                            LeftOperand = index,
                            RightOperand = new CompileTimeConstant() { Type = FHost.PlatformType.SystemInt32, Value = inputArrayOffset },
                            Type = iterator.Type
                        };
                    }
                    return new ArrayIndexer()
                    {
                        IndexedObject = new BoundExpression()
                        {
                            Definition = inputArray,
                            Type = inputArray.Type
                        },
                        Indices = new List<IExpression>() { index },
                        Type = parameter.Type
                    };
                },
                () =>
                {
                    return new CastIfPossible()
                    {
                        ValueToCast = new ArrayIndexer()
                        {
                            IndexedObject = new BoundExpression()
                            {
                                Definition = FInstanceArray,
                                Type = FInstanceArray.Type
                            },
                            Indices = new List<IExpression>()
                            {
                                new BoundExpression() { Definition = iterator, Type = iterator.Type }
                            },
                            Type = FHost.PlatformType.SystemObject
                        },
                        TargetType = FInstanceType,
                        Type = FInstanceType
                    };
                }
               );
            
            // outputArray[outputArrayOffset + i] = someResult
            // output2Array[output2ArrayOffset + i] = otherResult
            // ...
            foreach (var parameter in outputParameters)
            {
                var outputArrayOffset = FArrayOffsets[parameter];
                var outputArray = FArrays[parameter.Type];
                var localResult = localResults[parameter];
                IExpression index = new BoundExpression() { Definition = iterator, Type = iterator.Type };
                if (outputArrayOffset > 0)
                {
                    index = new Addition()
                    {
                        LeftOperand = index,
                        RightOperand = new CompileTimeConstant() { Type = FHost.PlatformType.SystemInt32, Value = outputArrayOffset },
                        Type = iterator.Type
                    };
                }
                innerLoopBlock.Statements.Add(
                    new ExpressionStatement()
                    {
                        Expression = new Assignment()
                        {
                            Source = new BoundExpression()
                            {
                                Definition = localResult,
                                Type = localResult.Type
                            },
                            Target = new TargetExpression()
                            {
                                Definition = new ArrayIndexer()
                                {
                                    IndexedObject = new BoundExpression()
                                    {
                                        Definition = outputArray,
                                        Type = outputArray.Type
                                    },
                                    Indices = new List<IExpression>() { index },
                                    Type = methodToCall.Type
                                },
                                Instance = new BoundExpression()
                                {
                                    Definition = outputArray,
                                    Type = outputArray.Type
                                },
                                Type = methodToCall.Type
                            },
                            Type = FHost.PlatformType.SystemVoid
                        }
                    }
                   );
            }
            
            var innerLoop = new ForStatement()
            {
                InitStatements = new List<IStatement>()
                {
                    new LocalDeclarationStatement()
                    {
                        LocalVariable = iterator,
                        InitialValue = new CompileTimeConstant()
                        {
                            Type = iterator.Type,
                            Value = 0
                        }
                    }
                },
                Condition = new LessThan()
                {
                    LeftOperand = new BoundExpression()
                    {
                        Definition = iterator,
                        Type = iterator.Type
                    },
                    RightOperand = new BoundExpression()
                    {
                        Definition = blockSize,
                        Type = blockSize.Type
                    },
                    Type = FHost.PlatformType.SystemBoolean
                },
                IncrementStatements = new List<IStatement>()
                {
                    new ExpressionStatement()
                    {
                        Expression = new Assignment()
                        {
                            Target = new TargetExpression()
                            {
                                Type = iterator.Type,
                                Definition = iterator
                            },
                            Source = new Addition()
                            {
                                LeftOperand = new BoundExpression()
                                {
                                    Type = iterator.Type,
                                    Definition = iterator
                                },
                                RightOperand = new CompileTimeConstant()
                                {
                                    Type = iterator.Type,
                                    Value = 1
                                },
                                Type = iterator.Type
                            },
                            Type = FHost.PlatformType.SystemVoid
                        }
                    }
                },
                Body = innerLoopBlock
            };
            
            block.Statements.Add(innerLoop);
            
            // Call Write method
            foreach (var parameter in outputParameters)
            {
                var writer = writers[parameter];
                var array = FArrays[parameter.Type];
                var arrayOffset = FArrayOffsets[parameter];
                var writeMethod = writer.Type.ResolvedType.GetMethodDefinition(
                    nameTable.GetNameFor("Write"),
                    array.Type,
                    FHost.PlatformType.SystemInt32,
                    FHost.PlatformType.SystemInt32,
                    FHost.PlatformType.SystemInt32
                   );
                
                block.Statements.Add(
                    new ExpressionStatement()
                    {
                        Expression = new MethodCall()
                        {
                            Arguments = new List<IExpression>()
                            {
                                new BoundExpression()
                                {
                                    Definition = array,
                                    Type = array.Type
                                },
                                new CompileTimeConstant()
                                {
                                    Type = FHost.PlatformType.SystemInt32,
                                    Value = arrayOffset
                                },
                                new BoundExpression()
                                {
                                    Definition = blockSize,
                                    Type = blockSize.Type
                                },
                                new CompileTimeConstant()
                                {
                                    Type = writeMethod.Parameters.Last().Type,
                                    Value = writeMethod.ResolvedMethod.Parameters.Last().DefaultValue.Value,
                                }
                            },
                            IsVirtualCall = true,
                            MethodToCall = writeMethod,
                            ThisArgument = new BoundExpression()
                            {
                                Definition = writer,
                                Type = writer.Type
                            },
                            Type = writeMethod.Type
                        }
                    }
                   );
            }
            
            // numSlicesToRead = numSlicesToRead - chunkSize;
            block.Statements.Add(
                new ExpressionStatement()
                {
                    Expression = new Assignment()
                    {
                        Source = new Subtraction()
                        {
                            CheckOverflow = true,
                            LeftOperand = new BoundExpression()
                            {
                                Definition = numSlicesToRead,
                                Type = numSlicesToRead.Type
                            },
                            RightOperand = new BoundExpression()
                            {
                                Definition = blockSize,
                                Type = blockSize.Type
                            },
                            Type = blockSize.Type
                        },
                        Target = new TargetExpression()
                        {
                            Definition = numSlicesToRead,
                            Type = numSlicesToRead.Type
                        },
                        Type = numSlicesToRead.Type
                    }
                }
               );
        }
        
        private void Emit_ReadArray(BlockStatement block, ILocalDefinition reader, ILocalDefinition array, int arrayOffset, ILocalDefinition blockSize)
        {
            var readMethod = reader.Type.ResolvedType.GetMethodDefinition(
                FHost.NameTable.GetNameFor("Read"),
                array.Type,
                FHost.PlatformType.SystemInt32,
                FHost.PlatformType.SystemInt32,
                FHost.PlatformType.SystemInt32
               );
            
            block.Statements.Add(
                new ExpressionStatement()
                {
                    Expression = new MethodCall()
                    {
                        Arguments = new List<IExpression>()
                        {
                            new BoundExpression()
                            {
                                Definition = array,
                                Type = array.Type
                            },
                            new CompileTimeConstant()
                            {
                                Type = FHost.PlatformType.SystemInt32,
                                Value = arrayOffset
                            },
                            new BoundExpression()
                            {
                                Definition = blockSize,
                                Type = blockSize.Type
                            },
                            new CompileTimeConstant()
                            {
                                Type = readMethod.Parameters.Last().Type,
                                Value = readMethod.ResolvedMethod.Parameters.Last().DefaultValue.Value,
                            }
                        },
                        IsVirtualCall = true,
                        MethodToCall = readMethod,
                        ThisArgument = new BoundExpression()
                        {
                            Definition = reader,
                            Type = reader.Type
                        },
                        Type = readMethod.Type
                    }
                }
               );
        }
        
        private Dictionary<IParameterDefinition, int> GetArrayOffsets(Dictionary<ITypeReference, ILocalDefinition> arrays)
        {
            var finalArrayOffsets = new Dictionary<IParameterDefinition, int>();
            var tmpArrayOffsets = new Dictionary<ITypeReference, int>();

            // Allocate arrays and array segments
            foreach (var parameter in FParameters)
            {
                int offset = 0;
                var parameterType = parameter.Type;
                if (!tmpArrayOffsets.TryGetValue(parameterType, out offset))
                    tmpArrayOffsets[parameterType] = 0;
                tmpArrayOffsets[parameterType] += StreamUtils.BUFFER_SIZE;
                finalArrayOffsets[parameter] = offset;
            }
            
            return finalArrayOffsets;
        }

        private void Emit_MethodCall(
            BlockStatement block,
            IMethodReference methodToCall,
            IExpression stateFieldIndexExpression,
            Dictionary<IParameterDefinition, ILocalDefinition> localResults,
            Func<IParameterDefinition, IExpression> inputArgumentFactory,
            Func<IExpression> thisArgumentFactory
           )
        {
            var methodCall = new MethodCall()
            {
                IsStaticCall = methodToCall.IsStatic,
                MethodToCall = methodToCall,
                Type = methodToCall.Type
            };

            if (!methodCall.IsStaticCall)
                methodCall.ThisArgument = thisArgumentFactory();

            var called = false;
            foreach (var parameter in FParameters)
            {
                if (parameter.IsOut())
                {
                    var localResult = localResults[parameter];
                    // Watch out for our fake parameter
                    if (!IsFakeOutParameter(parameter))
                    {
                        methodCall.Arguments.Add(
                            new OutArgument()
                            {
                                Expression = new TargetExpression()
                                {
                                    Definition = localResult,
                                    Type = parameter.Type
                                },
                                Type = parameter.Type
                            }
                            );
                    }
                    else
                    {
                        called = true;
                        block.Statements.Add(
                            new ExpressionStatement()
                            {
                                Expression = new Assignment()
                                {
                                    Source = methodCall,
                                    Target = new TargetExpression()
                                    {
                                        Definition = localResult,
                                        Type = localResult.Type
                                    },
                                    Type = methodCall.Type
                                }
                            });
                    }
                }
                else if (parameter.IsByReference)
                {
                    throw new NotSupportedException("Parameters by reference are not supported yet!");
                }
                else
                {
                    var argumentExpr = inputArgumentFactory(parameter);
                    methodCall.Arguments.Add(argumentExpr);
                }
            }
            if (!called)
                block.Statements.Add(new ExpressionStatement() { Expression = methodCall } );
        }

        private bool IsFakeOutParameter(IParameterDefinition p) => p.ContainingSignature != FMethodToCall;
        
        private void Emit_DisposeCalls(BlockStatement block, IEnumerable<ILocalDefinition> disposables)
        {
            var nameTable = FHost.NameTable;
            foreach (var disposable in disposables)
            {
                var disposeMethod = disposable.Type.ResolvedType.GetMethodDefinition(nameTable.GetNameFor("Dispose"));
                block.Statements.Add(
                    new ExpressionStatement()
                    {
                        Expression = new MethodCall()
                        {
                            IsVirtualCall = true,
                            MethodToCall = disposeMethod,
                            ThisArgument = new BoundExpression()
                            {
                                Definition = disposable,
                                Type = disposable.Type
                            },
                            Type = FHost.PlatformType.SystemVoid
                        }
                    }
                   );
            }
        }
    }
}
