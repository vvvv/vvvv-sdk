using System;
using System.Xml.Linq;

namespace VVVV.Core.Serialization
{
    public interface ISerializer<T>
    {
        XElement Serialize(T value, Serializer serializer);
        T Deserialize(XElement data, Type type, Serializer serializer);
    }

    public interface ISerializer
    {
        XElement Serialize(object value, Serializer serializer);
        object Deserialize(XElement data, Type type, Serializer serializer);
    }

    //wrapper for generic serializer
    public class GenericSerializer<T> : ISerializer
    {
        private ISerializer<T> InternalSerializer;

        public GenericSerializer(ISerializer<T> genSerializer)
        {
            InternalSerializer = genSerializer;
        }

        public XElement Serialize(object value, Serializer serializer)
        {
            return InternalSerializer.Serialize((T)value, serializer);
        }

        public object Deserialize(XElement data, Type type, Serializer serializer)
        {
            return InternalSerializer.Deserialize(data, type, serializer);
        }
    }

    //public static class ISerializerExtensions
    //{
    //    public static ISerializer AsNonGeneric<T>(this ISerializer<T> gen)
    //    {
    //        return new GenericSerializer<T>(gen);
    //    }

    //    //public static ISerializer Serialize<T>(this ISerializer<T> gen)
    //    //{
    //    //    return new GenericSerializer<T>(gen).Serialize();
    //    //}
    //}
}
