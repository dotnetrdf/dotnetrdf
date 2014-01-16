using VDS.RDF.Storage;
using System;
using System.Linq;

using System.Text;
using VDS.RDF;
using org.topbraid.spin.model;
using VDS.RDF.Query;

using org.topbraid.spin.system;
using org.topbraid.spin.model.impl;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.SparqlUtil;
using System.Collections.Generic;
using org.topbraid.spin.vocabulary;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using org.topbraid.spin.arq;
namespace org.topbraid.spin.util
{


    /**
     * Static utilities on SPIN Expressions.
     * 
     * @author Holger Knublauch
     */
    public class SPINExpressions
    {

        public readonly static INamespaceMapper emptyPrefixMapping = new NamespaceMapper();


        public static String checkExpression(String str, ISparqlDataset model)
        {
            String queryString = "ASK WHERE { LET (?xqoe := (" + str + ")) }";
            try
            {
                //ARQFactory.get().createQuery(model, queryString);
                //TODO handle namespace prefixes
                new SparqlQueryParser().ParseFromString(queryString);
                return null;
            }
            catch (RdfParseException ex)
            {
                String s = ex.Message;
                int startIndex = s.IndexOf("at line ");
                if (startIndex >= 0)
                {
                    int endIndex = s.IndexOf('.', startIndex);
                    StringBuilder sb = new StringBuilder();
                    sb.Append(s.Substring(0, startIndex));
                    sb.Append("at column ");
                    sb.Append(ex.EndPosition - 27);
                    sb.Append(s.Substring(endIndex));
                    return sb.ToString();
                }
                else
                {
                    return s;
                }
            }
        }


        // TODO remove this : should not be needed but still there for compilation purpose.
        /**
         * Evaluates a given SPIN expression.
         * Prior to calling this, the caller must make sure that the expression has the
         * most specific Java type, e.g. using SPINFactory.asExpression().
         * @param expression  the expression (must be cast into the best possible type)
         * @param queryModel  the Model to query
         * @param bindings  the initial bindings
         * @return the result INode or null
         */
        public static INode evaluate(IResource expression, SpinWrappedDataset queryModel, Dictionary<String, INode> bindings)
        {
            return null; //evaluate(expression, queryModel.ListGraphs(), bindings);
        }


        public static INode evaluate(IResource expression, IEnumerable<Uri> dataset, Dictionary<String, INode> bindings)
        {
            if (expression.canAs(SP.ClassVariable)) //expression.isVariable()
            {
                // Optimized case if the expression is just a variable
                String varName = ((IVariable)expression).getName();
                return bindings[varName];
            }
            else if (expression.isUri())
            {
                return expression;
            }
            else
            {
                IQuery arq = SPINFactory.asQuery(expression); //TODO ARQFactory.get().createExpressionQuery(expression);
                SparqlParameterizedString qexec = new SparqlParameterizedString(); // DatasetUtil.createQuery(arq, dataset, bindings);
                try
                {
                    /*
                    IEnumerator<SparqlResult> rs = qexec.execSelect().getEnumerator();
                    if (rs.MoveNext())
                    {
                        String varName = rs.Current.Variables.First();
                        INode result = rs.Current.Value(varName);
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                    */
                    return null;
                }
                finally
                {
                    //qexec.close();
                }
            }
        }


        public static String getExpressionString(IResource expression)
        {
            return getExpressionString(expression, true);
        }


        public static String getExpressionString(IResource expression, bool usePrefixes)
        {
            if (usePrefixes)
            {
                //StringSparqlPrinter p = new StringSparqlPrinter();
                //p.setUsePrefixes(usePrefixes);
                //SPINExpressions.printExpressionString(p, expression, false, false, expression.getSource().Graph.NamespaceMap);
                //return p.getString();
                return String.Empty;
            }
            else
            {
                return ARQFactory.get().createExpressionString(expression);
            }
        }


        /**
         * Checks whether a given INode is an expression.
         * In order to be regarded as expression it must be a well-formed
         * function call, aggregation or variable.
         * @param node  the INode
         * @return true if node is an expression
         */
        public static bool isExpression(IResource node)
        {
            if (SP.existsModel(node.getSource().Graph))
            {
                INode expr = SPINFactory.asExpression(node);
                if (expr is IVariable)
                {
                    return true;
                }
                else if (!(node is IBlankNode))
                {
                    return false;
                }
                if (expr is IFunctionCall)
                {
                    IResource function = ((IFunctionCall)expr).getFunction();
                    if (function is IUriNode)
                    {
                        if (SPINModuleRegistry.getFunction(function.Uri(), node.getModel()) != null)
                        {
                            return true;
                        }
                        /*TODO 
                        if (FunctionRegistry.get().isRegistered(function.Uri()))
                        {
                            return true;
                        }
                         */
                    }
                }
                else
                {
                    return expr is IAggregation;
                }
            }
            return false;
        }


        public static ISparqlExpression parseARQExpression(String str, SpinProcessor model)
        {
            String queryString = "ASK WHERE { LET (?xqoe := (" + str + ")) }";
            //SparqlParameterizedString arq = ARQFactory.get().createQuery(model, queryString);
            // TODO
            /*
            IElementGroup group = (IElementGroup)arq.getQueryPattern();
            ElementAssign assign = (ElementAssign)group.getElements()[0];
            */
            ISparqlExpression expr = null;//assign.getExpr();
            return expr;
        }


        public static IResource parseExpression(String str, SpinProcessor model)
        {
            ISparqlExpression expr = parseARQExpression(str, model);
            return parseExpression(expr, model);
        }


        public static IResource parseExpression(ISparqlExpression expr, SpinProcessor model)
        {
            //SpinSyntax.ARQ2SPIN a2s = new ARQ2SPIN(model);
            return null;// Resource.Get(expr.ToSpinRdf(model._queryModelInferredSpinConfig, null), model);
        }


        public static void printExpressionString(IContextualSparqlPrinter p, IResource node, bool nested, bool force, INamespaceMapper prefixMapping)
        {
            if (SPINFactory.asVariable(node) == null)
            {
                IAggregation aggr = SPINFactory.asAggregation(node);
                if (aggr != null)
                {
                    IContextualSparqlPrinter pc = p.clone();
                    pc.setNested(nested);
                    aggr.print(pc);
                    return;
                }

                IFunctionCall call = SPINFactory.asFunctionCall(node);
                if (call != null)
                {
                    IContextualSparqlPrinter pc = p.clone();
                    pc.setNested(nested);
                    call.print(pc);
                    return;
                }
            }
            if (force)
            {
                p.print("(");
            }
            if (node.canAs(SP.ClassVariable) || node.isUri())
            {
                AbstractSPINResource.printVarOrResource(p, node);
            }
            else if (node is IElementList)
            {
                ((IElementList)node).print(p);
            }
            else
            {
                INamespaceMapper pm = p.getUsePrefixes() ? prefixMapping : emptyPrefixMapping;
                String str = DatasetUtil.StringForNode(node, pm);
                p.print(str);
            }
            if (force)
            {
                p.print(")");
            }
        }
    }
}