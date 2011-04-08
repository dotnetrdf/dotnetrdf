using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which writes the handled Triples out to a <see cref="TextWriter">TextWriter</see> using a provided <see cref="ITripleFormatter">ITripleFormatter</see>
    /// </summary>
    public class WriteThroughHandler : BaseRdfHandler
    {
        private Type _formatterType;
        private ITripleFormatter _formatter;
        private TextWriter _writer;
        private bool _closeOnEnd = true;
        private INamespaceMapper _formattingMapper = new QNameOutputMapper();

        public WriteThroughHandler(ITripleFormatter formatter, TextWriter writer, bool closeOnEnd)
        {
            if (writer == null) throw new ArgumentNullException("writer", "Cannot use a null TextWriter with the Write Through Handler");
            if (formatter != null)
            {
                this._formatter = formatter;
            }
            else
            {
                this._formatter = new NTriplesFormatter();
            }
            this._writer = writer;
            this._closeOnEnd = closeOnEnd;
        }

        public WriteThroughHandler(ITripleFormatter formatter, TextWriter writer)
            : this(formatter, writer, true) { }

        public WriteThroughHandler(TextWriter writer, bool closeOnEnd)
            : this((ITripleFormatter)null, writer, closeOnEnd) { }

        public WriteThroughHandler(TextWriter writer)
            : this((ITripleFormatter)null, writer, true) { }

        public WriteThroughHandler(Type formatterType, TextWriter writer, bool closeOnEnd)
        {
            if (writer == null) throw new ArgumentNullException("writer", "Cannot use a null TextWriter with the Write Through Handler");
            if (formatterType == null) throw new ArgumentNullException("formatterType", "Cannot use a null formatter type");
            this._formatterType = formatterType;
            this._writer = writer;
            this._closeOnEnd = closeOnEnd;
        }

        protected override void StartRdfInternal()
        {
            if (this._closeOnEnd && this._writer == null) throw new RdfParseException("Cannot use this WriteThroughHandler as an RDF Handler for parsing as you set closeOnEnd to true and you have already used this Handler and so the provided TextWriter was closed");

            if (this._formatterType != null)
            {
                this._formatter = null;
                this._formattingMapper = null;

                //Instantiate a new Formatter
                ConstructorInfo[] cs = this._formatterType.GetConstructors();
                Type qnameMapperType = typeof(QNameOutputMapper);
                Type nsMapperType = typeof(INamespaceMapper);
                foreach (ConstructorInfo c in cs.OrderByDescending(c => c.GetParameters().Count()))
                {
                    ParameterInfo[] ps = c.GetParameters();
                    try
                    {
                        if (ps.Length == 1)
                        {
                            if (ps[0].ParameterType.Equals(qnameMapperType))
                            {
                                this._formatter = Activator.CreateInstance(this._formatterType, new Object[] { this._formattingMapper }) as ITripleFormatter;
                            }
                            else if (ps[0].ParameterType.Equals(nsMapperType))
                            {
                                this._formatter = Activator.CreateInstance(this._formatterType, new Object[] { this._formattingMapper }) as ITripleFormatter;
                            }
                        }
                        else if (ps.Length == 0)
                        {
                            this._formatter = Activator.CreateInstance(this._formatterType) as ITripleFormatter;
                        }

                        if (this._formatter != null) break;
                    }
                    catch
                    {
                        //Suppress errors since we'll throw later if necessary
                    }
                }

                //If we get out here and the formatter is null then we throw an error
                if (this._formatter == null) throw new RdfParseException("Unable to instantiate a ITripleFormatter from the given Formatter Type " + this._formatterType.FullName);
            }
        }

        protected override void EndRdfInternal(bool ok)
        {
            if (this._closeOnEnd)
            {
                this._writer.Close();
                this._writer = null;
            }
        }

        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            if (this._formattingMapper != null)
            {
                this._formattingMapper.AddNamespace(prefix, namespaceUri);
            }

            if (this._formatter is INamespaceFormatter)
            {
                this._writer.WriteLine(((INamespaceFormatter)this._formatter).FormatNamespace(prefix, namespaceUri));
            }

            return true;
        }

        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            if (this._formatter is IBaseUriFormatter)
            {
                this._writer.WriteLine(((IBaseUriFormatter)this._formatter).FormatBaseUri(baseUri));
            }

            return true;
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            this._writer.WriteLine(this._formatter.Format(t));
            return true;
        }
    }
}
