using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Parser.Grammar;

namespace SGSFramework.Core.Parser.LRParseing
{
    /// <summary>
    /// Represents a production and the position in that production. These are
    /// uniquely part of a kernel as single state in an automaton
    /// </summary>
    public abstract class BaseLR0KernelItem : IEquatable<BaseLR0KernelItem>
    {
        /// <summary>
        /// Production Rules in this kernel
        /// </summary>
        public ProductionRule Production { get; private set; }
        /// <summary>
        /// An index (position) in that index
        /// </summary>
        public int Index { get; private set; }

        protected BaseLR0KernelItem(ProductionRule production, int index)
        {
            this.Production = production;
            this.Index = index;
        }

        public bool Equals(BaseLR0KernelItem other)
            => !(other is null)
            && (object.ReferenceEquals(this, other)
                || (object.Equals(this.Production, other.Production)
                    && this.Index == other.Index));

        public override bool Equals(object obj)
        {
            if (!(obj is BaseLR0KernelItem kernelItem))
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
