using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Interface;
using SGSFramework.Core.Token;
using Rhino.Geometry;
using SGSFramework.Core.BlockSetting.LabelBlockBase_2;
namespace SGSFramework.Core.Grammar
{    
    /*
     * ===================================Warming==================================
     * Compatible name ID(name) is for semantic purposes which will be remove from 
     * production rule.
     * The Index of production and semantic rule are correspondency.
     * ============================================================================
     * Given Rules
     * G : <P1>
     * <P1> = H I A I S I H I
     * where H, A and S are transformation terminals, and I is compatible Name terminals
     * It will be rewritten into 
     * <P1> = H A S H { <P1>.Val = TS(H) ID(I) TS(A) ID(I) TS(S) ID(I) TS(H) }
     * 
     * G: <P2>
     * <P2> = H I A I { <P2>.Val = S I H I }
     * where "{" "}" in the given rule means to redefine the semantic rule
     * and the former compatible name terminals will be removed
     * It will be rewritten into 
     * <P2> = H A { <P2>.Val = TS(S) ID(I) TS(H) ID(I) }
     * 
     * G: <P3>
     * <P3> = H A [ H ]
     * The "[" "]" represent the terminal Pop and Push. Since there aren't any Compatible 
     * Name terminals in the given rule, there aren't any compatible name in semantic rule
     * correspondingly.
     * It will be rewritten into
     * <P3> = H A [ H ] { <P3>.Val = TS(H) TS(A) Push TS(H) Pop }
     * 
     * G: <P4>
     * <P4> = 3 H I
     * Since there are integer in the rule, which represent the repitive actions for the rule,
     * it will be rewritten into
     * <P4> = H H H I { <P4>.Val = TS(H) TS(H) TS(H) ID(I) }
     * The operators cannot follows with any integer or the nonterminal first with operators 
     * Namely
     * G: <P5>
     * <P5> = 3 [ H I ]
     * This is error rules
     * 
     * G: <P6>
     * <P6> = <P4>
     * <P4> is a nonterminal
     * it woule be rewritten into
     * <P6> = <P4> { <P6>.Val = <P4>.Val }
     * where in the syntax directed translation, the <P6>.Val will inherit the <P4>.Val's 
     * semantic, which is TS(H) TS(H) TS(H) ID(I)
     * And
     * G: <P7>
     * <P7> = 4 <P4>
     * <P7> = <P4> <P4> <P4> { <P7>.Val = <P4>.Val <P4>.Val <P4>.Val }
     * 
     * G: <P8>
     * <P8> = H I | 3 <P3> 
     * <P3> = H A [ H ]
     * if the rule consist of "|" it would be reckoned as "or" will be devided into two production 
     * rule with different Index.
     * It will be rewritten into
     * <P8> = H I      { <P8>.Val = TS(H) ID(I) } Index : 8
     * <P8> = 3 <P3>   { <P8>.Val = <P3>.Val <P3>.Val <P3>.Val } Index : 9
     * The Or(8,9) is a function for top-down interprete selecting one of rules
     * Custom sementic need to be seperated with ","
     * 
     * G: <P9>
     * <P9> = H I | A I { <P9>.Val = 3 H I | 2 A I }
     * In this case, the rules will be seperated into two production rules and semantic rules, 
     * respectively 
     * It will be rewritten into
     * <P9> = H { <P9>.Val = TS(H) TS(H) TS(H) ID(I) }  Index : 10
     * <P9> = A { <V9>.Val = TS(A) TS(A) ID(I) }  Index : 11
     * However, if the or count cannot match to each others, either production rule or semantic 
     * rule will be cloned until both sides are same number or error. 
     * G: <P10>
     * <P10> = H I { <P10>.Val = 3 H I | 2 A I } //Error
     * It will be rewritten into
     * <P10> = H { <P10>.Val = TS(H) TS(H) TS(H) ID(I) }  Index : 12
     * <P10> = H { <P10>.Val = TS(A) TS(A) ID(I) }  Index : 13
     * <P10> = H Index: 12 and Index: 13 is Duplicated which cannot using in parser
     * Unless, we have other macro preprocessors to solve the semantic ambiguity
     * 
     * G: <P11>
     * <P11> = H I | A I { <P11>.Val = 3 H I }
     * It will be rewritten into
     * <P11> = H { <P11>.Val = TS(H) TS(H) TS(H) ID(I) }  Index : 14
     * <P11> = A { <P11>.Val = TS(H) TS(H) TS(H) ID(I) }  Index : 15
     * 
     * G: <P12>
     * <P12> = H I |     { <P12>.Val = TS(H) ID(I) | empty }
     * if a rule contain "", empty, or " " represent the Epsilon nonterminal, which can be place
     * in semantic rule as well. The Epsilon nonterminal don't do any actions for productions.
     * 
     */
    /// <summary>
    /// The Semantic rule is contains a sequence of label action or label
    /// </summary>
    public class SemanticRule : IRule<ActionToken>
    {
        public string Head { get; private set; }
        //public SGSGrammar sgsGrammar;
        public IReadOnlyList<ActionToken> Body { get; private set; }
        internal static SemanticRule UnSet { get; }
        public int Index { get; private set; }
        static SemanticRule()
        {
            UnSet = new SemanticRule(string.Empty, -1);
        }
        private SemanticRule(string Head, int index)
        {
            this.Head = Head;
            this.Index = index;
        }
        public List<ActionToken> ExecuteSemantic()
        {
            Stack<Transform> TSStack = new Stack<Transform>();
            if (this.Body.Count == 1)
            {
                object Tokens = new List<ActionToken>();

                if (this.Body[0].Run(ref Tokens, ref TSStack))
                    return (List<ActionToken>)Tokens;
            }
            else
            {
                List<ActionToken> Tokens = new List<ActionToken>();
                bool GotoPostExecute = false;
                for (int i = 1; i < this.Body.Count; i++)
                {
                    var FuncAction = Body[i] as IFuntionAction;
                    if (FuncAction.IsPreOrPostExecute == PreOrPostExecute.PreExecute)
                    {
                        object RefObj = new object();
                        Body[i].Run(ref RefObj, ref TSStack);
                        if (RefObj is List<ActionToken> RefObjActionTokens)
                        {
                            Tokens.AddRange(RefObjActionTokens);
                        }
                        else if (RefObj is ActionToken RefObjActionToken)
                        {
                            Tokens.Add(RefObjActionToken);
                        }
                    }
                    GotoPostExecute = !FuncAction.Executable();
                }
                if (GotoPostExecute)
                {
                    goto PostExecute;
                }

                var TempObj = new Object();

                if (this.Body[0].Run(ref TempObj, ref TSStack))
                {
                    Tokens.AddRange((List<ActionToken>)TempObj);
                }

                PostExecute:

                GotoPostExecute = false;
                for (int i = 1; i < this.Body.Count; i++)
                {
                    var FuncAction = Body[i] as IFuntionAction;
                    if (FuncAction.IsPreOrPostExecute == PreOrPostExecute.PostExecute)
                    {
                        object RefObj = new object();
                        Body[i].Run(ref RefObj, ref TSStack);
                        if (RefObj is List<ActionToken> RefObjActionTokens)
                        {
                            Tokens.AddRange(RefObjActionTokens);
                        }
                        else if (RefObj is ActionToken RefObjActionToken)
                        {
                            Tokens.Add(RefObjActionToken);
                        }
                    }
                    GotoPostExecute = !FuncAction.Executable();
                }
                if (GotoPostExecute)
                    return null;
                else
                    return Tokens;
            }

            return null;
        }
        public SemanticRule(string Head, IReadOnlyList<ActionToken> words, int index) : this(Head, index)
        {
            this.Body = words;
            //this.sgsGrammar = sgsGrammar;
        }
        public static IEnumerable<SemanticRule> CreateSemanticRules(SGSGrammar sgsGrammar, IEnumerable<string> Rules)
            => CreateSemanticRules(sgsGrammar, Rules.ToArray());
        public static IEnumerable<SemanticRule> CreateSemanticRules(SGSGrammar sgsGrammar, params string[] Rules)
        {
            ///Input are the pure semantic rule without "{" and "}"
            int index = 0;
            for (int j = 0; j < Rules.Length; j++)
            {
                var rule = Util.Util.CleanSequence(Rules[j]);
                string[] Tokens = rule.Split(' ');
                string Heading = "";
                List<ActionToken> AttTokens = new List<ActionToken>();
                bool FunctionTokenStart = false;
                Dictionary<int, KeyValuePair<string, List<ActionToken>>> TempRules = new Dictionary<int, KeyValuePair<string, List<ActionToken>>>();
                List<string> Input = new List<string>();
                for (int i = 0; i < Tokens.Length; i++)
                {
                    var token = Tokens[i];
                    if (token == "=")
                    {
                        if (AttTokens.Count != 1 || !(AttTokens[0] is NonterminalActionToken))
                        {
                            throw new InvalidOperationException("Syntax error");
                        }
                        Heading = AttTokens[0]._name;
                        AttTokens = new List<ActionToken>();
                    }
                    else if (token.Contains(".val"))
                    {
                        token = token.Remove(token.Length - 4);
                        AttTokens.Add(new NonterminalActionToken(token, sgsGrammar, true));
                    }
                    else if (token.Contains(".trans"))
                    {
                        token = token.Remove(token.Length - 6);
                        AttTokens.Add(new NonterminalActionToken(token, sgsGrammar, false));
                    }
                    else if (int.TryParse(token, out int number) && !FunctionTokenStart)
                    {
                        if (i != Tokens.Length - 1)
                        {
                            //======================================Caution======================================
                            ///Adustment from ActionBase.DefinedOperators.Contains....
                            var DefinedOperators = new List<string>() { "|", "{", "}", "[", "]" };
                            if (!DefinedOperators.Contains(Tokens[i + 1]))
                            {
                                for (int k = 0; k < number; k++)
                                {
                                    var TempToken = Tokens[i + 1];
                                    if (Tokens[i + 1].Contains(".val"))
                                    {
                                        TempToken = Tokens[i + 1].Remove(Tokens[i + 1].Length - 4);
                                        AttTokens.Add(new NonterminalActionToken(TempToken, sgsGrammar));
                                    }
                                    else if (Tokens[i + 1].Contains(".trans"))
                                    {
                                        TempToken = Tokens[i + 1].Remove(Tokens[i + 1].Length - 6);
                                        AttTokens.Add(new NonterminalActionToken(TempToken, sgsGrammar, false));
                                    }
                                    else if (sgsGrammar.ComputeNonterminals().Contains(TempToken))
                                    {
                                        AttTokens.Add(new NonterminalActionToken(TempToken, sgsGrammar));
                                    }
                                    else if (sgsGrammar.ComputeTerminals().Contains(TempToken))
                                    {
                                        if (sgsGrammar.labelBlockTable.TryGetBlockToken(TempToken, out var blockToken))
                                            AttTokens.Add(new TerminalActionToken(blockToken));
                                    }
                                }
                                i++;
                            }
                            //===================================================================================
                        }
                        else
                        {
                            throw new InvalidOperationException("Syntax error");
                        }
                    }
                    else if (token == " " || token == "" && !FunctionTokenStart)
                    {
                        continue;
                    }
                    else if (token == "empty" && !FunctionTokenStart)
                        continue;
                    else if (token == "[" && !FunctionTokenStart)
                        AttTokens.Add(new TerminalActionToken(PopAndPush.Push));
                    else if (token == "]" && !FunctionTokenStart)
                        AttTokens.Add(new TerminalActionToken(PopAndPush.Pop));
                    else if (sgsGrammar.ComputeNonterminals().Contains(token) && !FunctionTokenStart)
                        AttTokens.Add(new NonterminalActionToken(token, sgsGrammar));
                    else if (sgsGrammar.labelBlockTable.Terminals.Contains(token) && !FunctionTokenStart)
                    {
                        if (sgsGrammar.labelBlockTable.TryGetBlockToken(token, out var blockToken))
                            AttTokens.Add(new TerminalActionToken(blockToken));
                        else
                            throw new InvalidOperationException("Syntax error");
                    }
                    else if (token == "|" && !FunctionTokenStart)
                    {
                        if (Tokens.Contains(","))
                        {
                            if (TempRules.Count == 0)
                            {
                                TempRules.Add(
                                    index++,
                                    new KeyValuePair<string, List<ActionToken>>
                                    (Heading, new List<ActionToken>() { new SeqValActionToken(AttTokens) }
                                    ));
                            }
                            else
                                TempRules.Add(index++, TempRules[index]);
                            AttTokens = new List<ActionToken>();
                        }
                        else
                        {
                            var TempAttTokens = new List<ActionToken>() { new SeqValActionToken(AttTokens) };
                            yield return new SemanticRule(Heading, TempAttTokens, index++);
                            AttTokens = new List<ActionToken>();
                        }
                    }
                    else if (token == ",")
                    {
                        if (!FunctionTokenStart)
                        {
                            ///Place the nonterminal and terminal action list into tempRules

                            var AttToken = new SeqValActionToken(AttTokens);
                            TempRules.Add(index++, new KeyValuePair<string, List<ActionToken>>(Heading, new List<ActionToken>() { AttToken }));
                            AttTokens = new List<ActionToken>();

                            FunctionTokenStart = true;
                        }
                        else
                        {
                            foreach (var kvp in TempRules)
                            {
                                TempRules[kvp.Key].Value.AddRange(AttTokens);
                            }
                            AttTokens = new List<ActionToken>();
                            Input = new List<string>();
                        }
                    }
                    else if (FunctionTokenStart)
                    {
                        Input.Add(token);
                        foreach (var FunAction in sgsGrammar.AdditionalSementicFunction)
                        {
                            if (FunAction.Action(out ActionToken AttToken, Input.ToArray()))
                            {
                                AttTokens.Add(AttToken);
                                Input = new List<string>();
                            }
                        }
                    }
                    else if (i == Tokens.Length - 1)
                    {
                        
                    }
                }
                if (!Tokens.Contains(","))
                {
                    var TempAttTokens = new List<ActionToken>() { new SeqValActionToken(AttTokens) };
                    yield return new SemanticRule(Heading, TempAttTokens, index++);
                }
                else
                {
                    foreach (var kvp in TempRules)
                    {
                        AttTokens = new List<ActionToken>();
                        var ActionTokenList = TempRules[kvp.Key].Value;
                        var ProductionRuleIndex = sgsGrammar.ProductionRules.Where(x => x.Head == Heading).Select(x => x.Index).ToList();
                        if (ProductionRuleIndex.Contains(kvp.Key))
                            yield return new SemanticRule(Heading, ActionTokenList, kvp.Key);
                        else
                            throw new Exception("Production rules cannot match");
                    }
                }
            }
        }
        public override string ToString()
        {
            return string.Concat(
                this.Head + ".val",
                " = ",
                this.Body.Count > 0 ?
                string.Join(" , ", this.Body) : "empty"
                );
        }
        public bool Equals(SemanticRule other)
        {
            if (this is null && other is null) return true;
            else if ((this is null && !(other is null)) ||
                (!(this is null) && other is null)) return false;
            else
                return ReferenceEquals(other, this)
                       || (this.Head.Equals(other.Head)
                           && this.Body.SequenceEqual(other.Body));
        }
        public override bool Equals(object obj)
        => obj as ProductionRule is null && this.Equals(obj as SemanticRule);
        public override int GetHashCode()
            => base.GetHashCode();
        public static bool operator ==(SemanticRule A, SemanticRule B)
        {
            if (A is null && B is null) return true;
            else if ((A is null && !(B is null)) ||
                (!(A is null) && B is null)) return false;
            else
                return ReferenceEquals(A, B)
                || (A.Head.Equals(B.Head)
                    && A.Body.SequenceEqual(B.Body));
        }
        public static bool operator !=(SemanticRule A, SemanticRule B)
            => !(A == B);
    }
}
