using System;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using System.Threading;
using VVVV.Core;
using VVVV.Core.Commands;
using VVVV.Core.Service;

namespace VVVV.Core.Serialization
{
    public class Serializer
    {
        //mapper from type to serializer
        public Mapper SerializerMapper { get; private set; }
        public MappingRegistry MappingRegistry { get; private set; }

        public Serializer()
        {
            MappingRegistry = new MappingRegistry();
            SerializerMapper = new Mapper(MappingRegistry);
            
            //register internal classes
            RegisterGeneric<Command, Command.CommandSerializer>();
            Register<CompoundCommand, CompoundCommand.CompoundCommandSerializer>();
            Register(typeof(AddCommand<,>), typeof(AddCommand<,>.AddCommandSerializer));
            Register(typeof(RemoveCommand<,>), typeof(RemoveCommand<,>.RemoveCommandSerializer));
            Register(typeof(SetPropertyCommand<>), typeof(SetPropertyCommand<>.SetPropertyCommandSerializer));
            RegisterGeneric<RenameCommand, RenameCommand.RenameCommandSerializer>();
        }

        //culture handling
        private CultureInfo FPreviousCulture = Thread.CurrentThread.CurrentCulture;
        private int FCultureSwitchCounter = 0;

        private void SwitchToInvariantCulture()
        {
            if (FCultureSwitchCounter <= 0)
            {
                // Save off previous culture and switch to invariant for serialization.
                FPreviousCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            }

            FCultureSwitchCounter++;
        }

        private void SwitchCultureBack()
        {
            FCultureSwitchCounter--;

            if (FCultureSwitchCounter <= 0)
            {
                //return to previoues culture
                Thread.CurrentThread.CurrentCulture = FPreviousCulture;
            }
        }

        //Serialize------------------------------------------------

        public XElement Serialize(object value)
        {
            SwitchToInvariantCulture();

            try
            {
                if (value != null)
                {
                    if (SerializerMapper.CanMap<ISerializer>(value.GetType()))
                    {
                        return SerializerMapper.Map<ISerializer>(value.GetType()).Serialize(value, this);
                    }

                    //if (SerializerMapper.CanMap(value.GetType(), typeof(ISerializer<>)))
                    //{
                    //    ret = ((ISerializer<object>)SerializerMapper.Map(value.GetType(), typeof(ISerializer<>))).Serialize(value, this);
                    //}

                    //Type st = typeof(ISerializer<>).MakeGenericType(value.GetType());
                    //if (SerializerMapper.CanMap(value.GetType(), st))
                    //{
                    //    ret = ((ISerializer<object>)SerializerMapper.Map(value.GetType(), st)).Serialize(value, this);
                    //}
                }
                
                return ToXml(value);
            }
            finally
            {
                SwitchCultureBack();
            }
        }

        //Deserialize------------------------------------------------

        //generic
        public T Deserialize<T>(XElement data, Type type)
        {
            return (T)Deserialize(data, type);
        }

        public T Deserialize<T>(XElement data)
        {
            return (T)Deserialize(data, typeof(T));
        }
        
        //default
        public object Deserialize(XElement data, Type type)
        {
            SwitchToInvariantCulture();

            try
            {
                if (SerializerMapper.CanMap<ISerializer>(type))
                {
                    return SerializerMapper.Map<ISerializer>(type).Deserialize(data, type, this);
                }
                else
                {
                    //if (SerializerMapper.CanMap(type, typeof(ISerializer<>)))
                    //{
                    //    ret = ((ISerializer<object>)SerializerMapper.Map(type, typeof(ISerializer<>))).Deserialize(data, type, this);
                    //}
                    //else
                    //{
                    //Type st = typeof(ISerializer<>).MakeGenericType(type);
                    //if (SerializerMapper.CanMap(type, st))
                    //{
                    //    ret = ((ISerializer<object>)SerializerMapper.Map(type, st)).Deserialize(data, type, this);
                    //}
                    //else
                    return FromXml(data, type);
                    //}
                }
            }
            finally
            {
                SwitchCultureBack();
            }
        }

        //standard type conversion------------------------
        
