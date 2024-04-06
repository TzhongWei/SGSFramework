using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Interpreter.InterpreterReader
{
    [Obsolete]
    public class MutiPatternsAttributeAction : ActionBase, CustomOperatorAction
    {
        public override int GetPrecedence { get; protected set; }

        public override string Example => "<N> = #Production a* ∈ {V|N} - P # ( .Val = #Semantic Rule a* ∈ {{V|N}}  - P # )";

        private HashSet<string> SemanticHeading { get; }
        public override string Action(ref Phrase phrase, string heading)
        {
            if (!phrase.FindRule(heading, out string sequence))
                return $"Rule {heading} construct failed \n";

            string _cmd = "=================MutiPatternsAttributeAction================";
            _cmd += $"{heading} starts \n";
            var Rule = sequence.Split('(')[0];
            var Semantic = sequence.Split('(')[1];
            Semantic = Semantic.Remove(Semantic.Length - 1);

            _cmd += $"Grammar rule is {heading}.CFG = {Rule} \n" +
                $"Semantic Rule is {heading}{Semantic} \n";

            var TempList = new List<string>();
            TempList.Add("[");

            var TempHeading = heading + ".val";
            var SemanticSeq = Util.Util.CleanSequence(Semantic.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries)[1]);
            var TempSequence = $"{TempHeading} = {SemanticSeq}";
            phrase.AddRule(TempHeading, SemanticSeq);

            if (this.Interpreter.TargetAction(TempSequence, out int index, out _))
            {
                _cmd += this.Interpreter._actionRule[index](ref phrase, TempHeading);
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
            TempSequence = $"{TempHeading} = {Rule}";
            phrase.AddRule(TempHeading, Rule);

            if (this.Interpreter.TargetAction(TempSequence, out index, out _))
            {
                _cmd += this.Interpreter._actionRule[index](ref phrase, TempHeading);
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
        public MutiPatternsAttributeAction() : base()
        {
            AddOperator();
            AddSemanticHeading();
        }
        public void AddSemanticHeading()
        {
            if (!(SemanticHeading is null))
                return;
            foreach (var kvpRule in this.Interpreter.phraseInfo.Rules)
            {
                if (kvpRule.Value.Contains("(") && kvpRule.Value.Contains(")"))
                    this.SemanticHeading.Add(kvpRule.Key);
            }
        }
        public void AddOperator()
        {
            if (ActionBase.DefinedOperators.Contains("(") || ActionBase.DefinedOperators.Contains(")"))
            {
                ActionBase.DefinedOperators.Remove("(");
                ActionBase.DefinedOperators.Remove(")");
            }
            int IndexOf = ActionBase.DefinedOperators.IndexOf("|");
            ActionBase.DefinedOperators.Insert(IndexOf, "(");
            ActionBase.DefinedOperators.Insert(IndexOf, ")");
            GetPrecedence = IndexOf;
        }

        public override bool ExecuteRule(string RuleSequence, out string TargetHeading)
        {
            if (!RuleSequence.Contains("="))
            {
                TargetHeading = "";
                return false;
            }
            
            var KeyCount = RuleSequence.Split(' ').Where(x => x == ")" || x == "(" || x == ".Val").ToList().Count;
            if (RuleSequence.Contains("(") && RuleSequence.Contains(")") && RuleSequence.Contains(".Val")
                && RuleSequence.Last() == ')' && KeyCount == 3)
            {
                var SemanticSe = Util.Util.CleanSequence(RuleSequence.Split('(')[1]);
                TargetHeading = this.GetNonterminal(RuleSequence);
                //No recursion in for Semantic rule
                if (!this.SemanticHeading.Contains(TargetHeading))
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
