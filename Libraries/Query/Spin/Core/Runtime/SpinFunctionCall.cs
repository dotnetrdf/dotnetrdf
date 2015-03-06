using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.PropertyFunctions;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Nodes;
using System.Text;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Spin.Core.Runtime
{
    /// <summary>
    /// NOTE: we will have to define the way to evaluate results for non Sparql function.
    /// This would idealy done through a combination of special dedicated algebras and Query/ResultHandler/ResultBindiner chain
    /// </summary>
    public class SpinFunctionCall
        : ISparqlExpression
    {
        private static SparqlQueryParser _parser = new SparqlQueryParser();

        private IFunctionResource _declaration;
        //private SparqlParameterizedString _queryTemplate;

        private bool _isSparqlFunction = true;
        // We still maintain those as lists for when multiple/arguments and results will be supported
        private List<ISparqlExpression> _arguments;
        private IVariableNode _resultVariable;

        internal SpinFunctionCall(SpinModel model)
        {
        }

        /// <summary>
        /// Creates a new SpinFunctionCall configured from a SPIN resource
        /// </summary>
        internal SpinFunctionCall(IFunctionResource declaration)
            : this(declaration.GetModel())
        {
            _declaration = declaration;
            ArgumentNames = declaration.getArguments(true).Select(arg => arg.getVarName()).ToList();

            BaseSparqlPrinter printer = new BaseSparqlPrinter();
            ICommandResource body = declaration.getBody();
            // if a body is provided it must be a SPARQL Select/Ask Query
            if (body != null)
            {
                // TODO determine how to handle functions that require computation that is not convertible to SPARQL
                body.Print(printer);
                QueryTemplate = new SparqlParameterizedString(printer.GetString());
            }
            else // otherwise the function must provide a hard-coded implementation through a Javascript file/code  or a .NET assembly
            {
                _isSparqlFunction = false;
                QueryTemplate = new SparqlParameterizedString();
            }
        }

        public bool IsSparqlFunction {
            get {
                return _isSparqlFunction;
            }
        }
        /// <summary>
        /// Gets the Function URI for the property function
        /// </summary>
        public virtual Uri FunctionUri
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
                return _arguments.SelectMany(arg => arg.Variables);
                //return ((Dictionary<string, INode>)this._queryTemplate.Variables).Keys;
            }
        }

        /// <summary>
        /// Gets the Variables used in the property function
        /// </summary>
        public List<String> ArgumentNames { get; protected set; }

        /// <summary>
        /// Gets the name of the variable to which the function result is assigned
        /// </summary>
        public String ResultVariable
        {
            get
            {
                return _resultVariable.VariableName;
            }
            protected set
            {
                _resultVariable = RDFHelper.CreateTempVariableNode();
                QueryTemplate.SetVariable(value, _resultVariable);
            }
        }

        protected virtual SparqlParameterizedString QueryTemplate { get; set; }

        /// <summary>
        /// Gets the expanded Sparql patterns/subquery for the fuction
        /// </summary>
        /// <returns></returns>
        public SubQueryPattern ToSparql()
        {
            if (QueryTemplate == null) return null;
            foreach (KeyValuePair<String, INode> localVariable in QueryTemplate.Variables.Where(kv=>kv.Value==null).ToList()) {
                int argIndex = ArgumentNames.IndexOf(localVariable.Key);
                if (argIndex > 0)
                {
                    BindArgument(argIndex, _arguments[argIndex]);
                }
                else {
                    QueryTemplate.SetVariable(localVariable.Key, RDFHelper.CreateTempVariableNode());
                }
            }
            return new SubQueryPattern(_parser.ParseFromString(QueryTemplate));
        }

        private void BindArgument(int index, ISparqlExpression value)
        {
            if (value.Type != SparqlExpressionType.Primary)
            {
                throw new ArgumentException("Spin function call arguments must be primary expressions");
            }
            INode boundValue = null;
            if (value is VariableTerm)
            {
                boundValue = RDFHelper.CreateVariableNode(((VariableTerm)value).Variables.First());
            }
            else if (value is ConstantTerm)
            {
                boundValue = ((ConstantTerm)value).Node;
            }
            else
            {
                throw new NotSupportedException("Spin function call arguments can only be constant or variable expressions");
            }
            if (QueryTemplate == null) return;
            QueryTemplate.SetVariable(ArgumentNames[index], boundValue);
        }


        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            throw new NotSupportedException("Spin functions can only be evaluated through a SpinProcessor");
        }

        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        public string Functor
        {
            get
            {
                return this.FunctionUri.ToString();
            }
        }

        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return _arguments;
            }
            set
            {
                _arguments = value.ToList();
            }
        }

        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }

        public bool CanParallelise
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(this.Functor.Replace(">", "\\>"));
            output.Append('>');
            output.Append('(');
            for (int i = 0; i < this._arguments.Count; i++)
            {
                output.Append(this._arguments[i].ToString());

                if (i < this._arguments.Count - 1)
                {
                    output.Append(", ");
                }
            }
            output.Append(')');
            return output.ToString();
        }
    }
}
