/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.PropertyFunctions
{
    /// <summary>
    /// Helper Class containing functions useful in working with property functions
    /// </summary>
    public static class PropertyFunctionHelper
    {
        /// <summary>
        /// Used to extract the patterns that make up property functions
        /// </summary>
        /// <param name="patterns">Triple Patterns</param>
        /// <returns></returns>
        public static List<IPropertyFunctionPattern> ExtractPatterns(IEnumerable<ITriplePattern> patterns)
        {
            return ExtractPatterns(patterns, Enumerable.Empty<IPropertyFunctionFactory>());
        }

        /// <summary>
        /// Used to extract the patterns that make up property functions
        /// </summary>
        /// <param name="patterns">Triple Patterns</param>
        /// <param name="localFactories">Locally scoped factories</param>
        /// <returns></returns>
        public static List<IPropertyFunctionPattern> ExtractPatterns(IEnumerable<ITriplePattern> patterns, IEnumerable<IPropertyFunctionFactory> localFactories)
        {
            // Do a first pass which simply looks to find any 'magic' properties
            Dictionary<PatternItem, PropertyFunctionInfo> funcInfo = new Dictionary<PatternItem, PropertyFunctionInfo>();
            List<IMatchTriplePattern> ps = patterns.OfType<IMatchTriplePattern>().ToList();
            if (ps.Count == 0) return new List<IPropertyFunctionPattern>();
            foreach (IMatchTriplePattern tp in ps)
            {
                NodeMatchPattern predItem = tp.Predicate as NodeMatchPattern;
                if (predItem == null) continue;
                IUriNode predNode = predItem.Node as IUriNode;
                if (predNode == null) continue;
                if (PropertyFunctionFactory.IsPropertyFunction(predNode.Uri, localFactories))
                {
                    PropertyFunctionInfo info = new PropertyFunctionInfo(predNode.Uri);
                    info.Patterns.Add(tp);
                    funcInfo.Add(tp.Subject, info);
                }
            }
            // Remove any Patterns we found from the original patterns
            foreach (PropertyFunctionInfo info in funcInfo.Values)
            {
                info.Patterns.ForEach(tp => ps.Remove(tp));
            }

            if (funcInfo.Count == 0) return new List<IPropertyFunctionPattern>();

            // Now for each 'magic' property we found do a further search to see if we are using
            // the collection forms to provide extended arguments
            foreach (PatternItem key in funcInfo.Keys)
            {
                if (key.VariableName != null && key.VariableName.StartsWith("_:"))
                {
                    // If LHS is a blank node may be collection form
                    int count = funcInfo[key].Patterns.Count;
                    ExtractRelatedPatterns(key, key, ps, funcInfo, funcInfo[key].SubjectArgs);
                    if (funcInfo[key].Patterns.Count == count)
                    {
                        // If no further patterns found just single LHS argument
                        funcInfo[key].SubjectArgs.Add(key);
                    }
                }
                else
                {
                    // Otherwise key is the only LHS argument
                    funcInfo[key].SubjectArgs.Add(key);
                }
                PatternItem searchKey = funcInfo[key].Patterns.First().Object;
                if (searchKey.VariableName != null && searchKey.VariableName.StartsWith("_:"))
                {
                    // If RHS is a blank node may be collection form
                    int count = funcInfo[key].Patterns.Count;
                    ExtractRelatedPatterns(key, searchKey, ps, funcInfo, funcInfo[key].ObjectArgs);
                    if (funcInfo[key].Patterns.Count == count)
                    {
                        // If no further patterns found just single RHS argument
                        funcInfo[key].ObjectArgs.Add(searchKey);
                    }
                }
                else
                {
                    // Otherwise single RHS argument
                    funcInfo[key].ObjectArgs.Add(searchKey);
                }
            }

            // Now try to create actual property functions
            List<IPropertyFunctionPattern> propFunctions = new List<IPropertyFunctionPattern>();
            foreach (PatternItem key in funcInfo.Keys)
            {
                IPropertyFunctionPattern propFunc;
                if (PropertyFunctionFactory.TryCreatePropertyFunction(funcInfo[key], localFactories, out propFunc))
                {
                    propFunctions.Add(propFunc);
                }
            }
            return propFunctions;
        }

        /// <summary>
        /// Used to help extract the patterns that make up a property function pattern
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="subj">Subject</param>
        /// <param name="ps">Patterns</param>
        /// <param name="funcInfo">Function Information</param>
        /// <param name="argList">Argument List to add discovered arguments to</param>
        static void ExtractRelatedPatterns(PatternItem key, PatternItem subj, List<IMatchTriplePattern> ps, Dictionary<PatternItem, PropertyFunctionInfo> funcInfo, List<PatternItem> argList)
        {
            bool recurse = true, any = false, argSeen = false;
            PatternItem nextSubj = subj;
            while (recurse)
            {
                any = false;
                foreach (IMatchTriplePattern tp in ps.ToList())
                {
                    if (tp.Subject.VariableName == nextSubj.VariableName)
                    {
                        NodeMatchPattern predItem = tp.Predicate as NodeMatchPattern;
                        if (predItem == null) continue;
                        IUriNode predNode = predItem.Node as IUriNode;
                        if (predNode == null) continue;
                        if (!argSeen && predNode.Uri.AbsoluteUri.Equals(RdfSpecsHelper.RdfListFirst))
                        {
                            funcInfo[key].Patterns.Add(tp);
                            argList.Add(tp.Object);
                            ps.Remove(tp);
                            any = true;
                            argSeen = true;
                        }
                        else if (argSeen && predNode.Uri.AbsoluteUri.Equals(RdfSpecsHelper.RdfListRest))
                        {
                            funcInfo[key].Patterns.Add(tp);
                            ps.Remove(tp);
                            recurse = tp.Object.VariableName != null;
                            nextSubj = tp.Object;
                            any = true;
                            argSeen = false;
                        }
                    }
                }
                if (!any) recurse = false;

                if (nextSubj == null) throw new RdfQueryException("Failed to find expected rdf:rest property");
            }
        }
    }
}
