using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Utilities.Convert
{
    public class ConversionProgressHandler
        : BaseRdfHandler, IWrappingRdfHandler
    {
        private IRdfHandler _handler;
        private int _batches = 0;
        private long _count = 0;
        private Stopwatch _timer = new Stopwatch();

        private const long ReportingInterval = 50000;

        public ConversionProgressHandler(IRdfHandler handler)
        {
            this._handler = handler;
        }

        protected override void StartRdfInternal()
        {
            this._handler.StartRdf();
            this._count = 0;
            this._timer.Stop();
            this._timer.Reset();
            this._timer.Start();
        }

        protected override void EndRdfInternal(bool ok)
        {
            this._handler.EndRdf(ok);
            this._timer.Stop();
            long triples = ((ReportingInterval * this._batches) + this._count);
            Console.WriteLine("rdfConvert: Info: Converted " + triples + " triple(s) in " + this._timer.Elapsed);
            double speed = ((double)triples / (double)this._timer.ElapsedMilliseconds) * 1000;
            Console.WriteLine("rdfConvert: Info: Average Conversion Speed was " + speed + " Triples/second");
        }

        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return this._handler.HandleBaseUri(baseUri);
        }

        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return this._handler.HandleNamespace(prefix, namespaceUri);
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            this._count++;
            if (this._count >= ReportingInterval)
            {
                this._batches++;
                this._count = 0;
                Console.WriteLine("rdfConvert: Info: Converted " + (this._batches * ReportingInterval) + " triples in " + this._timer.Elapsed + " so far..." + (this._handler is WriteToFileHandler ? String.Empty : "(NB - Due to options/target format actual conversion to output format will happen at end of input parsing)"));
            }
            return this._handler.HandleTriple(t);
        }

        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }

        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return new IRdfHandler[] { this._handler };
            }
        }
    }
}
