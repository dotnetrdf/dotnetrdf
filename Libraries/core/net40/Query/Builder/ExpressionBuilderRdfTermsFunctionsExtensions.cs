using VDS.RDF.Parsing;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;

namespace VDS.RDF.Query.Builder
{
    public static class ExpressionBuilderRdfTermsFunctionsExtensions
    {
        public static BooleanExpression IsIRI(this ExpressionBuilder eb, SparqlExpression term)
        {
            var isIri = new IsIriFunction(term.Expression);
            return new BooleanExpression(isIri);
        }

        /// <summary>
        /// Creates the isIRI function with a variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="variableName">name of variable to check</param>
        public static BooleanExpression IsIRI(this ExpressionBuilder eb, string variableName)
        {
            return eb.IsIRI(eb.Variable(variableName));
        }

        public static BooleanExpression IsBlank(this ExpressionBuilder eb, SparqlExpression term)
        {
            var isBlank = new IsBlankFunction(term.Expression);
            return new BooleanExpression(isBlank);
        }

        public static BooleanExpression IsBlank(this ExpressionBuilder eb, string variableName)
        {
            return IsBlank(eb, eb.Variable(variableName));
        }

        public static BooleanExpression IsLiteral(this ExpressionBuilder eb, SparqlExpression term)
        {
            var isLiteral = new IsLiteralFunction(term.Expression);
            return new BooleanExpression(isLiteral);
        }

        /// <summary>
        /// Creates the isLiteral function with a variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="variableName">name of variable to check</param>
        public static BooleanExpression IsLiteral(this ExpressionBuilder eb, string variableName)
        {
            return eb.IsLiteral(eb.Variable(variableName));
        }

        public static BooleanExpression IsNumeric(this ExpressionBuilder eb, SparqlExpression term)
        {
            var isNumeric = new IsNumericFunction(term.Expression);
            return new BooleanExpression(isNumeric);
        }

        /// <summary>
        /// Creates the isNumeric function with a variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="variableName">name of variable to check</param>
        public static BooleanExpression IsNumeric(this ExpressionBuilder eb, string variableName)
        {
            return eb.IsNumeric(eb.Variable(variableName));
        }

        public static SimpleLiteralExpression Str(this ExpressionBuilder eb, VariableExpression variable)
        {
            return Str(variable.Expression);
        }

        public static SimpleLiteralExpression Str(this ExpressionBuilder eb, LiteralExpression literal)
        {
            return Str(literal.Expression);
        }

        public static SimpleLiteralExpression Str(this ExpressionBuilder eb, IriExpression iriTerm)
        {
            return Str(iriTerm.Expression);
        }

        private static SimpleLiteralExpression Str(ISparqlExpression expression)
        {
            return new SimpleLiteralExpression(new StrFunction(expression));
        }

        public static SimpleLiteralExpression Lang(this ExpressionBuilder eb, VariableExpression variable)
        {
            return new SimpleLiteralExpression(new LangFunction(variable.Expression));
        }

        public static SimpleLiteralExpression Lang(this ExpressionBuilder eb, LiteralExpression literal)
        {
            return new SimpleLiteralExpression(new LangFunction(literal.Expression));
        }

        public static IriExpression Datatype(this ExpressionBuilder eb, VariableExpression variable)
        {
            return Datatype(eb, variable.Expression);
        }

        public static IriExpression Datatype(this ExpressionBuilder eb, LiteralExpression literal)
        {
            return Datatype(eb, literal.Expression);
        }

        private static IriExpression Datatype(ExpressionBuilder eb, ISparqlExpression expression)
        {
            var dataTypeFunction = eb.SparqlVersion == SparqlQuerySyntax.Sparql_1_0
                                       ? new DataTypeFunction(expression)
                                       : new DataType11Function(expression);
            return new IriExpression(dataTypeFunction);
        }

        public static BlankNodeExpression BNode(this ExpressionBuilder eb)
        {
            return new BlankNodeExpression(new BNodeFunction());
        }

        public static BlankNodeExpression BNode(this ExpressionBuilder eb, SimpleLiteralExpression simpleLiteral)
        {
            return new BlankNodeExpression(new BNodeFunction(simpleLiteral.Expression));
        }

        public static BlankNodeExpression BNode(this ExpressionBuilder eb, TypedLiteralExpression<string> stringLiteral)
        {
            return new BlankNodeExpression(new BNodeFunction(stringLiteral.Expression));
        }
    }
}