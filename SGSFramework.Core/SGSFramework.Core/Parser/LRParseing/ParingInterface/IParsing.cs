using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Parser.Grammar;

namespace SGSFramework.Core.Parser.LRParseing.ParingInterface
{
    public interface IParsing<out TAction>
        where TAction : class, IAction
    {
        CFGGrammar cfgGrammar { get; }
        void Classify();
        IParsingTable<TAction> CreateParsingTable();
    }
}
