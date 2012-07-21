/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
