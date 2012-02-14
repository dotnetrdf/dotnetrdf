/*

Copyright Robert Vesse 2009-12
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
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public class UpdateTask : NonCancellableTask<TaskResult>
    {
        private IGenericIOManager _manager;
        private String _update;
        private SparqlUpdateCommandSet _cmds;

        public UpdateTask(IGenericIOManager manager, String update)
            : base("SPARQL Update")
        {
            this._manager = manager;
            this._update = update;
        }

        protected override TaskResult RunTaskInternal()
        {
            SparqlUpdateParser parser = new SparqlUpdateParser();
            this._cmds = parser.ParseFromString(this._update);
            GenericUpdateProcessor processor = new GenericUpdateProcessor(this._manager);
            processor.ProcessCommandSet(this._cmds);
            return new TaskResult(true);
        }

        public SparqlUpdateCommandSet Updates
        {
            get
            {
                return this._cmds;
            }
        }
    }
}
