using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class GroupBy
        : BaseUnaryAlgebra
    {
        public GroupBy(IAlgebra innerAlgebra, IEnumerable<KeyValuePair<IExpression, String>> groupExpressions, IEnumerable<KeyValuePair<IAggregateExpression, String>> aggregators)
            : base(innerAlgebra)
        {
            if (aggregators == null) throw new ArgumentNullException("aggregators");
            this.GroupExpressions = groupExpressions != null ? groupExpressions.ToList().AsReadOnly() : new List<KeyValuePair<IExpression, string>>().AsReadOnly();
            this.Aggregators = aggregators.ToList().AsReadOnly();

            if (this.GroupExpressions.Count == 0 && this.Aggregators.Count == 0) throw new ArgumentException("Must provide at least one group expression or aggregator");
        }

        public GroupBy(IAlgebra algebra, IEnumerable<KeyValuePair<IAggregateExpression, string>> aggregators)
            : this(algebra, null, aggregators) { }

        public IList<KeyValuePair<IExpression, string>> GroupExpressions { get; private set; }

        public IList<KeyValuePair<IAggregateExpression, String>> Aggregators { get; private set; }

        public override IAlgebra Copy(IAlgebra innerAlgebra)
        {
            return new GroupBy(innerAlgebra, this.GroupExpressions, this.Aggregators);
        }

        public override IEnumerable<string> ProjectedVariables
        {
            get { return base.ProjectedVariables; }
        }

        public override IEnumerable<string> FixedVariables
        {
            get { return base.FixedVariables; }
        }

        public override IEnumerable<string> FloatingVariables
        {
            get { return base.FloatingVariables; }
        }

        public override void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");

            StringBuilder builder = new StringBuilder();
            builder.Append("(group (");
            for (int i = 0; i < this.GroupExpressions.Count; i++)
            {
                if (i > 0) builder.Append(' ');
                builder.Append(this.GroupExpressions[i].Key.ToPrefixString(formatter));
            }
            builder.Append(") (");
            for (int i = 0; i < this.Aggregators.Count; i++)
            {
                if (i > 0) builder.Append(' ');
                builder.Append('(');
                builder.Append(formatter.Format(new VariableNode(this.Aggregators[i].Value)));
                builder.Append(" (");
                builder.Append(this.Aggregators[i].Key.ToPrefixString(formatter));
                builder.Append(")");
            }
            builder.AppendLine(")");
            builder.AppendLineIndented(this.InnerAlgebra.ToString(formatter), 2);
            builder.AppendLine(")");
            return builder.ToString();
        }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is GroupBy)) return false;

            GroupBy groupBy = (GroupBy) other;
            if (this.GroupExpressions.Count != groupBy.GroupExpressions.Count) return false;
            if (this.Aggregators.Count != groupBy.Aggregators.Count) return false;

            for (int i = 0; i < this.GroupExpressions.Count; i++)
            {
                if (!this.GroupExpressions[i].Equals(groupBy.GroupExpressions[i])) return false;
            }
            for (int i = 0; i < this.Aggregators.Count; i++)
            {
                if (!this.Aggregators[i].Equals(groupBy.Aggregators[i])) return false;
            }

            return this.InnerAlgebra.Equals(groupBy.InnerAlgebra);
        }
    }
}
