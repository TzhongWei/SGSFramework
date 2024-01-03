using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SGSFramework.Core.Interface;

namespace SGSFramework.Core.Tokeniser
{
    public class TokenTable : IEnumerable<CrvToken>, IEnumerator<CrvToken>
    {
        private List<CrvToken> tokens = new List<CrvToken>();
        public int Count => tokens.Count;
        public CrvToken this[int Index]
        {
            get
            {
                if (Index < Count - 1 && Index >= 0)
                {
                    return tokens[Index];
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
            set
            {
                try
                {
                    tokens[Index] = value;
                }
                catch 
                { 
                    throw new IndexOutOfRangeException(nameof(value));
                }
            }
        }
        public TokenTable() { }
        public TokenTable(List<CrvToken> tokens)
        {
            this.tokens = tokens;
        }
        public void Add(CrvToken token)
        { 
            tokens.Add(token);
        }
        public bool Contains(CrvToken token)
        {
            return tokens.Contains(token);
        }
        public void Clear()
        {
            this.tokens.Clear();
        }
        public List<Token_Name> Token_Names
        {
            get
            {
                if (Count > 0)
                {
                    return this.tokens.Select(x => x._name).ToList();
                }
                else { 
                    return new List<Token_Name>(); 
                }
            }
        }
        public List<int> Token_Ptrs
        {
            get
            {
                if (Count > 0)
                {
                    return this.tokens.Select(x => x._attributePtr).ToList();
                }
                else
                {
                    return new List<int>();
                }
            }
        }
        public IEnumerator<CrvToken> GetEnumerator()
            => (IEnumerator<CrvToken>) new List<CrvToken>(this.tokens);

        IEnumerator IEnumerable.GetEnumerator()
            => (IEnumerator)GetEnumerator();
        
        public CrvToken Current
        {
            get
            {
                try
                {
                    return this[Position];
                }
                catch (IndexOutOfRangeException) 
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }
        private int Position = -1;
        object IEnumerator.Current
        => Current;
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            Position++;
            return (Position < this.Count);
        }

        public void Reset()
        {
            this.Position = -1;
        }
    }
    public struct CrvToken:IToken<Token_Name, int>
    {
        public Token_Name _name { get;}
        public int _attributePtr { get;}
        public CrvToken(Token_Name _name, int _attributeID)
        {
            this._name = _name;
            this._attributePtr = _attributeID;
        }
        public static CrvToken Unset => new CrvToken();

        public bool Equals(IToken<Token_Name, int> other)
        => _name == other._name && _attributePtr == other._attributePtr;
    }
    public enum Token_Name
    {
        S,
        R,
        L,
        eS,
        Se
    }
}
