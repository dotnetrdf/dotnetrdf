/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;
using VDS.RDF.Query.FullText.Search;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// An Algebra Optimiser that converts relevant Triple Patterns into <see cref="FullTextMatch">FullTextMatch</see> operators
    /// </summary>
    public class FullTextOptimiser
        : IAlgebraOptimiser, IConfigurationSerializable
    {
        private IFullTextSearchProvider _provider;

        /// <summary>
        /// Creates a Full Text Optimiser
        /// </summary>
        /// <param name="provider">Full Text Search Provider</param>
        public FullTextOptimiser(IFullTextSearchProvider provider)
        {
            if (provider == null) throw new ArgumentNullException("Full Text Search Provider cannot be null");
            this._provider = provider;
        }

        /// <summary>
        /// Optimises the Algebra so that BGPs containing relevant patterns are converted to use the <see cref="FullTextMatch">FullTextMatch</see> operator
        /// </summary>
        /// <param name="algebra">Algebra to optimise</param>
        /// <returns></returns>
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            if (algebra is IAbstractJoin)
            {
                return ((IAbstractJoin)algebra).Transform(this);
            }
            else if (algebra is IUnaryOperator)
            {
                return ((IUnaryOperator)algebra).Transform(this);
            }
            else if (algebra is IBgp)
            {
                IBgp current = (IBgp)algebra;
                if (current.PatternCount == 0)
                {
                    return current;
                }
                else
                {
                    ISparqlAlgebra result = new Bgp();

                    List<FullTextPattern> fps = FullTextHelper.ExtractPatterns(current.TriplePatterns);
                    if (fps.Count == 0) return algebra;
                    List<ITriplePattern> ps = current.TriplePatterns.ToList();

                    //First we want to find where each FullTextPattern is located in the original triple patterns
                    Dictionary<int, int> locations = new Dictionary<int, int>();
                    for (int i = 0; i < fps.Count; i++)
                    {
                        locations.Add(i, -1);
                        ITriplePattern first = fps[i].OriginalPatterns.First();
                        for (int j = 0; j < ps.Count; j++)
                        {
                            if (first.Equals(ps[j]))
                            {
                                locations[i] = j;
                                break;
                            }
                        }
                        //If we fail to locate this we've failed to optimise here so must abort
                        if (locations[i] == -1) return algebra;
                    }

                    //Knowing this we can then start splitting the BGP into several BGPs
                    int locBase = 0;
                    foreach (int i in Enumerable.Range(0, fps.Count).OrderBy(x => locations[x]).ToList())
                    {
                        //Wrap everything up to that point in a BGP excluding any patterns relevant to any FullTextPattern
                        result = Join.CreateJoin(result, new Bgp(ps.Skip(locBase).Take(locations[i]).Where(tp => !(tp is TriplePattern) || !fps.Any(fp => fp.OriginalPatterns.Contains((TriplePattern)tp)))));
                        locBase = locations[i] + 1;
                        //Then apply the FullTextMatch operator over it
                        FullTextPattern ftp = fps[i];
                        result = new FullTextMatch(this._provider, result, ftp.MatchVariable, ftp.ScoreVariable, ftp.SearchTerm, ftp.Limit, ftp.ScoreThreshold);
                    }

                    //If there are any patterns left over remember to include them
                    if (locBase < ps.Count)
                    {
                        result = Join.CreateJoin(result, new Bgp(ps.Skip(locBase).Where(tp => !(tp is TriplePattern) || !fps.Any(fp => fp.OriginalPatterns.Contains((TriplePattern)tp)))));
                    }
                    return result;

                    //List<ITriplePattern> patterns = new List<ITriplePattern>();
                    //List<ITriplePattern> ps = new List<ITriplePattern>(current.TriplePatterns.ToList());
                    //for (int i = 0; i < current.PatternCount; i++)
                    //{
                    //    if (!(ps[i] is TriplePattern))
                    //    {
                    //        patterns.Add(ps[i]);
                    //    }
                    //    else
                    //    {
                    //        //Check to see if the Predicate of the Pattern match the Full Text Match Predicate URI
                    //        TriplePattern tp = (TriplePattern)ps[i];
                    //        PatternItem pred = tp.Predicate;
                    //        if (pred is NodeMatchPattern)
                    //        {
                    //            INode predNode = ((NodeMatchPattern)pred).Node;
                    //            if (predNode.NodeType == NodeType.Uri)
                    //            {
                    //                String predUri = ((IUriNode)predNode).Uri.ToString();
                    //                if (predUri.Equals(FullTextHelper.FullTextMatchPredicateUri))
                    //                {
                    //                    if (patterns.Count > 0) result = Join.CreateJoin(result, new Bgp(patterns));
                    //                    result = new FullTextMatch(this._provider, result, tp.Subject, tp.Object);
                    //                    patterns.Clear();
                    //                }
                    //                else
                    //                {
                    //                    patterns.Add(ps[i]);
                    //                }
                    //            }
                    //            else
                    //            {
                    //                patterns.Add(ps[i]);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            patterns.Add(ps[i]);
                    //        }
                    //    }
                    //}

                    //if (patterns.Count == current.PatternCount)
                    //{
                    //    //If count of remaining patterns same as original pattern count there was no optimisation
                    //    //to do so return as is
                    //    return current;
                    //}
                    //else if (patterns.Count > 0)
                    //{
                    //    //If any patterns left at end join as a BGP with result so far
                    //    result = Join.CreateJoin(result, new Bgp(patterns));
                    //    return result;
                    //}
                    //else
                    //{
                    //    return result;
                    //}
                }
            }
            else if (algebra is ITerminalOperator)
            {
                return algebra;
            }
            else
            {
                return algebra;
            }
        }

        /// <summary>
        /// Returns that the optimiser is applicable to all queries
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlQuery q)
        {
            return true;
        }

        /// <summary>
        /// Returns that the optimiser is applicable to all updates
        /// </summary>
        /// <param name="cmds">Updates</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return true;
        }

        /// <summary>
        /// Serializes the Optimisers Configuration
        /// </summary>
        /// <param name="context">Serialization Context</param>
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            context.EnsureObjectFactory(typeof(FullTextObjectFactory));

            INode optObj = context.NextSubject;

            context.Graph.Assert(optObj, context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassAlgebraOptimiser));
            context.Graph.Assert(optObj, ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType), context.Graph.CreateLiteralNode(this.GetType().FullName + ", dotNetRDF.Query.FullText"));

            if (this._provider is IConfigurationSerializable)
            {
                INode searcherObj = context.Graph.CreateBlankNode();
                context.NextSubject = searcherObj;
                ((IConfigurationSerializable)this._provider).SerializeConfiguration(context);
                context.Graph.Assert(optObj, context.Graph.CreateUriNode(new Uri(FullTextHelper.PropertySearcher)), searcherObj);
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to serialize configuration for this Full Text Optimiser as the Search Provider used does not implement the required IConfigurationSerializable interface");
            }
        }
    }
}
