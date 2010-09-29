using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Alexandria.Documents
{
    public class FileDocument : IDocument
    {
        private String _filename;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private bool _canRead = true, _canWrite = true;

        public FileDocument(String filename)
        {
            this._filename = filename;
        }

        public String Name
        {
            get
            {
                return this._filename;
            }
        }

        public bool CanRead
        {
            get 
            {
                return this.Exists && this._canRead;
            }
        }

        public bool CanWrite
        {
            get 
            {
                return this._canWrite; 
            }
        }

        public bool Exists
        {
            get 
            {
                return File.Exists(this._filename); 
            }
        }

        public TextWriter BeginWrite()
        {
            try
            {
                this._canRead = false;
                this._canWrite = false;
                this._lock.EnterWriteLock();

                return new StreamWriter(this._filename);
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("Failed to obtain a Write Lock for the Document " + this._filename, ex);
            }
        }

        public void EndWrite()
        {
            try
            {
                this._lock.ExitWriteLock();
                this._canRead = true;
                this._canWrite = true;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("Failed to exit a Write Lock for the Document " + this._filename, ex);
            }
        }

        public StreamReader BeginRead()
        {
            try
            {
                this._canWrite = false;
                this._lock.EnterReadLock();

                return new StreamReader(File.OpenRead(this._filename));
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("Failed to obtain a Read Lock for the Document " + this._filename, ex);
            }
        }

        public void EndRead()
        {
            try
            {
                this._lock.ExitReadLock();
                this._canWrite = (this._lock.CurrentReadCount == 0);
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("Failed to exit a Read Lock for the Document " + this._filename, ex);
            }
        }

        public void Dispose()
        {
            //Need to wait for any reads/writes to complete
            while (!this.CanWrite || this._lock.CurrentReadCount > 0)
            {
                Thread.Sleep(50);
            }

            this._lock.Dispose();
        }
    }
}
