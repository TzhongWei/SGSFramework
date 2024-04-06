using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Interpreter.InterpreterReader
{
    [Obsolete]
    public class DefinedLabelAction : ActionBase, CustomOperatorAction
    {
        public DefinedLabelAction() : base()
        {
            AddOperator();
            ChangedLabel = new HashSet<string>();
        }
        public void AddOperator()
        {
            if (!DefinedOperators.Contains("#defined"))
                DefinedOperators.Insert(0, "#defined");
            
            this._getprocedence = DefinedOperators.IndexOf("#defined");
        }
        private int _getprocedence = 0;
        public override int GetPrecedence { get => _getprocedence; protected set => _getprocedence = value; }

        public override string Example => "#defined OLDNONTERMINAL NEWNONTERMINAL";

        public override bool ExecuteRule(string RuleSequence, out string TargetHeading)
        {
            IsNoCommand = true;
            var Operators = ActionBase.DefinedOperators.Where(x => x != "#defined").ToList();
            var words = RuleSequence.Split(' ');
            var result = words.Contains("#defined");
            for (int i = 0; i < Operators.Count; i++)
                result &= !words.Contains(Operators[i]);

            if (result)
            {
                TargetHeading = RuleSequence;
                return true;
            }
            else
            {
                TargetHeading = "";
                return false;
            }
        }
        private HashSet<string> ChangedLabel;
        public override string Action(ref Phrase phrase, string heading)
        {
            //#defined OLDLABEL NEWLABEL
            var words = heading.Split(' ');
            if (words.Length != 3)
                return $"The {"#defined"} format incorrect, it should be #defined OLD_LABEL NEW_LABEL";
            string _cmd = "===============DefinedLabelAction==============\n";
            _cmd += $"{heading} starts \n";
            var OldLabel = words[1];
            var NewLabel = words[2];
            if (this.Interpreter.labelblockTable.Terminals.Contains(NewLabel) || ChangedLabel.Contains(NewLabel))
                _cmd += "The New label is an existed terminals in the labelblocks, please use another key.\n";
            else if (this.Interpreter.labelblockTable.Terminals.Contains(OldLabel))
            {
                //The label alteration only can function in this rules phrase set
                if (phrase.ChangeLabel(OldLabel, NewLabel))
                {
                    _cmd = $"replace {OldLabel} into {NewLabel} successfully.\n";
                }
                ChangedLabel.Add(NewLabel);
            }
            else
            {
                _cmd = $"There is no {OldLabel} in the rules\n";
            }
            return _cmd;
        }
        public override string ToString()
        {
            return this.GetType().Name;
        }
    }
}
