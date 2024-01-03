using System;
using System.Collections.Generic;
using System.Linq;

namespace SGSFramework.Core.Interpreter.InterpreterReader
{
    [Obsolete]
    public class ChangeAttributeAction : ActionBase, CustomOperatorAction
    {
        private int _getPrecedence = 0;
        public ChangeAttributeAction() : base()
        {
            AddOperator();
        }
        public override int GetPrecedence { get => _getPrecedence; protected set => _getPrecedence = value; }

        public override string Example => $"<N> = #Production a* ∈ {"{V|N}"} - P U {"|"} # ( .Val = #Semantic Rule a* ∈ {"{V|N}"}  - P U {"|"} # )";

        public override string Action(ref Phrase phrase, string heading)
        {
            if (!phrase.FindRule(heading, out string sequence))
                return $"Rule {heading} construct failed \n";

            string _cmd = "===============DynamicAction==============\n";
            _cmd += $"{heading} starts \n";
            var words = sequence.Split(' ').ToList();
            string PatternBag = "";
            string SemanticBag = "";
            bool Switch = true;

            for (int i = 0; i < words.Count - 1; i++)
            {
                if (Switch && words[i] != "(")
                {
                    if (!this.Interpreter.labelblockTable.IsCompactingName(words[i]))
                        PatternBag += words[i] + " ";
                    else
                    {
                        _cmd += "Compacting Name isn't allow here.\n";
                    }
                }
                else
                {
                    if (words[i] != "(")
                        SemanticBag += words[i] + " ";
                }
                if (words[i] == "(" && words[i+1] == ".Val")
                    Switch = false;
            }
            if (SemanticBag.Length == 0)
            {
                _cmd += $"there are some syntax errors in {heading} = {sequence}\n";
                return _cmd;
            }
            var TempList = new List<string>();
            PatternBag = Util.Util.CleanSequence(PatternBag);
            SemanticBag = Util.Util.CleanSequence(SemanticBag);
            _cmd += $"Grammar Rule is {heading}.CFG = {PatternBag} \n" +
                $"Semantic Rule is {heading}{SemanticBag} \n";
            this.Interpreter.CalledBlockList.Add("[");

            var TempHeading = heading + ".val";
            var SemanticSeq = Util.Util.CleanSequence((SemanticBag.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries)[1]));
            var TempSequence = $"{TempHeading} = {SemanticSeq}";
            phrase.AddRule(TempHeading, SemanticSeq);

            if (this.Interpreter.TargetAction(TempSequence, out int Index, out _))
            {
                _cmd += this.Interpreter._actionRule[Index](ref phrase, TempHeading);
                TempList.AddRange(this.Interpreter.CalledBlockList);
                this.Interpreter.CalledBlockList.Clear();
            }
            else
            {
                _cmd += $"There are some error in the {TempHeading} \n";
                return _cmd;
            }
            TempList.Add("]");

            TempHeading = heading + ".cfg";
            TempSequence = $"{TempHeading} = {PatternBag}";
            phrase.AddRule(TempHeading, PatternBag);

            if (this.Interpreter.TargetAction(TempSequence, out Index, out _))
            {
                _cmd += this.Interpreter._actionRule[Index](ref phrase, TempHeading);
                TempList.AddRange(this.Interpreter.CalledBlockList);
                this.Interpreter.CalledBlockList.Clear();
            }
            else
            {
                _cmd += $"There are some error in the {TempHeading} \n";
                return _cmd;
            }


            _cmd += $"{heading} Ends \n=========================================\n";
            this.Interpreter.CalledBlockList = TempList;
            return _cmd;
        }
        public void AddOperator()
        {
            if (ActionBase.DefinedOperators.Contains("(") || ActionBase.DefinedOperators.Contains(")")) 
                return;
            int IndexOf = ActionBase.DefinedOperators.IndexOf("|");
            ActionBase.DefinedOperators.Insert(IndexOf + 1, "(");
            ActionBase.DefinedOperators.Insert(IndexOf + 1, ")");
            _getPrecedence = IndexOf + 1;
        }
        public override bool ExecuteRule(string RuleSequence, out string TargetHeading)
        {
            if (!RuleSequence.Contains("="))
            {
                TargetHeading = "";
                return false;
            }

            var KeyCount = RuleSequence.Split(' ').Where(x => x == ")" || x == "(" || x == ".Val").ToList().Count;

            if (RuleSequence.Contains("(") && RuleSequence.Contains(")") && 
                RuleSequence.Contains(".Val") && RuleSequence.Last() ==')' && KeyCount == 3 && 
                !RuleSequence.Contains("|"))
            {
                var SemanticSe = Util.Util.CleanSequence(RuleSequence.Split('(')[1]);
                TargetHeading = this.GetNonterminal(RuleSequence);
                if (!SemanticSe.Contains(TargetHeading) && !SemanticSe.Contains("|"))
                    return true;
                else
                {
                    TargetHeading = "";
                    return false;
                }
            }
            else
            {
                TargetHeading = "";
                return false;
            }
        }
    }
}
