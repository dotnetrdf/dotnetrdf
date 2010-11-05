using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;
using dotSesameQuery = org.openrdf.query;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Storage;

namespace VDS.RDF.Interop.Sesame
{
    public abstract class ProcessorGraphQuery : BaseGraphQuery
    {
        private ISparqlQueryProcessor _processor;

        public ProcessorGraphQuery(String query, ISparqlQueryProcessor processor)
            : base(query)
        {
            this._processor = processor;
        }

        protected override IGraph EvaluateQuery()
        {
            try
            {
                SparqlQuery q = BaseQuery.ParseQuery(this.SparqlQuery);
                if (this.QueryTimeout > 0) q.Timeout = (long)this.QueryTimeout;
                Object results = this._processor.ProcessQuery(q);
                if (results is IGraph)
                {
                    if (q.QueryTime > this._evalTime) this._evalTime = q.QueryTime;
                    return (IGraph)results;
                }
                else
                {
                    throw new dotSesameQuery.QueryEvaluationException("Query Evaluation resulted in an unexpected Result");
                }
            }
            catch (RdfParseException parseEx)
            {
                throw new dotSesameQuery.QueryEvaluationException("Unable to evaluate the Query as the given Query did not parse into a valid SPARQL Query due to the following error: " + parseEx.Message);
            }
            catch (RdfQueryTimeoutException)
            {
                throw new dotSesameQuery.QueryInterruptedException("Query exceeded the set Timeout of " + this.getMaxQueryTime() + " Seconds");
            }
            catch (RdfQueryException queryEx)
            {
                throw new dotSesameQuery.QueryEvaluationException("Query Evaluation failed due to the following error: " + queryEx.Message);
            }
            catch (Exception ex)
            {
                throw new dotSesameQuery.QueryEvaluationException("Query Evaluation failed due to an unexpected error: " + ex.Message);
            }
        }
    }

    public class InMemoryGraphQuery : ProcessorGraphQuery
    {
        private LeviathanQueryProcessor _processor;

        public InMemoryGraphQuery(String query, IInMemoryQueryableStore store)
            : base(query, new LeviathanQueryProcessor(store))  { }
    }

    public class GenericGraphQuery : ProcessorGraphQuery
    {
        public GenericGraphQuery(String query, IQueryableGenericIOManager manager)
            : base(query, new GenericQueryProcessor(manager)) { }
    }

    public abstract class ProcessorBooleanQuery : BaseBooleanQuery
    {
        private ISparqlQueryProcessor _processor;

        public ProcessorBooleanQuery(String query, ISparqlQueryProcessor processor)
            : base(query)
        {
            this._processor = processor;
        }

        protected override SparqlResultSet EvaluateQuery()
        {
            try
            {
                SparqlQuery q = BaseQuery.ParseQuery(this.SparqlQuery);
                if (this.QueryTimeout > 0) q.Timeout = (long)this.QueryTimeout;
                Object results = this._processor.ProcessQuery(q);
                if (results is SparqlResultSet)
                {
                    if (q.QueryTime > this._evalTime) this._evalTime = q.QueryTime;
                    return (SparqlResultSet)results;
                }
                else
                {
                    throw new dotSesameQuery.QueryEvaluationException("Query Evaluation resulted in an unexpected Result");
                }
            }
            catch (RdfParseException parseEx)
            {
                throw new dotSesameQuery.QueryEvaluationException("Unable to evaluate the Query as the given Query did not parse into a valid SPARQL Query due to the following error: " + parseEx.Message);
            }
            catch (RdfQueryTimeoutException)
            {
                throw new dotSesameQuery.QueryInterruptedException("Query exceeded the set Timeout of " + this.getMaxQueryTime() + " Seconds");
            }
            catch (RdfQueryException queryEx)
            {
                throw new dotSesameQuery.QueryEvaluationException("Query Evaluation failed due to the following error: " + queryEx.Message);
            }
            catch (Exception ex)
            {
                throw new dotSesameQuery.QueryEvaluationException("Query Evaluation failed due to an unexpected error: " + ex.Message);
            }
        }
    }

    public class InMemoryBooleanQuery : ProcessorBooleanQuery
    {
        public InMemoryBooleanQuery(String query, IInMemoryQueryableStore store)
            : base(query, new LeviathanQueryProcessor(store)) { }
    }

    public class GenericBooleanQuery : ProcessorBooleanQuery
    {
        public GenericBooleanQuery(String query, IQueryableGenericIOManager manager)
            : base(query, new GenericQueryProcessor(manager)) { }
    }

    public class ProcessorTupleQuery : BaseTupleQuery
    {
        private ISparqlQueryProcessor _processor;

        public ProcessorTupleQuery(String query, ISparqlQueryProcessor processor)
            : base(query)
        {
            this._processor = processor;
        }

        protected override SparqlResultSet EvaluateQuery()
        {
            try
            {
                SparqlQuery q = BaseQuery.ParseQuery(this.SparqlQuery);
                if (this.QueryTimeout > 0) q.Timeout = (long)this.QueryTimeout;
                Object results = this._processor.ProcessQuery(q);
                if (results is SparqlResultSet)
                {
                    if (q.QueryTime > this._evalTime) this._evalTime = q.QueryTime;
                    return (SparqlResultSet)results;
                }
                else
                {
                    throw new dotSesameQuery.QueryEvaluationException("Query Evaluation resulted in an unexpected Result");
                }
            }
            catch (RdfParseException parseEx)
            {
                throw new dotSesameQuery.QueryEvaluationException("Unable to evaluate the Query as the given Query did not parse into a valid SPARQL Query due to the following error: " + parseEx.Message);
            }
            catch (RdfQueryTimeoutException)
            {
                throw new dotSesameQuery.QueryInterruptedException("Query exceeded the set Timeout of " + this.getMaxQueryTime() + " Seconds");
            }
            catch (RdfQueryException queryEx)
            {
                throw new dotSesameQuery.QueryEvaluationException("Query Evaluation failed due to the following error: " + queryEx.Message);
            }
            catch (Exception ex)
            {
                throw new dotSesameQuery.QueryEvaluationException("Query Evaluation failed due to an unexpected error: " + ex.Message);
            }
        }
    }

    public class InMemoryTupleQuery : ProcessorTupleQuery
    {
        public InMemoryTupleQuery(String query, IInMemoryQueryableStore store)
            : base(query, new LeviathanQueryProcessor(store)) { }
    }

    public class GenericTupleQuery : ProcessorTupleQuery
    {
        public GenericTupleQuery(String query, IQueryableGenericIOManager manager)
            : base(query, new GenericQueryProcessor(manager)) { }
    }
}
