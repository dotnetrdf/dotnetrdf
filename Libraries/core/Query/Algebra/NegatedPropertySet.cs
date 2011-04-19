using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    public class NegatedPropertySet : ISparqlAlgebra
    {
        private List<INode> _properties = new List<INode>();
        private PatternItem _start, _end;
        private bool _inverse;

        public NegatedPropertySet(PatternItem start, PatternItem end, IEnumerable<Property> properties, bool inverse)
        {
            this._start = start;
            this._end = end;
            this._properties.AddRange(properties.Select(p => p.Predicate));
            this._inverse = inverse;
        }

        public NegatedPropertySet(PatternItem start, PatternItem end, IEnumerable<Property> properties)
            : this(start, end, properties, false) { }

        public PatternItem PathStart
        {
            get
            {
                return this._start;
            }
        }

        public PatternItem PathEnd
        {
            get
            {
                return this._end;
            }
        }

        public IEnumerable<INode> Properties
        {
            get
            {
                return this._properties;
            }
        }

        #region ISparqlAlgebra Members

        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            IEnumerable<Triple> ts;
            String subjVar = this._start.VariableName;
            String objVar = this._end.VariableName;
            if (subjVar != null && context.InputMultiset.ContainsVariable(subjVar))
            {
                if (objVar != null && context.InputMultiset.ContainsVariable(objVar))
                {
                    ts = (from s in context.InputMultiset.Sets
                          where s[subjVar] != null && s[objVar] != null
                          from t in context.Data.GetTriplesWithSubjectObject(s[subjVar], s[objVar])
                          select t);
                }
                else
                {
                    ts = (from s in context.InputMultiset.Sets
                          where s[subjVar] != null
                          from t in context.Data.GetTriplesWithSubject(s[subjVar])
                          select t);
                }
            }
            else if (objVar != null && context.InputMultiset.ContainsVariable(objVar))
            {
                ts = (from s in context.InputMultiset.Sets
                      where s[objVar] != null
                      from t in context.Data.GetTriplesWithObject(s[objVar])
                      select t);
            }
            else
            {
                ts = context.Data.Triples;
            }

            context.OutputMultiset = new Multiset();
            if (this._inverse)
            {
                String temp = objVar;
                objVar = subjVar;
                subjVar = temp;
            }
            foreach (Triple t in ts)
            {
                if (!this._properties.Contains(t.Predicate))
                {
                    Set s = new Set();
                    if (subjVar != null) s.Add(subjVar, t.Subject);
                    if (objVar != null) s.Add(objVar, t.Object);
                    context.OutputMultiset.Add(s);
                }
            }

            if (subjVar == null && objVar == null)
            {
                if (context.OutputMultiset.Count == 0)
                {
                    context.OutputMultiset = new NullMultiset();
                }
                else
                {
                    context.OutputMultiset = new IdentityMultiset();
                }
            }

            return context.OutputMultiset;
        }

        public IEnumerable<string> Variables
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public SparqlQuery ToQuery()
        {
            throw new NotImplementedException();
        }

        public GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp;
            if (this._inverse)
            {
                pp = new PropertyPathPattern(this.PathStart, new NegatedSet(Enumerable.Empty<Property>(), this._properties.Select(p => new Property(p))), this.PathEnd);
            }
            else
            {
                pp = new PropertyPathPattern(this.PathStart, new NegatedSet(this._properties.Select(p => new Property(p)), Enumerable.Empty<Property>()), this.PathEnd);
            }
            gp.AddTriplePattern(pp);
            return gp;
        }

        #endregion

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("NegatedPropertySet(");
            output.Append(this._start.ToString());
            output.Append(", {");
            for (int i = 0; i < this._properties.Count; i++)
            {
                output.Append(this._properties[i].ToString());
                if (i < this._properties.Count - 1)
                {
                    output.Append(", ");
                }
            }
            output.Append("}, ");
            output.Append(this._end.ToString());
            output.Append(')');

            return output.ToString();
        }
    }
}