        //to xml:
        private XElement ToXml(object value)
        {
            if (value == null)
            {
                return new XElement("NULL");
            }
            
            var type = value.GetType();

            var element = new XElement("OBJECT");
            element.AddTypeAttribute(type);
            
            if (type.IsPrimitive)
            {
                var converter = TypeDescriptor.GetConverter(type);
                element.Value = converter.ConvertToString(value);
            }
            else if (type == typeof(string))
            {
                var cdata = new XCData(value as string);
                element.Add(cdata);
            }
            else
            {
                var memoryStream = new MemoryStream();
                var binaryFormatter = new BinaryFormatter();
                
                binaryFormatter.Serialize(memoryStream, value);
                
                var base64String = Convert.ToBase64String(memoryStream.ToArray());
                var cdata = new XCData(base64String);
                element.Add(cdata);
            }
            
            return element;
        }

        //to object:
        private object FromXml(XElement element, Type type)
        {
            if (element.Name == "NULL")
            {
                return null;
            }
            
            if (type.IsPrimitive)
            {
                var converter = TypeDescriptor.GetConverter(type);
                return converter.ConvertFromString(element.Value);
            }
            else if (type == typeof(string))
            {
                var cdata = element.FirstNode as XCData;
                return cdata.Value;
            }
            else
            {
                var cdata = element.FirstNode as XCData;
                var base64String = cdata.Value;
                var memoryStream = new MemoryStream(Convert.FromBase64String(base64String));
                
                var binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(memoryStream);
            }
        }

        //register serializers
        public void Register<TSource, TDest>() where TDest : ISerializer
        {
            MappingRegistry.RegisterMapping<TSource, ISerializer, TDest>(MapInstantiation.PerClass);
        }

        public void RegisterGeneric<TSource, TDest>() where TDest : ISerializer<TSource>
        {
            MappingRegistry.RegisterMapping<TSource, ISerializer<TSource>, TDest>(MapInstantiation.PerClass);
            MappingRegistry.RegisterMapping<TSource, ISerializer, GenericSerializer<TSource>>(MapInstantiation.PerClass);
        }

        public void Register(Type sourceType, Type serializerType)
        {
            if (typeof(ISerializer).IsAssignableFrom(serializerType))
                MappingRegistry.RegisterMapping(sourceType, typeof(ISerializer), serializerType, MapInstantiation.PerClass);
            else
            {
                Type genericSerializerInterface = serializerType.GetInterface(typeof(ISerializer<>).FullName);
                if (genericSerializerInterface != null)
                {
                    MappingRegistry.RegisterMapping(sourceType, typeof(ISerializer<>).CloseByParameterization(new Type[] { sourceType }), serializerType, MapInstantiation.PerClass);
                    MappingRegistry.RegisterMapping(sourceType, typeof(ISerializer), typeof(GenericSerializer<>).MakeGenericType(sourceType), MapInstantiation.PerClass);
                }
                else
                    throw new Exception(string.Format("can't register serializer {0} since it doesn't implement ISerializer nor ISerializer<{1}>.", serializerType.Name, sourceType.Name));
            }
        }

        //public void RegisterGeneric(Type sourceType, Type serializerType)
        //{
        //    MappingRegistry.RegisterMapping(TSource, ISerializer<TSource>, TDest>(;
        //    MappingRegistry.RegisterMapping<TSource, ISerializer, GenericSerializer<TSource>>();
        //}

        //standard XML serializer
        //        static public T DeserializeXML<T>(XElement elem)
        //        {
        //            return (T)Serializer.DeserializeXML(elem, typeof(T));
        //        }
//
        //        static public object DeserializeXML(XElement elem, Type type)
        //        {
        //            XmlSerializer serializer = new XmlSerializer(type);
//
        //            var sr = new StringReader(elem.ToString());
//
        //            object obj = serializer.Deserialize(sr);
//
        //            sr.Close();
//
        //            return obj;
        //        }
//
        //        static public XElement SerializeXML(object value)
        //        {
        //            XmlSerializer serializer = new XmlSerializer(value.GetType());
//
        //            XmlSerializerNamespaces xmlnsEmpty = new XmlSerializerNamespaces();
        //            xmlnsEmpty.Add("", "");
//
        //            var sr = new StringWriter();
        //            serializer.Serialize(sr, value, xmlnsEmpty);
//
        //            var xelem = XElement.Parse(sr.GetStringBuilder().ToString());
//
        //            sr.Close();
//
        //            return xelem;
        //        }
    }
}
