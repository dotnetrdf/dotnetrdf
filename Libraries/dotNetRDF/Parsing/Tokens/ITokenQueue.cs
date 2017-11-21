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

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// Interface for implementing Token Queues which provide Bufferable wrappers to Tokenisers
    /// </summary>
    public interface ITokenQueue
    {
        /// <summary>
        /// Removes the first Token from the Queue
        /// </summary>
        /// <returns></returns>
        IToken Dequeue();

        /// <summary>
        /// Adds a Token to the end of the Queue
        /// </summary>
        /// <param name="t">Token to add</param>
        void Enqueue(IToken t);

        /// <summary>
        /// Gets the first Token from the Queue without removing it
        /// </summary>
        /// <returns></returns>
        IToken Peek();

        /// <summary>
        /// Tokeniser that this is a Queue for
        /// </summary>
        ITokeniser Tokeniser
        {
            get;
            set;
        }

        /// <summary>
        /// Clears the Token Queue
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the number of Tokens in the Queue
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Initialises the Buffer
        /// </summary>
        void InitialiseBuffer();

        /// <summary>
        /// Initialises the Buffer and sets the Buffering Level
        /// </summary>
        /// <param name="amount">Buffering Amount</param>
        void InitialiseBuffer(int amount);

        /// <summary>
        /// Gets the underlying Queue of Tokens
        /// </summary>
        Queue<IToken> Tokens
        {
            get;
        }

        /// <summary>
        /// Gets/Sets whether Tokeniser Tracing should be used
        /// </summary>
        bool Tracing
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Token Type of the last Token dequeued
        /// </summary>
        int LastTokenType
        {
            get;
        }
    }

    /// <summary>
    /// Abstract base implementation of a Token Queue
    /// </summary>
    public abstract class BaseTokenQueue : ITokenQueue
    {
        /// <summary>
        /// Tokeniser used to fill the Token Queue
        /// </summary>
        protected ITokeniser _tokeniser;
        /// <summary>
        /// Variable indicating whether Tokeniser Tracing is enabled
        /// </summary>
        protected bool _tracing;
        /// <summary>
        /// Type of Last Token dequeued
        /// </summary>
        protected int _lasttokentype = Token.BOF;

        /// <summary>
        /// Abstract Definition of Interface Method
        /// </summary>
        /// <returns></returns>
        public abstract IToken Dequeue();

        /// <summary>
        /// Abstract Definition of Interface Method
        /// </summary>
        public abstract void Enqueue(IToken t);

        /// <summary>
        /// Abstract Definition of Interface Method
        /// </summary>
        public abstract IToken Peek();

        /// <summary>
        /// Sets the Tokeniser used by the Queue
        /// </summary>
        /// <remarks>Setting the Tokeniser causes the Queue to clear itself</remarks>
        public ITokeniser Tokeniser
        {
            get
            {
                return _tokeniser;
            }
            set
            {
                _tokeniser = value;
                Clear();
            }
        }

        /// <summary>
        /// Abstract Definition of Interface Method
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Abstract Definition of Interface Property
        /// </summary>
        public abstract int Count
        {
            get;
        }

        /// <summary>
        /// Abstract Definition of Interface Method
        /// </summary>
        public abstract void InitialiseBuffer();

        /// <summary>
        /// Abstract Definition of Interface Method
        /// </summary>
        /// <param name="amount">Buffering Amount</param>
        public abstract void InitialiseBuffer(int amount);

        /// <summary>
        /// Abstract Definition of Interface Property
        /// </summary>
        public abstract Queue<IToken> Tokens
        {
            get;
        }

        /// <summary>
        /// Gets/Sets Tracing for the Token Queue
        /// </summary>
        public bool Tracing
        {
            get
            {
                return _tracing;
            }
            set
            {
                _tracing = value;
            }
        }

        /// <summary>
        /// Gets the Token Type of the last Token dequeued
        /// </summary>
        public int LastTokenType
        {
            get
            {
                return _lasttokentype;
            }
        }
    }
}
