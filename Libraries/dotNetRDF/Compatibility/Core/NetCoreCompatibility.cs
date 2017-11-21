/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;

namespace VDS.RDF
{ 
    /// <summary>
    /// Extension class providing some methods required to cross-compile between .NET 4.0 and .NET Standard 1.4
    /// </summary>
    public static class NetCoreCompatibility
    {
        /// <summary>
        /// Compatibility implementaton of String.Copy()
        /// </summary>
        /// <param name="str">The string to be copied</param>
        /// <returns>A new string that is a copy of <paramref name="str"/></returns>
        public static string Copy(this string str)
        {
            return new string(str.ToCharArray());
        }

        /// <summary>
        /// Synchronously return the response to an HttpWebRequest
        /// </summary>
        /// <param name="request">The request to get the response to</param>
        /// <returns>The response returned by processing <paramref name="request"/></returns>
        public static WebResponse GetResponse(this HttpWebRequest request)
        {
            var asyncResult = request.BeginGetResponse(ar => { }, null);
            return request.EndGetResponse(asyncResult);
        }

        /// <summary>
        /// Get the stream that can be used to write the request body
        /// </summary>
        /// <param name="request">The request to update</param>
        /// <returns>The stream to use to write to the request body</returns>
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
            stream.Dispose();
        }

        /// <summary>
        /// Provides a default no-op implementation of StreamWriter.Close() for the NET Standard Library
        /// </summary>
        /// <param name="theWriter"></param>
        public static void Close(this StreamWriter theWriter)
        {
            theWriter.Dispose();
        }

        /// <summary>
        /// Provides a default no-op implementation of StreamReader.Close() for the NET Standard Library
        /// </summary>
        /// <param name="reader"></param>
        public static void Close(this StreamReader reader)
        {
            reader.Dispose();
        }

        /// <summary>
        /// Provides a default no-op implementation of TextReader.Close() for the NET Standard Library
        /// </summary>
        /// <param name="reader"></param>
        public static void Close(this TextReader reader)
        {
            reader.Dispose();
        }

        /// <summary>
        /// Provides a default no-op implementation of TextWriter.Close() for the NET Standard Library
        /// </summary>
        /// <param name="theWriter"></param>
        public static void Close(this TextWriter theWriter)
        {
            theWriter.Dispose();
        }

        /// <summary>
        /// Provides a default no-op implementation of XmlReader.Close() for the NET Standard Library
        /// </summary>
        /// <param name="reader"></param>
        public static void Close(this XmlReader reader)
        {
            reader.Dispose();
        }

        /// <summary>
        /// Provides a default no-op implementation of XmlWriter.Close() for the NET Standard Library
        /// </summary>
        /// <param name="theWriter"></param>
        public static void Close(this XmlWriter theWriter)
        {
            theWriter.Dispose();
        }
        /// <summary>
        /// Provides a default no-op implementation of HttpWebResponse.Close() for the NET Standard Library
        /// </summary>
        public static void Close(this HttpWebResponse response)
        {
            // No-op    
        }

        /// <summary>
        /// Return the constructors of a given Type
        /// </summary>
        /// <param name="t">The Type</param>
        /// <returns>The constructors of <paramref name="t"/></returns>
        public static ConstructorInfo[] GetConstructors(this Type t)
        {
            return t.GetTypeInfo().DeclaredConstructors.ToArray();
        }

        /// <summary>
        /// Determine if a type is an Enum
        /// </summary>
        /// <param name="t">the Type</param>
        /// <returns>True if <paramref name="t"/> is an enum type, false otherwise</returns>
        public static bool IsEnum(this Type t)
        {
            return t.GetTypeInfo().IsEnum;
        }

        /// <summary>
        /// Determine if a type is a generic type
        /// </summary>
        /// <param name="t">the Type</param>
        /// <returns>True if <paramref name="t"/> is a generic type, false otherwise</returns>
        public static bool IsGenericType(this Type t)
        {
            return t.GetTypeInfo().IsGenericType;
        }
    }

    /// <summary>
    /// Compatibility replacement for System.ComponentModel.DesriptionAttribute
    /// </summary>
    public class DescriptionAttribute : Attribute
    {
        /// <summary>
        /// Attribute constructor
        /// </summary>
        /// <param name="desc">Description text</param>
        public DescriptionAttribute(string desc) { }
    }

    /// <summary>
    /// Compatibility replacement for System.ComponentModel.CategoryAttribute
    /// </summary>
    public class CategoryAttribute : Attribute
    {
        /// <summary>
        /// Attribute constructor
        /// </summary>
        /// <param name="name">Category name</param>
        public CategoryAttribute(string name) { }
    }

    /// <summary>
    /// Compatibiltiy replacement for System.ComponentModel.ExpandableObjectConverter
    /// </summary>
    /// <remarks>This class implements no functionality. It is provided simply as a stub to enable easier cross-compilation</remarks>
    public class ExpandableObjectConverter : TypeConverter
    {
        /// <summary>
        /// No-op constructor
        /// </summary>
        public ExpandableObjectConverter()
        {
        }
    }
}
