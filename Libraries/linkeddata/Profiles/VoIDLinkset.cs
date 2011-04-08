using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.LinkedData.Profiles
{
    /// <summary>
    /// Class for representing VoID Linksets
    /// </summary>
    public class VoIDLinkset
    {
        private bool _directed = false;
        protected INode _subjectTarget, _objectsTarget;
        protected List<INode> _predicates = new List<INode>();

        protected VoIDLinkset()
        {

        }

        public VoIDLinkset(IGraph voidDescription, INode linksetSubj)
        {
            IUriNode target = voidDescription.CreateUriNode("void:target");
            IUriNode subjTarget = voidDescription.CreateUriNode("void:subjectsTarget");
            IUriNode objTarget = voidDescription.CreateUriNode("void:objectsTarget");
            IUriNode linkPredicate = voidDescription.CreateUriNode("void:linkPredicate");

            if (voidDescription.GetTriplesWithSubjectPredicate(linksetSubj, target).Any())
            {
                IEnumerable<Triple> ts = voidDescription.GetTriplesWithSubjectPredicate(linksetSubj, target).Take(2);
                this._subjectTarget = ts.First().Object;
                this._objectsTarget = ts.Last().Object;
            }
            else
            {
                this._subjectTarget = voidDescription.GetTriplesWithSubjectPredicate(linksetSubj, subjTarget).First().Object;
                this._objectsTarget = voidDescription.GetTriplesWithSubjectPredicate(linksetSubj, objTarget).First().Object;
                this._directed = true;
            }

            this._predicates.AddRange(voidDescription.GetTriplesWithSubjectPredicate(linksetSubj, linkPredicate).Select(t => t.Object));
        }

        public bool IsDirected
        {
            get
            {
                return this._directed;
            }
        }

        public INode SubjectsTarget
        {
            get
            {
                return this._subjectTarget;
            }
        }

        public INode ObjectsTarget
        {
            get
            {
                return this._objectsTarget;
            }
        }

        public IEnumerable<INode> Predicates
        {
            get
            {
                return this._predicates;
            }
        }
    }
}
