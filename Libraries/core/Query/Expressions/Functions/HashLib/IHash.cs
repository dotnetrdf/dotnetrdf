using System;
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
    public interface IHash
    {
        /// <summary>
        /// Gets the name of the Hash Type
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets the Block Size
        /// </summary>
        int BlockSize { get; }
        /// <summary>
        /// Gets the Hash Size
        /// </summary>
        int HashSize { get; }

        /// <summary>
        /// Computes the Hash of an Object
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeObject(object a_data);
        /// <summary>
        /// Computes the Hash of a Byte
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeByte(byte a_data);
        /// <summary>
        /// Computes the Hash of a Char
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeChar(char a_data);
        /// <summary>
        /// Computes the Hash of a Char
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_encoding"></param>
        /// <returns></returns>
        HashResult ComputeChar(char a_data, Encoding a_encoding);
        /// <summary>
        /// Computes the Hash of a Short
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeShort(short a_data);
        /// <summary>
        /// Computes the Hash of an Unsigned Short
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeUShort(ushort a_data);
        /// <summary>
        /// Computes the Hash of an Integer
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeInt(int a_data);
        /// <summary>
        /// Computes the Hash of an Unsigned Integer
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeUInt(uint a_data);
        /// <summary>
        /// Computers the Hash of a Long
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeLong(long a_data);
        /// <summary>
        /// Computers the Hash of an Unsigned Long
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeULong(ulong a_data);
        /// <summary>
        /// Computers the Hash of a Float
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeFloat(float a_data);
        /// <summary>
        /// Computers the Hash of a Double
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeDouble(double a_data);
        /// <summary>
        /// Computers the Hash of a String
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeString(string a_data);
        /// <summary>
        /// Computers the Hash of a String
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_encoding"></param>
        /// <returns></returns>
        HashResult ComputeString(string a_data, Encoding a_encoding);
        /// <summary>
        /// Computers the Hash of some Bytes
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeBytes(byte[] a_data);
        /// <summary>
        /// Computers the Hash of some Chars
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeChars(char[] a_data);
        /// <summary>
        /// Computers the Hash of some Chars
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_encoding"></param>
        /// <returns></returns>
        HashResult ComputeChars(char[] a_data, Encoding a_encoding);
        /// <summary>
        /// Computers the Hash of some Shorts
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeShorts(short[] a_data);
        /// <summary>
        /// Computers the Hash of some Unsigned Shorts
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeUShorts(ushort[] a_data);
        /// <summary>
        /// Computers the Hash of some Integers
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeInts(int[] a_data);
        /// <summary>
        /// Computers the Hash of some Unsigned Integers
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeUInts(uint[] a_data);
        /// <summary>
        /// Computers the Hash of some Longs
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeLongs(long[] a_data);
        /// <summary>
        /// Computers the Hash of some Unsigned Longs
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeULongs(ulong[] a_data);
        /// <summary>
        /// Computers the Hash of some Doubles
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeDoubles(double[] a_data);
        /// <summary>
        /// Computers the Hash of some Floats
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        HashResult ComputeFloats(float[] a_data);

        /// <summary>
        /// Initializes the Hash Algorithm
        /// </summary>
        void Initialize();

        /// <summary>
        /// Transforms some Bytes
        /// </summary>
        /// <param name="a_data"></param>
        void TransformBytes(byte[] a_data);
        /// <summary>
        /// Transforms some Bytes
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_index"></param>
        void TransformBytes(byte[] a_data, int a_index);
        /// <summary>
        /// Transforms some Bytes
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_index"></param>
        /// <param name="a_length"></param>
        void TransformBytes(byte[] a_data, int a_index, int a_length);

        /// <summary>
        /// Produces the final Hash of all the Transformed Inputs
        /// </summary>
        /// <returns></returns>
        HashResult TransformFinal();

        /// <summary>
        /// Transforms an Object
        /// </summary>
        /// <param name="a_data"></param>
        void TransformObject(object a_data);
        /// <summary>
        /// Transforms a Byte
        /// </summary>
        /// <param name="a_data"></param>
        void TransformByte(byte a_data);
        /// <summary>
        /// Transforms a Char
        /// </summary>
        /// <param name="a_data"></param>
        void TransformChar(char a_data);
        /// <summary>
        /// Transforms a Char
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_encoding"></param>
        void TransformChar(char a_data, Encoding a_encoding);
        /// <summary>
        /// Transforms a Short
        /// </summary>
        /// <param name="a_data"></param>
        void TransformShort(short a_data);
        /// <summary>
        /// Transforms an Unsigned Short
        /// </summary>
        /// <param name="a_data"></param>
        void TransformUShort(ushort a_data);
        /// <summary>
        /// Transforms an Integer
        /// </summary>
        /// <param name="a_data"></param>
        void TransformInt(int a_data);
        /// <summary>
        /// Transforms an Unsigned Integer
        /// </summary>
        /// <param name="a_data"></param>
        void TransformUInt(uint a_data);
        /// <summary>
        /// Transforms a Long
        /// </summary>
        /// <param name="a_data"></param>
        void TransformLong(long a_data);
        /// <summary>
        /// Transforms an Unsigned Long
        /// </summary>
        /// <param name="a_data"></param>
        void TransformULong(ulong a_data);
        /// <summary>
        /// Transforms a Float
        /// </summary>
        /// <param name="a_data"></param>
        void TransformFloat(float a_data);
        /// <summary>
        /// Transforms a Double
        /// </summary>
        /// <param name="a_data"></param>
        void TransformDouble(double a_data);
        /// <summary>
        /// Transforms a String
        /// </summary>
        /// <param name="a_data"></param>
        void TransformString(string a_data);
        /// <summary>
        /// Transforms a String
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_encoding"></param>
        void TransformString(string a_data, Encoding a_encoding);
        /// <summary>
        /// Transforms some Chars
        /// </summary>
        /// <param name="a_data"></param>
        void TransformChars(char[] a_data);
        /// <summary>
        /// Transforms some Chars
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_encoding"></param>
        void TransformChars(char[] a_data, Encoding a_encoding);
        /// <summary>
        /// Transforms some Shorts
        /// </summary>
        /// <param name="a_data"></param>
        void TransformShorts(short[] a_data);
        /// <summary>
        /// Transforms some Unsigned Shorts
        /// </summary>
        /// <param name="a_data"></param>
        void TransformUShorts(ushort[] a_data);
        /// <summary>
        /// Transforms some Integers
        /// </summary>
        /// <param name="a_data"></param>
        void TransformInts(int[] a_data);
        /// <summary>
        /// Transforms some Unsigned Integers
        /// </summary>
        /// <param name="a_data"></param>
        void TransformUInts(uint[] a_data);
        /// <summary>
        /// Transforms some Longs
        /// </summary>
        /// <param name="a_data"></param>
        void TransformLongs(long[] a_data);
        /// <summary>
        /// Transforms some Unsigned Longs
        /// </summary>
        /// <param name="a_data"></param>
        void TransformULongs(ulong[] a_data);
        /// <summary>
        /// Transforms some Doubles
        /// </summary>
        /// <param name="a_data"></param>
        void TransformDoubles(double[] a_data);
        /// <summary>
        /// Transforms some Floats
        /// </summary>
        /// <param name="a_data"></param>
        void TransformFloats(float[] a_data);
    }
}
