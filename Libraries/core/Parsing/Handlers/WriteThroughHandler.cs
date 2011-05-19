/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatter">Triple Formatter to use</param>
        /// <param name="writer">Text Writer to write to</param>
        /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling</param>
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

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatter">Triple Formatter to use</param>
        /// <param name="writer">Text Writer to write to</param>
        public WriteThroughHandler(ITripleFormatter formatter, TextWriter writer)
            : this(formatter, writer, true) { }

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatterType">Type of the formatter to create</param>
        /// <param name="writer">Text Writer to write to</param>
        /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling</param>
        public WriteThroughHandler(Type formatterType, TextWriter writer, bool closeOnEnd)
        {
            if (writer == null) throw new ArgumentNullException("writer", "Cannot use a null TextWriter with the Write Through Handler");
            if (formatterType == null) throw new ArgumentNullException("formatterType", "Cannot use a null formatter type");
            this._formatterType = formatterType;
            this._writer = writer;
            this._closeOnEnd = closeOnEnd;
        }

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatterType">Type of the formatter to create</param>
        /// <param name="writer">Text Writer to write to</param>
        public WriteThroughHandler(Type formatterType, TextWriter writer)
            : this(formatterType, writer, true) { }

        /// <summary>
        /// Starts RDF Handling instantiating a Triple Formatter if necessary
        /// </summary>
        protected override void StartRdfInternal()
        {
            if (this._closeOnEnd && this._writer == null) throw new RdfParseException("Cannot use this WriteThroughHandler as an RDF Handler for parsing as you set closeOnEnd to true and you have already used this Handler and so the provided TextWriter was closed");

            if (this._formatterType != null)
            {
                this._formatter = null;
                this._formattingMapper = new QNameOutputMapper();

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

        /// <summary>
        /// Ends RDF Handling closing the <see cref="TextWriter">TextWriter</see> being used if the setting is enabled
        /// </summary>
        /// <param name="ok">Indicates whether parsing completed without error</param>
        protected override void EndRdfInternal(bool ok)
        {
            if (this._closeOnEnd)
            {
                this._writer.Close();
                this._writer = null;
            }
        }

        /// <summary>
        /// Handles Namespace Declarations passing them to the underlying formatter if applicable
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
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

        /// <summary>
        /// Handles Base URI Declarations passing them to the underlying formatter if applicable
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            if (this._formatter is IBaseUriFormatter)
            {
                this._writer.WriteLine(((IBaseUriFormatter)this._formatter).FormatBaseUri(baseUri));
            }

            return true;
        }

        /// <summary>
        /// Handles Triples by writing them using the underlying formatter
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            this._writer.WriteLine(this._formatter.Format(t));
            return true;
        }

        /// <summary>
        /// Gets that the Handler accepts all Triples
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return true;
            }
        }
    }
}
