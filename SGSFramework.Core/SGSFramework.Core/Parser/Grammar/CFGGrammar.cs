using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Parser.Grammar
{
    public class CFGGrammar
    {
        public IReadOnlyList<ProductionRule> productions => _productions;
        private List<ProductionRule> _productions { get; }
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

        private string _start { get; set; }
        //Each nonterminal has a set of terminals as a first token
        private IDictionary<string, HashSet<string>> _firstSet;
        //Each nonterminal has a set of terminals as a first token
        private IDictionary<string, HashSet<string>> _followSet;

        private CFGGrammar()
        {
            _productions = new List<ProductionRule>();
            _tokens = new HashSet<string>();
        }
        public CFGGrammar(IEnumerable<string> Rules):this()
        {
            this.AddProductionRule(ProductionRule.CreateProductionRules(Rules));
            this.ComputeFirstCycle();
        }
        public void AddProductionRule(ProductionRule production)
        {
            this._productions.Add(production);
            this._tokens.Add(production.Head);
            foreach (var token in production.Body)
            {
                this._tokens.Add(token);
            }
        }
        public void AddProductionRule(IEnumerable<ProductionRule> productions)
        {
            foreach (var Pro in productions)
            {
                this.AddProductionRule(Pro);
            }
        }
        public string ComputeStartNonTerminal()
        {
            if (this._start is null)
            {
                this._start = this._productions[0].Head;
            }
            return this._start;
        }
        public HashSet<string> ComputeNonTerminals()
        {
            if (!(this._nonterminals is null))
            {
                return this._nonterminals;
            }

            HashSet<string> nonterminals = new HashSet<string>();
            foreach(ProductionRule pro in this._productions)
            {
                nonterminals.Add(pro.Head);
            }
            return this._nonterminals = nonterminals;
        }
        public HashSet<string> ComputeTerminals()
        {
            if (!(this._terminals is null))
            {
                return this._terminals;
            }
            var terminals = new HashSet<string>();
            var nonterminals = this.ComputeNonTerminals();
            foreach (var token in this._tokens)
            {
                if (!nonterminals.Contains(token))
                {
                    terminals.Add(token);
                }
            }
            return this._terminals = terminals;
        }
        public HashSet<string> ComputeNullable()
        {
            if (!(this._nullable is null))
                return this._nullable;

            var nullable = new HashSet<string>();
            IList<ProductionRule> productions = this._productions;
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

            var Nonterminals = this.ComputeNonTerminals();
            IList<ProductionRule> productions = this._productions;
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

            HashSet<string> nonTerminals = this.ComputeNonTerminals();
            IList<ProductionRule> productions = this._productions;
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
            HashSet<string> nonTerminals = this.ComputeNonTerminals();
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
            HashSet<string> nonTerminals = this.ComputeNonTerminals();
            HashSet<string> nullable = this.ComputeNullable();
            Relation relation = new Relation();
            int j, k;

            foreach (ProductionRule production in this._productions)
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
    }
}
