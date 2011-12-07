/* 
 * Copyright (C) 2007, Andrew Matthews http://aabs.wordpress.com/
 *
 * This file is Free Software and part of LinqToRdf http://code.google.com/fromName/linqtordf/
 *
 * It is licensed under the following license:
 *   - Berkeley License, V2.0 or any newer version
 *
 * You may not use this file except in compliance with the above license.
 *
 * See http://code.google.com/fromName/linqtordf/ for the complete text of the license agreement.
 *
 */
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using System.Collections.Generic;

namespace VDS.RDF.Linq.Sparql
{
    public class LinqToSparqlExpTranslator<T> : RdfExpressionTranslator<T>, IQueryFormatTranslator
    {
        protected Logger Logger  = new Logger(typeof(LinqToSparqlExpTranslator<T>));
        public LinqToSparqlExpTranslator()
        {
            #region Tracing
#line hidden
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("creating LinqToSparqlExpTranslator.");
            }
#line default
            #endregion
            stringBuilder = new StringBuilder();
        }

        public LinqToSparqlExpTranslator(StringBuilder stringBuilder)
        {
            using (var ls = new LoggingScope("LinqToSparqlExpTranslator ctor"))
            {
                this.stringBuilder = stringBuilder;
            }    
        }

        public void Dispatch(Expression expression)
        {
            #region Tracing
#line hidden
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Dispatching {0} Expression.", expression.NodeTypeName());
            }
#line default
            #endregion
            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                    Add(expression);
                    break;
                case ExpressionType.AddChecked:
                    AddChecked(expression);
                    break;
                case ExpressionType.And:
                    And(expression);
                    break;
                case ExpressionType.AndAlso:
                    AndAlso(expression);
                    break;
                case ExpressionType.ArrayIndex:
                    ArrayIndex(expression);
                    break;
                case ExpressionType.ArrayLength:
                    ArrayLength(expression);
                    break;
                case ExpressionType.Call:
                    Call(expression);
                    break;
                case ExpressionType.Coalesce:
                    Coalesce(expression);
                    break;
                case ExpressionType.Conditional:
                    Conditional(expression);
                    break;
                case ExpressionType.Constant:
                    Constant(expression);
                    break;
                case ExpressionType.Convert:
                    Convert(expression);
                    break;
                case ExpressionType.ConvertChecked:
                    ConvertChecked(expression);
                    break;
                case ExpressionType.Divide:
                    Divide(expression);
                    break;
                case ExpressionType.Equal:
                    Equal(expression);
                    break;
                case ExpressionType.ExclusiveOr:
                    ExclusiveOr(expression);
                    break;
                case ExpressionType.GreaterThan:
                    GreaterThan(expression);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    GreaterThanOrEqual(expression);
                    break;
                case ExpressionType.Invoke:
                    Invoke(expression);
                    break;
                case ExpressionType.Lambda:
                    Lambda(expression);
                    break;
                case ExpressionType.LeftShift:
                    LeftShift(expression);
                    break;
                case ExpressionType.LessThan:
                    LessThan(expression);
                    break;
                case ExpressionType.LessThanOrEqual:
                    LessThanOrEqual(expression);
                    break;
                case ExpressionType.ListInit:
                    ListInit(expression);
                    break;
                case ExpressionType.MemberAccess:
                    MemberAccess(expression);
                    break;
                case ExpressionType.MemberInit:
                    MemberInit(expression);
                    break;
                case ExpressionType.Modulo:
                    Modulo(expression);
                    break;
                case ExpressionType.Multiply:
                    Multiply(expression);
                    break;
                case ExpressionType.MultiplyChecked:
                    MultiplyChecked(expression);
                    break;
                case ExpressionType.Negate:
                    Negate(expression);
                    break;
                case ExpressionType.NegateChecked:
                    NegateChecked(expression);
                    break;
                case ExpressionType.New:
                    New(expression);
                    break;
                case ExpressionType.NewArrayBounds:
                    NewArrayBounds(expression);
                    break;
                case ExpressionType.NewArrayInit:
                    NewArrayInit(expression);
                    break;
                case ExpressionType.Not:
                    Not(expression);
                    break;
                case ExpressionType.NotEqual:
                    NotEqual(expression);
                    break;
                case ExpressionType.Or:
                    Or(expression);
                    break;
                case ExpressionType.OrElse:
                    OrElse(expression);
                    break;
                case ExpressionType.Parameter:
                    Parameter(expression);
                    break;
                case ExpressionType.Power:
                    Power(expression);
                    break;
                case ExpressionType.Quote:
                    Quote(expression);
                    break;
                case ExpressionType.RightShift:
                    RightShift(expression);
                    break;
                case ExpressionType.Subtract:
                    Subtract(expression);
                    break;
                case ExpressionType.SubtractChecked:
                    SubtractChecked(expression);
                    break;
                case ExpressionType.TypeAs:
                    TypeAs(expression);
                    break;
                case ExpressionType.TypeIs:
                    TypeIs(expression);
                    break;
                case ExpressionType.UnaryPlus:
                    UnaryPlus(expression);
                    break;
            }

        }

        public void ExclusiveOr(Expression expression)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotImplementedException();
        }

        public void NegateChecked(Expression expression)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotImplementedException();
        }

        public void Power(Expression expression)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotImplementedException();
        }

        public void UnaryPlus(Expression expression)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotImplementedException();
        }

        private void GenerateBinaryExpression(Expression e, string op)
        {
            if (e == null)
                throw new ArgumentNullException("e was null");
            if (op == null)
                throw new ArgumentNullException("op was null");
            if (op.Length == 0)
                throw new ArgumentNullException("op.Length was empty");
            BinaryExpression be = e as BinaryExpression;
            if (be != null)
            {
                QueryAppend("(");
                Dispatch(be.Left);
                QueryAppend(")" + op + "(");
                Dispatch(be.Right);
                QueryAppend(")");
                Log("+ :{0} Handled", e.NodeType);
            }
        }

        public static readonly string tripleFormatString = "{0} <{1}> {2} .\n";
        public static readonly string tripleFormatStringLiteral = "{0} <{1}> \"{2}\" .\n";

        public HashSet<MemberInfo> Parameters
        {
            get
            {
                if (parameters == null)
                    parameters = new HashSet<MemberInfo>();
                return parameters;
            }
            set { parameters = value; }
        }

        private HashSet<MemberInfo> parameters;

        #region Expression Node Handlers

        public void Add(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.Add);
            GenerateBinaryExpression(e, "+");
        }

        public void AddChecked(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.AddChecked);
            GenerateBinaryExpression(e, "+");
        }

        public void And(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.And);
            GenerateBinaryExpression(e, "&&");
        }

        public void AndAlso(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.AndAlso);
            GenerateBinaryExpression(e, "&&");
        }

        public void TypeAs(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation TypeAs not supported");
        }

        public void BitwiseAnd(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation Bitwise And not supported");
            /*
                        BinaryExpression be = e as BinaryExpression;
                        if (be != null)
                        {
                            QueryAppend("(");
                            Dispatch(be.Left);
                            QueryAppend(")&(");
                            Dispatch(be.Right);
                            QueryAppend(")");
                        }
                        Log("+ :{0} Handled", e.NodeType);
            */
        }

        public void BitwiseNot(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation BitwiseNot not supported");
        }

        public void BitwiseOr(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation BitwiseOr not supported");
        }

        public void BitwiseXor(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation BitwiseXor not supported");
        }

        public void Cast(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation Cast not supported");
        }

        public void Coalesce(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation Coalesce not supported");
        }

        public void Conditional(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation Conditional not supported");
        }

        public void Constant(Expression e)
        {
            ConstantExpression ce = (ConstantExpression)e;
            QueryAppend(TypeTranslator.Get(e.Type, ce.Value).ToString());
        }

        public void Convert(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation Convert not supported");
        }

        public void ConvertChecked(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation ConvertChecked not supported");
        }

        public void Divide(Expression e)
        {
            BinaryExpression be = e as BinaryExpression;
            if (be != null)
            {
                QueryAppend("(");
                Dispatch(be.Left);
                QueryAppend(")/(");
                Dispatch(be.Right);
                QueryAppend(")");
            }
            Log("+ :{0} Handled", e.NodeType);
        }

        public void Equal(Expression e)
        {
            Expression lh;
            Expression rh;
            if (e is BinaryExpression)
            {
                BinaryExpression be = e as BinaryExpression;
                lh = be.Left;
                rh = be.Right;
            }
            else if (e is MethodCallExpression)
            {
                MethodCallExpression mce = e as MethodCallExpression;
                lh = mce.Arguments[0];
                rh = mce.Arguments[1];
            }
            else
            {
                throw new LinqToRdfException("Unrecognised equality expression type");
            }

            if (lh != null && rh != null)
            {
                XsdtPrimitiveDataType dt = TypeTranslator.GetDataType(lh.Type);
                if (dt == XsdtPrimitiveDataType.XsdtString)
                {
                    QueryAppend("regex(");
                    Dispatch(lh);
                    QueryAppend(", ");
                    Dispatch(rh);
                    QueryAppend(") ");
                }
                else
                {
                    //QueryAppend("(");
                    Dispatch(lh);
                    QueryAppend(" = ");
                    //QueryAppend(")=(");
                    Dispatch(rh);
                    //QueryAppend(")");
                    //Log("+ :{0} Handled", e.NodeType);
                }
            }
            else
            {
                Log("Failure during generation of Equal expression");
            }
        }

        public void Funclet(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation Funclet not supported");
        }

        public void GreaterThan(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.GreaterThan);
            GenerateBinaryExpression(e, ">");
        }

        public void GreaterThanOrEqual(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.GreaterThanOrEqual);
            GenerateBinaryExpression(e, ">=");
        }

        public void ArrayIndex(Expression e)
        {
            BinaryExpression be = (BinaryExpression)e;
            Dispatch(be.Left);
            QueryAppend("[");
            Dispatch(be.Right);
            QueryAppend("]");
        }

        public void Invoke(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation Invoke not supported");
        }

        public void TypeIs(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation TypeIs not supported");
        }

        public void Lambda(Expression e)
        {
            Dispatch(((LambdaExpression)e).Body);
        }

        public void LessThanOrEqual(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.LessThanOrEqual);
            GenerateBinaryExpression(e, "<=");
        }

        public void ArrayLength(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation ArrayLength not supported");
        }

        public void ListInit(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation ListInit not supported");
        }

        public void LeftShift(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation LShift not supported");
        }

        public void LessThan(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.LessThan);
            GenerateBinaryExpression(e, "<");
        }

        public void MemberAccess(Expression e)
        {
            MemberExpression me = e as MemberExpression;
            if (me != null)
            {
                stringBuilder.Append("?" + me.Member.Name);
                if (me.Member.MemberType == MemberTypes.Property)
                    Parameters.Add(me.Member);
            }
        }

        public void MemberInit(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation MemberInit not supported");
        }

        public void Call(Expression e)
        {
            MethodCallExpression mce = (MethodCallExpression)e;
            if (mce.Method.DeclaringType == typeof(string))
            {
                ProcessStringOperations(mce);
                return;
            }
            switch (mce.Method.Name)
            {
                case "OccursAsStmtObjectWithUri":
                    // well we caught it. Now What?
                    // 1) get the parameter name of the instance forming the subject of the triple
                    MemberExpression me = (MemberExpression)mce.Arguments[0];
                    ParameterExpression pe = (ParameterExpression)me.Expression;
                    string name = pe.Name;
                    // 2) get the URI of the predicate/relationship
                    string relnUri = me.Member.GetOwlResourceUri();
                    // 3) get the URI of the instance forming the object of the triple
                    string instanceUri = ((ConstantExpression)mce.Arguments[1]).Value.ToString();
                    QueryAppend("${0} <{1}> <{2}>.", name, relnUri, instanceUri);
                    break;
                case "HasInstanceUri":
                    // well we caught it. Now What?
                    // 1 get the parameter name of the instance
                    ParameterExpression pe2 = (ParameterExpression)mce.Arguments[0];
                    string name2 = pe2.Name;
                    // 2 get the URI of the instance
                    string instanceUri2 = ((ConstantExpression)mce.Arguments[1]).Value.ToString();
                    QueryAppend("${0} = \"{1}\"", name2, instanceUri2);
                    break;
                case "StmtObjectWithSubjectAndPredicate":
                    // 1) get the parameter name of the instance forming the object of the triple
                    ParameterExpression pe3 = (ParameterExpression)mce.Arguments[0];
                    string name3 = pe3.Name;
                    // 2) get the triple's subject
                    string subjectUri3 = ((ConstantExpression)mce.Arguments[1]).Value.ToString();
                    // 3) get the triple's predicate
                    string predicateUri3 = ((ConstantExpression)mce.Arguments[2]).Value.ToString();
                    QueryAppend("<{0}> <{1}> ${2} .", subjectUri3, predicateUri3, name3);
                    break;
                case "StmtSubjectWithObjectAndPredicate":
                    // 1) get the parameter name of the instance forming the subject of the triple
                    ParameterExpression pe4 = (ParameterExpression)mce.Arguments[0];
                    string name4 = pe4.Name;
                    // 2) get the triple's object
                    string objectUri4 = ((ConstantExpression)mce.Arguments[1]).Value.ToString();
                    // 3) get the triple's predicate
                    string predicateUri4 = ((ConstantExpression)mce.Arguments[2]).Value.ToString();
                    QueryAppend("${0} <{1}> <{2}> .", name4, predicateUri4, objectUri4);
                    break;
                case "ToInt16":
                case "ToInt32":
                case "ToInt64":
                case "ToFloat":
                case "ToDouble":
                case "ToDecimal":
                    ProcessCastOperators(e);
                    break;
                case "op_Equality":
                    Equal(e);
                    break;
                default:
                    Dispatch(mce.Object);
                    QueryAppend("." + mce.Method.Name + "(");
                    string sep = "";
                    for (int i = 0; i < mce.Arguments.Count; i++)
                    {
                        QueryAppend(sep);
                        Dispatch(mce.Arguments[i]);
                        sep = ", ";
                    }
                    QueryAppend(")");
                    break;
            }
        }

        string SafeDispatch(Expression e)
        {
            StringBuilder currentStringBuilder = StringBuilder;
            try
            {
                StringBuilder = new StringBuilder();
                // we can be confident that this really is ontology cast operation, therefore there will be only one parameter
                Dispatch(e);
                return StringBuilder.ToString();
            }
            finally
            {
            StringBuilder = currentStringBuilder;
            }
        }

        private void ProcessCastOperators(Expression e)
        {
            MethodCallExpression mce = (MethodCallExpression)e;
            XsdtTypeConverter tc = new XsdtTypeConverter();
            string typeToCastTo = tc.GetXsdtAttrFor(mce.Type).TypeUri;
            string argName = SafeDispatch(mce.Arguments[0]);
            QueryAppend("xsd:{1}({0})", argName, typeToCastTo);
        }

        public void MethodCallVirtual(Expression e)
        {
            MethodCallExpression mce = (MethodCallExpression)e;
            MethodInfo mi = mce.Method;

            // is it eligible for ontology regex operation?
            if (mi.DeclaringType == typeof(string))
            {
                ProcessStringOperations(mce);
                return;
            }
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation MethodCallVirtual not supported for Method '" + mi.Name + "'");
        }

        private void ProcessStringOperations(MethodCallExpression mce)
        {
            MethodInfo mi = mce.Method;
            switch (mi.Name)
            {
                case "Contains":
                    GenerateRegexComparison(mce);
                    return;
                case "StartsWith":
                    GenerateRegexStartsWith(mce);
                    return;
                case "EndsWith":
                    GenerateRegexEndsWith(mce);
                    return;
            }

            throw new NotImplementedException("operation MethodCallVirtual not supported for Method '" + mi.Name + "'");
        }
        private void GenerateRegexStartsWith(MethodCallExpression mce)
        {
            ConstantExpression constantExpression = (ConstantExpression)mce.Arguments[0];
            MemberExpression memberExpression = (MemberExpression)mce.Object;
            QueryAppend("regex({0}, \"^{1}\") ", "?" + memberExpression.Member.Name, constantExpression.Value);
        }

        private void GenerateRegexEndsWith(MethodCallExpression mce)
        {
            ConstantExpression constantExpression = (ConstantExpression)mce.Arguments[0];
            MemberExpression memberExpression = (MemberExpression)mce.Object;
            QueryAppend("regex({0}, \"{1}$\") ", "?" + memberExpression.Member.Name, constantExpression.Value);
        }

        /// <summary>
        /// Create ontology regex string comparison
        /// </summary>
        /// <param name="mce">the MethodCallExpression for ontology string.Compare</param>
        /// <remarks>
        /// <see cref="http://www.w3.org/TR/xpath-functions/#regex-syntax"/> for acceptable regex syntax
        /// <see cref="http://www.w3.org/TR/xpath-functions/#func-matches"/> for usage hints
        /// Should produce ontology filter regex of the form <c>regex(?name, "^ali", "i")</c>.
        /// The <see cref="System.String.Compare"/> is case-sensitive and culture-insensitive
        /// </remarks>
        private void GenerateRegexComparison(MethodCallExpression mce)
        {
            ConstantExpression constantExpression = (ConstantExpression)mce.Arguments[0];
            MemberExpression memberExpression = (MemberExpression)mce.Object;
            QueryAppend("regex({0}, \"{1}\") ", "?" + memberExpression.Member.Name, constantExpression.Value.ToString());
        }

        private void GenerateRegex(MemberExpression memberExpression, ConstantExpression constantExpression)
        {
        }

        public void Modulo(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation Modulo not supported");
        }

        public void Multiply(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.Multiply);
            GenerateBinaryExpression(e, "*");
        }

        public void MultiplyChecked(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.MultiplyChecked);
            GenerateBinaryExpression(e, "*");
        }

        public void Negate(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation Negate not supported");
        }

        public void NotEqual(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.NotEqual);
            GenerateBinaryExpression(e, "!=");
        }

        public void New(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation New not supported");
        }

        public void NewArrayInit(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation NewArrayInit not supported");
        }

        public void NewArrayBounds(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation NewArrayBounds not supported");
        }

        public void Not(Expression e)
        {
            Debug.Assert(e is UnaryExpression && e.NodeType == ExpressionType.Not);
            UnaryExpression ue = (UnaryExpression)e;
            QueryAppend("!(");
            Dispatch(ue.Operand);
            QueryAppend(")");
        }

        public void Or(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.Or);
            GenerateBinaryExpression(e, "||");
        }

        public void OrElse(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.OrElse);
            GenerateBinaryExpression(e, "||");
        }

        public void Parameter(Expression e)
        {
            ParameterExpression pe = (ParameterExpression)e;
            stringBuilder.Append(pe.Name);
        }

        public void Quote(Expression e)
        {
            UnaryExpression q = (UnaryExpression)e;
            QueryAppend("\"");
            Dispatch(q.Operand);
            QueryAppend("\"");
        }

        public void RightShift(Expression e)
        {
            #region Tracing
#line hidden
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("unsupported feature requested.");
            }
#line default
            #endregion
            throw new NotSupportedException("operation RShift not supported");
        }

        public void Subtract(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.Subtract);
            GenerateBinaryExpression(e, "-");
        }

        public void SubtractChecked(Expression e)
        {
            Debug.Assert(e is BinaryExpression && e.NodeType == ExpressionType.SubtractChecked);
            GenerateBinaryExpression(e, "-");
        }

        #endregion
    }
}
