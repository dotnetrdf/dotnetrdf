using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.PropertyFunctions;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Core.Runtime
{
    public class SpinMagicPropertyCall
        : ISparqlPropertyFunction
    {
        private static SparqlQueryParser _parser = new SparqlQueryParser();

        private IFunctionResource _declaration;
        private SparqlParameterizedString _query;

        //private List<String> _vars = new List<String>();
        // We still maintain those as lists for when multiple/arguments and results will be supported
        private List<IArgumentResource> _arguments;

        /// <summary>
        /// Creates a new FunctionCallWrapper
        /// </summary>
        internal SpinMagicPropertyCall(IFunctionResource declaration)
        {
            _declaration = declaration;
            _arguments = declaration.getArguments(true);
            List<string> argNames = _arguments.Select(arg => arg.getVarName()).ToList();

            BaseSparqlPrinter printer = new BaseSparqlPrinter();
            declaration.getBody().Print(printer);
            _query = new SparqlParameterizedString(printer.GetString());
            SparqlQuery sparqlQuery = _parser.ParseFromString(_query);

            // TODO also handle fixed blank nodes remapping
            foreach (SparqlVariable variable in sparqlQuery.Variables)
            {
                if (!argNames.Contains(variable.Name))
                {
                    _query.SetVariable(variable.Name, RDFHelper.CreateTempVariableNode());
                }
                else
                {
                    // TODO bind default values
                }
            }
        }

        /// <summary>
        /// Gets the Function URI for the property function
        /// </summary>
        public Uri FunctionUri
        {
            get
            {
                return _declaration.Uri;
            }
        }

        /// <summary>
        /// Gets the Variables used in the property function
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return ((Dictionary<string, INode>)this._query.Variables).Keys;
            }
        }

        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            throw new NotSupportedException("Spin magic properties can only be evaluated through a SpinProcessor");
        }

        public SubQueryPattern ToSparql(List<PatternItem> arguments)
        {
            // TODO map arguments to variables
            for (int i = 0, l = Math.Min(arguments.Count, _arguments.Count); i < l; i++)
            {
                BindArgument(i, arguments[i]);
            }
            return new SubQueryPattern(_parser.ParseFromString(_query));
        }

        private void BindArgument(int index, PatternItem value)
        {
            INode boundValue = null;
            if (value is NodeMatchPattern)
            {
                boundValue = ((NodeMatchPattern)value).Node;
            }
            else if (value is VariablePattern)
            {
                boundValue = RDFHelper.CreateVariableNode(((VariablePattern)value).VariableName);
            }
            else if (value is BlankNodePattern)
            {
                boundValue = RDFHelper.CreateBlankNode();
            }
            else if (value is FixedBlankNodePattern)
            {
                boundValue = RDFHelper.CreateBlankNode(((FixedBlankNodePattern)value).InternalID);
            }
            else
            {
                throw new NotSupportedException("Spin function call arguments can only be constant or variable expressions");
            }
            _query.SetVariable(_arguments[index].getVarName(), boundValue);
        }
    }
}