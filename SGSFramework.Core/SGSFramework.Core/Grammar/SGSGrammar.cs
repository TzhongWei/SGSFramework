using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Interface;
using SGSFramework.Core.BlockSetting.LabelBlockBase_2;
using SGSFramework.Core.Grammar.Preprocessor;
namespace SGSFramework.Core.Grammar
{
    /// <summary>
    /// This grammar class contains the collections of context-free grammar with production rules
    /// and attribute grammar with semantic rules
    /// </summary>
    public class SGSGrammar
    {
        public string cmd { get; private set; } = "";
        /// <summary>
        /// labelBlockTable to set up semantic
        /// </summary>
        public LabelBlockTable labelBlockTable;
        public IReadOnlyList<ProductionRule> ProductionRules { get; private set; }
        public IReadOnlyList<SemanticRule> SemanticRules { get; private set; }
        public HashSet<Macro> Macros { get; private set; } = new HashSet<Macro>();
        /// <summary>
        /// All tokens in the production rules
        /// </summary>
        private readonly HashSet<string> _tokens;
        /// <summary>
        ///  A list of all terminal tokens
        /// A terminal is the smallest common denominator,
        /// it can not be split into other non-terminals or terminals
        /// </summary>
        private HashSet<string> _terminals;
        /// <summary>
        /// A list of all non-terminal tokens
        /// A non-terminal consists of 0 or more terminals
        /// or other non-terminals
        /// </summary>
        private HashSet<string> _nonterminals;
        /// <summary>
        /// A list of nullable non-terminals
        /// </summary>
        private HashSet<string> _nullable;
        /// <summary>
        /// The axiom of this grammar
        /// </summary>
        private string _start { get; set; }
        //Each nonterminal has a set of terminals as a first token
        private IDictionary<string, HashSet<string>> _firstSet;
        //Each nonterminal has a set of terminals as a first token
        private IDictionary<string, HashSet<string>> _followSet;
        /// <summary>
        /// Additional Semantic function that semantic rule can construct. 
        /// The additional semantic function will be set gobally.
        /// </summary>
        public HashSet<IFuntionAction> AdditionalSementicFunction { get; private set; } = new HashSet<IFuntionAction>();
        public List<string> OriginalRule { get; }
        /// <summary>
        /// Constructor
        /// </summary>
        private SGSGrammar()
        {
            this.ProductionRules = new List<ProductionRule>();
            this.SemanticRules = new List<SemanticRule>();
            this._tokens = new HashSet<string>();
        }
        public SGSGrammar(IEnumerable<string> Rules, LabelBlockTable labelBlockTable) : this()
        {
            this.labelBlockTable = labelBlockTable;
            this.AddPreprocessor(new DefinedMacro());
            OriginalRule = Rules.ToList();
        }
        /// <summary>
        /// Get the axiom of grammar
        /// </summary>
        /// <returns></returns>
        public string ComputeStartNonTerminal()
        {
            if (this._start is null)
            {
                this._start = this.ProductionRules[0].Head;
            }
            return this._start;
        }
        /// <summary>
        /// Compute nonterminal string labels from production rule
        /// </summary>
        /// <returns></returns>
        public HashSet<string> ComputeNonterminals()
        {
            if (!(this._nonterminals is null))
                return this._nonterminals;

            HashSet<string> nonterminals = new HashSet<string>();
            foreach (var pro in this.ProductionRules)
                nonterminals.Add(pro.Head);
            return this._nonterminals = nonterminals;
        }
        /// <summary>
        /// Compute terminal string labels from production rule
        /// </summary>
        /// <returns></returns>
        public HashSet<string> ComputeTerminals()
        {
            if (!(this._terminals is null))
                return this._terminals;
            var terminals = new HashSet<string>();
            var nonterminals = this.ComputeNonterminals();
            foreach (var token in this.ComputeToken())
            {
                if (!nonterminals.Contains(token))
                    terminals.Add(token);
            }
            return this._terminals = terminals;
        }
        /// <summary>
        /// Compute token string labels from production rule
        /// </summary>
        /// <returns></returns>
        public HashSet<string> ComputeToken()
        {
            if (!(this._tokens.Count == 0))
                return this._tokens;

            foreach (var Pro in ProductionRules)
            {
                this._tokens.Add(Pro.Head);
                foreach (var body in Pro.Body)
                {
                    this._tokens.Add(body);
                }
            }
            return this._tokens;
        }
        public HashSet<string> ComputeNullable()
        {
            if (!(this._nullable is null))
                return this._nullable;

            var nullable = new HashSet<string>();
            IList<ProductionRule> productions = this.ProductionRules.ToList();
            List<string> added = new List<string>();
            int currentRightPosition;

            do
            {
                added.Clear();
                foreach (var production in productions)
                {
                    for (currentRightPosition = 0; currentRightPosition < production.Body.Count; currentRightPosition++)
                    {
                        //Break at the first unmarked body token
                        if (!nullable.Contains(
                            production.Body[currentRightPosition]
                            ))
                        {
                            break;
                        }
                    }
                    //If all body tokens were nullable, the head
                    //is nullable, and continue the loop to verify with
                    //the newly added nullable
                    if (!nullable.Contains(production.Head)
                        && currentRightPosition == production.Body.Count)
                    {
                        nullable.Add(production.Head);
                        added.Add(production.Head);
                    }
                }

            } while (added.Count > 0);
            return this._nullable = nullable;
        }
        public HashSet<string> First(string Nonterminal)
        {
            if (this._firstSet.Count == 0) this.ComputeFirstSet();
            return this._firstSet[Nonterminal];
        }
        public IDictionary<string, HashSet<string>> ComputeFirstSet()
        {
            if (!(this._firstSet is null) && this._firstSet.Count > 0)
            {
                return this._firstSet;
            }

            var Nonterminals = this.ComputeNonterminals();
            IList<ProductionRule> productions = this.ProductionRules.ToList();
            var nullable = this.ComputeNullable();

            Relation immediate, propagation;
            int j;
            immediate = new Relation();

            //For each token in the body of a production, add the
            //first non-terminal after a sequence of nullable tokens
            foreach (ProductionRule production in productions)
            {
                for (j = 0; j < production.Body.Count; j++)
                {
                    if (!nullable.Contains(production.Body[j]))
                    {
                        break;
                    }
                }
                //If the first non-nullable symbol is a terminal, add it to the
                //immediate set of the head
                if (j < production.Body.Count &&
                    !Nonterminals.Contains(production.Body[j]))
                {
                    immediate.AddRelation(production.Head, production.Body[j]);
                }

            }

            propagation = new Relation();
            //Add all nullable non-terminals in the body and the
            //next non-terminal (if any)
            foreach (ProductionRule production in productions)
            {
                foreach (string str in production.Body)
                {
                    if (Nonterminals.Contains(str))
                    {
                        propagation.AddRelation(production.Head, str);
                    }

                    if (!nullable.Contains(str))
                    {
                        break;
                    }
                }
            }

            //Propagate the relations
            return this._firstSet
                    = Relation.Propagate(immediate, propagation);

        }
        /// <summary>
        /// Compute all tokens which can follow a particular token
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, HashSet<string>> ComputeFollowSet()
        {
            if (!(this._followSet is null))
            {
                return this._followSet;
            }

            HashSet<string> nonTerminals = this.ComputeNonterminals();
            IList<ProductionRule> productions = this.ProductionRules.ToList();
            HashSet<string> nullable = this.ComputeNullable();
            IDictionary<string, HashSet<string>> firstSet
                = this.ComputeFirstSet();

            Relation immediate, propagation;

            immediate = new Relation();
            propagation = new Relation();

            //EOF is always in the followset of Start
            immediate.AddRelation
            (
                this.ComputeStartNonTerminal(),
                "$"
            );

            /* Add the first set of every non-terminal k following every
             * non-terminal j to the immediate follow set of j until the first
             * terminal has been reached or until the non-terminal k is not 
             * nullable
             *
             * Given a production X -> ... A β, follow(A) includes first(β),
             * except for the empty string. */
            foreach (var production in productions)
            {
                for (int j = 0; j < production.Body.Count - 1; j++)
                {
                    //Skip terminals
                    if (!nonTerminals.Contains(production.Body[j]))
                    {
                        continue;
                    }

                    //Add the first set of the tokens following token j
                    for (int k = j + 1; k < production.Body.Count; k++)
                    {
                        //If the symbol is a terminal, add it and stop adding
                        if (!nonTerminals.Contains(production.Body[k]))
                        {
                            immediate.AddRelation
                            (
                                production.Body[j],
                                production.Body[k]
                            );

                            break;
                        }

                        //If the token is a non-terminal, add each token in the
                        //first set of that non-terminal
                        foreach (string s in firstSet[production.Body[k]])
                        {
                            immediate.AddRelation(production.Body[j], s);
                        }

                        //Stop adding if the non-terminal is not nullable
                        if (!nullable.Contains(production.Body[k]))
                        {
                            break;
                        }
                    }
                }
            }

            //Given a production B -> ... A β where β is nullable, follow(A)
            //includes follow(B)
            foreach (var production in productions)
            {
                //Scan from the end of the body of the production to the
                //beginning...
                for (int j = production.Body.Count - 1; j >= 0; j--)
                {
                    //If the symbol is a non-terminal, add the head as a
                    //relation of that non-terminal
                    if (nonTerminals.Contains(production.Body[j]))
                    {
                        propagation.AddRelation
                        (
                            production.Body[j],
                            production.Head
                        );
                    }

                    //Stop adding if the token is not nullable
                    if (!nullable.Contains(production.Body[j]))
                    {
                        break;
                    }
                }
            }

            return this._followSet
                    = Relation.Propagate(immediate, propagation);
        }
        //Compute the first set from a range of tokens
        public IEnumerable<string> GetFirst(IEnumerable<string> tokens)
        {
            HashSet<string> nonTerminals = this.ComputeNonterminals();
            HashSet<string> terminals = this.ComputeTerminals();
            HashSet<string> nullable = this.ComputeNullable();
            IDictionary<string, HashSet<string>> firstSet = this.ComputeFirstSet();

            foreach (string s in tokens)
            {
                //First of EOF remains EOF
                if (s == tokens.Last())
                {
                    yield return s;
                    yield break;
                }
                //First is always the first terminal
                else if (terminals.Contains(s))
                {
                    yield return s;
                    yield break;
                }
                else if (nonTerminals.Contains(s))
                {
                    if (firstSet.ContainsKey(s))
                    {
                        //Return the entire first set for s
                        foreach (string k in firstSet[s])
                        {
                            yield return k;
                        }
                    }

                    //If the non-terminal s is not nullable, the entire first
                    //set has been found, else continue with any remaining (non)
                    //terminals
                    if (!nullable.Contains(s))
                    {
                        yield break;
                    }
                }
                else
                {
                    throw new InvalidOperationException
                    (
                        $"Unexpected symbol {s}"
                    );
                }
            }
        }

