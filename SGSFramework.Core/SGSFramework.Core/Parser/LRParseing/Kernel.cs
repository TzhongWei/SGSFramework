using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Parser.LRParseing
{

    /// <summary>
    /// Represents a set of one or more kernel items (production with an index in
    /// that production)  which signify a state in an LR automaton
    /// </summary>
    /// <typeparam name="TKernelItem"></typeparam>
    public class Kernel<TKernelItem>
        : IKernel<TKernelItem>, IList<TKernelItem>,
            IEquatable<Kernel<TKernelItem>>
        where TKernelItem : BaseLR0KernelItem, IEquatable<TKernelItem>
    {
        private readonly IList<TKernelItem> _items;

        public TKernelItem this[int index]
        {
            get => this._items[index];
            set => this._items[index] = value;
        }
        public int Count => this._items.Count;
        public bool IsReadOnly => this._items.IsReadOnly;

        public Kernel()
        {
            this._items = new List<TKernelItem>();
        }

        public void Add(TKernelItem item) => this._items.Add(item);

        public void Clear() => this._items.Clear();

        public bool Contains(TKernelItem item) => this._items.Contains(item);

        public void CopyTo(TKernelItem[] array, int arrayIndex)
            => this._items.CopyTo(array, arrayIndex);

        public IEnumerator<TKernelItem> GetEnumerator()
            => this._items.GetEnumerator();

        public int IndexOf(TKernelItem item)
            => this._items.IndexOf(item);

        public void Insert(int index, TKernelItem item)
            => this._items.Insert(index, item);

        public bool Remove(TKernelItem item)
            => this._items.Remove(item);

        public void RemoveAt(int index)
            => this._items.RemoveAt(index);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this._items.GetEnumerator();

        //Implemented so order of kernel items DOES NOT matter
        public bool Equals(Kernel<TKernelItem> other)
        {
            if (other is null)
            {
                return false;
            }

            return this.OrderlessSequenceEqual(other);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Kernel<TKernelItem> other))
            {
                return false;
            }

            return this.Equals(other);
        }

        public override int GetHashCode()
            => this._items.GetHashCode();
    }
}
