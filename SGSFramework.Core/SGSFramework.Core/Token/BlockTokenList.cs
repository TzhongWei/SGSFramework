using System;
using System.Collections;
using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Interface;

namespace SGSFramework.Core.Token
{
    public class BlockTokenList : ITokenList<BlockToken>, IEquatable<BlockTokenList>, ICloneable
    {
        public string Name;
        public BlockToken this[int index] { 
            get => this.TokenList.ToList()[index];
            set
            {
                var BlockList = this.TokenList.ToList();
                BlockList[index] = value;
                this.TokenList = BlockList.ToHashSet();
            } 
        }
        public HashSet<BlockToken> TokenList { get; private set; }
        public int Count => TokenList.Count;
        private BlockTokenList() { }
        public BlockTokenList(string Name)
        {
            this.TokenList = new HashSet<BlockToken>();
            this.Name = Name;
        }
        private BlockTokenList(string Name, bool IsReadonly = true) : this(Name)
        {
            IsReadonly = true;
        }
        public bool IsReadOnly => false;
        public void Add(BlockToken item)
        {
            if (!this.TokenList.Add(item))
            {
                throw new Exception($"{item._name} is exist");
            }
        }
        public void AddRange(HashSet<BlockToken> blockTokens)
        {
            foreach (var item in blockTokens)
                this.Add(item);
        }
        public void Add(string _Name, Transform transform)
        {
            this.Add(new BlockToken(_Name, transform));
        }
        public void Add(string _Name, string CompatibleName)
        {
            this.Add(new BlockToken(_Name, CompatibleName));
        }
        public void Clear()
        {
            this.TokenList.Clear();
        }
        public List<string> GetCompatibleName
            => this.TokenList
            .Where(x => x.Type == "CompatibleName")
            .Select(x => $"{x._name}, {x.Action.ToString()}").ToList();
        public List<string> VocabularyLabel =>
                            this.TokenList.Where(x => x.Type == "Vocabulary")
                            .Select(x => x._name).ToList();
        public List<string> CompatibleLabel =>
                            this.TokenList.Where(x => x.Type == "CompatibleName")
                            .Select(x => x._name).ToList();
        public void Sort()
        {
            var BlockList = this.TokenList.ToList();
            BlockList.Sort();
            this.TokenList = BlockList.ToHashSet();
        }
        public bool Contains(BlockToken item)
         => this.TokenList.Contains(item);
        public void CopyTo(BlockToken[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<BlockToken> GetEnumerator()
            => this.TokenList.GetEnumerator();

        public int IndexOf(BlockToken item)
            => this.TokenList.ToList().IndexOf(item);
        public int IndexOf(string Label)
            => this.TokenList.Select(x => x._name).ToList().IndexOf(Label);

        public void Insert(int index, BlockToken item)
        {
            var BlockList = this.TokenList.ToList();
            BlockList.Insert(index, item);
            this.TokenList = BlockList.ToHashSet();
        }

        public bool Remove(BlockToken item)
        => this.TokenList.Remove(item);
        public void RemoveAt(int index)
        {
            var BlockList = this.TokenList.ToList();
            BlockList.RemoveAt(index);
            this.TokenList = BlockList.ToHashSet();
        }
        IEnumerator IEnumerable.GetEnumerator()
            => this.TokenList.GetEnumerator();
        public bool ResetVocabulary(string Prefix, bool Suffix = false)
        {
            foreach (var Item in this)
            {
                if (Item.Type == "Vocabulary")
                {
                    if (Suffix)
                    {
                        var TempName = Item._name + Prefix;
                        Item.ResetName(TempName);
                    }
                    else
                    {
                        var TempName = Prefix + Item._name;
                        Item.ResetName(TempName);
                    }
                }
            }

            return true;
        }
        public bool ResetVocabulary(IEnumerable<string> NewName)
        {
            if (this.VocabularyLabel.Count != NewName.ToList().Count) return false;
            int Count = 0;
            foreach (var Item in this)
            {
                if (Item.Type == "Vocabulary")
                {
                    Item.ResetName(NewName.ToList()[Count]);
                    Count++;
                }
            }
            
            return true;
        }
        public bool ResetCompatibleName(string OldName, string NewName)
        {
            foreach (var item in this)
            {
                if (item.Type == "CompatibleName" && item._name == OldName)
                {
                    item.ResetName(NewName);
                    return true;
                }
            }
            return false;
        }
        public BlockTokenList GetVocabularyToken()
        {
            var NewList = new BlockTokenList(this.Name + "Vocabulary", true);
            foreach (var Item in this)
            {
                if (Item.Type == "Vocabulary")
                    NewList.Add(Item);
            }
            return NewList;
        }
        public BlockTokenList GetCompatibleNameToken()
        {
            var NewList = new BlockTokenList(this.Name + "CompatibleName", true);
            foreach (var Item in this)
            {
                if (Item.Type == "CompatibleName")
                    NewList.Add(Item);
            }
            return NewList;
        }
        public override string ToString()
          => string.Join("\n", this.TokenList);
        public bool HasVocaLabel(string Label)
        {
            foreach (var Item in this)
            {
                if (Item.Type == "Vocabulary" && Item._name == Label)
                    return true;
            }

            return false;
        }
        public bool HasComLabel(string Label)
        {
            foreach (var Item in this)
            {
                if (Item.Type == "CompatibleName" && Item._name == Label)
                    return true;
            }

            return false;
        }
        public bool TryGetTransform(string Label, out Transform TS)
        {
            if (HasVocaLabel(Label))
            {
                TS = (Transform)this[IndexOf(Label)].Action;
                return true;
            }
            else
            {
                TS = new Transform();
                return false;
            }
        }
        public bool TryGetCompactible(string Label, out string BlockName)
        {
            if (HasComLabel(Label))
            {
                foreach (var item in this)
                {
                    if (item._name == Label)
                    {
                        BlockName = item.Action.ToString();
                        return true;
                    }
                }
                BlockName = "";
                return false;
            }
            else
            {
                BlockName = "";
                return false;
            }
        }
        public bool Equals(BlockTokenList other)
         => this.TokenList.Count == other.TokenList.Count && this.TokenList == other.TokenList;
        public static bool IsLabelDuplicated(BlockTokenList A, BlockTokenList B, out List<(int, int)> DuplicatedIndex)
        {
            DuplicatedIndex = new List<(int, int)>();
            bool Overlap = false;
            for (int i = 0; i < A.Count; i++)
                for (int j = 0; j < B.Count; j++)
                {
                    if (A[i]._name == B[j]._name)
                    {
                        Overlap = true;
                        DuplicatedIndex.Add((i, j));
                    }
                }
            return Overlap;
        }

        public object Clone()
        {
            var NewThis = new BlockTokenList(this.Name);
            NewThis.AddRange(this.TokenList);
          
            return NewThis;
        }

        public static BlockTokenList operator +(BlockTokenList A, BlockTokenList B)
        {
            if (A.Equals(B)) return A;
            var NewList = new BlockTokenList("ComposeList");
            if (A.Name == B.Name)
            {
                var AVocaTokenList = A.GetVocabularyToken();
                var BVocaTokenList = B.GetVocabularyToken();
                var AComTokenList = A.GetCompatibleNameToken();
                var BComTokenList = B.GetCompatibleNameToken();
                NewList.AddRange(AComTokenList.TokenList);
                foreach (var item in BComTokenList)
                {
                    if (AComTokenList.CompatibleLabel.Contains(item._name))
                    {
                        item.ResetName(B.Name + item._name);
                        NewList.Add(item);
                    }
                    else
                        NewList.Add(item);
                }
            }
            else
            {
                if (IsLabelDuplicated(A, B, out _))
                {
                    B.ResetVocabulary(B.Name);
                    A.AddRange(B.TokenList);
                    NewList.AddRange(A.TokenList);
                }
                else
                {
                    A.AddRange(B.TokenList);
                    NewList.AddRange(A.TokenList);
                }
            }
            return NewList;
        }
    }
}