        //Return the first cycle if there is one
        public IEnumerable<string> ComputeFirstCycle()
        {
            HashSet<string> nonTerminals = this.ComputeNonterminals();
            HashSet<string> nullable = this.ComputeNullable();
            Relation relation = new Relation();
            int j, k;

            foreach (ProductionRule production in this.ProductionRules)
            {
                for (j = 0; j < production.Body.Count; j++)
                {
                    //If the token is a terminal, skip it
                    if (!nonTerminals.Contains(production.Body[j]))
                    {
                        continue;
                    }

                    //Only add the relation between the head and j
                    //if all other tokens are terminals or nullable
                    for (k = 0; k < production.Body.Count; k++)
                    {
                        if (j == k)
                        {
                            continue;
                        }

                        if (!nonTerminals.Contains(production.Body[k]))
                        {
                            break;
                        }

                        if (!nullable.Contains(production.Body[k]))
                        {
                            break;
                        }
                    }

                    if (k == production.Body.Count)
                    {
                        relation.AddRelation
                        (
                            production.Head,
                            production.Body[j]
                        );
                    }
                }
            }

            return Relation.Cycle(relation);
        }
        public bool SetRules()
        {
            var Rules = OriginalRule;
            if (Preprocessor(ref Rules))
            {
                var RulePairs = GenerateRule(Rules).ToList();
                if (RulePairs.Count == 0)
                {
                    cmd += "production rule and semantic rule generate failed\n";
                    return false;
                }
                var ProductionList = new List<string>();
                var SemanticList = new List<string>();
                foreach (var Pair in RulePairs)
                {
                    if (Pair == ("", ""))
                    {
                        cmd += "Some Rules setting errors\n";
                    }
                    else
                    {
                        ProductionList.Add(Pair.Item1);
                        SemanticList.Add(Pair.Item2);
                    }
                }
                this.ProductionRules = ProductionRule.CreateProductionRules(ProductionList).ToList();
                this.SemanticRules = SemanticRule.CreateSemanticRules(this, SemanticList).ToList();
                if (this.ProductionRules.Count != this.SemanticRules.Count)
                {
                    cmd += "Rules count cannot match\n";
                    return false;
                }
                for (int i = 0; i < this.ProductionRules.Count; i++)
                {
                    var ProRule = ProductionRules[i];
                    var SemRule = SemanticRules[i];
                    cmd += ProRule.Head + $", Index : {ProRule.Index}  =>\n";
                    cmd += "  " + ProRule.ToString() + "\n";
                    cmd += "  " + SemRule.ToString() + "\n";
                }
                return true;
            }
            else
            {
                cmd += "Preprocessor construct failed \n";
                return false;
            }
        }
        public void AddPreprocessor(Macro macro)
        {
            macro.sgsGrammar = this;
            this.Macros.Add(macro);
        }
        public bool Preprocessor(ref List<string> Rules)
        {
            bool Result = true;
            var CopyRule = new List<string>(Rules);
            foreach (var rule in Rules)
            {
                if (rule.Contains("#"))
                {
                    foreach (var marco in Macros)
                    {
                        if (marco.Execute(rule))
                        {
                            Result |= marco.Preprocessing(ref CopyRule);
                            cmd += marco.cmd;
                        }
                    }
                }
            }
            Rules = CopyRule;
            return Result;
        }
        private IEnumerable<(string, string)> GenerateRule(IEnumerable<string> Rules)
        {
            foreach (var Rule in Rules)
            {
                var RulePair = GenerateRule(Rule);
                if (RulePair == ("", ""))
                    cmd += $"{Rule} differentiate failed \n";
                yield return RulePair;
            }
        }
        private (string, string) GenerateRule(string Rule)
        {
            string InitialProRule, InitialSemRule;
            if (Rule.Contains("{") && Rule.Contains("}") && Rule.Last() == '}')
            {
                Rule = Rule.Remove(Rule.Length - 1);
                InitialProRule = Util.Util.CleanSequence(Rule.Split('{')[0]);
                InitialSemRule = Util.Util.CleanSequence(Rule.Split('{')[1]);
                if (!AcceptRule(InitialProRule) || !AcceptRule(InitialSemRule))
                {
                    cmd += $"{Rule} Syntax error\n";
                    return ("", "");
                }
                var InitialSemRuleHead = Util.Util.CleanSequence(InitialSemRule.Split('=')[0]);
                if (!InitialSemRuleHead.Contains(".val"))
                    InitialSemRuleHead += ".val";
                if (!HeadingMatch(InitialProRule.Split('=')[0], InitialSemRuleHead))
                {
                    cmd += $"{Rule} production and semantic headings cannot match \n";
                    if (InitialSemRule.Split('=')[0].Contains(".Val"))
                    {
                        cmd += $"{InitialSemRule} contains .Val\n";
                    }
                    return ("", "");
                }
                InitialProRule = CleanProRule(InitialProRule);

                if (!CheckProRule(InitialProRule))
                {
                    cmd += $"{InitialProRule} contains duplicated production rules \n";
                    return ("", "");
                }
                var SemWords = Util.Util.CleanSequence(InitialSemRule.Split('=')[1].Split(',')[0]);
                var RestSemWords = InitialSemRule.Split('=')[1].Split(',').Select(x => Util.Util.CleanSequence(x)).ToList();
                RestSemWords.RemoveAt(0);
                var InitialProRuleHead = Util.Util.CleanSequence(InitialProRule.Split('=')[0]);
                var initialProRuleBody = Util.Util.CleanSequence(InitialProRule.Split('=')[1]);
                if (!AmbiguityChecking(InitialProRule, ref InitialSemRule))
                    return ("", "");
                //SemWords = $"[ {SemWords} ] {initialProRuleBody}"; <= composed in AmbiguityChecking

                TransOrValPro(ref initialProRuleBody);
                InitialProRule = $"{InitialProRuleHead} = {initialProRuleBody}";
                var SemRuleSeq = Util.Util.CleanSequence(InitialSemRule.Split('=')[1]);
                InitialSemRule = $"{InitialSemRuleHead} = {SemRuleSeq} " + ((RestSemWords.Count == 0) ? "" : " , " + string.Join(" , ", RestSemWords));

                return (InitialProRule, InitialSemRule);
            }
            else
            {
                if (!AcceptRule(Rule)) return ("", "");
                InitialProRule = CleanProRule(Rule);
                InitialSemRule = Util.Util.CleanSequence(Rule.Split('=')[0]) + ".val" + " =" + Rule.Split('=')[1];

                var InitialProRuleHead = Util.Util.CleanSequence(InitialProRule.Split('=')[0]);
                var initialProRuleBody = Util.Util.CleanSequence(InitialProRule.Split('=')[1]);
                TransOrValPro(ref initialProRuleBody);
                InitialProRule = $"{InitialProRuleHead} = {initialProRuleBody}";

                return (InitialProRule, InitialSemRule);
            }

            bool CheckProRule(string CleanedProRule)
            {
                if (CleanedProRule.Contains('|'))
                {
                    var PureProRule = Util.Util.CleanSequence(CleanedProRule.Split('=')[1]);
                    var ProRuleSegs = PureProRule.Split('|').Select(x => Util.Util.CleanSequence(x)).ToList();
                    for (int i = 0; i < ProRuleSegs.Count; i++)
                    {
                        for (int j = 0; j < ProRuleSegs.Count; j++)
                        {
                            if (i != j && ProRuleSegs[i] == ProRuleSegs[j])
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                else
                    return true;
            }

            void TransOrValPro(ref string ProRule)
            {
                var Words = ProRule.Split(' ');
                for (int i = 0; i < Words.Length; i++)
                {
                    if (Words[i].Contains(".val"))
                        Words[i] = Words[i].Remove(Words[i].Length - 4);
                    else if (Words[i].Contains(".trans"))
                        Words[i] = Words[i].Remove(Words[i].Length - 6);
                }
                ProRule = Util.Util.CleanSequence(string.Join(" ", Words));
            }

            ///Check semantic ambiguity issues
            bool AmbiguityChecking(string FinalProRule, ref string FinalSemRule)
            {
                var ProHead = Util.Util.CleanSequence(FinalProRule.Split('=')[0]);
                var SemHead = Util.Util.CleanSequence(FinalSemRule.Split('=')[0]);
                var ProSeq = Util.Util.CleanSequence(FinalProRule.Split('=')[1]);
                var SemSeq = Util.Util.CleanSequence(FinalSemRule.Split('=')[1]);
                var ProSegCount = ProSeq.Split('|').Length;
                var SemSegCount = SemSeq.Split('|').Length;
                if (ProSegCount == SemSegCount && ProSegCount == 1)
                {
                    FinalSemRule = $"{SemHead} = [ {SemSeq} ] {ProSeq}";
                    return true;
                }
                else if (ProSegCount == SemSegCount && ProSegCount > 1)
                {
                    List<string> TempSeq = new List<string>();
                    for (int i = 0; i < ProSegCount; i++)
                    {
                        TempSeq.Add($"[ {SemSeq.Split('|')[i]} ] {ProSeq.Split('|')[i]}");
                    }
                    FinalSemRule = SemHead  + " = " + string.Join(" | ", TempSeq);
                    return true;
                }
                else if (ProSeq.Split('|').Length > SemSeq.Split('|').Length)
                {
                    List<string> TempSeq = new List<string>();
                    for (int i = 1; i < ProSegCount - SemSegCount; i++)
                        SemSeq += " | " + SemSeq.Split('|').Last();

                    for (int i = 0; i < ProSegCount; i++)
                    {
                        TempSeq.Add($"[ {SemSeq.Split('|')[i]} ] {ProSeq.Split('|')[i]}");
                    }
                    FinalSemRule = SemHead + " = " + string.Join(" | ", TempSeq);
                    return true;
                }
                else
                {
                    cmd += $"{ProHead} has semantic ambiguity\n";
                    return false;
                }
            }

            bool AcceptRule(string AnyRule)
                => AnyRule.Split('=').Length == 2;

            bool HeadingMatch(string ProRuleHead, string SemRuleHead)
            {
                var TempProHead = Util.Util.CleanSequence(ProRuleHead);
                var TempSemHead = Util.Util.CleanSequence(SemRuleHead.Remove(SemRuleHead.Length - 4));
                return TempProHead == TempSemHead;
            }

            ///Remove Compatible name from production rule
            string CleanProRule(string OriProRule)
            {
                var words = OriProRule.Split(' ').ToList();
                var NewBody = new List<string>();
                for (int i = 0; i < words.Count; i++)
                {
                    if (int.TryParse(words[i], out _))
                        if (this.labelBlockTable.IsCompactingName(words[i + 1]))
                            continue;

                    if (this.labelBlockTable.IsCompactingName(words[i]))
                        continue;
                    NewBody.Add(words[i]);
                }
                return string.Join(" ", NewBody);
            }
        }
        public void AddSemanticAction(IFuntionAction SemFunctions)
        {
            SemFunctions.sgsGrammar = this;
            this.cmd += SemFunctions.FunctionName + " is included. \n";
            this.AdditionalSementicFunction.Add(SemFunctions);
        }
        public bool ContainIndex(int index)
        {
            foreach (var rule in ProductionRules)
                if (rule.Index == index)
                    return true;
            return false;
        }
        /// <summary>
        /// Get the index of production and semantic rule from a nonterminal label
        /// </summary>
        /// <param name="Nonterminal"></param>
        /// <returns></returns>
        public int[] NonterminalIndex(string Nonterminal)
            => this.ComputeNonterminals().Contains(Nonterminal) ?
            this.ProductionRules.Where(x => x.Head == Nonterminal).Select(x => x.Index).ToArray() : new int[1]{-1};
    }
}
