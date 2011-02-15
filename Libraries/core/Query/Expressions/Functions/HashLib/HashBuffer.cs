using System;
using System.Diagnostics;

namespace HashLib
{
    /// <summary>
    /// Code incorporated from the <a href="http://hashlib.codeplex.com">HashLib</a> project to support SHA224 and provide support for all Hash functions under Silverlight
    /// </summary>
    /// <remarks>
    /// Slightly modified to downgrade the code to C# 3 syntax
    /// </remarks>
    public class HashBuffer 
    {
        private byte[] m_data;
        private int m_pos;

        /// <summary>
        /// Creates a new Hash Buffer
        /// </summary>
        /// <param name="a_length"></param>
        public HashBuffer(int a_length)
        {
            Debug.Assert(a_length > 0);

            m_data = new byte[a_length];

            Initialize();
        }

        /// <summary>
        /// Initializes the Hash Buffer
        /// </summary>
        public void Initialize()
        {
            m_pos = 0;
        }

        /// <summary>
        /// Gets the Bytes in the Buffer
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Debug.Assert(IsFull);

            m_pos = 0;
            return m_data;
        }

        /// <summary>
        /// Gets the Bytes in the Buffer with Zero Padding
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytesZeroPadded()
        {
            Array.Clear(m_data, m_pos, m_data.Length - m_pos); 
            m_pos = 0;
            return m_data;
        }

        /// <summary>
        /// Feeds data into the Buffer
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_startIndex"></param>
        /// <param name="a_length"></param>
        /// <param name="a_processedBytes"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Feeds data into the Buffer
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_startIndex"></param>
        /// <param name="a_length"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Feeds data into the Buffer
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_length"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Feeds data into the Buffer
        /// </summary>
        /// <param name="a_data"></param>
        /// <returns></returns>
        public bool Feed(byte a_data)
        {
            Debug.Assert(!IsFull);

            m_data[m_pos] = a_data;
            m_pos++;

            return IsFull;
        }

        /// <summary>
        /// Gets whether the Buffer is empty
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return m_pos == 0;
            }
        }

        /// <summary>
        /// Gets the Position in the Buffer
        /// </summary>
        public int Pos
        {
            get
            {
                return m_pos;
            }
        }

        /// <summary>
        /// Gets the Length of the Buffer
        /// </summary>
        public int Length
        {
            get
            {
                return m_data.Length;
            }
        }

        /// <summary>
        /// Gets whether the Buffer is full
        /// </summary>
        public bool IsFull
        {
            get
            {
                return (m_pos == m_data.Length);
            }
        }
    }
}
