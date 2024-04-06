using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Interface;

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
    /// The production rule is the transformation setting or other nonterminals for grammar rule. 
    /// Only and only if Semantic Rule contains placing block.
    /// Each label need to be seperated with a space " ".
    /// </summary>
    public class ProductionRule : IRule<string>, IEquatable<ProductionRule>
    {
        public string Head { get; private set; }

        public IReadOnlyList<string> Body { get; }

        public int Index { get; private set; }
        //A static "Unset" production so it can be used in certain contexts
        //(like dictionaries)
        internal static ProductionRule UnSet { get; }
        static ProductionRule()
        {
            ProductionRule.UnSet = new ProductionRule(string.Empty, -1);
        }
        private ProductionRule(string heading, int index)
        {
            this.Head = heading;
            this.Index = index;
        }
        public ProductionRule(string heading, IReadOnlyList<string> words, int index) : this(heading, index)
        {
            this.Body = words;
        }
        public static IEnumerable<ProductionRule> CreateProductionRules(IEnumerable<string> Rules)
            => CreateProductionRules(Rules.ToArray());
        public static IEnumerable<ProductionRule> CreateProductionRules
            (params string[] Rules)
        {
            //The input also cannot be marco in this compiler
            //The input rule must be the pure production rules with any Compatible Name in the rule
            int index = 0;
            for (int j = 0; j < Rules.Length; j++)
            {
                var rule = Rules[j];
                string[] Tokens = rule.Split(' ');
                string Heading = "";
                List<string> lrTokens = new List<string>();
                for (int i = 0; i < Tokens.Length; i++)
                {
                    var token = Tokens[i];
                    if (token == "=")
                    {
                        if (lrTokens.Count != 1 || lrTokens[0] == " ")
                        {
                            throw new InvalidOperationException("Syntax error");
                        }
                        Heading = lrTokens[0];
                        lrTokens = new List<string>();
                    }
                    else if (int.TryParse(token, out int number))
                    {
                        if (i != Tokens.Length - 1)
                        {
                            //======================================Caution======================================
                            ///Adustment from ActionBase.DefinedOperators.Contains....
                            var DefinedOperators = new List<string>() { "|", "{", "}", "[", "]" };
                            if (!DefinedOperators.Contains(Tokens[i + 1]))
                            {
                                for (int k = 0; k < number; k++)
                                    lrTokens.Add(Tokens[i + 1]);
                                i++;
                            }
                            //===================================================================================
                        }
                        else
                        {
                            throw new InvalidOperationException("Syntax error");
                        }
                    }
                    else if (token == " " || token == "")
                    {
                        continue;
                    }
                    else if (token == "empty")
                        continue;
                    else if (token == "|")
                    {
                        yield return new ProductionRule(Heading, lrTokens, index++);
                        lrTokens = new List<string>();
                    }
                    else
                    {
                        lrTokens.Add(token);
                    }
                }
                yield return new ProductionRule(Heading, lrTokens, index++);
            }
        }
        public override string ToString()
        {
            return string.Concat(
                this.Head,
                " = ",
                this.Body.Count > 0 ?
                string.Join(" ", this.Body) : "empty"
                );
        }
        public bool Equals(ProductionRule other)
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
        => obj as ProductionRule is null && this.Equals(obj as ProductionRule);
        public override int GetHashCode()
            => base.GetHashCode();
        public static bool operator ==(ProductionRule A, ProductionRule B)
        {
            if (A is null && B is null) return true;
            else if ((A is null && !(B is null)) ||
                (!(A is null) && B is null)) return false;
            else
                return ReferenceEquals(A, B)
                || (A.Head.Equals(B.Head)
                    && A.Body.SequenceEqual(B.Body));
        }
        public static bool operator !=(ProductionRule A, ProductionRule B)
            => !(A == B);
    }
}
