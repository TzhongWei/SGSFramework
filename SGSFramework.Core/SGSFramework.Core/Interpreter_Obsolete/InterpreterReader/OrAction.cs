using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SGSFramework.Core.Interpreter.InterpreterReader
{
    [Obsolete]
    public class OrAction : ActionBase
    {
        private Random Rand = new Random(DateTime.Now.Second);
        public override int GetPrecedence { get; protected set; }

        public override string Example => "<N> = # Production a ∈ {V|N U '|' }";

        public OrAction() : base()
        {
            GetPrecedence = ActionBase.DefinedOperators.IndexOf("|");
        }
        public override string Action(ref Phrase phrase, string heading)
        {
            if (!phrase.FindRule(heading, out string sequence))
                return $"Rule {heading} construct failed \n";

            string _cmd = "================OrActionc=================\n";
            var Segments = sequence.Split('|').ToList();
            var RandIndex = Rand.Next(0,Segments.Count - 1);
            var TempSeg = Segments[RandIndex];
            _cmd = $"{TempSeg} is selected \n";
            int Count = 0;
            var TempHeading = heading + $"{Count}";
            phrase.AddRule(TempHeading, TempSeg);
            Count++;
            if (this.Interpreter.TargetAction(TempSeg, out int Index, out _))
            {
                _cmd += this.Interpreter._actionRule[Index](ref phrase, TempSeg);
            }
            else
            {
                _cmd += "There are some error in the OrAction \n";
                return _cmd;
            }
            phrase.RemoveRule(TempHeading);
            _cmd += $"{heading} Ends \n=========================================\n";
            return _cmd;
        }

        public override bool ExecuteRule(string RuleSequence, out string TargetHeading)
        {
            if (!RuleSequence.Contains("="))
            {
                TargetHeading = "";
                return false;
            }
            TargetHeading = this.GetNonterminal(RuleSequence);
            return RuleSequence.Contains("|");
        }
    }
}
