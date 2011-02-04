using System;

namespace HashLib.Crypto
{
    internal abstract class MDBase : HashCryptoNotBuildIn
    {
        protected readonly uint[] m_state;

        protected const uint C1 = 0x50a28be6;
        protected const uint C2 = 0x5a827999;
        protected const uint C3 = 0x5c4dd124;
        protected const uint C4 = 0x6ed9eba1;
        protected const uint C5 = 0x6d703ef3;
        protected const uint C6 = 0x8f1bbcdc;
        protected const uint C7 = 0x7a6d76e9;
        protected const uint C8 = 0xa953fd4e;

        protected MDBase(int a_stateLength, int a_hashSize) 
            : base(a_hashSize, 64, -1)
        {
            m_state = new uint[a_stateLength];

            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override byte[] GetResult()
        {
            return Converters.ConvertUIntsToBytes(m_state);
        }

        protected override void Finish()
        {
            ulong bits = m_processedBytes * 8;
            int padindex = (m_buffer.Pos < 56) ? (56 - m_buffer.Pos) : (120 - m_buffer.Pos);

            byte[] pad = new byte[padindex + 8];

            pad[0] = 0x80;

            Converters.ConvertULongToBytes(bits, pad, padindex);
            padindex += 8;

            TransformBytes(pad, 0, padindex);
        }
    }
}
