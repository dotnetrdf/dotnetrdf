using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VDS.Web
{
    public static class Tools
    {
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            while (true)
            {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0) return;
                output.Write(buffer, 0, read);
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
