using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Linq.Sparql
{
    public static class LinqToSparqlExtensions
    {
        public static LinqToSparqlQuery<T> Reduced<T>(this LinqToSparqlQuery<T> q)
        {
            LinqToSparqlQuery<T> temp = q.CloneQueryForNewType<T>();
            if (!temp.Expressions.ContainsKey("Reduced"))
            {
                temp.Expressions.Add("Reduced", null);
            }
            return temp;
        }

        public static IQueryable<T> Reduced<T>(this IQueryable<T> q)
        {
            if (q is LinqToSparqlQuery<T>)
            {
                return ((LinqToSparqlQuery<T>)q).Reduced<T>();
            }
            else
            {
                return q.Distinct().AsQueryable<T>();
            }
        }

        public static IEnumerable<T> Reduced<T>(this IEnumerable<T> e)
        {
            return e.AsQueryable<T>().Reduced<T>();
        }
    }
}
