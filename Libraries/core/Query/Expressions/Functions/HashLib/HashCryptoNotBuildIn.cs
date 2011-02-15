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
    public abstract class HashCryptoNotBuildIn : HashCrypto, ICryptoNotBuildIn
    {
        /// <summary>
        /// Hash Buffer
        /// </summary>
        protected readonly HashBuffer m_buffer;
        /// <summary>
        /// Counter for processed Bytes
        /// </summary>
        protected ulong m_processedBytes;

        /// <summary>
        /// Creates a new non-built-in Hash Function
        /// </summary>
        /// <param name="a_hashSize"></param>
        /// <param name="a_blockSize"></param>
        /// <param name="a_bufferSize"></param>
        protected HashCryptoNotBuildIn(int a_hashSize, int a_blockSize, int a_bufferSize) 
            : base(a_hashSize, a_blockSize)
        {
            if (a_bufferSize == -1)
                a_bufferSize = a_blockSize;

            m_buffer = new HashBuffer(a_bufferSize);
            m_processedBytes = 0;
        }

        /// <summary>
        /// Transforms some Bytes
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_index"></param>
        /// <param name="a_length"></param>
        public override void TransformBytes(byte[] a_data, int a_index, int a_length)
        {
            Debug.Assert(a_index >= 0);
            Debug.Assert(a_length >= 0);
            Debug.Assert(a_index + a_length <= a_data.Length);

            if (!m_buffer.IsEmpty)
            {
                if (m_buffer.Feed(a_data, ref a_index, ref a_length, ref m_processedBytes))
                    TransformBuffer();
            }

            while (a_length >= m_buffer.Length)
            {
                m_processedBytes += (ulong)m_buffer.Length;
                TransformBlock(a_data, a_index);
                a_index += m_buffer.Length;
                a_length -= m_buffer.Length;
            }

            if (a_length > 0)
                m_buffer.Feed(a_data, ref a_index, ref a_length, ref m_processedBytes);
        }

        /// <summary>
        /// Initializes the Hash Function
        /// </summary>
        public override void Initialize()
        {
            m_buffer.Initialize();
            m_processedBytes = 0;
        }

        /// <summary>
        /// Produces the final Hash of all the Transformed Inputs
        /// </summary>
        /// <returns></returns>
        public override HashResult TransformFinal()
        {
            Finish();

            Debug.Assert(m_buffer.IsEmpty);

            byte[] result = GetResult();

            Debug.Assert(result.Length == HashSize);

            Initialize();
            return new HashResult(result);
        }

        /// <summary>
        /// Transforms the Buffer
        /// </summary>
        protected void TransformBuffer()
        {
            Debug.Assert(m_buffer.IsFull);

            TransformBlock(m_buffer.GetBytes(), 0);
        }

        /// <summary>
        /// Signals that the Hash is finished
        /// </summary>
        protected abstract void Finish();

        /// <summary>
        /// Transforms a Block
        /// </summary>
        /// <param name="a_data"></param>
        /// <param name="a_index"></param>
        protected abstract void TransformBlock(byte[] a_data, int a_index);

        /// <summary>
        /// Gets the Result as Bytes
        /// </summary>
        /// <returns></returns>
        protected abstract byte[] GetResult();
    }
}
