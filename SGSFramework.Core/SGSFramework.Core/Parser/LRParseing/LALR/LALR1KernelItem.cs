using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Parser.Grammar;

namespace SGSFramework.Core.Parser.LRParseing.LALR
{   
    public class LALR1KernelItem : BaseLR1KernelItem, IEquatable<LALR1KernelItem>
    {
        public override HashSet<string> LookAheads => this._lookAheads;
        private readonly HashSet<string> _lookAheads;
        public LALR1KernelItem
            (
            ProductionRule Production,
            int index
            ) : base(Production, index)
        {
            this._lookAheads = new HashSet<string>();
        }
        public LALR1KernelItem
            (
            ProductionRule Production,
            int index,
            IEnumerable<string> lookAheads
            ) : base(Production, index)
        {
            if (lookAheads is HashSet<string> hashSet)
            {
                this._lookAheads = new HashSet<string>(hashSet);
            }
            else
            {
                this._lookAheads = new HashSet<string>();
                foreach (string lookAhead in lookAheads)
                {
                    this._lookAheads.Add(lookAhead);
                }
            }
        }

        public LALR1KernelItem(ProductionRule Production, int index, HashSet<string> lookahead)
            : base(Production, index)
        {
            this._lookAheads = lookahead;
        }
        public LALR1KernelItem
        (
            BaseLR0KernelItem lr0KernelItem,
            HashSet<string> lookAheads
        )
            : this(lr0KernelItem.Production, lr0KernelItem.Index, lookAheads)
        {
            this._lookAheads = lookAheads;
        }
        public LALR1KernelItem(BaseLR0KernelItem lr0KernelItem)
            : this(lr0KernelItem.Production, lr0KernelItem.Index)
        { }
        public LALR1KernelItem(LALR1KernelItem lalr1KernelItem)
            : this
            (
                lalr1KernelItem.Production,
                lalr1KernelItem.Index,
                new HashSet<string>(lalr1KernelItem.LookAheads)
            )
        { }
        public override bool AddLookAhead(string token)
    => this._lookAheads.Add(token);

        public bool Equals(LALR1KernelItem other)
            => base.Equals(other);
        public override bool Equals(object obj)
            => base.Equals(obj);
        public override int GetHashCode()
            => base.GetHashCode();
    }
}
