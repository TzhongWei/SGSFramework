using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Interpreter.SemanticManager;
using Rhino.Geometry;

namespace SGSFramework.Core.Interpreter.InterpreterReader
{
    /// <summary>
    /// The derived class allows the shape interpreter can read the specific rules type or customise rule type
    /// </summary>
    [Obsolete("This actionbase and derived classes are obsolete, please refer to interpreter_2", false)]
    public abstract class ActionBase
    {
        public abstract string Example { get; }
        public bool IsNoCommand { get; protected set; } = false;
        public ShapeInterpreter Interpreter { get; set; }
        protected ActionBase() 
        { 
            Interpreter = null; 
        }
        public static List<string> DefinedOperators { get; protected set; } = new List<string>() { "|", "{", "}", "[", "]" };
        public abstract bool ExecuteRule(string RuleSequence, out string TargetHeading);
        /// <summary>
        /// Set up the behaviour that the interpreter can read specific syntax
        /// </summary>
        /// <param name="phrase">reference a phrase for modifing the rule</param>
        /// <param name="heading">a nonterminal label of appling rule</param>
        /// <returns></returns>
        public abstract string Action(ref Phrase phrase, string heading);
        public override string ToString()
        {
            return this.GetType().Name + $"\n {Example}";
        }
        public abstract int GetPrecedence { get; protected set; }
        protected string GetNonterminal(string Line)
        {
            return Util.Util.CleanSequence(Line.Split('=')[0]);
        }
    }
    public interface CustomOperatorAction
    {
        void AddOperator();
    }
}
