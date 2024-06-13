# Implementing Extension Functions

Extension Functions are a standardised extension point provided by the [SPARQL Specification](http://www.w3.org/TR/sparql11-query/#extensionFunctions) which allows you to introduce new functions by naming them with URIs.  dotNetRDF includes full support for this functionality and includes a whole range of extension functions described on the [Function Libraries](function_libraries.md) page.  This page covers how to add your own extension functions.

Adding an extension function requires implementing two interfaces:
* <xref:VDS.RDF.Query.Expressions.ISparqlExpression>
* <xref:VDS.RDF.Query.Expressions.ISparqlCustomExpressionFactory>

Note that by default the library allows unknown extension functions and just treats them as always producing an error, you can change this behaviour to disallow unknown extension functions by setting the `Options.QueryAllowUnknownFunctions` property to `false`

## Implement ISparqlExpression

The <xref:VDS.RDF.Query.Expressions.ISparqlExpression> interface implements the actual function logic, there are a variety of abstract implementations that may be relevant depending on the kind of function you wish to implement.  You may find it easiest to start by copying the code from a similar existing function or extending the same base class as a similar existing function.

Let's take a look at an example implementation from dotNetRDF itself:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ namespace() function
    /// </summary>
    public class NamespaceFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new ARQ Namespace function
        /// </summary>
        /// <param name="expr">Expression</param>
        public NamespaceFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._expr.Evaluate(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Uri)
                {
                    IUriNode u = (IUriNode)temp;
                    if (!u.Uri.Fragment.Equals(String.Empty))
                    {
                        return new StringNode(null, u.Uri.AbsoluteUri.Substring(0, u.Uri.AbsoluteUri.LastIndexOf('#') + 1));
                    }
                    else
                    {
                        return new StringNode(null, u.Uri.AbsoluteUri.Substring(0, u.Uri.AbsoluteUri.LastIndexOf('/') + 1));
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot find the Local Name for a non-URI Node");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot find the Local Name for a null");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Namespace + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Namespace;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new NamespaceFunction(transformer.Transform(this._expr));
        }
    }
}
```

Here we are attempting to split URIs (and only URIs) to determine a suitable namespace to use for them.  As you can see we extend `VDS.RDF.Query.Expressions.BaseUnaryExpression` which means we don't have to implement that much ourselves.

The main method of interest is the `Evaluate()` method which is where the actual function evaluation is done.  The usual pattern is to first evaluate the child expressions and then attempt to apply the function on the result, if the function cannot be applied due to type mismatches or any other error then a `RdfQueryException` **must** be thrown.  If the function succeeds then it should return an appropriate `VDS.RDF.Nodes.IValuedNode` instance which represents the result of the function.

The remaining methods and properties are primarily around having the expression play nice with other APIs such as query serialisation and optimisation.

## Implements ISparqlCustomExpressionFactory

The <xref:VDS.RDF.Query.Expressions.ISparqlCustomExpressionFactory> interface provides a factory for your custom expressions, each factory can potentially produce many different extension functions depending on how you've implemented it.

Let's take a look at an example from the dotNetRDF code:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Functions.Arq;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Expression Factory which generates ARQ Function expressions
    /// </summary>
    /// <remarks>
    /// <para>
    /// Designed to help provide feature parity with the ARQ query engine contained in Jena
    /// </para>
    /// </remarks>
    public class ArqFunctionFactory : ISparqlCustomExpressionFactory
    {
        /// <summary>
        /// ARQ Function Namespace
        /// </summary>
        public const String ArqFunctionsNamespace = "http://jena.hpl.hp.com/ARQ/function#";

        /// <summary>
        /// Constants for ARQ Numeric functions
        /// </summary>
        public const String Max = "max",
                            Min = "min",
                            Pi = "pi",
                            E = "e";

        /// <summary>
        /// Constants for ARQ Graph functions
        /// </summary>
        public const String BNode = "bnode",
                            LocalName = "localname",
                            Namespace = "namespace";

        /// <summary>
        /// Constants for ARQ String functions
        /// </summary>
        public const String Substr = "substr",
                            Substring = "substring",
                            StrJoin = "strjoin";

        /// <summary>
        /// Constants for ARQ Miscellaneous functions
        /// </summary>
        public const String Sha1Sum = "sha1sum",
                            Now = "now";

        /// <summary>
        /// Array of Extension Function URIs
        /// </summary>
        private String[] FunctionUris = {
                                            Max,
                                            Min,
                                            Pi,
                                            E,
                                            BNode,
                                            LocalName,
                                            Namespace,
                                            Substr,
                                            Substring,
                                            StrJoin,
                                            Sha1Sum,
                                            Now
                                        };

        /// <summary>
        /// Tries to create an ARQ Function expression if the function Uri correseponds to a supported ARQ Function
        /// </summary>
        /// <param name="u">Function Uri</param>
        /// <param name="args">Function Arguments</param>
        /// <param name="scalarArgs">Scalar Arguments</param>
        /// <param name="expr">Generated Expression</param>
        /// <returns>Whether an expression was successfully generated</returns>
        public bool TryCreateExpression(Uri u, List<ISparqlExpression> args, Dictionary<String,ISparqlExpression> scalarArgs, out ISparqlExpression expr)
        {
            //If any Scalar Arguments are present then can't possibly be an ARQ Function
            if (scalarArgs.Count > 0)
            {
                expr = null;
                return false;
            }

            String func = u.AbsoluteUri;
            if (func.StartsWith(ArqFunctionFactory.ArqFunctionsNamespace))
            {
                func = func.Substring(ArqFunctionFactory.ArqFunctionsNamespace.Length);
                ISparqlExpression arqFunc = null;

                switch (func)
                {
                    case ArqFunctionFactory.BNode:
                        if (args.Count == 1)
                        {
                            arqFunc = new BNodeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ bnode() function");
                        }
                        break;
                    case ArqFunctionFactory.E:
                        if (args.Count == 0)
                        {
                            arqFunc = new EFunction();
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ e() function");
                        }
                        break;
                    case ArqFunctionFactory.LocalName:
                        if (args.Count == 1)
                        {
                            arqFunc = new LocalNameFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ localname() function");
                        }
                        break;
                    case ArqFunctionFactory.Max:
                        if (args.Count == 2)
                        {
                            arqFunc = new MaxFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ max() function");
                        }
                        break;
                    case ArqFunctionFactory.Min:
                        if (args.Count == 2)
                        {
                            arqFunc = new MinFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ min() function");
                        }
                        break;
                    case ArqFunctionFactory.Namespace:
                        if (args.Count == 1)
                        {
                            arqFunc = new NamespaceFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ namespace() function");
                        }
                        break;
                    case ArqFunctionFactory.Now:
                        if (args.Count == 0)
                        {
                            arqFunc = new NowFunction();
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ now() function");
                        }
                        break;
                    case ArqFunctionFactory.Pi:
                        if (args.Count == 0)
                        {
                            arqFunc = new PiFunction();
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ pi() function");
                        }
                        break;
                    case ArqFunctionFactory.Sha1Sum:
                        if (args.Count == 1)
                        {
                            arqFunc = new Sha1Function(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ sha1sum() function");
                        }
                        break;
                    case ArqFunctionFactory.StrJoin:
                        if (args.Count >= 2)
                        {
                            arqFunc = new StringJoinFunction(args.First(), args.Skip(1));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ strjoing() function");
                        }
                        break;
                    case ArqFunctionFactory.Substr:
                    case ArqFunctionFactory.Substring:
                        if (args.Count == 2)
                        {
                            arqFunc = new SubstringFunction(args.First(), args.Last());
                        }
                        else if (args.Count == 3)
                        {
                            arqFunc = new SubstringFunction(args.First(), args[1], args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ " + func + "() function");
                        }
                        break;
                }

                if (arqFunc != null)
                {
                    expr = arqFunc;
                    return true;
                }
            }
            expr = null;
            return false;  
        }

        /// <summary>
        /// Gets the Extension Function URIs supported by this Factory
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionFunctions
        {
            get
            {
                return (from u in FunctionUris
                        select UriFactory.Create(ArqFunctionsNamespace + u));
            }
        }

        /// <summary>
        /// Gets the Extension Aggregate URIs supported by this Factory
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionAggregates
        {
            get
            {
                return Enumerable.Empty<Uri>();
            }
        }
    }
}
```

This is the factory corresponding to our earlier example as well as some other functions.  You can see that the key method is the `TryCreateExpression` function which tries to create an extension function returning true if it succeeds.  In the case where the factory sees a function URI it recognises but an incorrect number of arguments it throws a `RdfParseException`

### Registering Factories

In order for your factories to be used they have to be registered, factories can be registered in several places depending on the scope you want them to have.

If you want your factory to have global scope and apply to all queries and updates then you can register it with the <xref:VDS.RDF.Query.Expressions.SparqlExpressionFactory> class like so:

```csharp
SparqlExpressionFactory.AddCustomFactory(new MyCustomFactory());
```

If you want your factory to have local scope and apply only to queries/updates produced by a specific parser then you can do this by setting the `ExpressionFactories` properties on a parser instance like so:

```csharp
SparqlQueryParser parser = new SparqlQueryParser();
parser.ExpressionFactories = new ISparqlCustomExpressionFactory[] { new MyCustomFactory(); }
```

Once registered all subsequent queries and updates may now use your newly registered extension functions.

When using the [Configuration API](../../user_guide/configuration/index.md) then you can configure custom expression factories as detailed on the [Configuring SPARQL Expression Factories](../../user_guide/configuration/sparql_expression_factories.md) page.