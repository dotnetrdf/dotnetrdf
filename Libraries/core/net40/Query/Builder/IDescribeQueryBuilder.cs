using System;

namespace VDS.RDF.Query.Builder
{
    public interface IDescribeQueryBuilder : IQueryBuilder
    {
        IDescribeQueryBuilder And(params string[] describeVariableNames);
        IDescribeQueryBuilder And(params Uri[] urisToDescribe);
    }
}