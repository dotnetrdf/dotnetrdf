using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VDS.RDF.Compatability
{
    internal class AsyncOperationState
    {
        private readonly ManualResetEvent _waitHandle;
        private Exception _operationException;

        public AsyncOperationState()
        {
            _waitHandle = new ManualResetEvent(false);
        }

        public void OperationCompleted()
        {
            _waitHandle.Set();
        }

        public void OperationFailed(Exception ex)
        {
            _operationException = ex;
        }

        public void WaitForCompletion()
        {
            _waitHandle.WaitOne();
            if (_operationException != null)
            {
                throw _operationException;
            }
        }
    }
}
