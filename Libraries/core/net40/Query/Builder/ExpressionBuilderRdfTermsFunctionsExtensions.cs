using System;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for creating SPARQL functions, which operate on RDF Terms
    /// </summary>
    public static class ExpressionBuilderRdfTermsFunctionsExtensions
    {
        /// <summary>
        /// Creates a call to the isIRI function with an expression parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="term">any SPARQL expression</param>
        public static BooleanExpression IsIRI(this ExpressionBuilder eb, SparqlExpression term)
        {
            var isIri = new IsIriFunction(term.Expression);
            return new BooleanExpression(isIri);
        }

        /// <summary>
        /// Creates a call to the isIRI function with a variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="variableName">name of variable to check</param>
        public static BooleanExpression IsIRI(this ExpressionBuilder eb, string variableName)
        {
            return eb.IsIRI(eb.Variable(variableName));
        }

        /// <summary>
        /// Creates a call to the isBlank function with an expression parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="term">any SPARQL expression</param>
        public static BooleanExpression IsBlank(this ExpressionBuilder eb, SparqlExpression term)
        {
            var isBlank = new IsBlankFunction(term.Expression);
            return new BooleanExpression(isBlank);
        }

        /// <summary>
        /// Creates a call to the isBlank function with a variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="variableName">name of variable to check</param>
        public static BooleanExpression IsBlank(this ExpressionBuilder eb, string variableName)
        {
            return IsBlank(eb, eb.Variable(variableName));
        }

        /// <summary>
        /// Creates a call to the isLiteral function with an expression parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="term">any SPARQL expression</param>
        public static BooleanExpression IsLiteral(this ExpressionBuilder eb, SparqlExpression term)
        {
            var isLiteral = new IsLiteralFunction(term.Expression);
            return new BooleanExpression(isLiteral);
        }

        /// <summary>
        /// Creates a call to the isLiteral function with a variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="variableName">name of variable to check</param>
        public static BooleanExpression IsLiteral(this ExpressionBuilder eb, string variableName)
        {
            return eb.IsLiteral(eb.Variable(variableName));
        }

        /// <summary>
        /// Creates a call to the isNumeric function with an expression parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="term">any SPARQL expression</param>
        public static BooleanExpression IsNumeric(this ExpressionBuilder eb, SparqlExpression term)
        {
            var isNumeric = new IsNumericFunction(term.Expression);
            return new BooleanExpression(isNumeric);
        }

        /// <summary>
        /// Creates a call to the isNumeric function with a variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="variableName">name of variable to check</param>
        public static BooleanExpression IsNumeric(this ExpressionBuilder eb, string variableName)
        {
            return eb.IsNumeric(eb.Variable(variableName));
        }

        /// <summary>
        /// Creates a call to the STR function with a variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="variable">a SPARQL variable</param>
        public static SimpleLiteralExpression Str(this ExpressionBuilder eb, VariableExpression variable)
        {
            return Str(variable.Expression);
        }

        /// <summary>
        /// Creates a call to the STR function with a literal expression parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="literal">a SPARQL literal expression</param>
        public static SimpleLiteralExpression Str(this ExpressionBuilder eb, LiteralExpression literal)
        {
            return Str(literal.Expression);
        }

        /// <summary>
        /// Creates a call to the STR function with an variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="iriTerm">an RDF IRI term</param>
        public static SimpleLiteralExpression Str(this ExpressionBuilder eb, IriExpression iriTerm)
        {
            return Str(iriTerm.Expression);
        }

        private static SimpleLiteralExpression Str(ISparqlExpression expression)
        {
            return new SimpleLiteralExpression(new StrFunction(expression));
        }

        /// <summary>
        /// Creates a call to the LANG function with a variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="variable">a SPARQL variable</param>
        public static SimpleLiteralExpression Lang(this ExpressionBuilder eb, VariableExpression variable)
        {
            return new SimpleLiteralExpression(new LangFunction(variable.Expression));
        }

        /// <summary>
        /// Creates a call to the LANG function with a literal expression parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="literal">a SPARQL literal expression</param>
        public static SimpleLiteralExpression Lang(this ExpressionBuilder eb, LiteralExpression literal)
        {
            return new SimpleLiteralExpression(new LangFunction(literal.Expression));
        }

        /// <summary>
        /// Creates a call to the DATATYPE function with a variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="variable">a SPARQL variable</param>
        /// <remarks>depending on <see cref="ExpressionBuilder.SparqlVersion"/> will use a different flavour of datatype function</remarks>
        public static IriExpression Datatype(this ExpressionBuilder eb, VariableExpression variable)
        {
            return Datatype(eb, variable.Expression);
        }

        /// <summary>
        /// Creates a call to the DATATYPE function with a literal expression parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="literal">a SPARQL literal expression</param>
        /// <remarks>depending on <see cref="ExpressionBuilder.SparqlVersion"/> will use a different flavour of datatype function</remarks>
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

        /// <summary>
        /// Creates a parameterless call to the BNODE function
        /// </summary>
        /// <param name="eb"> </param>
        public static BlankNodeExpression BNode(this ExpressionBuilder eb)
        {
            return new BlankNodeExpression(new BNodeFunction());
        }

        /// <summary>
        /// Creates a call to the BNODE function with a simple literal parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="simpleLiteral">a SPARQL simple literal</param>
        public static BlankNodeExpression BNode(this ExpressionBuilder eb, SimpleLiteralExpression simpleLiteral)
        {
            return new BlankNodeExpression(new BNodeFunction(simpleLiteral.Expression));
        }

        /// <summary>
        /// Creates a call to the BNODE function with a string literal parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="stringLiteral">a SPARQL string literal</param>
        public static BlankNodeExpression BNode(this ExpressionBuilder eb, TypedLiteralExpression<string> stringLiteral)
        {
            return new BlankNodeExpression(new BNodeFunction(stringLiteral.Expression));
        }

        private static LiteralExpression StrDt(ISparqlExpression lexicalForm, ISparqlExpression datatypeIri)
        {
            return new LiteralExpression(new StrDtFunction(lexicalForm, datatypeIri));
        }

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a IRI expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="lexicalForm">a SPARQL simple literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public static LiteralExpression StrDt(this ExpressionBuilder eb, SimpleLiteralExpression lexicalForm, IriExpression datatypeIri)
        {
            return StrDt(lexicalForm.Expression, datatypeIri.Expression);
        }

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a <see cref="Uri"/> parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="lexicalForm">a SPARQL simple literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public static LiteralExpression StrDt(this ExpressionBuilder eb, SimpleLiteralExpression lexicalForm, Uri datatypeIri)
        {
            return StrDt(lexicalForm.Expression, new ConstantTerm(new UriNode(null, datatypeIri)));
        }

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a variable parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="lexicalForm">a SPARQL simple literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public static LiteralExpression StrDt(this ExpressionBuilder eb, SimpleLiteralExpression lexicalForm, VariableExpression datatypeIri)
        {
            return StrDt(lexicalForm.Expression, datatypeIri.Expression);
        }

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a IRI expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public static LiteralExpression StrDt(this ExpressionBuilder eb, string lexicalForm, IriExpression datatypeIri)
        {
            return StrDt(lexicalForm.ToConstantTerm(), datatypeIri.Expression);
        }

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a IRI expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public static LiteralExpression StrDt(this ExpressionBuilder eb, string lexicalForm, VariableExpression datatypeIri)
        {
            return StrDt(lexicalForm.ToConstantTerm(), datatypeIri.Expression);
        }

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a <see cref="Uri"/> parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public static LiteralExpression StrDt(this ExpressionBuilder eb, string lexicalForm, Uri datatypeIri)
        {
            return StrDt(lexicalForm.ToConstantTerm(), new ConstantTerm(new UriNode(null, datatypeIri)));
        }

        /// <summary>
        /// Creates a call to the STRDT function with a variable and a <see cref="Uri"/> parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public static LiteralExpression StrDt(this ExpressionBuilder eb, VariableExpression lexicalForm, Uri datatypeIri)
        {
            return StrDt(lexicalForm.Expression, new ConstantTerm(new UriNode(null, datatypeIri)));
        }

        /// <summary>
        /// Creates a call to the STRDT function with a variable and a <see cref="Uri"/> parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public static LiteralExpression StrDt(this ExpressionBuilder eb, VariableExpression lexicalForm, VariableExpression datatypeIri)
        {
            return StrDt(lexicalForm.Expression, datatypeIri.Expression);
        }

        /// <summary>
        /// Creates a call to the STRDT function with a variable and a IRI expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public static LiteralExpression StrDt(this ExpressionBuilder eb, VariableExpression lexicalForm, IriExpression datatypeIri)
        {
            return StrDt(lexicalForm.Expression, datatypeIri.Expression);
        }

        /// <summary>
        /// Creates a call to the UUID function
        /// </summary>
        /// <param name="eb"> </param>
        public static IriExpression UUID(this ExpressionBuilder eb)
        {
            return new IriExpression(new UUIDFunction());
        }

        /// <summary>
        /// Creates a call to the StrUUID function
        /// </summary>
        /// <param name="eb"> </param>
        public static SimpleLiteralExpression StrUUID(this ExpressionBuilder eb)
        {
            return new SimpleLiteralExpression(new StrUUIDFunction());
        }
    }
}