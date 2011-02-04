using System;
using System.Diagnostics;

namespace HashLib
{
    public class HashBuffer 
    {
        private byte[] m_data;
        private int m_pos;

        public HashBuffer(int a_length)
        {
            Debug.Assert(a_length > 0);

            m_data = new byte[a_length];

            Initialize();
        }

        public void Initialize()
        {
            m_pos = 0;
        }

        public byte[] GetBytes()
        {
            Debug.Assert(IsFull);

            m_pos = 0;
            return m_data;
        }

        public byte[] GetBytesZeroPadded()
        {
            Array.Clear(m_data, m_pos, m_data.Length - m_pos); 
            m_pos = 0;
            return m_data;
        }

        public bool Feed(byte[] a_data, ref int a_startIndex, ref int a_length, ref ulong a_processedBytes)
        {
            Debug.Assert(a_startIndex >= 0);
            Debug.Assert(a_length >= 0);
            Debug.Assert(a_startIndex + a_length <= a_data.Length);
            Debug.Assert(!IsFull);

            if (a_data.Length == 0)
                return false;

            if (a_length == 0)
                return false;

            int length = m_data.Length - m_pos;
            if (length > a_length)
                length = a_length;

            Array.Copy(a_data, a_startIndex, m_data, m_pos, length);

            m_pos += length;
            a_startIndex += length;
            a_length -= length;
            a_processedBytes += (ulong)length;

            return IsFull;
        }

        public bool Feed(byte[] a_data, ref int a_startIndex, ref int a_length)
        {
            Debug.Assert(a_startIndex >= 0);
            Debug.Assert(a_length >= 0);
            Debug.Assert(a_startIndex + a_length <= a_data.Length);
            Debug.Assert(!IsFull);
            
            if (a_data.Length == 0)
                return false;

            if (a_length == 0)
                return false;

            int length = m_data.Length - m_pos;
            if (length > a_length)
                length = a_length;

            Array.Copy(a_data, a_startIndex, m_data, m_pos, length);

            m_pos += length;
            a_startIndex += length;
            a_length -= length;

            return IsFull;
        }

        public bool Feed(byte[] a_data, int a_length)
        {
            Debug.Assert(a_length >= 0);
            Debug.Assert(a_length <= a_data.Length);
            Debug.Assert(!IsFull);

            if (a_data.Length == 0)
                return false;

            if (a_length == 0)
                return false;

            int length = m_data.Length - m_pos;
            if (length > a_length)
                length = a_length;

            Array.Copy(a_data, 0, m_data, m_pos, length);

            m_pos += length;

            return IsFull;
        }

        public bool Feed(byte a_data)
        {
            Debug.Assert(!IsFull);

            m_data[m_pos] = a_data;
            m_pos++;

            return IsFull;
        }

        public bool IsEmpty
        {
            get
            {
                return m_pos == 0;
            }
        }

        public int Pos
        {
            get
            {
                return m_pos;
            }
        }

        public int Length
        {
            get
            {
                return m_data.Length;
            }
        }

        public bool IsFull
        {
            get
            {
                return (m_pos == m_data.Length);
            }
        }
    }
}
