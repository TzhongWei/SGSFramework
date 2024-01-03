using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Parser.LRParseing
{
    /// <summary>
    /// A state in an LR automaton. It represents one or more productions 
    /// (kernel), an index in in each of those productions(kernel item) and a
    /// set of states it can transition to from a particular token(index in a
    /// production). An index from a counter gets included for easier
    /// referencing.
    /// </summary>
    /// <typeparam name="TKernelItem"></typeparam>
    public class State<TKernelItem>
        : IState<TKernelItem>, IEquatable<State<TKernelItem>>
        where TKernelItem : BaseLR0KernelItem, IEquatable<TKernelItem>
    {
        //The "main" kernel of this state consiting of 1 or more kernel items
        public Kernel<TKernelItem> Kernel { get; internal set; }

        //The full closure of the kernel Kernel
        public Kernel<TKernelItem> Items { get; internal set; }

        //All possible transitions from the closure
        public IDictionary<string, State<TKernelItem>> Transitions
        {
            get;
            internal set;
        }

        //A unique identifier for this state
        public int Index { get; private set; }

        #region IState
        IKernel<TKernelItem> IState<TKernelItem>.Kernel => this.Kernel;
        IKernel<TKernelItem> IState<TKernelItem>.Items => this.Items;
        #endregion

        internal State(int index)
        {
            this.Index = index;
        }

        public State(int index, Kernel<TKernelItem> kernel)
            : this(index)
        {
            this.Kernel = kernel;
        }

        public State
        (
            int index,
            Kernel<TKernelItem> kernel,
            Kernel<TKernelItem> items
        )
            : this(index, kernel)
        {
            this.Items = items;
        }

        public State
        (
            int index,
            Kernel<TKernelItem> kernel,
            Kernel<TKernelItem> items,
            IDictionary<string, State<TKernelItem>> transitions
        )
            : this(index, kernel, items)
        {
            this.Transitions = transitions;
        }

        public void AddKernel(BaseLR0KernelItem item)
        {
            if (!(item is TKernelItem other))
            {
                throw new InvalidCastException();
            }

            this.Kernel.Add(other);
        }

        public void AddItem(BaseLR0KernelItem item)
        {
            if (!(item is TKernelItem other))
            {
                throw new InvalidCastException();
            }

            this.Items.Add(other);
        }

        //Verify complete equality
        public bool Equals(State<TKernelItem> other)
            => !(other is null)
            && (object.ReferenceEquals(this, other)
                || (this.Kernel.Equals(other.Kernel)
                    && this.Items.Equals(other.Items)
                    && this.Index == other.Index));

        //Verify equality as LR(0) cores
        public bool Equals(IState<BaseLR0KernelItem> other)
            => !(other is null)
                && this.Items.Equals(other.Items);

        public override bool Equals(object obj)
        {
            if (!(obj is State<TKernelItem> other))
            {
                return false;
            }

            return this.Equals(other);
        }

        public override int GetHashCode()
            => this.Index;
    }
}
