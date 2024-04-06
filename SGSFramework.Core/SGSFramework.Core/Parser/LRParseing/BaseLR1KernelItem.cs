using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Grammar;

namespace SGSFramework.Core.Parser.LRParseing
{
    public abstract class BaseLR1KernelItem : BaseLR0KernelItem, IEquatable<BaseLR1KernelItem>
    {
        public abstract HashSet<string> LookAheads { get; }
        protected BaseLR1KernelItem(ProductionRule Production, int index):base(Production, index)
        { 
        }
        public abstract bool AddLookAhead(string token);

        public bool Equals(BaseLR1KernelItem other)
          => !(other is null)
            && (object.ReferenceEquals(this, other)
            || (object.Equals(this.Production, other.Production)
            && this.Index == other.Index && this.LookAheads.SetEquals(other.LookAheads)));
        public override bool Equals(object obj)
        {
            if (!(obj is BaseLR1KernelItem kernelItem))
            {
                return false;
            }

            return this.Equals(kernelItem);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
