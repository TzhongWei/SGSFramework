using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Interpreter.InterpreterReader
{
    [Obsolete]
    public class DefinedParamsAction : ActionBase, CustomOperatorAction
    {
        public override string Example => "#defined Param IDENTIFER NATURAL_NUMBER";

        public override int GetPrecedence { get; protected set; }
        public DefinedParamsAction() : base()
        {
            AddOperator();
        }

        public override string Action(ref Phrase phrase, string heading)
        {
            var words = heading.Split(' ');
            if(words.Length != 4)
                return $"The {"#defined Param"} format incorrect, it should be {Example}";
            string _cmd = "===============DefinedLabelAction==============\n";
            _cmd += $"heading starts \n";
            var Param = words[2];
            var Num = words[3];

            if (phrase.ChangeLabel(Num, Param))
            {
                _cmd = $"replace {Param} into {Num} successfully.\n";
            }
            else
            {
                _cmd = $"There is no {Param} in the rules\n";
            }
            return _cmd;
        }

        public void AddOperator()
        {
            if (!DefinedOperators.Contains("Param"))
                DefinedOperators.Insert(0, "Param");
            if (!DefinedOperators.Contains("#defined"))
                DefinedOperators.Insert(0, "#defined");

            this.GetPrecedence = DefinedOperators.IndexOf("Param");
        }

        public override bool ExecuteRule(string RuleSequence, out string TargetHeading)
        {
            IsNoCommand = true;
            var Operators = ActionBase.DefinedOperators.Where(x => x != "#defined" ).ToList();
            Operators = Operators.Where(x => x != "Param").ToList();
            var words = RuleSequence.Split(' ');
            var result = words.Contains("#defined") && words.Contains("Param");

            for (int i = 0; i < Operators.Count; i++)
                result &= !words.Contains(Operators[i]);

            result &= words[0] == "#defined" & words[1] == "Param";

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
    }
}
