/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Net;
using System.Xml;

namespace VDS.Web
{
    /// <summary>
    /// Useful extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts an IP Address into a loggable address
        /// </summary>
        /// <param name="address">IP Address</param>
        /// <returns></returns>
        public static String ToLogString(this IPAddress address)
        {
            String temp = address.ToString();
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return temp.ToLogString();
            }
            else
            {
                return temp;
            }
        }

        /// <summary>
        /// Converts an IP Address into a loggable address
        /// </summary>
        /// <param name="address">IP Address</param>
        /// <returns></returns>
        public static String ToLogString(this String address)
        {
            if (address.Contains(":"))
            {
                return address.Substring(0, address.IndexOf(':'));
            }
            else
            {
                return address;
            }
        }

        /// <summary>
        /// Tries to get a named item from an XML Attributes collection returning null if the item is not present
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public static String GetSafeNamedItem(this XmlAttributeCollection attributes, String name)
        {
            XmlNode attr = attributes.GetNamedItem(name);
            if (attr == null) return null;
            return attr.Value;
        }
    }

    /// <summary>
    /// Useful utility methods
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Copies one stream to another in 8k chunks
        /// </summary>
        /// <param name="input">Input Stream</param>
        /// <param name="output">Output Stream</param>
        /// <returns>Number of bytes copied</returns>
        public static long CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8192];
            long total = 0;
            while (true)
            {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0) return total;
                output.Write(buffer, 0, read);
                total += read;
            }
        }

        /// <summary>
        /// Converts a DateTime to RFC 2822 format
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <returns></returns>
        internal static String ToRfc2822(this DateTime dt)
        {
            return dt.ToString("ddd, d MMM yyyy HH:mm:ss K");
        }
    }
}
