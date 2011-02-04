using System;
using System.Diagnostics;

namespace HashLib
{
    public abstract class HashCryptoNotBuildIn : HashCrypto, ICryptoNotBuildIn
    {
        protected readonly HashBuffer m_buffer;
        protected ulong m_processedBytes;

        protected HashCryptoNotBuildIn(int a_hashSize, int a_blockSize, int a_bufferSize) 
            : base(a_hashSize, a_blockSize)
        {
            if (a_bufferSize == -1)
                a_bufferSize = a_blockSize;

            m_buffer = new HashBuffer(a_bufferSize);
            m_processedBytes = 0;
        }

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

        public override void Initialize()
        {
            m_buffer.Initialize();
            m_processedBytes = 0;
        }

        public override HashResult TransformFinal()
        {
            Finish();

            Debug.Assert(m_buffer.IsEmpty);

            byte[] result = GetResult();

            Debug.Assert(result.Length == HashSize);

            Initialize();
            return new HashResult(result);
        }

        protected void TransformBuffer()
        {
            Debug.Assert(m_buffer.IsFull);

            TransformBlock(m_buffer.GetBytes(), 0);
        }

        protected abstract void Finish();
        protected abstract void TransformBlock(byte[] a_data, int a_index);
        protected abstract byte[] GetResult();
    }
}
