using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace VDS.Alexandria.Documents
{
    public abstract class BaseDocument<TReader,TWriter> : IDocument<TReader, TWriter>
    {
        private IDocumentManager<TReader,TWriter> _manager;
        private String _name;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private bool _canWrite = true;

        public BaseDocument( String name, IDocumentManager<TReader,TWriter> manager)
        {
            this._name = name;
            this._manager = manager;
        }

        public IDocumentManager<TReader,TWriter> DocumentManager
        {
            get
            {
                return this._manager;
            }
        }

        public String Name
        {
            get
            {
                return this._name;
            }
        }

        public abstract bool Exists
        {
            get;
        }

        public TWriter BeginWrite(bool append)
        {
            try
            {
                this._canWrite = false;
                this._lock.EnterWriteLock();

                return this.BeginWriteInternal(append);
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("Failed to obtain a Write Lock for the Document " + this._name, ex);
            }
        }

        protected abstract TWriter BeginWriteInternal(bool append);

        public void EndWrite()
        {
            try
            {
                this.EndWriteInternal();
                this._lock.ExitWriteLock();
                this._canWrite = (this._lock.WaitingWriteCount == 0);
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("Failed to exit a Write Lock for the Document " + this._name, ex);
            }
        }

        protected virtual void EndWriteInternal()
        {

        }

        public TReader BeginRead()
        {
            try
            {
                this._lock.EnterReadLock();

                return this.BeginReadInternal();
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("Failed to obtain a Read Lock for the Document " + this._name, ex);
            }
        }

        protected abstract TReader BeginReadInternal();

        public void EndRead()
        {
            try
            {
                this.EndReadInternal();
                this._lock.ExitReadLock();
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("Failed to exit a Read Lock for the Document " + this._name, ex);
            }
        }

        protected virtual void EndReadInternal()
        {

        }

        public virtual void Dispose()
        {
            //Need to wait for any reads/writes to complete
            while (!this._canWrite || this._lock.WaitingReadCount > 0 || this._lock.WaitingWriteCount > 0 || this._lock.CurrentReadCount > 0)
            {
                Thread.Sleep(50);
            }

            this._lock.Dispose();
        }
    }
}
