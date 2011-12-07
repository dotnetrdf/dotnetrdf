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
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;

namespace VDS.RDF.Linq
{
    public static class FunctionalExtensions
    {
        /// <summary>
        /// Applies a function <see cref="f"/> to the sequence <see cref="seq"/> yielding a new sequence of <see cref="R"/>.
        /// </summary>
        /// <typeparam name="T">The type of the original sequence</typeparam>
        /// <typeparam name="R">the type of the new sequence</typeparam>
        /// <param name="seq">The sequence on which the function <see cref="f"/> will be applied.</param>
        /// <param name="f">The function to apply to each element of <see cref="seq"/>.</param>
        /// <returns>a new sequence containing the results of applying <see cref="f"/> to <see cref="seq"/></returns>
        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> seq, Func<T, R> f)
        {
            using (var ls = new LoggingScope("Map"))
            {
                foreach (T t in seq)
                    yield return f(t);
            }
        }

        public static Func<T, T> On<T>(this Func<T, T> inner, Func<T, T> outer)
        {
            using (var ls = new LoggingScope("On"))
            {
                return t => outer(inner(t));
            }
        }

        public static void ForEach<T>(this IEnumerable<T> seq, Action<T> task)
        {
            using (var ls = new LoggingScope("ForEach"))
            {
                foreach (T t in seq)
                {
                    task(t);
                }
            }
        }

        public static string NodeTypeName(this Expression ex)
        {
            return Enum.GetName(ex.NodeType.GetType(), ex.NodeType);
        }

        public static bool ContainsAnyOf<T>(this IEnumerable<T> seq, IEnumerable<T> x)
        {
            using (var ls = new LoggingScope("ContainsAnyOf"))
            {
                bool result = false;
                x.ForEach(a => result |= seq.Contains(a));
                return result;
            }
        }

        public static bool OccursAsStmtObjectWithUri<T>(this EntitySet<T> set, string Uri) where T : class
        {
            return true;
        }

        public static bool OccursAsStmtObjectWithUri(this OwlInstanceSupertype set, string Uri)
        {
            return true;
        }

        public static bool StmtObjectWithSubjectAndPredicate(this OwlInstanceSupertype set, string subjectUri,
                                                             string predicateUri)
        {
            return true;
        }

        public static bool StmtSubjectWithObjectAndPredicate(this OwlInstanceSupertype set, string objectUri,
                                                             string predicateUri)
        {
            return true;
        }
    }
}