#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.Documents;
using Alexandria.Indexing;

namespace Alexandria
{
    public class TestWrapper<TReader,TWriter>
    {
        private AlexandriaManager<TReader,TWriter> _manager;

        public TestWrapper(AlexandriaManager<TReader,TWriter> manager)
        {
            this._manager = manager;
        }

        public IDocumentManager<TReader,TWriter> DocumentManager
        {
            get
            {
                return this._manager.DocumentManager;
            }
        }

        public IIndexManager IndexManager
        {
            get
            {
                return this._manager.IndexManager;
            }
        }
    }
}

#endif
