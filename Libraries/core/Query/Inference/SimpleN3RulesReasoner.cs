using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Validation;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Inference
{
    public class SimpleN3RulesReasoner : IInferenceEngine
    {
        private List<String[]> _rules = new List<String[]>();
        private SparqlUpdateValidator _validator = new SparqlUpdateValidator();
        private SparqlFormatter _formatter = new SparqlFormatter();

        public void Apply(IGraph g)
        {
            this.Apply(g, g);
        }

        public void Apply(IGraph input, IGraph output)
        {
            TripleStore store = new TripleStore();
            store.Add(input);
            if (!ReferenceEquals(input, output))
            {
                store.Add(output);
            }

            //Apply each rule in turn
            foreach (String[] rule in this._rules)
            {
                //Build the final version of the rule text for the given input and output
                StringBuilder ruleText = new StringBuilder();

                //If there's a Base URI on the Output Graph need a WITH clause
                if (output.BaseUri != null)
                {
                    ruleText.AppendLine("WITH <" + this._formatter.FormatUri(output.BaseUri) + ">");
                }
                ruleText.AppendLine(rule[0]);
                //If there's a Base URI on the Input Graph need a USING clause
                if (input.BaseUri != null)
                {
                    ruleText.AppendLine("USING <" + this._formatter.FormatUri(input.BaseUri) + ">");
                }
                ruleText.AppendLine(rule[1]);

                ISyntaxValidationResults results = this._validator.Validate(ruleText.ToString());
                if (results.IsValid)
                {
                    store.ExecuteUpdate((SparqlUpdateCommandSet)results.Result);
                }
            }
        }

        public void Initialise(IGraph g)
        {
            INode implies = g.CreateUriNode(new Uri("http://www.w3.org/2000/10/swap/log#implies"));

            foreach (Triple t in g.GetTriplesWithPredicate(implies))
            {
                if (t.Subject.NodeType == NodeType.GraphLiteral && t.Object.NodeType == NodeType.GraphLiteral)
                {
                    this.TryCreateRule(t);
                }
            }
        }

        private void TryCreateRule(Triple t)
        {
            String[] rule = new String[2];
            Dictionary<INode, INode> variableMap = new Dictionary<INode, INode>();
            int nextVarID = 1;
            VariableContext vars = null;
            if (t.Context != null && t.Context is VariableContext) vars = (VariableContext)t.Context;

            StringBuilder output = new StringBuilder();

            //Generate the INSERT part of the Command
            output.AppendLine("INSERT");
            output.AppendLine("{");
            foreach (Triple x in ((GraphLiteralNode)t.Object).SubGraph.Triples)
            {
                if (vars == null)
                {
                    output.AppendLine(this._formatter.Format(x));
                }
                else
                {
                    if (vars.IsVariable(x.Subject))
                    {
                        if (!variableMap.ContainsKey(x.Subject))
                        {
                            variableMap.Add(x.Subject, new VariableNode(null, "auto-rule-var-" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(this._formatter.Format(variableMap[x.Subject]));
                    } 
                    else
                    {
                        output.Append(this._formatter.Format(x.Subject));
                    }
                    output.Append(' ');
                    if (vars.IsVariable(x.Predicate))
                    {
                        if (!variableMap.ContainsKey(x.Predicate))
                        {
                            variableMap.Add(x.Predicate, new VariableNode(null, "auto-rule-var-" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(this._formatter.Format(variableMap[x.Predicate]));
                    }
                    else
                    {
                        output.Append(this._formatter.Format(x.Predicate));
                    }
                    output.Append(' ');
                    if (vars.IsVariable(x.Object))
                    {
                        if (!variableMap.ContainsKey(x.Object))
                        {
                            variableMap.Add(x.Object, new VariableNode(null, "auto-rule-var-" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(this._formatter.Format(variableMap[x.Object]));
                    }
                    else
                    {
                        output.Append(this._formatter.Format(x.Object));
                    }
                    output.AppendLine(" .");
                }
            }
            output.AppendLine("}");
            rule[0] = output.ToString();

            //Generate the WHERE part of the Command
            output = new StringBuilder();
            output.AppendLine("WHERE");
            output.AppendLine("{");
            foreach (Triple x in ((GraphLiteralNode)t.Subject).SubGraph.Triples)
            {
                if (vars == null)
                {
                    output.AppendLine(this._formatter.Format(x));
                }
                else
                {
                    if (vars.IsVariable(x.Subject))
                    {
                        if (!variableMap.ContainsKey(x.Subject))
                        {
                            variableMap.Add(x.Subject, new VariableNode(null, "auto-rule-var-" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(this._formatter.Format(variableMap[x.Subject]));
                    } 
                    else
                    {
                        output.Append(this._formatter.Format(x.Subject));
                    }
                    output.Append(' ');
                    if (vars.IsVariable(x.Predicate))
                    {
                        if (!variableMap.ContainsKey(x.Predicate))
                        {
                            variableMap.Add(x.Predicate, new VariableNode(null, "auto-rule-var-" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(this._formatter.Format(variableMap[x.Predicate]));
                    }
                    else
                    {
                        output.Append(this._formatter.Format(x.Predicate));
                    }
                    output.Append(' ');
                    if (vars.IsVariable(x.Object))
                    {
                        if (!variableMap.ContainsKey(x.Object))
                        {
                            variableMap.Add(x.Object, new VariableNode(null, "auto-rule-var-" + nextVarID));
                            nextVarID++;
                        }
                        output.Append(this._formatter.Format(variableMap[x.Object]));
                    }
                    else
                    {
                        output.Append(this._formatter.Format(x.Object));
                    }
                    output.AppendLine(" .");
                }
            }
            output.AppendLine("}");
            rule[1] = output.ToString();
            
            ISyntaxValidationResults results = this._validator.Validate(rule[0] + rule[1]);
            if (results.IsValid)
            {
                this._rules.Add(rule);
            }
        }
    }
}
