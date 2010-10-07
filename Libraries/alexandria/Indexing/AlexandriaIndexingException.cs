using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Alexandria.Indexing
{
    public class AlexandriaIndexingException : AlexandriaException
    {
        public AlexandriaIndexingException(String message)
            : base(message) { }

        public AlexandriaIndexingException(String message, Exception cause)
            : base(message, cause) { }
    }

    class AlexandriaNoIndexException : AlexandriaIndexingException
    {
        public AlexandriaNoIndexException(String index)
            : base("Unable to access the " + index + " Index since this store does not have that index") { }
    }
}
