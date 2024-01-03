using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace SGSFramework.Core.Interpreter.InterpreterReader
{
    [Obsolete]
    public class CommonAction : ActionBase
    {
        public override int GetPrecedence { get => 
                this.Interpreter._actionExe.Count - 1; 
            protected set => throw new Exception("Always the last procedence. Cannot be set"); }

        public override string Example => $"<N> = # Production a* ∈ {"{V|N}"} - P U {"|"} #";

        private int StackCount = 0;
        public CommonAction() : base() { StackCount = 0; }
        public override string Action(ref Phrase phrase, string heading)
        {
            if (!phrase.FindRule(heading, out string sequence))
                return $"Rule {heading} construct failed \n";
            string _cmd = "===============CommonAction==============\n";
            _cmd += $"{heading} starts \n";
            var words = sequence.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (int.TryParse(words[i], out int result))
                {
                    if (i == words.Length - 1)
                    {
                        _cmd += $"Rule {words[i]} cannot be compute.\n";
                        break;
                    }
                    if (this.Interpreter.labelblockTable.Terminals.Contains(words[i + 1]))
                    {
                        for (int j = 0; j < result - 1; j++)
                        {
                            this.Interpreter.CalledBlockList.Add(words[i + 1]);
                            _cmd += $"{i} => {words[i + 1]} \n";
                        }
                    }
                    else if (this.Interpreter.phraseInfo.Head.Contains(words[i + 1]))
                    {
                        this.Interpreter.phraseInfo.FindRule(words[i + 1], out string Sequence);
                        var Line = words[i + 1] + " = " + Sequence;
                        if (this.Interpreter.TargetAction(Line, out int Index, out _))
                        {
                            StackCount++;
                            _cmd += $"Stack start => {StackCount} \n";
                            for (int j = 0; j < result - 1; j++)
                            {
                                _cmd += this.Interpreter._actionRule[Index](ref phrase, words[i + 1]);
                            }
                            StackCount--;
                            _cmd += $"Stack start => {StackCount} \n";
                        }
                        else
                        {
                            _cmd += $"Rule {heading} => {i+1}, {words[i + 1]} cannot be compute.\n";
                            break;
                        }
                    }
                    else if (words[i] == " ")
                    {
                        continue;
                    }
                    else
                    {
                        _cmd += $"Rule {heading} => {i}, {words[i]} cannot be compute.";
                        break;
                    }
                }
                else if (this.Interpreter.labelblockTable.Terminals.Contains(words[i]))
                {
                    this.Interpreter.CalledBlockList.Add(words[i]);
                    _cmd += $"{i} => {words[i]}\n";
                }
                else if (this.Interpreter.phraseInfo.Head.Contains(words[i]))
                {
                    this.Interpreter.phraseInfo.FindRule(words[i], out string Sequence);
                    var Line = words[i] + " = " + Sequence;
                    if (this.Interpreter.TargetAction(Line, out int Index, out _))
                    {
                        StackCount++;
                        _cmd += $"Stack start => {StackCount} \n";
                        _cmd += this.Interpreter._actionRule[Index](ref phrase, words[i]);
                        StackCount--;
                        _cmd += $"Stack start => {StackCount} \n";
                    }
                    else
                    {
                        _cmd += $"Rule {words[i + 1]} cannot be compute.\n";
                        break;
                    }
                }
                else
                {
                    _cmd += $"Rule {words[i]} cannot be compute.\n";
                    break;
                }
            }
            _cmd += $"{heading} Ends \n=========================================\n";
            return _cmd;
        }
        public bool IsRecursion(string line)
        {
            var heading = Util.Util.CleanSequence(line.Split('=')[0]);
            var Sequence = Util.Util.CleanSequence(line.Split('=')[1]);

            if (Sequence.Contains(heading))
                return true;
            else
                return false;
        }
        public override bool ExecuteRule(string RuleSequence, out string TargetHeading)
        {
            if (!RuleSequence.Contains("="))
            {
                TargetHeading = "";
                return false;
            }
            var words = RuleSequence.Split(' ');
            for (int i = 0; i < DefinedOperators.Count; i++)
            {
                if (words.Contains(DefinedOperators[i]))
                {
                    TargetHeading = "";
                    return false;
                }
            }

            TargetHeading = this.GetNonterminal(RuleSequence);
            return true;


        }
        public override string ToString()
        {
            return "CommonAction";
        }
    }
}
