using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.IO.Pointers;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO
{
    public class PointerRegistry : IORegistryBase
    {
        public PointerRegistry()
        {
            RegisterInput<FastValueInput>(
            (factory, context) =>
            {
                var t = context.IOType;
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueFastIn)));
                var stream = Activator.CreateInstance(t, container.RawIOObject) as IInStream;

                if (context.IOAttribute.AutoValidate)
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            RegisterInput<ValueInput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueFastIn)));
                    var stream = Activator.CreateInstance(t, container.RawIOObject) as IInStream;

                    if (context.IOAttribute.AutoValidate)
                        return IOContainer.Create(context, stream, container, s => s.Sync());
                    else
                        return IOContainer.Create(context, stream, container);
                });

            RegisterOutput<ValueOutput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                    var stream = Activator.CreateInstance(t, container.RawIOObject) as IOutStream;

                    return IOContainer.Create(context, stream, container); 
                });

            RegisterInput<ColorInput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IColorIn)));
                    var stream = Activator.CreateInstance(t, container.RawIOObject) as IOutStream;

                    return IOContainer.Create(context, stream, container);
                });

            RegisterOutput<ColorOutput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IColorOut)));
                    var stream = Activator.CreateInstance(t, container.RawIOObject) as IOutStream;

                    return IOContainer.Create(context, stream, container);
                });

            RegisterInput<TransformInput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(ITransformIn)));
                    var stream = Activator.CreateInstance(t, container.RawIOObject) as IOutStream;

                    return IOContainer.Create(context, stream, container);
                });
            RegisterOutput<TransformOutput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(ITransformOut)));
                    var stream = Activator.CreateInstance(t, container.RawIOObject) as IOutStream;

                    return IOContainer.Create(context, stream, container);
                });

            //Plugin Interface version
            RegisterInput<IFastValuePointerInput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueFastIn)));
                    var stream = Activator.CreateInstance(typeof(FastValueInput), container.RawIOObject) as IInStream;

                    if (context.IOAttribute.AutoValidate)
                        return IOContainer.Create(context, stream, container, s => s.Sync());
                    else
                        return IOContainer.Create(context, stream, container);
                });
            RegisterInput<IValuePointerInput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueFastIn)));
                    var stream = Activator.CreateInstance(typeof(ValueInput), container.RawIOObject) as IInStream;

                    if (context.IOAttribute.AutoValidate)
                        return IOContainer.Create(context, stream, container, s => s.Sync());
                    else
                        return IOContainer.Create(context, stream, container);
                });

            RegisterOutput<IValuePointerOutput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                    var stream = Activator.CreateInstance(typeof(ValueOutput), container.RawIOObject) as IOutStream;

                    return IOContainer.Create(context, stream, container);
                });

            RegisterInput<IColorPointerInput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IColorIn)));
                    var stream = Activator.CreateInstance(typeof(ColorInput), container.RawIOObject) as IOutStream;

                    return IOContainer.Create(context, stream, container);
                });

            RegisterOutput<IColorPointerOutput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IColorOut)));
                    var stream = Activator.CreateInstance(typeof(ColorOutput), container.RawIOObject) as IOutStream;

                    return IOContainer.Create(context, stream, container);
                });

            RegisterInput<ITransformPointerInput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(ITransformIn)));
                    var stream = Activator.CreateInstance(typeof(TransformInput), container.RawIOObject) as IInStream;

                    return IOContainer.Create(context, stream, container);
                });
            RegisterOutput<ITransformPointerOutput>(
                (factory, context) =>
                {
                    var t = context.IOType;
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(ITransformOut)));
                    var stream = Activator.CreateInstance(typeof(TransformOutput), container.RawIOObject) as IOutStream;

                    return IOContainer.Create(context, stream, container);
                });
        }
    }
}
