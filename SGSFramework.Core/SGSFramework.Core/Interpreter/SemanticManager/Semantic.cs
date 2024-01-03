using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Interpreter.SemanticManager
{
    /// <summary>
    /// The class store the Semantic from the grammar rules.
    /// This process is to transform a t item into a key for syntax directed process
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete]
    public struct Semantic<T> : IPhraseRules<List<T>>
    {
        public Dictionary<string, List<T>> SemanticPair { get; private set; }

        public List<string> Head => SemanticPair.Keys.ToList();

        public int Count => SemanticPair.Count;

        public Dictionary<string, List<T>> Rules => SemanticPair;
        public void AddSemantic(string NonterminalKey, T item)
        {
            if (SemanticPair == null)
                this.SemanticPair = new Dictionary<string, List<T>>();
            if (SemanticPair.ContainsKey(NonterminalKey))
                SemanticPair[NonterminalKey].Add(item);
            else
            {
                SemanticPair.Add(NonterminalKey, new List<T>() { item });
            }
        }
        public string Match(T t)
        {
            var NonterminalKeys = SemanticPair.Keys.ToList();
            for (int i = 0; i < SemanticPair.Values.ToList().Count; i++)
            {
                if (SemanticPair.Values.ToList()[i].Contains(t))
                    return NonterminalKeys[i];
            }
            return "";
        }
        public bool HasRule(string Key) => SemanticPair.ContainsKey(Key);
        public bool AddRule(string NonterminalSymbol, List<T> Sequence)
        {
            if (HasRule(NonterminalSymbol))
            {
                return false;
            }
            else
            {
                this.SemanticPair.Add(NonterminalSymbol, Sequence);
                return true;
            }
        }

        public bool RemoveRule(string NonterminalSymbol)
        {
            if (HasRule(NonterminalSymbol))
            {
                SemanticPair.Remove(NonterminalSymbol);
                return true;
            }
            else
                return false;
        }
        public bool FindRule(string NonterminalSymbol, out List<T> Sequence)
        {
            if (HasRule(NonterminalSymbol))
            {
                Sequence = this.SemanticPair[NonterminalSymbol];
                return true;
            }
            else
            {
                Sequence = new List<T>();
                return false;
            }
        }
        public List<T> this[string Key]
        {
            get
            {
                if (SemanticPair.ContainsKey(Key))
                    return SemanticPair[Key];
                else
                    throw new Exception($"No {Key} in this semantic pair");
            }
        }
    }
}
