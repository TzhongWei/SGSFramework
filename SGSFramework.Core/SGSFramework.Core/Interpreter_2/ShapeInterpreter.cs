using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using SGSFramework.Core.Grammar;
using SGSFramework.Core.Token;
using SGSFramework.Core.BlockSetting.LabelBlockBase_2;

namespace SGSFramework.Core.Interpreter_2
{
    /// <summary>
    /// This class here is just an interface to get the semantic from the rules and constitute them 
    /// into a complexity, which is suitable for testing production rules and compose hierarchy component
    /// </summary>
    public class ShapeInterpreter
    {
        /// <summary>
        /// _cmd is a property to record the data processing
        /// </summary>
        private StringBuilder _cmd = new StringBuilder();
        /// <summary>
        /// print cmd
        /// </summary>
        public string cmd => _cmd.ToString();
        /// <summary>
        /// Grammar for managing rules
        /// </summary>
        public SGSGrammar sgsGrammar;
        private ShapeInterpreter() { }
        public ShapeInterpreter(SGSGrammar sgsGrammar)
        {
            if (!(sgsGrammar is null))
            {
                this._cmd.Append("=====================================================\n");
                this.sgsGrammar = sgsGrammar;

                this._cmd.Append(sgsGrammar.cmd + "\n Grammar rules is set up \n");
                this._cmd.Append("=====================================================\n");
            }
            else
            {
                this._cmd.Append("sgsGrammar is a null grammar");
            }
        }
        public ShapeInterpreter(IEnumerable<string> Rules, LabelBlockTable labelBlockTable)
        {
            this._cmd.Append("=====================================================\n");
            if (!labelBlockTable.Isinvalid)
            {
                this.sgsGrammar = new SGSGrammar(Rules, labelBlockTable);
            }
            this._cmd.Append(this.sgsGrammar.cmd);
            this._cmd.Append(this.sgsGrammar.cmd + "\n Grammar rules is set up \n");
            this._cmd.Append("=====================================================\n");
        }
        public Dictionary<string, TerminalActionTokenList> CallAttributeTokenList => sgsGrammar.labelBlockTable.Actions;
        /// <summary>
        /// Run the target production rule
        /// </summary>
        /// <param name="Nonterminal">provides a target rule nonterminal or Unset which is start 
        /// nonterminal of rules</param>
        /// <param name="Index">If the grammar rules is ambigious, index can be used to assign the 
        /// specific rule or it's always the first one</param>
        public void Run(string Nonterminal = "UnSet", int Index = -1, bool ReCompute = false)
        {
            if (sgsGrammar.ProductionRules.Count == 0) return;
            
            this.sgsGrammar.labelBlockTable.ResetBlocks();
            Nonterminal = Nonterminal == "UnSet" || !sgsGrammar.ComputeNonterminals().Contains(Nonterminal)?
                this.sgsGrammar.ComputeStartNonTerminal() : Nonterminal;
            Index = Index == -1 || !sgsGrammar.NonterminalIndex(Nonterminal).ToList().Contains(Index)? 
                sgsGrammar.NonterminalIndex(Nonterminal)[0] : Index;

            if (sgsGrammar.labelBlockTable.Actions.ContainsKey(Nonterminal) && !ReCompute)
            {
                _cmd.Append($"{Nonterminal} is set up");
                return;
            }

            if (Index == -1)
            {
                _cmd.Append($"Input {Nonterminal} and it's Index is failed \n"); 
                return;
            }
            _cmd.Append($"{Nonterminal} Index: {Index} is computesd\n");
            var SemanticAxiom = this.sgsGrammar.SemanticRules.
                Where(x => x.Head == Nonterminal && x.Index == Index).ToList()[0];
            TerminalActionTokenList terminalActionTokens = new TerminalActionTokenList(sgsGrammar.labelBlockTable);
            int Level = 0;
            this.InterpretSemantic(SemanticAxiom, ref terminalActionTokens, ref Level);
            if (terminalActionTokens.Count == 0)
            {
                _cmd.Append("Semantic construct failed \n");
            }
            if (this.sgsGrammar.labelBlockTable.Actions.ContainsKey(Nonterminal))
            {
                this.sgsGrammar.labelBlockTable.Actions[Nonterminal] = terminalActionTokens;
                this.sgsGrammar.labelBlockTable.CalledLabels[Nonterminal] = terminalActionTokens.Select(x => x._name).ToList();
            }
            else
            {
                this.sgsGrammar.labelBlockTable.Actions.Add(Nonterminal, terminalActionTokens);
                this.sgsGrammar.labelBlockTable.CalledLabels.Add(Nonterminal, terminalActionTokens.Select(x => x._name).ToList());
            }
        }
        private void InterpretSemantic(SemanticRule _SemanticRule, ref TerminalActionTokenList terminalActions, ref int Levels)
        {
            int Count = 0;
            var TSStack = new Stack<Transform>();
            object Rule = new object();
            string margin = "|";
            if (Levels > 0)
            {
                for (int i = 0; i < Levels; i++)
                    margin += " ";
                margin += "|";
                _cmd.Append($"{margin} Tree => {Levels}\n");
            }
            foreach (var ActionToken in _SemanticRule.ExecuteSemantic())
            {
                if (ActionToken is TerminalActionToken TerminalToken)
                {
                    _cmd.Append($"{margin}{Count} => {TerminalToken.Print} \n");
                    terminalActions.Add(TerminalToken);
                }
                else if (ActionToken is NonterminalActionToken NonterminalToken)
                {
                    if (NonterminalToken.Run(ref Rule, ref TSStack))
                    {
                        Levels++;
                        var TempSemRule = Rule as SemanticRule;
                        _cmd.Append($"{margin}{Count} => {TempSemRule.Head} \n");
                        InterpretSemantic(TempSemRule, ref terminalActions, ref Levels);
                        Levels--;
                    }
                    else
                    {
                        _cmd.Append($"{margin}{NonterminalToken.Print} cannot found \n");
                        return;
                    }
                }
                else
                {
                    if (ActionToken.Run(ref Rule, ref TSStack))
                    {
                        _cmd.Append($"{ActionToken.Print} is executed \n");
                    }
                    else
                    {
                        _cmd.Append($"{ActionToken.Print} is executed failed \n");
                    }
                }
                Count++;
            }
        }
        public Dictionary<string, List<string>> CalledLabels => sgsGrammar.labelBlockTable.CalledLabels;
    }
}
