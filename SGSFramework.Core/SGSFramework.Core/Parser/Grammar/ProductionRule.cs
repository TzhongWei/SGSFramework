using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Interpreter.InterpreterReader;
using SGSFramework.Core.Interface;

namespace SGSFramework.Core.Parser.Grammar
{
    /// <summary>
    /// The production rule only accept string, the rule input will be 
    /// segmentised into words for parsing
    /// </summary>
    [Obsolete("This namespace is no-longer available", true)]
    public class ProductionRule : IRule<string>, IEquatable<ProductionRule>
    {
        //The head token which is always a non-terminal
        public string Head => _head;
        //a list of characters substitute the non-terminal
        public IReadOnlyList<string> Body => this._words;
        //The index of this production for easier debugging and table generation
        public int Index { get; private set; }

        //A static "Unset" production so it can be used in certain contexts
        //(like dictionaries)
        internal static ProductionRule UnSet { get; }
        private readonly string _head;
        private readonly IReadOnlyList<string> _words;
        static ProductionRule()
        {
            ProductionRule.UnSet = new ProductionRule(string.Empty, -1);
        }
        private ProductionRule(string heading, int index)
        {
            this._head = heading;
            this.Index = index;
        }
        public ProductionRule(string heading, IReadOnlyList<string> words, int index) : this(heading, index)
        {
            this._words = words;
        }
        public static IEnumerable<ProductionRule> CreateProductionRules(IEnumerable<string> Rules)
            => CreateProductionRules(Rules.ToArray());
        public static IEnumerable<ProductionRule> CreateProductionRules
            (params string[] Rules)
        {
            int index = 0;
            for(int j = 0; j < Rules.Length; j++)
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
                this._head, 
                " = ", 
                this._words.Count > 0 ? 
                string.Join(" ", this._words) : "empty"
                );
        }
        public bool Equals(ProductionRule other)
        {
            if (this is null && other is null) return true;
            else if ((this is null && !(other is null)) ||
                (!(this is null) && other is null)) return false;
            else
                return ReferenceEquals(other, this)
                       || (this._head.Equals(other._head)
                           && this._words.SequenceEqual(other._words));
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
                || (A._head.Equals(B._head)
                    && A._words.SequenceEqual(B._words));
        }
        public static bool operator !=(ProductionRule A, ProductionRule B)
            => !(A == B);
    }
}
