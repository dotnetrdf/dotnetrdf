using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashLib
{
    public class HashResult
    {
        private byte[] m_hash;

        public HashResult(uint a_hash)
        {
            m_hash = BitConverter.GetBytes(a_hash);
        }

        internal HashResult(int a_hash)
        {
            m_hash = BitConverter.GetBytes(a_hash);
        }

        public HashResult(ulong a_hash)
        {
            m_hash = BitConverter.GetBytes(a_hash);
        }

        internal HashResult(long a_hash)
        {
            m_hash = BitConverter.GetBytes(a_hash);
        }

        public HashResult(byte[] a_hash)
        {
            m_hash = a_hash;
        }

        public HashResult(string a_hash)
        {
            m_hash = Converters.ConvertHexStringToBytes(a_hash);
        }

        public byte[] GetBytes()
        {
            return m_hash.ToArray();
        }

        public uint GetUInt()
        {
            if (m_hash.Length != 4)
                throw new InvalidOperationException();

            return BitConverter.ToUInt32(m_hash, 0);
        }

        public ulong GetULong()
        {
            if (m_hash.Length != 8)
                throw new InvalidOperationException();

            return BitConverter.ToUInt64(m_hash, 0);
        }

        public override string ToString()
        {
            return Converters.ConvertBytesToHexString(m_hash, true);
        }

        public override bool Equals(Object a_obj)
        {
            HashResult hash_result = a_obj as HashResult;
            if ((HashResult)hash_result == null)
                return false;

            return Equals(hash_result);
        }

        public bool Equals(HashResult a_hashResult)
        {
            return HashResult.SameArrays(a_hashResult.GetBytes(), m_hash);
        }

        public override int GetHashCode()
        {
            return Convert.ToBase64String(m_hash).GetHashCode();
        }

        public static bool operator ==(HashResult a_left, HashResult a_right)
        {
            if (Object.ReferenceEquals(a_left, a_right))
                return true;

            if (((object)a_left == null) || ((object)a_right == null))
                return false;

            return a_left.Equals(a_right);
        }

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
