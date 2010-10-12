using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents the SPARQL Update INSERT DATA command
    /// </summary>
    public class InsertDataCommand : SparqlUpdateCommand
    {
        private GraphPattern _pattern;

        /// <summary>
        /// Creates a new INSERT DATA command
        /// </summary>
        /// <param name="pattern">Pattern containing concrete Triples to insert</param>
        public InsertDataCommand(GraphPattern pattern)
            : base(SparqlUpdateCommandType.InsertData) 
        {
            if (!pattern.TriplePatterns.All(p => p is IConstructTriplePattern && ((IConstructTriplePattern)p).HasNoExplicitVariables)) throw new SparqlUpdateException("Cannot create a INSERT DATA command where any of the Triple Patterns are not concrete triples - variables are not permitted");
            this._pattern = pattern;
        }

        /// <summary>
        /// Gets the Data Pattern containing Triples to insert
        /// </summary>
        public GraphPattern DataPattern
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            if (!this._pattern.TriplePatterns.All(p => p is IConstructTriplePattern && ((IConstructTriplePattern)p).HasNoExplicitVariables)) throw new SparqlUpdateException("Cannot evaluate a INSERT DATA command where any of the Triple Patterns are not concrete triples - variables are not permitted");

            //Get the Target Graph
            IGraph target;
            Uri graphUri;
            if (this._pattern.IsGraph)
            {
                switch (this._pattern.GraphSpecifier.TokenType)
                {
                    case Token.QNAME:
                        throw new NotImplementedException("Graph Specifiers as QNames for INSERT DATA Commands are not supported - please specify an absolute URI instead");
                    case Token.URI:
                        graphUri = new Uri(this._pattern.GraphSpecifier.Value);
                        break;
                    default:
                        throw new SparqlUpdateException("Cannot evaluate an INSERT DATA Command as the Graph Specifier is not a QName/URI");
                }
            }
            else
            {
                graphUri = null;
            }
            if (context.Data.HasGraph(graphUri))
            {
                target = context.Data.GetModifiableGraph(graphUri);
            }
            else
            {
                //If the Graph does not exist then it must be created
                target = new Graph();
                target.BaseUri = graphUri;
                context.Data.AddGraph(target);
            }

            //Insert the actual Triples
            INode subj, pred, obj;
            ConstructContext constructContext = new ConstructContext(target, null, true);
            foreach (IConstructTriplePattern p in this._pattern.TriplePatterns.OfType<IConstructTriplePattern>())
            {
                subj = p.Subject.Construct(constructContext);//((NodeMatchPattern)tp.Subject).Node.CopyNode(target);
                pred = p.Predicate.Construct(constructContext);//((NodeMatchPattern)tp.Predicate).Node.CopyNode(target);
                obj = p.Object.Construct(constructContext);//((NodeMatchPattern)tp.Object).Node.CopyNode(target);

                target.Assert(new Triple(subj, pred, obj));
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessInsertDataCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("INSERT DATA {");
            output.AppendLine(this._pattern.ToString());
            output.AppendLine("}");
            return output.ToString();
        }
    }
}
