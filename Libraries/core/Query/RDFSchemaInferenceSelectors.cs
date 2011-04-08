/*

Copyright Robert Vesse 2009-10
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
using System.Diagnostics;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Selector which finds all Triples where the Subject is of a given Class
    /// </summary>
    public class ExactClassSelector : ISelector<Triple>
    {
        private IUriNode _targetclass;
        private IUriNode _type;

        /// <summary>
        /// Creates a new ExactClassSelector for the given Graph with the given Target Class
        /// </summary>
        /// <param name="g"></param>
        /// <param name="targetClass"></param>
        public ExactClassSelector(IGraph g, IUriNode targetClass)
        {
            this._targetclass = targetClass;
            this._type = g.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));
        }

        /// <summary>
        /// Accepts Triples where the Predicate is rdf:type and the Object is the Target Class that this Selector was instantiated with
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Is the Predicate equal to rdf:type?
            if (obj.Predicate.Equals(this._type))
            {
                //Does the Object match the Target Class
                return obj.Object.Equals(this._targetclass);
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Selector which finds all Triples which define SubClasses of the given Class
    /// </summary>
    public class SubClassSelector : ISelector<Triple>
    {
        private IUriNode _targetclass;
        private IUriNode _subclassof;

        /// <summary>
        /// Creates a new SubClassSelector for the given Graph with the given Target Class
        /// </summary>
        /// <param name="g"></param>
        /// <param name="targetClass"></param>
        public SubClassSelector(IGraph g, IUriNode targetClass)
        {
            this._targetclass = targetClass;
            this._subclassof = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"));
        }

        /// <summary>
        /// Accepts Triples where the Predicate is rdfs:subClassOf and the Object is the Target Class that this Selector was instantiated with
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Is the Predicate equal to rdfs:subClassOf?
            if (obj.Predicate.Equals(this._subclassof))
            {
                //Does the Object match the Target Class
                return obj.Object.Equals(this._targetclass);
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Selector which finds all Triples where define SuperClasses of the given Class
    /// </summary>
    public class SuperClassSelector : ISelector<Triple>
    {
        private IUriNode _targetclass;
        private IUriNode _subclassof;

        /// <summary>
        /// Creates a new SuperClassSelector for the given Graph with the given Target Class
        /// </summary>
        /// <param name="g"></param>
        /// <param name="targetClass"></param>
        public SuperClassSelector(IGraph g, IUriNode targetClass)
        {
            this._targetclass = targetClass;
            this._subclassof = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"));
        }

        /// <summary>
        /// Accepts Triples where the Predicate is rdfs:subClassOf and the Subject is the Target Class that this Selector was instantiated with
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Is the Predicate equal to rdfs:subClassOf?
            if (obj.Predicate.Equals(this._subclassof))
            {
                //Does the Subject match the Target Class
                return obj.Subject.Equals(this._targetclass);
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Selector which finds all Triples where the Subject is of a given Class (or subclass thereof)
    /// </summary>
    /// <remarks>
    /// This Class will not automatically apply the standard RDF Schema class hierarchy unless it has been defined in the Graph explicitly
    /// </remarks>
    public class ClassSelector : ISelector<Triple>
    {
        private List<IUriNode> _classes = new List<IUriNode>();
        private IUriNode _type;

        /// <summary>
        /// Creates a new Class Selector for the given Graph and Target Class
        /// </summary>
        /// <param name="g">Graph to infer Class Hierarchy from</param>
        /// <param name="targetClass">Target Class which you want to find all Sub Classes of</param>
        public ClassSelector(IGraph g, IUriNode targetClass)
        {
            //Add the Target Class to the list of possible classes
            this._classes.Add(targetClass);
            //Create the Type node for later use
            this._type = g.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));

            //Perform the inference on the Graph to find subclasses
            this.InferSubClasses(g, targetClass);
        }

        /// <summary>
        /// Helper method which performs Inference on the Graph to determine all subclasses of the Target Class
        /// </summary>
        /// <param name="g">Graph to perform inference on</param>
        /// <param name="parent">The Class to find SubClasses of</param>
        /// <remarks>This method is called when the Selector is instantiated and will be called recursively till it has found all SubClasses defined within the given Graph</remarks>
        private void InferSubClasses(IGraph g, IUriNode parent)
        {
            //Use another Selector to find SubClasses
            SubClassSelector scsel = new SubClassSelector(g, parent);

            foreach (Triple t in g.GetTriples(scsel))
            {
                //Add newly discovered class to list of classes
                if (t.Subject.NodeType == NodeType.Uri) 
                {
                    IUriNode subclass = (IUriNode)t.Subject;
                    this._classes.Add(subclass);

                    //Recurse to find SubClasses of the SubClass
                    this.InferSubClasses(g, subclass);
                }
            }
        }

        /// <summary>
        /// Accepts Triples where the Predicate is rdf:type and the Object is the Target Class that this Selector was instantiated with (or subclass thereof)
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Is the Predicate equal to rdf:type?
            if (obj.Predicate.Equals(this._type))
            {
                //Does the Object match the Target Class or a subclass thereof
                if (obj.Object.NodeType == NodeType.Uri)
                {
                    IUriNode test = (IUriNode)obj.Object;
                    return this._classes.Contains(test);
                }
                else
                {
                    //Can't be a match if the Object isn't a UriNode
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Selector which finds all Triples where the Subject is of a given Class (or superclass thereof)
    /// </summary>
    /// <remarks>
    /// This Class will not automatically apply the standard RDF Schema class hierarchy unless it has been defined in the Graph explicitly
    /// </remarks>
    public class WideningClassSelector : ISelector<Triple>
    {
        private List<IUriNode> _classes = new List<IUriNode>();
        private IUriNode _type;

        /// <summary>
        /// Creates a new Widening Class Selector for the given Graph and Target Class
        /// </summary>
        /// <param name="g">Graph to infer Class Hierarchy from</param>
        /// <param name="targetClass">Target Class which you want to find all Super Classes of</param>
        public WideningClassSelector(IGraph g, IUriNode targetClass)
        {
            //Add the Target Class to the list of possible classes
            this._classes.Add(targetClass);
            //Create the Type node for later use
            this._type = g.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));

            //Perform the inference on the Graph to find subclasses
            this.InferSuperClasses(g, targetClass);
        }

        /// <summary>
        /// Helper method which performs Inference on the Graph to determine all superclasses of the Target Class
        /// </summary>
        /// <param name="g">Graph to perform inference on</param>
        /// <param name="parent">The Class to find SuperClasses of</param>
        /// <remarks>This method is called when the Selector is instantiated and will be called recursively till it has found all SuperClasses defined within the given Graph</remarks>
        private void InferSuperClasses(IGraph g, IUriNode parent)
        {
            //Use another Selector to find SubClasses
            SuperClassSelector scsel = new SuperClassSelector(g, parent);

            foreach (Triple t in g.GetTriples(scsel))
            {
                //Add newly discovered class to list of classes
                if (t.Object.NodeType == NodeType.Uri)
                {
                    IUriNode supclass = (IUriNode)t.Object;
                    this._classes.Add(supclass);

                    //Recurse to find SuperClasses of the SuperClass
                    this.InferSuperClasses(g, supclass);
                }
            }
        }

        /// <summary>
        /// Accepts Triples where the Predicate is rdf:type and the Object is the Target Class that this Selector was instantiated with (or superclass thereof)
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Is the Predicate equal to rdf:type?
            if (obj.Predicate.Equals(this._type))
            {
                //Does the Object match the Target Class or a superclass thereof
                if (obj.Object.NodeType == NodeType.Uri)
                {
                    IUriNode test = (IUriNode)obj.Object;
                    return this._classes.Contains(test);
                }
                else
                {
                    //Can't be a match if the Object isn't a UriNode
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Selector which finds all Triples where the Predicate is a given Property
    /// </summary>
    public class HasExactPropertySelector : ISelector<Triple>
    {
        private IUriNode _targetproperty;

        /// <summary>
        /// Creates a new HasExactPropertySelector which will select Triples with the given Property as the Predicate
        /// </summary>
        /// <param name="targetProperty">The Property Triples to be selected must contain as their Predicate</param>
        public HasExactPropertySelector(IUriNode targetProperty)
        {
            this._targetproperty = targetProperty;
        }

        /// <summary>
        /// Accepts Triples where the Predicate is the Target Property that this Selector was instantiated with
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //If the Predicate matches the given Property then return it
            return obj.Predicate.Equals(this._targetproperty);
        }

    }

    /// <summary>
    /// Selector which finds all Triples where the Subject is defined as a SubProperty of a given Property
    /// </summary>
    public class SubPropertySelector : ISelector<Triple>
    {
        private IUriNode _targetproperty;
        private IUriNode _subpropof;

        /// <summary>
        /// Creates a new SubPropertySelector for the given Graph with the given Target Property
        /// </summary>
        /// <param name="g">Graph selection will occur in</param>
        /// <param name="targetProperty">Property you wish to select upon</param>
        public SubPropertySelector(IGraph g, IUriNode targetProperty)
        {
            this._targetproperty = targetProperty;
            this._subpropof = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subPropertyOf"));
        }

        /// <summary>
        /// Accepts Triples where the Predicate is rdfs:subPropertyOf and the Object is the Target Property that this Selector was instantiated with
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Is the Predicate equal to rdfs:subPropertyOf?
            if (obj.Predicate.Equals(this._subpropof))
            {
                //Does the Object match the Target Property
                return obj.Object.Equals(this._targetproperty);
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Selector which finds all Triples where the Predicate is a given Property (or subproperty thereof)
    /// </summary>
    public class HasPropertySelector : ISelector<Triple>
    {
        private List<IUriNode> _properties = new List<IUriNode>();

        /// <summary>
        /// Creates a new Has Property Selector for the given Graph and Target Property
        /// </summary>
        /// <param name="g">Graph to infer Property hierarchy</param>
        /// <param name="targetProperty">Target Property which you wish to find all sub properties of</param>
        public HasPropertySelector(IGraph g, IUriNode targetProperty)
        {
            //Add the Target Class to the list of possible classes
            this._properties.Add(targetProperty);

            //Perform the inference on the Graph to find subproperties
            this.InferSubProperties(g, targetProperty);
        }

        private void InferSubProperties(IGraph g, IUriNode parent)
        {
            //Use another Selector to find SubClasses
            SubPropertySelector spsel = new SubPropertySelector(g, parent);

            foreach (Triple t in g.GetTriples(spsel))
            {
                //Add newly discovered property to list of properties
                if (t.Subject.NodeType == NodeType.Uri)
                {
                    IUriNode subprop = (IUriNode)t.Subject;
                    this._properties.Add(subprop);

                    //Recurse to find SubProperties of the SubProperty
                    this.InferSubProperties(g, subprop);
                }
            }
        }

        /// <summary>
        /// Accepts Triples where the Predicate is the Target Property (or subproperty thereof) that this Selector was instantiated with
        /// </summary>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(Triple obj)
        {
            //Does the Predicate match the Target Property or a subproperty thereof
            if (obj.Predicate.NodeType == NodeType.Uri)
            {
                IUriNode test = (IUriNode)obj.Predicate;
                return this._properties.Contains(test);
            }
            else
            {
                //Can't be a match if the Object isn't a UriNode
                return false;
            }
        }
    }
}