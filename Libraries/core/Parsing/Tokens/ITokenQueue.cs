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
                return this._tokeniser;
            }
            set
            {
                this._tokeniser = value;
                this.Clear();
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
                return this._tracing;
            }
            set
            {
                this._tracing = value;
            }
        }

        /// <summary>
        /// Gets the Token Type of the last Token dequeued
        /// </summary>
        public int LastTokenType
        {
            get
            {
                return this._lasttokentype;
            }
        }
    }
}
