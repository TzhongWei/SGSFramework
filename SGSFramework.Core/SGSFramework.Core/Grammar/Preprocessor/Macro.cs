using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Grammar.Preprocessor
{
    public abstract class Macro
    {
        public SGSGrammar sgsGrammar;
        public string cmd { get; protected set; } = "";
        public abstract string Example { get; }
        protected Macro() {}
        public abstract string Key { get; }
        public virtual bool Execute(string Line)
        => Line.Contains(Key);
        /// <summary>
        /// Pre process all rule before set into production rules and semantic rules
        /// </summary>
        /// <param name="Rule"></param>
        /// <returns></returns>
        public abstract bool Preprocessing(ref List<string> Rules);
        public override string ToString()
        => Example;
    }
}
