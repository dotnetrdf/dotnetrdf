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
using System.Threading;

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// Basic Token Queue which provides no Buffering except in the sense that it queues all possible Tokens when the InitialiseBuffer method is called
    /// </summary>
    /// <remarks>This is the recommended Token Queue and the one used by default in the Parser implementations</remarks>
    public class TokenQueue : BaseTokenQueue
    {
        /// <summary>
        /// Internal Queue object which this class is a wrapper around
        /// </summary>
        protected Queue<IToken> _tokens = new Queue<IToken>();

        /// <summary>
        /// Creates a new Token Queue
        /// </summary>
        public TokenQueue()
        {

        }

        /// <summary>
        /// Creates a new Token Queue with the given Tokeniser
        /// </summary>
        /// <param name="tokeniser">Tokeniser</param>
        public TokenQueue(ITokeniser tokeniser)
        {
            this._tokeniser = tokeniser;
        }

        /// <summary>
        /// Removes and returns the first Token from the Queue
        /// </summary>
        /// <returns>First Token in the Queue</returns>
        public override IToken Dequeue()
        {
            this._lasttokentype = this._tokens.Peek().TokenType;
            return this._tokens.Dequeue();
        }

        /// <summary>
        /// Adds a Token to the end of the Queue
        /// </summary>
        /// <param name="t">Token to add</param>
        public override void Enqueue(IToken t)
        {
            this._tokens.Enqueue(t);
        }

        /// <summary>
        /// Gets the first Token from the Queue without removing it
        /// </summary>
        /// <returns>First Token in the Queue</returns>
        public override IToken Peek()
        {
            return this._tokens.Peek();
        }

        /// <summary>
        /// Empties the Token Queue
        /// </summary>
        public override void Clear()
        {
            this._tokens.Clear();
        }

        /// <summary>
        /// Gets the number of Tokens in the Queue
        /// </summary>
        public override int Count
        {
            get 
            {
                return this._tokens.Count;
            }
        }

        /// <summary>
        /// Initialises the Token Queue Buffer
        /// </summary>
        public override void InitialiseBuffer()
        {
            //Queue until we get the EOF Token
            IToken t;
            do
            {
                t = this._tokeniser.GetNextToken();
                //We discard comments at this stage
                if (t.TokenType != Token.COMMENT)
                {
                    this._tokens.Enqueue(t);
                }

                if (this._tracing)
                {
                    this.PrintTrace(t);
                }
            } while (t.TokenType != Token.EOF);
        }

        /// <summary>
        /// Initialises the Token Queue Buffer to the set Buffer Amount
        /// </summary>
        /// <param name="amount">Amount of Tokens to Buffer</param>
        public override void InitialiseBuffer(int amount)
        {
            //This Queue doesn't care about the Buffer amount
            this.InitialiseBuffer();
        }

        /// <summary>
        /// Gets the underlying Queue of Tokens
        /// </summary>
        public override Queue<IToken> Tokens
        {
            get 
            {
                return this._tokens;
            }
        }

        /// <summary>
        /// Internal Helper Method for Tokeniser Tracing
        /// </summary>
        /// <param name="t"></param>
        protected void PrintTrace(IToken t)
        {
            Console.WriteLine("[Lines " + t.StartLine + "-" + t.EndLine + " Columns " + t.StartPosition + "-" + t.EndPosition + "] " + t.GetType().Name + " " + t.Value);
        }
    }

    /// <summary>
    /// Token Queue which is not backed by a Tokeniser
    /// </summary>
    /// <remarks>
    /// Designed to be explicitly populated with Tokens for when a Parser needs to be invoked on a subset of the overall Tokens
    /// </remarks>
    public class NonTokenisedTokenQueue : TokenQueue
    {
        /// <summary>
        /// Creates a new non-Tokenised Queue
        /// </summary>
        public NonTokenisedTokenQueue()
        {

        }

        /// <summary>
        /// Removed and returns the first Token from the Queue
        /// </summary>
        /// <returns></returns>
        public override IToken Dequeue()
        {
            IToken temp;
            //Discard Comments
            do
            {
                temp = this._tokens.Dequeue();
                if (this._tracing) this.PrintTrace(temp);
            } while (temp.TokenType == Token.COMMENT);

            this._lasttokentype = temp.TokenType;
            return temp;
        }

        /// <summary>
        /// Gets the first Token from the Queue without removing it
        /// </summary>
        /// <returns>First Token in the Queue</returns>
        public override IToken Peek()
        {
            //Discard Comments
            while (this._tokens.Peek().TokenType == Token.COMMENT)
            {
                this._tokens.Dequeue();
            }

            return this._tokens.Peek();
        }

        /// <summary>
        /// Initialises the Buffer by doing nothing since there is no buffering on this Queue
        /// </summary>
        public override void InitialiseBuffer()
        {
            //Does nothing for this Token Queue as this Queue is designed to be populated explicitly
            return;
        }
    }

    /// <summary>
    /// A Buffered Queue for a Tokeniser which synchronously buffers a number of Tokens when the Queue is accessed and nothing is Buffered
    /// </summary>
    public class BufferedTokenQueue : TokenQueue
    {
        /// <summary>
        /// Variable storing the Buffer Size
        /// </summary>
        protected int _bufferAmount = 10;

        /// <summary>
        /// Creates a new Buffered Queue for the given Tokeniser
        /// </summary>
        /// <param name="tokeniser">Tokeniser to Buffer</param>
        public BufferedTokenQueue(ITokeniser tokeniser)
        {
            this._tokeniser = tokeniser;
        }

        /// <summary>
        /// Creates a new Buffered Queue
        /// </summary>
        public BufferedTokenQueue()
        {
        }

        /// <summary>
        /// Gets the next Token in the Queue and removes it from the Queue
        /// </summary>
        /// <returns>Token at the front of the Queue</returns>
        public override IToken Dequeue()
        {
            //Is there anything already buffered?
            if (this._tokens.Count > 0)
            {
                //Return first thing in the Buffer
                return base.Dequeue();
            }
            else
            {
                //Buffer something from the Tokeniser
                this.BufferInternal();

                //Return first thing in the Buffer
                return base.Dequeue();
            }
        }

        /// <summary>
        /// Gets the next Token in the Queue without removing it from the Queue
        /// </summary>
        /// <returns>Token at the front of the Queue</returns>
        public override IToken Peek()
        {
            //Is there anything already buffered?
            if (this._tokens.Count > 0)
            {
                //Return first thing in the Buffer
                return this._tokens.Peek();
            }
            else
            {
                //Buffer something from the Tokeniser
                this.BufferInternal();

                //Return first thing in the Buffer
                return this._tokens.Peek();
            }
        }

        /// <summary>
        /// Causes the Buffer to be filled using the Default Buffering level of 10
        /// </summary>
        public override void InitialiseBuffer()
        {
            //Buffer by the Default Amount
            this.BufferInternal();
        }

        /// <summary>
        /// Causes the Buffer to be filled and sets the Buffering level for the Queue
        /// </summary>
        /// <param name="amount">Number of Tokens to Buffer</param>
        /// <remarks>If a Buffer amount of less than zero is given then Buffer size will stay at default size (10) or existing size if it's previously been set</remarks>
        public override void InitialiseBuffer(int amount)
        {
            if (amount > 0)
            {
                this._bufferAmount = amount;
            }
            this.BufferInternal();
        }

        /// <summary>
        /// Internal Helper Method which performs the Buffering
        /// </summary>
        protected virtual void BufferInternal()
        {
            //Buffer up to the amount
            IToken t;
            int i = 0;
            do
            {
                t = this._tokeniser.GetNextToken();
                //Ensure that we discard Comments
                if (t.TokenType != Token.COMMENT)
                {
                    this._tokens.Enqueue(t);
                    i++;
                }

                if (this._tracing)
                {
                    this.PrintTrace(t);
                }
            } while (t.TokenType != Token.EOF && i < this._bufferAmount);
        }
    }

    /// <summary>
    /// An Asynchronous version of <see cref="BufferedTokenQueue">BufferedTokenQueue</see> which automatically Buffers as many Tokens as possible in a Background thread
    /// </summary>
    /// <remarks>
    /// Periodic instablility is fixed to the best of my knowledge, it is still recommended to use a <see cref="BufferedTokenQueue">BufferedTokenQueue</see> or the basic <see cref="TokenQueue">TokenQueue</see>.  This implementation offers little/no performance improvement over the other types of Token Queue.
    /// </remarks>
    public class AsynchronousBufferedTokenQueue : BufferedTokenQueue
    {
        private bool _started = false;
        private bool _finished = false;
        private Thread _bgbuffer;

        /// <summary>
        /// Creates a new Asynchronous Buffered Queue with the given Tokeniser
        /// </summary>
        /// <param name="tokeniser">Tokeniser to Buffer</param>
        public AsynchronousBufferedTokenQueue(ITokeniser tokeniser)
            : base(tokeniser)
        {

        }

        /// <summary>
        /// Creates a new Asynchronous Buffered Queue
        /// </summary>
        public AsynchronousBufferedTokenQueue()
            : base()
        {

        }

        /// <summary>
        /// Gets the next Token in the Queue and removes it from the Queue
        /// </summary>
        /// <returns>Token at the front of the Queue</returns>
        public override IToken Dequeue()
        {
            if (this._tokens.Count > 0)
            {
                try
                {
                    Monitor.Enter(this._tokens);
                    return base.Dequeue();
                }
                finally
                {
                    Monitor.Exit(this._tokens);
                }
            }
            else
            {
                if (!this._finished)
                {
                    //Wait for something to be Tokenised
                    while (this._tokens.Count == 0)
                    {
                        Thread.Sleep(50);
                    }
                }
                try
                {
                    Monitor.Enter(this._tokens);
                    return base.Dequeue();
                }
                finally
                {
                    Monitor.Exit(this._tokens);
                }
            }
        }

        /// <summary>
        /// Gets the next Token in the Queue without removing it from the Queue
        /// </summary>
        /// <returns>Token at the front of the Queue</returns>
        public override IToken Peek()
        {
            if (this._tokens.Count > 0)
            {
                try
                {
                    Monitor.Enter(this._tokens);
                    return base.Peek();
                }
                finally
                {
                    Monitor.Enter(this._tokens);
                }
            }
            else
            {
                if (!this._finished)
                {
                    //Wait for something to be Tokenised
                    while (this._tokens.Count == 0)
                    {
                        Thread.Sleep(50);
                    }
                }
                try
                {
                    Monitor.Enter(this._tokens);
                    return base.Peek();
                }
                finally
                {
                    Monitor.Enter(this._tokens);
                }
            }
        }

        /// <summary>
        /// Internal Helper Method which starts the Background Buffering if not already running
        /// </summary>
        protected override void BufferInternal()
        {
            if (!this._started)
            {
                this._bgbuffer = new Thread(new ThreadStart(BufferBackground));
                this._bgbuffer.IsBackground = true;
                this._started = true;
                this._bgbuffer.Start();
            }
        }

        /// <summary>
        /// Internal Thread Method which does the Background Buffering
        /// </summary>
        private void BufferBackground()
        {
            //Buffer until we get the EOF Token
            IToken t;
            int i = 0;
            do
            {
                t = this._tokeniser.GetNextToken();
                try
                {
                    Monitor.Enter(this._tokens);
                    if (t.TokenType != Token.COMMENT)
                    {
                        this._tokens.Enqueue(t);
                        i++;
                    }
                }
                finally
                {
                    Monitor.Exit(this._tokens);
                }

                if (this._tracing)
                {
                    this.PrintTrace(t);
                }
            } while (t.TokenType != Token.EOF);
            this._finished = true;
        }
    }
}
