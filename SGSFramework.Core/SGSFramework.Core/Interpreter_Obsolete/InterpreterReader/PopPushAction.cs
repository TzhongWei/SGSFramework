using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Interpreter.InterpreterReader
{
    [Obsolete]
    public class PopPushAction : ActionBase
    {
        public PopPushAction() : base() { }
        private int _getprecedence = 0;
        public override int GetPrecedence { get => _getprecedence; protected set => _getprecedence = value; }

        public override string Example => "<N> = # Production a ∈ {V|N U '[' U ']' - P U '|' }";

        public override string Action(ref Phrase phrase, string heading)
        {
            if (!phrase.FindRule(heading, out string sequence))
                return $"Rule {heading} construct failed \n";
            string _cmd = "===============PopPushAction==============\n";
            _cmd += $"{heading} starts \n";
            var words = sequence.Split(new char[] { '[', ']' });
            var PushAndPop = sequence.Split(' ').Where(x => x == "[" || x == "]").ToList();

            int Count = 0;
            var TempList = new List<string>();

            if (sequence[0] == '[' | sequence[0] == ']')
            {
                TempList.Add(PushAndPop[0]);
                PushAndPop.RemoveAt(0);
            }

            for (int i = 0; i < words.Length; i++)
            {
                if(words[i] == " ")
                {
                    _cmd += "Blank\n";
                    if (PushAndPop.Count > 0)
                    {
                        TempList.Add(PushAndPop[0]);
                        PushAndPop.RemoveAt(0);
                    }
                    continue;
                }
                _cmd += "Set a temperary segment between push and pop\n"+
                "=>  " +
                $"{heading}_{Count} = {words[i]} \n";

                phrase.AddRule($"{heading}_{Count}", words[i]);
                string TempHeading = $"{heading}_{Count}";
                var TempSequence = $"{heading}_{Count} = {words[i]}";
                Count++;
                if (this.Interpreter.TargetAction(TempSequence, out int Index, out _))
                {
                    _cmd += this.Interpreter._actionRule[Index](ref phrase, TempHeading);
                    ///merage Grammar
                    TempList.AddRange(this.Interpreter.CalledBlockList);
                    this.Interpreter.CalledBlockList.Clear();
                    if (PushAndPop.Count > 0)
                    {
                        TempList.Add(PushAndPop[0]);
                        PushAndPop.RemoveAt(0);
                    }
                }
                else
                {
                    _cmd += "There are some error in the PopPushAction \n";
                    break;
                }
                if (!phrase.RemoveRule(TempHeading))
                {
                    _cmd += $"{heading}_{Count} doesn't exist in the phrase. \n" +
                        $"The segment set up failed. \n";
                    return _cmd;
                }
            }

            _cmd += $"{heading} Ends \n=========================================\n";
            this.Interpreter.CalledBlockList = TempList;
            return _cmd;
        }
        public override bool ExecuteRule(string RuleSequence, out string TargetHeading)
        {
            if (!RuleSequence.Contains("="))
            {
                TargetHeading = "";
                return false;
            }
            var heading = Util.Util.CleanSequence(RuleSequence.Split('=')[0]);
            var Sequence = Util.Util.CleanSequence(RuleSequence.Split('=')[1]);
            if (!Sequence.Contains(heading) && RuleSequence.Contains("[") || RuleSequence.Contains("]"))
            {
                if (Sequence.Contains("["))
                    this._getprecedence = DefinedOperators.IndexOf("[");
                else
                    this._getprecedence = DefinedOperators.IndexOf("]");
                TargetHeading = this.GetNonterminal(RuleSequence);
                return true;
            }
            TargetHeading = "";
            return false;
        }
        public override string ToString()
        {
            return this.GetType().Name + "\n" + Example;
        }
    }
}
