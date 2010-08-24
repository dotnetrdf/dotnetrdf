// casey chesnut (casey@brains-N-brawn.com)
// (C) 2003, 2004 brains-N-brawn LLC
using System;
using System.Security.Cryptography;

namespace VDS.RDF
{
	public sealed class SHA256Managed
	{
		public SHA256Managed()
		{

		}

		private byte [] hash = null;
		public byte[] Hash 
		{ 
			get{return hash;}
		}

		public int HashSize 
		{ 
			get{return 256;}
		}

		public byte [] ComputeHash(byte [] buffer)
		{
			hash = SHA256.MessageSHA256(buffer);
			return hash;
		}
	}
}