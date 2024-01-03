using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Parser.LRParseing.ParingInterface;
using SGSFramework.Core.Parser.Grammar;

namespace SGSFramework.Core.Parser.LRParseing
{
    public abstract class BaseParsing<TAction> : IParsing<TAction>
        where TAction : class, IAction
    {
        public CFGGrammar cfgGrammar { get; private set; }
        public ParsingTable<TAction> ParsingTable { get; protected set; }
        protected BaseParsing(CFGGrammar cfgGrammar)
        {
            this.cfgGrammar = cfgGrammar;
        }
        /// <summary>
        /// Classify a grammar as this type of parser
        /// </summary>
        public abstract void Classify();
        /// <summary>
        /// Generate the parsing table for this type of parser
        /// </summary>
        /// <returns></returns>
        public abstract IParsingTable<TAction> CreateParsingTable();
        public override string ToString()
        {
                return this.GetType().Name;
        }
    }
}
