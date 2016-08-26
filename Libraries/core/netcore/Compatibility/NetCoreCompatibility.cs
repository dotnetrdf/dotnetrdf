using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Linq;

namespace VDS.RDF
{ 
    public static class NetCoreCompatibility
    {
        public static string Copy(this string str)
        {
            return new string(str.ToCharArray());
        }

        public static WebResponse GetResponse(this HttpWebRequest request)
        {
            var asyncResult = request.BeginGetResponse(ar => { }, null);
            return request.EndGetResponse(asyncResult);

        }

        public static Stream GetRequestStream(this HttpWebRequest request)
        {
            var asyncResult = request.BeginGetRequestStream(ar => { }, null);
            return request.EndGetRequestStream(asyncResult);
        }

        /// <summary>
        /// Provides a default no-op implementation of Stream.Close() for the NET Standard Library
        /// </summary>
        /// <param name="stream"></param>
        public static void Close(this Stream stream)
        {

        }

        /// <summary>
        /// Provides a default no-op implementation of StreamWriter.Close() for the NET Standard Library
        /// </summary>
        /// <param name="theWriter"></param>
        public static void Close(this StreamWriter theWriter)
        {

        }

        /// <summary>
        /// Provides a default no-op implementation of StreamReader.Close() for the NET Standard Library
        /// </summary>
        /// <param name="reader"></param>
        public static void Close(this StreamReader reader)
        {

        }

        /// <summary>
        /// Provides a default no-op implementation of TextReader.Close() for the NET Standard Library
        /// </summary>
        /// <param name="reader"></param>
        public static void Close(this TextReader reader)
        {

        }

        /// <summary>
        /// Provides a default no-op implementation of TextWriter.Close() for the NET Standard Library
        /// </summary>
        /// <param name="theWriter"></param>
        public static void Close(this TextWriter theWriter)
        {

        }

        /// <summary>
        /// Provides a default no-op implementation of XmlReader.Close() for the NET Standard Library
        /// </summary>
        /// <param name="reader"></param>
        public static void Close(this XmlReader reader)
        {

        }

        /// <summary>
        /// Provides a default no-op implementation of XmlWriter.Close() for the NET Standard Library
        /// </summary>
        /// <param name="theWriter"></param>
        public static void Close(this XmlWriter theWriter)
        {

        }
        /// <summary>
        /// Provides a default no-op implementation of HttpWebResponse.Close() for the NET Standard Library
        /// </summary>
        public static void Close(this HttpWebResponse response)
        {
            // No-op    
        }

        /// <summary>
        /// Implementation of Type.IsAssignableFrom(Type) for the .NET Standard Library
        /// </summary>
        /// <param name="t"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsAssignableFrom(this Type t, Type other)
        {
            return t.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
        }

        public static ConstructorInfo[] GetConstructors(this Type t)
        {
            return t.GetTypeInfo().DeclaredConstructors.ToArray();
        }

        public static bool IsEnum(this Type t)
        {
            return t.GetTypeInfo().IsEnum;
        }

        public static bool IsGenericType(this Type t)
        {
            return t.GetTypeInfo().IsGenericType;
        }

        public static IEnumerable<Type> GetInterfaces(this Type t)
        {
            return t.GetTypeInfo().ImplementedInterfaces;
        }
    }

    public class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string desc) { }
    }

    public class CategoryAttribute : Attribute
    {
        public CategoryAttribute(string name) { }
    }

        public class ExpandableObjectConverter : TypeConverter
        {

            public ExpandableObjectConverter()
            {
            }
        }
}
