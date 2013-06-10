using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

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

        /// <summary>
        /// Provides a default no-op implementation of XmlWriter.Close() for the Portable Class Library
        /// </summary>
        /// <param name="writer"></param>
        public static void Close(this XmlWriter writer)
        {
            
        }

        public static void Close(this StreamReader reader){}

        public static void Close(this TextReader reader)
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

        public static void ForEach<T>(this List<T> theList, Action<T> action)
        {
            foreach (var item in theList)
            {
                action(item);
            }
        }

        /// <summary>
        /// Replacement for the static String.Copy(string) method
        /// that is not present in the Portable Class Library
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Copy(this string s)
        {
            return new string(s.ToCharArray());
        }

    }

    /// <summary>
    /// Provides a replacement for System.Console in portable class libraries
    /// that writes to the debug output stream rather than to the console.
    /// </summary>
    public class Console
    {
        public static void WriteLine()
        {
            Error.WriteLine();
        }

        //public static void Write(string str)
        //{
        //    Out.Write(str);
        //}

        public static void Write(string fmt, params object[] args)
        {
            Out.Write(fmt, args);
        }

        public static void WriteLine(string fmt, params object[] args)
        {
            Out.WriteLine(fmt, args);
        }

        //public static void WriteLine(string str)
        //{
        //    Out.WriteLine(str);
        //}

        public static void WriteLine(object o)
        {
            Out.WriteLine(o.ToString());
        }

        public static readonly ConsoleStream Error = new ConsoleStream();
        public static readonly ConsoleStream Out = new ConsoleStream();
    }

    public class ConsoleStream : TextWriter
    {
        // TODO: Make this a more robust implementation of TextWriter.

        public override void WriteLine()
        {
            System.Diagnostics.Debug.WriteLine(String.Empty);
        }

        public override void WriteLine(string fmt, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(fmt, args);
        }

        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        public override void Write(string fmt, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(fmt, args); // No Write method to use
        }

        public void Flush()
        {
        }

        public override void Write(char value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

    }

    public static class Path
    {
        public static char DirectorySeparatorChar = '/';
        public static char AltDirectorySeparatorChar = '\\';
        public static char VolumeSeparatorChar = ':';

        public static string GetFileName(string path)
        {
            var ix = path.LastIndexOf(DirectorySeparatorChar) + 1;
            if (ix < path.Length)
            {
                return path.Substring(ix);
            }
            return null;
        }

        public static string GetExtension(string path)
        {
            var fileName = GetFileName(path);
            if (fileName != null && fileName.Contains('.'))
            {
                return fileName.Substring(fileName.LastIndexOf('.'));
            }
            return null;
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            var fileName = GetFileName(path);
            if (fileName != null && fileName.Contains('.'))
            {
                return fileName.Substring(0, fileName.LastIndexOf('.'));
            }
            return null;
        }

        public static string GetDirectoryName(string path)
        {
            var ix = path.LastIndexOfAny(new char[] {DirectorySeparatorChar, AltDirectorySeparatorChar});
            if (ix > 0)
            {
                return path.Substring(0, ix);
            }
            return null;
        }

        public static string Combine(string path1, string path2)
        {
            if (path2[0] == DirectorySeparatorChar || path2[0] == AltDirectorySeparatorChar ||
                (path2.Length > 1 && path2[1] == VolumeSeparatorChar))
            {
                // path2 is absolute so just return path2
                return path2;
            }
            return path1 + DirectorySeparatorChar + path2;
        }
    }
}
