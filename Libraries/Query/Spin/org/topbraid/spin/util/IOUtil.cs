using System.Text;
using System.IO;
namespace org.topbraid.spin.util
{

    public class IOUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="IOException">IOException</exception>
        public static StringBuilder loadString(TextReader reader)
        {
            StringBuilder sb = new StringBuilder();
            for(; ; )
            {
                int c = reader.Read();
                if (c < 0)
                {
                    break;
                }
                sb.Append((char)c);
            }
            reader.Close();
            return sb;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="IOException">IOException</exception>
        public static StringBuilder loadStringUTF8(Stream input)
        {
            return loadString(new StreamReader(input, Encoding.UTF8));
        }

    }
}