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
        private bool _canWrite = true;

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

        public bool Exists
        {
            get 
            {
                return File.Exists(this._filename); 
            }
        }

        public TextWriter BeginWrite(bool append)
        {
            try
            {
                this._canWrite = false;
                this._lock.EnterWriteLock();

                if (append)
                {
                    return new StreamWriter(File.Open(this._filename, FileMode.Append));
                }
                else
                {
                    return new StreamWriter(File.Open(this._filename, FileMode.Create));
                }
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
                this._canWrite = (this._lock.WaitingWriteCount == 0);
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
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("Failed to exit a Read Lock for the Document " + this._filename, ex);
            }
        }

        public void Dispose()
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
