using Rhino.Geometry;
using SGSFramework.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Token
{
    /// <summary>
    /// SeqValActionToken, sequence value actiontoken, save the semantic rules into a function, 
    /// it is different from .Val, which is a tag for nonterminal represent the value need to be 
    /// inheritance or synthesis
    /// <P>.val = SeqVal( TS(H) ID(I) <Y>.val )
    /// From the former example, SeqVal is a function to store and return the sequence value in the list. 
    /// If it is called, returns the list of sequence.
    /// </summary>
    public class SeqValActionToken : ActionToken
    {
        public override string Print { get; protected set; }
        public override string _name => "SeqVal";
        public IReadOnlyList<ActionToken> NonterminlAndTerminalActions;
        public override bool Equals(IToken<string, LabelAction<object>> other)
            => _name == other._name;
        public SeqValActionToken(IReadOnlyList<ActionToken> ActionTokens)
        {
            this.NonterminlAndTerminalActions = ActionTokens;
            this.Print = "SeqVal( " + string.Join(" ", NonterminlAndTerminalActions) + " )";
            this._attributePtr = SeqValActionLabel;
        }
        private object SeqValActionLabel(object Action, ref Stack<Transform> TSStack, params object[] CustomAction)
         => NonterminlAndTerminalActions;
        /// <summary>
        /// Using this run it will return a list of nonterminal and terminal ActionTokens
        /// </summary>
        /// <param name="Action">obsolete</param>
        /// <param name="TSStack">obsolete</param>
        /// <param name="CustomAction">obsolete</param>
        /// <returns>List of nonterminal and terminal ActionTokens</returns>
        public override bool Run(ref object Action, ref Stack<Transform> TSStack, params object[] CustomAction)
        {

            Action = this._attributePtr(Action, ref TSStack);
            return true;
        }
    }
}
