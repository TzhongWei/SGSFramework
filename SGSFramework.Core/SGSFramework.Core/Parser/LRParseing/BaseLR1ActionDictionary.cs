using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Parser.LRParseing
{
    public abstract class BaseLR1ActionDictionary<TKernelItem>
        : IDictionary<string, LR1ActionItem<TKernelItem>>, IAction
        where TKernelItem : BaseLR0KernelItem, IEquatable<TKernelItem>
    {
        public ICollection<string> Keys => this._items.Keys;
        public ICollection<LR1ActionItem<TKernelItem>> Values => this._items.Values;
        public int Count => this._items.Count;
        public bool IsReadOnly => false;
        public LR1ActionItem<TKernelItem> this[string Key]
        {
            get => this._items[Key];
            set => this._items[Key] = value;
        }
        private readonly Dictionary<string, LR1ActionItem<TKernelItem>> _items;
        public BaseLR1ActionDictionary()
        {
            this._items = new Dictionary<string, LR1ActionItem<TKernelItem>>();
        }
        public void Add(string key, LR1ActionItem<TKernelItem> value)
            => this._items.Add(key, value);
        public bool ContainsKey(string key) => this._items.ContainsKey(key);
        public bool Remove(string key) => this._items.Remove(key);
        public bool TryGetValue
        (
                string key,
                out LR1ActionItem<TKernelItem> value
            ) => this._items.TryGetValue(key, out value);

        public void Add(KeyValuePair<string, LR1ActionItem<TKernelItem>> item)
            => this._items.Add(item.Key, item.Value);
        public void Clear() => this._items.Clear();
        public bool Contains(KeyValuePair<string, LR1ActionItem<TKernelItem>> item)
        => this._items.ContainsKey(item.Key) && this._items.ContainsValue(item.Value);
        public void CopyTo(KeyValuePair<string, LR1ActionItem<TKernelItem>>[] array,
            int arrayIndex)
            => throw new NotImplementedException();
        public bool Remove(KeyValuePair<string, LR1ActionItem<TKernelItem>> item)
            => this._items.Remove(item.Key);
        public IEnumerator<KeyValuePair<string, LR1ActionItem<TKernelItem>>> GetEnumerator()
            => this._items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this._items.GetEnumerator();
    }
}
