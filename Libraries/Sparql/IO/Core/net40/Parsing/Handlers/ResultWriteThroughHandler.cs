using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VDS.RDF.Namespaces;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A Results Handler which writes the handled Results out to a <see cref="TextWriter">TextWriter</see> using a provided <see cref="IResultFormatter">IResultFormatter</see>
    /// </summary>
    public class ResultWriteThroughHandler 
        : BaseResultsHandler
    {
        private Type _formatterType;
        private IResultFormatter _formatter;
        private TextWriter _writer;
        private bool _closeOnEnd = true;
        private INamespaceMapper _formattingMapper = new QNameOutputMapper();
        private SparqlResultsType _currentType = SparqlResultsType.Boolean;
        private List<String> _currVariables = new List<String>();
        private bool _headerWritten = false;

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatter">Triple Formatter to use</param>
        /// <param name="writer">Text Writer to write to</param>
        /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling</param>
        public ResultWriteThroughHandler(IResultFormatter formatter, TextWriter writer, bool closeOnEnd)
        {
            if (writer == null) throw new ArgumentNullException("writer", "Cannot use a null TextWriter with the Result Write Through Handler");
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
        public ResultWriteThroughHandler(IResultFormatter formatter, TextWriter writer)
            : this(formatter, writer, true) { }

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatterType">Type of the formatter to create</param>
        /// <param name="writer">Text Writer to write to</param>
        /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling</param>
        public ResultWriteThroughHandler(Type formatterType, TextWriter writer, bool closeOnEnd)
        {
            if (writer == null) throw new ArgumentNullException("writer", "Cannot use a null TextWriter with the Result Write Through Handler");
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
        public ResultWriteThroughHandler(Type formatterType, TextWriter writer)
            : this(formatterType, writer, true) { }

        /// <summary>
        /// Starts writing results
        /// </summary>
        protected override void StartResultsInternal()
        {
            if (this._closeOnEnd && this._writer == null) throw new RdfParseException("Cannot use this ResultWriteThroughHandler as an Results Handler for parsing as you set closeOnEnd to true and you have already used this Handler and so the provided TextWriter was closed");
            this._currentType = SparqlResultsType.Unknown;
            this._currVariables.Clear();
            this._headerWritten = false;

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
                                this._formatter = Activator.CreateInstance(this._formatterType, new Object[] { this._formattingMapper }) as IResultFormatter;
                            }
                            else if (ps[0].ParameterType.Equals(nsMapperType))
                            {
                                this._formatter = Activator.CreateInstance(this._formatterType, new Object[] { this._formattingMapper }) as IResultFormatter;
                            }
                        }
                        else if (ps.Length == 0)
                        {
                            this._formatter = Activator.CreateInstance(this._formatterType) as IResultFormatter;
                        }

                        if (this._formatter != null) break;
                    }
                    catch
                    {
                        //Suppress errors since we'll throw later if necessary
                    }
                }

                //If we get out here and the formatter is null then we throw an error
                if (this._formatter == null) throw new RdfParseException("Unable to instantiate a IResultFormatter from the given Formatter Type " + this._formatterType.FullName);
            }
        }

        /// <summary>
        /// Ends the writing of results closing the <see cref="TextWriter">TextWriter</see> depending on the option set when this instance was instantiated
        /// </summary>
        /// <param name="ok"></param>
        protected override void EndResultsInternal(bool ok)
        {
            if (this._formatter is IResultSetFormatter)
            {
                this._writer.WriteLine(((IResultSetFormatter)this._formatter).FormatResultSetFooter());
            }
            if (this._closeOnEnd)
            {
                this._writer.Close();
                this._writer = null;
            }
            this._currentType = SparqlResultsType.Unknown;
            this._currVariables.Clear();
            this._headerWritten = false;
        }

        /// <summary>
        /// Writes a Boolean Result to the output
        /// </summary>
        /// <param name="result">Boolean Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            if (this._currentType != SparqlResultsType.Unknown) throw new RdfParseException("Cannot handle a Boolean Result when the handler has already handled other types of results");
            this._currentType = SparqlResultsType.Boolean;
            if (!this._headerWritten && this._formatter is IResultSetFormatter)
            {
                this._writer.WriteLine(((IResultSetFormatter)this._formatter).FormatResultSetHeader());
                this._headerWritten = true;
            }

            this._writer.WriteLine(this._formatter.FormatBooleanResult(result));
        }

        /// <summary>
        /// Writes a Variable declaration to the output
        /// </summary>
        /// <param name="var">Variable Name</param>
        /// <returns></returns>
        protected override bool HandleVariableInternal(string var)
        {
            if (this._currentType == SparqlResultsType.Boolean) throw new RdfParseException("Cannot handler a Variable when the handler has already handled a boolean result");
            this._currentType = SparqlResultsType.VariableBindings;
            this._currVariables.Add(var);
            return true;
        }

        /// <summary>
        /// Writes a Result to the output
        /// </summary>
        /// <param name="result">SPARQL Result</param>
        /// <returns></returns>
        protected override bool HandleResultInternal(SparqlResult result)
        {
            if (this._currentType == SparqlResultsType.Boolean) throw new RdfParseException("Cannot handle a Result when the handler has already handled a boolean result");
            this._currentType = SparqlResultsType.VariableBindings;
            if (!this._headerWritten && this._formatter is IResultSetFormatter)
            {
                this._writer.WriteLine(((IResultSetFormatter)this._formatter).FormatResultSetHeader(this._currVariables.Distinct()));
                this._headerWritten = true;
            }
            this._writer.WriteLine(this._formatter.Format(result));
            return true;
        }
    }
}