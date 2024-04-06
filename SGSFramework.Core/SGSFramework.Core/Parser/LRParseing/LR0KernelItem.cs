using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Grammar;

namespace SGSFramework.Core.Parser.LRParseing
{
    public class LR0KernelItem : BaseLR0KernelItem, IEquatable<LR0KernelItem>
    {
        public LR0KernelItem(ProductionRule production, int index)
            : base(production, index)
        { }

        public bool Equals(LR0KernelItem other)
            => base.Equals(other);

        public override bool Equals(object obj)
            => base.Equals(obj);


        public override int GetHashCode()
            => base.GetHashCode();
    }
}
