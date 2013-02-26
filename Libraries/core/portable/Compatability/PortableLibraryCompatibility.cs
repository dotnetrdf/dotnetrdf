using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace VDS.RDF
{
    public class CategoryAttribute : Attribute
    {
        public CategoryAttribute(string name) { }
    }

    public class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string desc) { }
    }

    public class ReadOnlyAttribute : Attribute
    {
        public ReadOnlyAttribute(bool readOnly)
        {
        }
    }

    public class TypeConverterAttribute : Attribute
    {
        public TypeConverterAttribute(Type converterType){}
    }

    public static class PortableClassLibraryExtensions
    {
        public static bool Contains(this string theString, char c)
        {
            return theString.Contains(c.ToSafeString());
        }

        public static void Close(this Stream theStream)
        {
            // Portable class library Stream implementation has no Close() method
            // This is just a shortcut to avoid having to #if out all  the calls to Close()
        }

        /// <summary>
        /// Provides a default no-op implementation of TextWriter.Close() for Portable Class Library
        /// </summary>
        /// <param name="theWriter"></param>
        public static void Close(this TextWriter theWriter)
        {
          
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

        public static void Close(this HttpWebResponse response)
        {
            // No-op    
        }
    }

    /// <summary>
    /// Provides a replacement for System.Console in portable class libraries
    /// that writes to the debug output stream rather than to the console.
    /// </summary>
    public class Console
    {
        public void WriteLine(string fmt, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(fmt, args);
        }
    }
}
