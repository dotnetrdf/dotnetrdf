/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace VDS.RDF.Query.Engine.Joins.Workers
{
    public class FixedHashJoinWorker
        : ReusableJoinWorker
    {
        public FixedHashJoinWorker(IList<String> joinVars, IEnumerable<ISolution> rhs)
        {
            if (joinVars == null) throw new ArgumentNullException("joinVars");
            this.JoinVariables = joinVars is ReadOnlyCollection<String> ? joinVars : new List<string>(joinVars).AsReadOnly();
            if (this.JoinVariables.Count == 0) throw new ArgumentException("Number of join variables must be >= 1", "joinVars");
            
            // Build the hash
            this.Hash = new Dictionary<ISolution, List<ISolution>>(new SetDistinctnessComparer(this.JoinVariables));
            foreach (ISolution s in rhs)
            {
                List<ISolution> sets;
                if (!this.Hash.TryGetValue(s, out sets))
                {
                    sets = new List<ISolution>();
                    this.Hash.Add(s, sets);
                }
                sets.Add(s);
            }
        }

        private IDictionary<ISolution, List<ISolution>> Hash { get; set; }

        public IList<String> JoinVariables { get; private set; } 

        public override IEnumerable<ISolution> Find(ISolution lhs, IExecutionContext context)
        {
            List<ISolution> sets;
            return this.Hash.TryGetValue(lhs, out sets) ? sets.Where(s => lhs.IsCompatibleWith(s, this.JoinVariables)) : Enumerable.Empty<ISolution>();
        }
    }
}
