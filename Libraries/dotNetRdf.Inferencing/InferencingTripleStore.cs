/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Query.Inference;

namespace VDS.RDF;

/// <summary>
/// An implementation of the <see cref="IInferencingTripleStore"/> interface over an in-memory <see cref="TripleStore"/>.
/// </summary>
public class InferencingTripleStore : TripleStore, IInferencingTripleStore
{
    /// <summary>
    /// List of Reasoners that are applied to Graphs as they are added to the Triple Store.
    /// </summary>
    protected List<IInferenceEngine> _reasoners = new();

    #region Loading with Inference

    /// <summary>
    /// Applies Inference to the given Graph.
    /// </summary>
    /// <param name="g">Graph to apply inference to.</param>
    public void ApplyInference(IGraph g)
    {
        // Apply Inference if we have any Inference Engines defined
        if (_reasoners.Count > 0)
        {
            // Set up Inference Graph if needed
            if (_storeInferencesExternally)
            {
                if (!Graphs.Contains(_inferenceGraphUri))
                {
                    IGraph i = new ThreadSafeGraph(_inferenceGraphUri) { BaseUri = _inferenceGraphUri.Uri };
                    base.Add(i, true);
                }
            }

            // Apply inference
            foreach (IInferenceEngine reasoner in _reasoners)
            {
                if (_storeInferencesExternally)
                {
                    reasoner.Apply(g, Graphs[_inferenceGraphUri]);
                }
                else
                {
                    reasoner.Apply(g);
                }
            }
        }
    }

    /// <summary>
    /// Adds an Inference Engine to the Triple Store.
    /// </summary>
    /// <param name="reasoner">Reasoner to add.</param>
    public void AddInferenceEngine(IInferenceEngine reasoner)
    {
        _reasoners.Add(reasoner);

        // Apply Inference to all existing Graphs
        if (_graphs.Count > 0)
        {
            lock (_graphs)
            {
                // Have to do a ToList() in case someone else inserts a Graph
                // Which ApplyInference may do if the Inference information is stored in a special Graph
                foreach (IGraph g in _graphs.ToList())
                {
                    ApplyInference(g);
                }
            }
        }
    }

    /// <summary>
    /// Removes an Inference Engine from the Triple Store.
    /// </summary>
    /// <param name="reasoner">Reasoner to remove.</param>
    public void RemoveInferenceEngine(IInferenceEngine reasoner)
    {
        _reasoners.Remove(reasoner);
    }

    /// <summary>
    /// Clears all Inference Engines from the Triple Store.
    /// </summary>
    public void ClearInferenceEngines()
    {
        _reasoners.Clear();
    }

    #endregion

    /// <summary>
    /// Event Handler for the <see cref="BaseGraphCollection.GraphAdded">Graph Added</see> event of the underlying Graph Collection which calls the normal event processing of the parent class <see cref="BaseTripleStore">BaseTripleStore</see> and then applies Inference to the newly added Graph.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Graph Event Arguments.</param>
    protected override void OnGraphAdded(object sender, GraphEventArgs args)
    {
        base.OnGraphAdded(sender, args);
        ApplyInference(args.Graph);
    }

}
