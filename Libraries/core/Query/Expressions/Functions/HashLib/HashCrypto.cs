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
    public abstract class HashCrypto : Hash, ICrypto, IBlockHash
    {
        protected HashCrypto(int a_hashSize, int a_blockSize)
            : base(a_hashSize, a_blockSize)
        {
        }
    }
}
