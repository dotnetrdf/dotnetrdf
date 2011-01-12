using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VDS.Alexandria.Utilities;

namespace VDS.Alexandria.Documents.GraphRegistry
{
    public class TsvGraphRegistry : BaseGraphRegistry
    {
        private IDocument<StreamReader,TextWriter> _doc;

        public TsvGraphRegistry(IDocument<StreamReader,TextWriter> doc)
        {
            this._doc = doc;
        }

        ~TsvGraphRegistry()
        {
            this.Dispose(false);
        }

        public override bool RegisterGraph(String graphUri, String name)
        {
            try
            {
                TextWriter writer = this._doc.BeginWrite(true);
                writer.WriteLine(name + "\t" + graphUri);
                writer.Close();
                this._doc.EndWrite();
                return true;
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to register a Graph", ex);
            }
        }

        public override bool UnregisterGraph(String graphUri, String name)
        {
            try
            {
                StringBuilder editedOutput = new StringBuilder();
                int lineCount = 0, editedLineCount = 0;

                using (StreamReader reader = this._doc.BeginRead())
                {
                    String toRemove = name + "\t" + graphUri;
                    while (!reader.EndOfStream)
                    {
                        String line = reader.ReadLine();
                        if (!line.Equals(toRemove))
                        {
                            editedOutput.AppendLine(line);
                            editedLineCount++;
                        }
                        lineCount++;
                    }
                    reader.Close();
                }
                this._doc.EndRead();

                if (lineCount > editedLineCount)
                {
                    if (editedLineCount > 0)
                    {
                        TextWriter writer = this._doc.BeginWrite(false);
                        writer.Write(editedOutput.ToString());
                        writer.Close();
                        this._doc.EndWrite();
                    }
                    else
                    {
                        this._doc.DocumentManager.ReleaseDocument(this._doc.Name);
                        this._doc.DocumentManager.DeleteDocument(this._doc.Name);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to unregister a Graph", ex);
            }
        }

        public override IEnumerable<String> DocumentNames
        {
            get
            {
                return new TsvSingleItemEnumerable(this._doc, 0);
            }
        }

        public override IEnumerable<String> GraphUris
        {
            get
            {
                return new TsvSingleItemEnumerable(this._doc, 1, true);
            }
        }

        public override IEnumerable<KeyValuePair<String, String>> DocumentToGraphMappings
        {
            get
            {
                return new TsvEnumerable(this._doc, 2).Select(values => new KeyValuePair<String, String>(values[0], values[1]));                        
            }
        }

        public override IEnumerable<KeyValuePair<String, String>> GraphToDocumentMappings
        {
            get
            {
                return new TsvEnumerable(this._doc, 2).Select(values => new KeyValuePair<String, String>(values[1], values[0]));
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            this._doc.DocumentManager.ReleaseDocument(this._doc.Name);
        }
    }
}
