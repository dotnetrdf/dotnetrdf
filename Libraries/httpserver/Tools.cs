using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace VDS.Web
{
    public static class Extensions
    {
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

        public static String GetSafeNamedItem(this XmlAttributeCollection attributes, String name)
        {
            XmlNode attr = attributes.GetNamedItem(name);
            if (attr == null) return null;
            return attr.Value;
        }
    }

    public static class Tools
    {
        public static long CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
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
        /// <param name="dt"></param>
        /// <returns></returns>
        internal static String ToRfc2822(this DateTime dt)
        {
            return dt.ToString("ddd, d MMM yyyy HH:mm:ss K");
        }
    }
}
