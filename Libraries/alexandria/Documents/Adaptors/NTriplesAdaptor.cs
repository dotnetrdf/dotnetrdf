using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.Alexandria.Documents.Adaptors
{
    public class NTriplesAdaptor : RdfAdaptor
    {
        private ITripleFormatter _formatter = new NTriplesFormatter();

        public NTriplesAdaptor()
            : base(new NTriplesParser(), new NTriplesWriter()) { }

        protected NTriplesAdaptor(ITripleFormatter formatter)
            : this()
        {
            this._formatter = formatter;
        }

        public override void AppendTriples(IEnumerable<Triple> ts, IDocument<StreamReader,TextWriter> document)
        {
            if (!ts.Any()) return;

            try
            {
                TextWriter writer = document.BeginWrite(true);
                foreach (Triple t in ts)
                {
                    writer.WriteLine(this._formatter.Format(t));
                }
                writer.Close();
                document.EndWrite();
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                document.EndWrite();
                throw new AlexandriaException("Error appending Triples to Document " + document.Name, ex);
            }
        }

        public override void DeleteTriples(IEnumerable<Triple> ts, IDocument<StreamReader,TextWriter> document)
        {
            if (!ts.Any()) return;

            //Generate the String forms of these Triples
            HashSet<String> triples = new HashSet<string>();
            foreach (Triple t in ts)
            {
                triples.Add(this._formatter.Format(t));
            }

            try
            {
                //Now read through the file line by line
                int lineCount = 0, editedLineCount = 0;
                StringBuilder editedOutput = new StringBuilder();
                StreamReader reader = document.BeginRead();

                while (!reader.EndOfStream)
                {
                    lineCount++;
                    String line = reader.ReadLine();

                    //If the Line is a Triple that isn't marked for deletion we pass it through to our edited output
                    if (!triples.Contains(line))
                    {
                        editedOutput.AppendLine(line);
                        editedLineCount++;
                    }
                }
                reader.Close();
                document.EndRead();

                if (lineCount > editedLineCount || lineCount == 0)
                {
                    //We deleted some Triples so need to reserialize the data
                    try
                    {
                        if (editedLineCount > 0)
                        {
                            TextWriter writer = document.BeginWrite(false);
                            writer.Write(editedOutput.ToString());
                            writer.Close();
                            document.EndWrite();
                        }
                        else
                        {
                            //If there are no lines in the result then delete the document
                            document.DocumentManager.ReleaseDocument(document.Name);
                            document.DocumentManager.DeleteDocument(document.Name);
                        }
                    }
                    catch (AlexandriaException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        document.EndWrite();
                        throw new AlexandriaException("Error deleting Triples from Document " + document.Name, ex);
                    }
                }
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                document.EndRead();
                throw new AlexandriaException("Error deleting Triples from Document " + document.Name, ex);
            }
        }
    }
}
