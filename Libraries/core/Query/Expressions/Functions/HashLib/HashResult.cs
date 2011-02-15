using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashLib
{
    /// <summary>
    /// Code incorporated from the <a href="http://hashlib.codeplex.com">HashLib</a> project to support SHA224 and provide support for all Hash functions under Silverlight
    /// </summary>
    /// <remarks>
    /// Slightly modified to downgrade the code to C# 3 syntax
    /// </remarks>
    public class HashResult
    {
        private byte[] m_hash;

        /// <summary>
        /// Creates a new Hash Result
        /// </summary>
        /// <param name="a_hash"></param>
        public HashResult(uint a_hash)
        {
            m_hash = BitConverter.GetBytes(a_hash);
        }

        /// <summary>
        /// Creates a new Hash Result
        /// </summary>
        /// <param name="a_hash"></param>
        internal HashResult(int a_hash)
        {
            m_hash = BitConverter.GetBytes(a_hash);
        }

        /// <summary>
        /// Creates a new Hash Result
        /// </summary>
        /// <param name="a_hash"></param>
        public HashResult(ulong a_hash)
        {
            m_hash = BitConverter.GetBytes(a_hash);
        }

        /// <summary>
        /// Creates a new Hash Result
        /// </summary>
        /// <param name="a_hash"></param>
        internal HashResult(long a_hash)
        {
            m_hash = BitConverter.GetBytes(a_hash);
        }

        /// <summary>
        /// Creates a new Hash Result
        /// </summary>
        /// <param name="a_hash"></param>
        public HashResult(byte[] a_hash)
        {
            m_hash = a_hash;
        }

        /// <summary>
        /// Creates a new Hash Result
        /// </summary>
        /// <param name="a_hash"></param>
        public HashResult(string a_hash)
        {
            m_hash = Converters.ConvertHexStringToBytes(a_hash);
        }

        /// <summary>
        /// Gets the Bytes in the Hash
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            return m_hash.ToArray();
        }

        /// <summary>
        /// Gets the Unsigned Integer of the Hash
        /// </summary>
        /// <returns></returns>
        public uint GetUInt()
        {
            if (m_hash.Length != 4)
                throw new InvalidOperationException();

            return BitConverter.ToUInt32(m_hash, 0);
        }

        /// <summary>
        /// Gets the Unsigned Long of the Hash
        /// </summary>
        /// <returns></returns>
        public ulong GetULong()
        {
            if (m_hash.Length != 8)
                throw new InvalidOperationException();

            return BitConverter.ToUInt64(m_hash, 0);
        }

        /// <summary>
        /// Gets the Hash as a hexadecimal string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Converters.ConvertBytesToHexString(m_hash, true);
        }

        /// <summary>
        /// Determines whether a Hash Result is equal to some other Object
        /// </summary>
        /// <param name="a_obj"></param>
        /// <returns></returns>
        public override bool Equals(Object a_obj)
        {
            HashResult hash_result = a_obj as HashResult;
            if ((HashResult)hash_result == null)
                return false;

            return Equals(hash_result);
        }

        /// <summary>
        /// Determines whether a Hash Result is equal to another Hash Result
        /// </summary>
        /// <param name="a_hashResult"></param>
        /// <returns></returns>
        public bool Equals(HashResult a_hashResult)
        {
            return HashResult.SameArrays(a_hashResult.GetBytes(), m_hash);
        }

        /// <summary>
        /// Gets the Hash Code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Convert.ToBase64String(m_hash).GetHashCode();
        }

        /// <summary>
        /// Equality operator for Hash Results
        /// </summary>
        /// <param name="a_left"></param>
        /// <param name="a_right"></param>
        /// <returns></returns>
        public static bool operator ==(HashResult a_left, HashResult a_right)
        {
            if (Object.ReferenceEquals(a_left, a_right))
                return true;

            if (((object)a_left == null) || ((object)a_right == null))
                return false;

            return a_left.Equals(a_right);
        }

        /// <summary>
        /// Inequality operator for Hash Results
        /// </summary>
        /// <param name="a_left"></param>
        /// <param name="a_right"></param>
        /// <returns></returns>
        public static bool operator !=(HashResult a_left, HashResult a_right)
        {
            return !(a_left == a_right);
        }

        private static bool SameArrays(byte[] a_ar1, byte[] a_ar2)
        {
            if (Object.ReferenceEquals(a_ar1, a_ar2))
                return true;

            if (a_ar1.Length != a_ar2.Length)
                return false;

            for (int i = 0; i < a_ar1.Length; i++)
                if (a_ar1[i] != a_ar2[i])
                    return false;

            return true;
        }
    }
}
