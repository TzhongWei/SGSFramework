using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SGSFramework.Core.BlockSetting.LabelBlockBase_2;


namespace SGSFramework.Core.Parser
{
    public class GrammarTable
    {
        public string cmd => _cmd.ToString();
        public StringBuilder _cmd = new StringBuilder();
        public List<string> _RuleLines { get; private set;}
        public GrammarTable()
        {
            _cmd = new StringBuilder();

            _RuleLines = new List<string>();
 
        }
        public bool AddRule(string Nonterminal, string Sequence)
        {
            return false;
        }
        public bool AddLabelTable(LabelBlockTable labelTable)
        {
            var Terminals = labelTable.Terminals;
            bool Result = false;
            for (int i = 0; i < Terminals.Count; i++)
            {
                Result = AddRule($"{Terminals[i]}", Terminals[i]);
            }
            return Result;
        }
    }
}
