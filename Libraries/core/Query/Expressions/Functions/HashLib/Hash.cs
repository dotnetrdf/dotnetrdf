using System;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace HashLib
{
    /// <summary>
    /// Code incorporated from the <a href="http://hashlib.codeplex.com">HashLib</a> project to support SHA224 and provide support for all Hash functions under Silverlight
    /// </summary>
    /// <remarks>
    /// Slightly modified to downgrade the code to C# 3 syntax
    /// </remarks>
    public abstract class Hash : IHash
    {
        private readonly int m_blockSize;
        private readonly int m_hashSize;

        internal static int BUFFER_SIZE = 64 * 1024;

        protected Hash(int a_hashSize, int a_blockSize)
        {
            Debug.Assert((a_blockSize > 0) || (a_blockSize == -1));
            Debug.Assert(a_hashSize > 0);

            m_blockSize = a_blockSize;
            m_hashSize = a_hashSize;
        }

        public virtual string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        public virtual int BlockSize
        {
            get
            {
                return m_blockSize;
            }
        }

        public virtual int HashSize
        {
            get
            {
                return m_hashSize;
            }
        }

        public virtual HashResult ComputeObject(object a_data)
        {
            if (a_data is byte)
                return ComputeByte((byte)a_data);
            else if (a_data is short)
                return ComputeShort((short)a_data);
            else if (a_data is ushort)
                return ComputeUShort((ushort)a_data);
            else if (a_data is char)
                return ComputeChar((char)a_data);
            else if (a_data is int)
                return ComputeInt((int)a_data);
            else if (a_data is uint)
                return ComputeUInt((uint)a_data);
            else if (a_data is long)
                return ComputeLong((long)a_data);
            else if (a_data is ulong)
                return ComputeULong((ulong)a_data);
            else if (a_data is float)
                return ComputeFloat((float)a_data);
            else if (a_data is double)
                return ComputeDouble((double)a_data);
            else if (a_data is string)
                return ComputeString((string)a_data);
            else if (a_data is byte[])
                return ComputeBytes((byte[])a_data);
            else if (a_data is short[])
                return ComputeShorts((short[])a_data);
            else if (a_data is ushort[])
                return ComputeUShorts((ushort[])a_data);
            else if (a_data is char[])
                return ComputeChars((char[])a_data);
            else if (a_data is int[])
                return ComputeInts((int[])a_data);
            else if (a_data is uint[])
                return ComputeUInts((uint[])a_data);
            else if (a_data is long[])
                return ComputeLongs((long[])a_data);
            else if (a_data is ulong[])
                return ComputeULongs((ulong[])a_data);
            else if (a_data is float[])
                return ComputeFloats((float[])a_data);
            else if (a_data is double[])
                return ComputeDoubles((double[])a_data);
            else
                throw new ArgumentException();
        }

        public virtual HashResult ComputeByte(byte a_data)
        {
            return ComputeBytes(new byte[] { a_data });
        }

        public virtual HashResult ComputeChar(char a_data)
        {
            return ComputeBytes(BitConverter.GetBytes(a_data));
        }

        public virtual HashResult ComputeShort(short a_data)
        {
            return ComputeBytes(BitConverter.GetBytes(a_data));
        }

        public virtual HashResult ComputeUShort(ushort a_data)
        {
            return ComputeBytes(BitConverter.GetBytes(a_data));
        }

        public virtual HashResult ComputeInt(int a_data)
        {
            return ComputeBytes(BitConverter.GetBytes(a_data));
        }

        public virtual HashResult ComputeUInt(uint a_data)
        {
            return ComputeBytes(BitConverter.GetBytes(a_data));
        }

        public virtual HashResult ComputeLong(long a_data)
        {
            return ComputeBytes(BitConverter.GetBytes(a_data));
        }

        public virtual HashResult ComputeULong(ulong a_data)
        {
            return ComputeBytes(BitConverter.GetBytes(a_data));
        }

        public virtual HashResult ComputeFloat(float a_data)
        {
            return ComputeBytes(Converters.ConvertFloatToBytes((a_data)));
        }

        public virtual HashResult ComputeDouble(double a_data)
        {
            return ComputeBytes(Converters.ConvertDoubleToBytes(a_data));
        }

        public virtual HashResult ComputeString(string a_data)
        {
            return ComputeBytes(Converters.ConvertStringToBytes(a_data));
        }

        public virtual HashResult ComputeChars(char[] a_data)
        {
            return ComputeBytes(Converters.ConvertCharsToBytes(a_data));
        }

        public virtual HashResult ComputeShorts(short[] a_data)
        {
            return ComputeBytes(Converters.ConvertShortsToBytes(a_data));
        }

        public virtual HashResult ComputeUShorts(ushort[] a_data)
        {
            return ComputeBytes(Converters.ConvertUShortsToBytes(a_data));
        }

        public virtual HashResult ComputeInts(int[] a_data)
        {
            return ComputeBytes(Converters.ConvertIntsToBytes(a_data));
        }

        public virtual HashResult ComputeUInts(uint[] a_data)
        {
            return ComputeBytes(Converters.ConvertUIntsToBytes(a_data));
        }

        public virtual HashResult ComputeLongs(long[] a_data)
        {
            return ComputeBytes(Converters.ConvertLongsToBytes(a_data));
        }

        public virtual HashResult ComputeULongs(ulong[] a_data)
        {
            return ComputeBytes(Converters.ConvertULongsToBytes(a_data));
        }

        public virtual HashResult ComputeDoubles(double[] a_data)
        {
            return ComputeBytes(Converters.ConvertDoublesToBytes(a_data));
        }

        public virtual HashResult ComputeFloats(float[] a_data)
        {
            return ComputeBytes(Converters.ConvertFloatsToBytes(a_data));
        }

        public virtual HashResult ComputeChar(char a_data, Encoding a_encoding)
        {
            return ComputeBytes(a_encoding.GetBytes(new char[] { a_data }));
        }

        public virtual HashResult ComputeString(string a_data, Encoding a_encoding)
        {
            return ComputeBytes(Converters.ConvertStringToBytes(a_data, a_encoding));
        }

        public virtual HashResult ComputeChars(char[] a_data, Encoding a_encoding)
        {
            return ComputeBytes(Converters.ConvertCharsToBytes(a_data, a_encoding));
        }

        public virtual HashResult ComputeBytes(byte[] a_data)
        {
            Initialize();
            TransformBytes(a_data);
            HashResult result = TransformFinal();
            Initialize();
            return result;
        }

        public void TransformObject(object a_data)
        {
            if (a_data is byte)
                TransformByte((byte)a_data);
            else if (a_data is short)
                TransformShort((short)a_data);
            else if (a_data is ushort)
                TransformUShort((ushort)a_data);
            else if (a_data is char)
                TransformChar((char)a_data);
            else if (a_data is int)
                TransformInt((int)a_data);
            else if (a_data is uint)
                TransformUInt((uint)a_data);
            else if (a_data is long)
                TransformLong((long)a_data);
            else if (a_data is ulong)
                TransformULong((ulong)a_data);
            else if (a_data is float)
                TransformFloat((float)a_data);
            else if (a_data is double)
                TransformDouble((double)a_data);
            else if (a_data is string)
                TransformString((string)a_data);
            else if (a_data is byte[])
                TransformBytes((byte[])a_data);
            else if (a_data is short[])
                TransformShorts((short[])a_data);
            else if (a_data is ushort[])
                TransformUShorts((ushort[])a_data);
            else if (a_data is char[])
                TransformChars((char[])a_data);
            else if (a_data is int[])
                TransformInts((int[])a_data);
            else if (a_data is uint[])
                TransformUInts((uint[])a_data);
            else if (a_data is long[])
                TransformLongs((long[])a_data);
            else if (a_data is ulong[])
                TransformULongs((ulong[])a_data);
            else if (a_data is float[])
                TransformFloats((float[])a_data);
            else if (a_data is double[])
                TransformDoubles((double[])a_data);
            else
                throw new ArgumentException();
        }

        public void TransformByte(byte a_data)
        {
            TransformBytes(new byte[] { a_data });
        }

        public void TransformChar(char a_data)
        {
            TransformBytes(BitConverter.GetBytes(a_data));
        }

        public void TransformShort(short a_data)
        {
            TransformBytes(BitConverter.GetBytes(a_data));
        }

        public void TransformUShort(ushort a_data)
        {
            TransformBytes(BitConverter.GetBytes(a_data));
        }

        public void TransformInt(int a_data)
        {
            TransformBytes(BitConverter.GetBytes(a_data));
        }

        public void TransformUInt(uint a_data)
        {
            TransformBytes(BitConverter.GetBytes(a_data));
        }

        public void TransformLong(long a_data)
        {
            TransformBytes(BitConverter.GetBytes(a_data));
        }

        public void TransformULong(ulong a_data)
        {
            TransformBytes(BitConverter.GetBytes(a_data));
        }

        public void TransformFloat(float a_data)
        {
            TransformBytes(Converters.ConvertFloatToBytes((a_data)));
        }

        public void TransformDouble(double a_data)
        {
            TransformBytes(BitConverter.GetBytes(a_data));
        }

        public void TransformString(string a_data)
        {
            TransformString(a_data, Encoding.Unicode);
        }

        public void TransformChars(char[] a_data)
        {
            TransformBytes(Converters.ConvertCharsToBytes(a_data));
        }

        public void TransformShorts(short[] a_data)
        {
            TransformBytes(Converters.ConvertShortsToBytes(a_data));
        }

        public void TransformUShorts(ushort[] a_data)
        {
            TransformBytes(Converters.ConvertUShortsToBytes(a_data));
        }

        public void TransformInts(int[] a_data)
        {
            TransformBytes(Converters.ConvertIntsToBytes(a_data));
        }

        public void TransformUInts(uint[] a_data)
        {
            TransformBytes(Converters.ConvertUIntsToBytes(a_data));
        }

        public void TransformLongs(long[] a_data)
        {
            TransformBytes(Converters.ConvertLongsToBytes(a_data));
        }

        public void TransformULongs(ulong[] a_data)
        {
            TransformBytes(Converters.ConvertULongsToBytes(a_data));
        }

        public void TransformDoubles(double[] a_data)
        {
            TransformBytes(Converters.ConvertDoublesToBytes(a_data));
        }

        public void TransformFloats(float[] a_data)
        {
            TransformBytes(Converters.ConvertFloatsToBytes(a_data));
        }

        public void TransformChar(char a_data, Encoding a_encoding)
        {
            TransformBytes(a_encoding.GetBytes(new char[] { a_data }));
        }

        public void TransformString(string a_data, Encoding a_encoding)
        {
            TransformBytes(Converters.ConvertStringToBytes(a_data, a_encoding));
        }

        public void TransformChars(char[] a_data, Encoding a_encoding)
        {
            TransformBytes(Converters.ConvertCharsToBytes(a_data, a_encoding));
        }

        public void TransformBytes(byte[] a_data)
        {
            TransformBytes(a_data, 0, a_data.Length);
        }

        public void TransformBytes(byte[] a_data, int a_index)
        {
            Debug.Assert(a_index >= 0);

            int length = a_data.Length - a_index;

            Debug.Assert(length >= 0);

            TransformBytes(a_data, a_index, length);
        }

        public abstract void Initialize();
        public abstract void TransformBytes(byte[] a_data, int a_index, int a_length);
        public abstract HashResult TransformFinal();
    }
}
