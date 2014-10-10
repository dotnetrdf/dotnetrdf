using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Collections;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Grouping;

namespace VDS.RDF.Query.Engine.Grouping
{
    public class GroupByEnumerable
        : WrapperEnumerable<ISolution>
    {
        public GroupByEnumerable(IEnumerable<ISolution> enumerable, IEnumerable<KeyValuePair<IExpression, String>> groupExpressions, IEnumerable<KeyValuePair<IAggregateExpression, String>> aggregators, IExecutionContext context)
            : base(enumerable)
        {
            if (aggregators == null) throw new ArgumentNullException("aggregators");
            if (context == null) throw new ArgumentNullException("context");

            this.GroupExpressions = groupExpressions != null ? groupExpressions.ToList().AsReadOnly() : new List<KeyValuePair<IExpression, string>>().AsReadOnly();
            this.Aggregators = aggregators.ToList().AsReadOnly();

            if (this.GroupExpressions.Count == 0 && this.Aggregators.Count == 0) throw new ArgumentException("Must provide at least one group expression or aggregator");

            this.ExecutionContext = context;
        }

        private IList<KeyValuePair<IExpression, string>> GroupExpressions { get; set; }

        private IList<KeyValuePair<IAggregateExpression, String>> Aggregators { get; set; }

        private IExecutionContext ExecutionContext { get; set; }

        public override IEnumerator<ISolution> GetEnumerator()
        {
            return new GroupByEnumerator(this.InnerEnumerable.GetEnumerator(), this.GroupExpressions, this.Aggregators, this.ExecutionContext);
        }
    }

    public class GroupByEnumerator
        : WrapperEnumerator<ISolution>
    {
        public GroupByEnumerator(IEnumerator<ISolution> enumerator, IEnumerable<KeyValuePair<IExpression, String>> groupExpressions, IEnumerable<KeyValuePair<IAggregateExpression, String>> aggregators, IExecutionContext context)
            : base(enumerator)
        {
            if (aggregators == null) throw new ArgumentNullException("aggregators");
            if (context == null) throw new ArgumentNullException("context");

            this.GroupExpressions = groupExpressions != null ? groupExpressions.ToList().AsReadOnly() : new List<KeyValuePair<IExpression, string>>().AsReadOnly();
            this.Aggregators = aggregators.ToList().AsReadOnly();

            if (this.GroupExpressions.Count == 0 && this.Aggregators.Count == 0) throw new ArgumentException("Must provide at least one group expression or aggregator");

            this.ExecutionContext = context;
        }

        private IEnumerator<KeyValuePair<ISolution, ISolutionGroup>> Groups { get; set; }

        private IList<KeyValuePair<IExpression, string>> GroupExpressions { get; set; }

        private IList<KeyValuePair<IAggregateExpression, String>> Aggregators { get; set; }

        private IExecutionContext ExecutionContext { get; set; }

        protected override void ResetInternal()
        {
            this.Groups.Dispose();
            this.Groups = null;
        }

        protected override bool TryMoveNext(out ISolution item)
        {
            if (this.Groups == null)
            {
                this.CollectGroups();
            }

            item = null;
            if (this.Groups.MoveNext())
            {
                item = this.Groups.Current.Value;
                return true;
            }
            return false;
        }

        private void CollectGroups()
        {
            IDictionary<ISolution, ISolutionGroup> groups = new Dictionary<ISolution, ISolutionGroup>(new GroupKeyComparer(this.CreateKeyVariables()));

            // Collect and group the solutions
            while (this.InnerEnumerator.MoveNext())
            {
                ISolution next = this.InnerEnumerator.Current;
                ISolution key = this.CreateKey(next);

                ISolutionGroup group;
                if (!groups.TryGetValue(key, out group))
                {
                    group = new SolutionGroup(key, this.PrepareAggregates());
                    groups.Add(key, group);
                }

                IExpressionContext context = this.ExecutionContext.CreateExpressionContext();
                foreach (IAccumulator accumulator in group.Accumulators.Select(kvp => kvp.Value))
                {
                    accumulator.Accumulate(next, context);
                }
            }

            // Finalize the groups
            foreach (ISolutionGroup group in groups.Values)
            {
                group.FinalizeGroup();
            }

            this.Groups = groups.GetEnumerator();
        }

        private IEnumerable<String> CreateKeyVariables()
        {
            if (this.GroupExpressions.Count == 0) return Enumerable.Empty<String>();

            List<String> keyVars = new List<string>();
            for (int i = 0; i < this.GroupExpressions.Count; i++)
            {
                keyVars.Add(this.GroupExpressions[i].Value ?? ".key" + i);
            }
            return keyVars;
        }

        private ISolution CreateKey(ISolution solution)
        {
            ISolution key = new Solution();
            IExpressionContext context = this.ExecutionContext.CreateExpressionContext();
            foreach (KeyValuePair<IExpression, String> kvp in this.GroupExpressions)
            {
                try
                {
                    IValuedNode n = kvp.Key.Evaluate(solution, context);
                    key.Add(kvp.Value, n);
                }
                catch (RdfQueryException)
                {
                    break;
                }
            }
            return key;
        }

        private IEnumerable<KeyValuePair<String, IAccumulator>> PrepareAggregates()
        {
            return this.Aggregators.Select(kvp => new KeyValuePair<string, IAccumulator>(kvp.Value, kvp.Key.CreateAccumulator()));
        }
    }
}