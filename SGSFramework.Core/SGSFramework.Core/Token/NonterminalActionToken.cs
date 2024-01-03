using Rhino.Geometry;
using SGSFramework.Core.Interface;
using System;
using System.Collections.Generic;
using SGSFramework.Core.Grammar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Token
{
    /// <summary>
    /// NonterminalActionToken only accept nonterminal where the token with 
    /// </summary>
    public class NonterminalActionToken : ActionToken
    {
        public override string Print { get; protected set;}
        private SGSGrammar sgsGrammar;
        public override string _name { get; }
        /// <summary>
        /// Return a semantic rules
        /// if nonterminal is ambigious, it would randomly select one of which.
        /// This val action can return the semantic of grammars
        /// </summary>
        /// <param name="Any"></param>
        /// <param name="TSStack"></param>
        /// <param name="CustomAction"></param>
        /// <returns></returns>
        private object NonterminalValAction(object Any, ref Stack<Transform> TSStack, params object[] CustomAction)
        {
            var semanticRules = sgsGrammar.SemanticRules.Where(x => x.Head == this._name).ToList();
            var Rand = new Random(DateTime.Now.Millisecond);
            if (semanticRules.Count > 1)
            {
                var Seed = Rand.Next(0, semanticRules.Count);
                return semanticRules[Seed];
            }
            else if (semanticRules.Count == 1)
                return semanticRules[0];
            else
                return null;
        }
        /// <summary>
        /// Return a semantic rules
        /// if nonterminal is ambigious, it would randomly select one of which.
        /// it could only implement the transformation semantic or address ".tran" with other nonterminals
        /// </summary>
        /// <param name="Any"></param>
        /// <param name="TSStack"></param>
        /// <param name="CustomAction"></param>
        /// <returns></returns>
        private object NonterminalTransAction(object Any, ref Stack<Transform> TSStack, params object[] CustomAction)
        {
            var productionRules = 
                sgsGrammar.ProductionRules.Where(x => x.Head == this._name).Select(x => x.Body).ToList();
            var Rand = new Random();
            IReadOnlyList<string> SelectRule =
                productionRules.Count == 1 ? productionRules[0] :
                productionRules[Rand.Next(0, productionRules.Count - 1)];
            List<ActionToken> NewActionToken = new List<ActionToken>();
            foreach (var token in SelectRule)
            {
                if (sgsGrammar.ComputeNonterminals().Contains(token))
                {
                    NewActionToken.Add(new NonterminalActionToken(token, sgsGrammar, false));
                }
                else if (token == "[")
                {
                    NewActionToken.Add(new TerminalActionToken(PopAndPush.Push));
                }
                else if (token == "]")
                    NewActionToken.Add(new TerminalActionToken(PopAndPush.Pop));
                else if (sgsGrammar.labelBlockTable.Terminals.Contains(token))
                {
                    if (sgsGrammar.labelBlockTable.TryGetBlockToken(token, out var ActionToken))
                        NewActionToken.Add(new TerminalActionToken(ActionToken));
                }
                else
                    ExceptionNotion += $"{token} is cannot be recongisable \n";
            }
            if (NewActionToken.Count == 0) return NewActionToken;
            else
            {
                return new SemanticRule(this._name,
                    new List<ActionToken>() { new SeqValActionToken(NewActionToken) }, -1);
            }
        }
        public override bool Equals(IToken<string, LabelAction<object>> other)
        => this._name == other._name && this.ToString() == other.ToString();
        public NonterminalActionToken(string NonterminalToken, SGSGrammar sgsGrammar, bool IsVal = true)
        {
            
            this.sgsGrammar = sgsGrammar;
            this._name = NonterminalToken;
            if (IsVal)
            {
                this._attributePtr = NonterminalValAction;
                Print = this._name + ".val";
            }
            else
            {
                this._attributePtr = NonterminalTransAction;
                Print = this._name + ".trans";
            }
        }
        public override bool Run(ref object semanticRules, ref Stack<Transform> TSStack, params object[] CustomAction)
        {
            var ActionTokens = (SemanticRule)this._attributePtr(new object(), ref TSStack);
            if (ActionTokens is null)
            {
                semanticRules = null;
                return false;
            }
            else
            {
                semanticRules = ActionTokens;
                return true;
            }
        }
    }
}
