/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Class representing the SPARQL Datatype() function
    /// </summary>
    public class DataTypeFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Datatype() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public DataTypeFunction(IExpression expr)
            : base(expr) {}

        public override IExpression Copy(IExpression argument)
        {
            return new DataTypeFunction(argument);
        }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode result = this.Argument.Evaluate(solution, context);
            if (result == null)
            {
                throw new RdfQueryException("Cannot return the Data Type URI of a NULL");
            }
            switch (result.NodeType)
            {
                case NodeType.Literal:
                    INode lit = result;
                    if (lit.DataType == null)
                    {
                        if (!lit.Language.Equals(string.Empty))
                        {
                            throw new RdfQueryException("Cannot return the Data Type URI of Language Specified Literals in SPARQL 1.0");
                        }
                        return new UriNode(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    return new UriNode(lit.DataType);

                default:
                    throw new RdfQueryException("Cannot return the Data Type URI of Nodes which are not Literal Nodes");
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordDataType; }
        }
    }

    /// <summary>
    /// Class representing the SPARQL Datatype() function in SPARQL 1.1
    /// </summary>
    /// <remarks>
    /// This is required because the changes to the function in SPARQL 1.1 are not backwards compatible with SPARQL 1.0
    /// </remarks>
    public class DataType11Function
        : DataTypeFunction
    {
        /// <summary>
        /// Creates a new DataType function
        /// </summary>
        /// <param name="expr">Expression</param>
        public DataType11Function(IExpression expr)
            : base(expr) {}

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode result = this.Argument.Evaluate(solution, context);
            if (result == null) throw new RdfQueryException("Cannot return the Data Type URI of a null");

            switch (result.NodeType)
            {
                case NodeType.Literal:
                    if (result.HasLanguage) return new UriNode(UriFactory.Create(RdfSpecsHelper.RdfLangString));
                    return result.HasDataType ? new UriNode(result.DataType) : new UriNode(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                default:
                    throw new RdfQueryException("Cannot return the Data Type URI of Nodes which are not Literal Nodes");
            }
        }

        public override IExpression Copy(IExpression argument)
        {
            return new DataType11Function(argument);
        }
    }
}