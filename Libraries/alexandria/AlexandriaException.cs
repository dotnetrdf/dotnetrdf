using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.Alexandria
{
    public class AlexandriaException : RdfStorageException
    {
        public AlexandriaException(String message)
            : base(message) { }

        public AlexandriaException(String message, Exception cause)
            : base(message, cause) { }
    }
}
