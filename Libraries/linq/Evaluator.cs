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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

/*
 * Taken from Matt Warren's Blog
 */

namespace VDS.RDF.Linq
{
    public static class Evaluator
    {
        static Logger Logger  = new Logger(typeof(Evaluator));
        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            using (var ls = new LoggingScope("Evaluator.PartialEval3"))
            {
                return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
            }
        }


        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression expression)
        {
            using (var ls = new LoggingScope("Evaluator.PartialEval1"))
            {
            return PartialEval(expression, CanBeEvaluatedLocally);
            }
        }


        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }

        #region Nested type: Nominator

        /// <summary>
        /// Performs bottom-up analysis to determine which nodes can possibly
        /// be part of an evaluated sub-tree.
        /// </summary>
        private class Nominator : ExpressionVisitor
        {
            private readonly Func<Expression, bool> fnCanBeEvaluated;

            private HashSet<Expression> candidates;

            private bool cannotBeEvaluated;


            internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
            {
                this.fnCanBeEvaluated = fnCanBeEvaluated;
            }


            internal HashSet<Expression> Nominate(Expression expression)
            {
                candidates = new HashSet<Expression>();

                Visit(expression);

                return candidates;
            }


            protected override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveCannotBeEvaluated = cannotBeEvaluated;

                    cannotBeEvaluated = false;

                    base.Visit(expression);

                    if (!cannotBeEvaluated)
                    {
                        if (fnCanBeEvaluated(expression))
                        {
                            candidates.Add(expression);
                        }

                        else
                        {
                            cannotBeEvaluated = true;
                        }
                    }

                    cannotBeEvaluated |= saveCannotBeEvaluated;
                }

                return expression;
            }
        }

        #endregion

        #region Nested type: SubtreeEvaluator

        /// <summary>
        /// Evaluates & replaces sub-trees when first candidate is reached (top-down)
        /// </summary>
        private class SubtreeEvaluator : ExpressionVisitor
        {
            private readonly HashSet<Expression> candidates;


            internal SubtreeEvaluator(HashSet<Expression> candidates)
            {
                this.candidates = candidates;
            }


            internal Expression Eval(Expression exp)
            {
                return Visit(exp);
            }


            protected override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }

                if (candidates.Contains(exp))
                {
                    return Evaluate(exp);
                }

                return base.Visit(exp);
            }


            private Expression Evaluate(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant)
                {
                    return e;
                }

                LambdaExpression lambda = Expression.Lambda(e);

                Delegate fn = lambda.Compile();

                return Expression.Constant(fn.DynamicInvoke(null), e.Type);
            }
        }

        #endregion
    }

    public abstract class ExpressionVisitor
    {
        protected virtual Expression Visit(Expression exp)
        {
            if (exp == null)

                return exp;

            switch (exp.NodeType)
            {
                case ExpressionType.Negate:

                case ExpressionType.NegateChecked:

                case ExpressionType.Not:

                case ExpressionType.Convert:

                case ExpressionType.ConvertChecked:

                case ExpressionType.ArrayLength:

                case ExpressionType.Quote:

                case ExpressionType.TypeAs:

                    return VisitUnary((UnaryExpression) exp);

                case ExpressionType.Add:

                case ExpressionType.AddChecked:

                case ExpressionType.Subtract:

                case ExpressionType.SubtractChecked:

                case ExpressionType.Multiply:

                case ExpressionType.MultiplyChecked:

                case ExpressionType.Divide:

                case ExpressionType.Modulo:

                case ExpressionType.And:

                case ExpressionType.AndAlso:

                case ExpressionType.Or:

                case ExpressionType.OrElse:

                case ExpressionType.LessThan:

                case ExpressionType.LessThanOrEqual:

                case ExpressionType.GreaterThan:

                case ExpressionType.GreaterThanOrEqual:

                case ExpressionType.Equal:

                case ExpressionType.NotEqual:

                case ExpressionType.Coalesce:

                case ExpressionType.ArrayIndex:

                case ExpressionType.RightShift:

                case ExpressionType.LeftShift:

                case ExpressionType.ExclusiveOr:

                    return VisitBinary((BinaryExpression) exp);

                case ExpressionType.TypeIs:

                    return VisitTypeIs((TypeBinaryExpression) exp);

                case ExpressionType.Conditional:

                    return VisitConditional((ConditionalExpression) exp);

                case ExpressionType.Constant:

                    return VisitConstant((ConstantExpression) exp);

                case ExpressionType.Parameter:

                    return VisitParameter((ParameterExpression) exp);

                case ExpressionType.MemberAccess:

                    return VisitMemberAccess((MemberExpression) exp);

                case ExpressionType.Call:

                    return VisitMethodCall((MethodCallExpression) exp);

                case ExpressionType.Lambda:

                    return VisitLambda((LambdaExpression) exp);

                case ExpressionType.New:

                    return VisitNew((NewExpression) exp);

                case ExpressionType.NewArrayInit:

                case ExpressionType.NewArrayBounds:

                    return VisitNewArray((NewArrayExpression) exp);

                case ExpressionType.Invoke:

                    return VisitInvocation((InvocationExpression) exp);

                case ExpressionType.MemberInit:

                    return VisitMemberInit((MemberInitExpression) exp);

                case ExpressionType.ListInit:

                    return VisitListInit((ListInitExpression) exp);

                default:

                    throw new LinqToRdfException(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }


        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:

                    return VisitMemberAssignment((MemberAssignment) binding);

                case MemberBindingType.MemberBinding:

                    return VisitMemberMemberBinding((MemberMemberBinding) binding);

                case MemberBindingType.ListBinding:

                    return VisitMemberListBinding((MemberListBinding) binding);

                default:

                    throw new LinqToRdfException(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }


        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<Expression> arguments = VisitExpressionList(initializer.Arguments);

            if (arguments != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }

            return initializer;
        }


        protected virtual Expression VisitUnary(UnaryExpression u)
        {
            Expression operand = Visit(u.Operand);

            if (operand != u.Operand)
            {
                return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }

            return u;
        }


        protected virtual Expression VisitBinary(BinaryExpression b)
        {
            Expression left = Visit(b.Left);

            Expression right = Visit(b.Right);

            Expression conversion = Visit(b.Conversion);

            if (left != b.Left || right != b.Right || conversion != b.Conversion)
            {
                if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)

                    return Expression.Coalesce(left, right, conversion as LambdaExpression);

                return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
            }

            return b;
        }


        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            Expression expr = Visit(b.Expression);

            if (expr != b.Expression)
            {
                return Expression.TypeIs(expr, b.TypeOperand);
            }

            return b;
        }


        protected virtual Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }


        protected virtual Expression VisitConditional(ConditionalExpression c)
        {
            Expression test = Visit(c.Test);

            Expression ifTrue = Visit(c.IfTrue);

            Expression ifFalse = Visit(c.IfFalse);

            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }

            return c;
        }


        protected virtual Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }


        protected virtual Expression VisitMemberAccess(MemberExpression m)
        {
            Expression exp = Visit(m.Expression);

            if (exp != m.Expression)
            {
                return Expression.MakeMemberAccess(exp, m.Member);
            }

            return m;
        }


        protected virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression obj = Visit(m.Object);

            IEnumerable<Expression> args = VisitExpressionList(m.Arguments);

            if (obj != m.Object || args != m.Arguments)
            {
                return Expression.Call(obj, m.Method, args);
            }

            return m;
        }


        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> list = null;

            for (int i = 0, n = original.Count; i < n; i++)
            {
                Expression p = Visit(original[i]);

                if (list != null)
                {
                    list.Add(p);
                }

                else if (p != original[i])
                {
                    list = new List<Expression>(n);

                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }

                    list.Add(p);
                }
            }

            if (list != null)
            {
                return list.AsReadOnly();
            }

            return original;
        }


        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression e = Visit(assignment.Expression);

            if (e != assignment.Expression)
            {
                return Expression.Bind(assignment.Member, e);
            }

            return assignment;
        }


        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = VisitBindingList(binding.Bindings);

            if (bindings != binding.Bindings)
            {
                return Expression.MemberBind(binding.Member, bindings);
            }

            return binding;
        }


        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(binding.Initializers);

            if (initializers != binding.Initializers)
            {
                return Expression.ListBind(binding.Member, initializers);
            }

            return binding;
        }


        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;

            for (int i = 0, n = original.Count; i < n; i++)
            {
                MemberBinding b = VisitBinding(original[i]);

                if (list != null)
                {
                    list.Add(b);
                }

                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);

                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }

                    list.Add(b);
                }
            }

            if (list != null)

                return list;

            return original;
        }


        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;

            for (int i = 0, n = original.Count; i < n; i++)
            {
                ElementInit init = VisitElementInitializer(original[i]);

                if (list != null)
                {
                    list.Add(init);
                }

                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);

                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }

                    list.Add(init);
                }
            }

            if (list != null)

                return list;

            return original;
        }


        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body = Visit(lambda.Body);

            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }

            return lambda;
        }


        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<Expression> args = VisitExpressionList(nex.Arguments);

            if (args != nex.Arguments)
            {
                if (nex.Members != null)

                    return Expression.New(nex.Constructor, args, nex.Members);

                return Expression.New(nex.Constructor, args);
            }

            return nex;
        }


        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);

            IEnumerable<MemberBinding> bindings = VisitBindingList(init.Bindings);

            if (n != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(n, bindings);
            }

            return init;
        }


        protected virtual Expression VisitListInit(ListInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);

            IEnumerable<ElementInit> initializers = VisitElementInitializerList(init.Initializers);

            if (n != init.NewExpression || initializers != init.Initializers)
            {
                return Expression.ListInit(n, initializers);
            }

            return init;
        }


        protected virtual Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> exprs = VisitExpressionList(na.Expressions);

            if (exprs != na.Expressions)
            {
                if (na.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(na.Type.GetElementType(), exprs);
                }

                return Expression.NewArrayBounds(na.Type.GetElementType(), exprs);
            }

            return na;
        }


        protected virtual Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> args = VisitExpressionList(iv.Arguments);

            Expression expr = Visit(iv.Expression);

            if (args != iv.Arguments || expr != iv.Expression)
            {
                return Expression.Invoke(expr, args);
            }

            return iv;
        }
    }
}