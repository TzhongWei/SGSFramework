using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Interpreter
{
    /// <summary>
    /// all grammar rules would be regulated in production 
    /// "Rule" which contains a <Nonterminals> = <Sequence>
    /// </summary>
    /// <typeparam name="T">T is the type of Sequence</typeparam>
    [Obsolete("IPhraseRules is obsolete, please refer to the the ProductionRule and SemanticRule in Grammar and interface", false)]
    public interface IPhraseRules<T>
    {
        List<string> Head { get; }
        int Count { get; }
        Dictionary<string, T> Rules { get; }
        bool HasRule(string NonterminalSymbol);
        bool AddRule(string NonterminalSymbol, T Sequence);
        bool RemoveRule(string NonterminalSymbol);
        bool FindRule(string NonterminalSymbol, out T Sequence);
    }
}
