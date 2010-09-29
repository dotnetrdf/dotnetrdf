using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace Alexandria
{
    public class AlexandriaException : RdfStorageException
    {
        public AlexandriaException(String message)
            : base(message) { }

        public AlexandriaException(String message, Exception cause)
            : base(message, cause) { }
    }
}
