using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Grammar;

namespace SGSFramework.Core.Parser.LRParseing
{
    public abstract class BaseLR1Parsing<TAction, TKernelItem> :
        BaseLRParsing<TAction, TKernelItem>
        where TAction : BaseLR1ActionDictionary<TKernelItem>
        where TKernelItem : BaseLR1KernelItem, IEquatable<TKernelItem>
    {
        public BaseLR1Parsing(SGSGrammar cfgGrammar) : base(cfgGrammar)
        { }
        protected TKernelItem CreateKernelItem
            (ProductionRule production, int index, params string[] lookAheads)
            => this.CreateKernelItem(production,
                index,
                (IEnumerable<string>)lookAheads);
        protected abstract TKernelItem CreateKernelItem
            (ProductionRule production, int index, IEnumerable<string> lookAheads);

        protected override Kernel<TKernelItem> CreateInitialKernel()
            => new Kernel<TKernelItem>
            {
                this.CreateKernelItem(ProductionRule.UnSet, 0, "$")   // "$" represent the end of production rule
            };
        protected override Kernel<TKernelItem> CreateClosure(Kernel<TKernelItem> kernel)
        {
            string start = this.cfgGrammar.ComputeStartNonTerminal();
            HashSet<string> nonterminal = this.cfgGrammar.ComputeNonterminals();
            Dictionary<ProductionRule, HashSet<string>> used = new Dictionary<ProductionRule, HashSet<string>>();
            Kernel<TKernelItem> result;
            //These are arrays because they get to be used for multiple times
            string[] remaining, lookaheads;
            string token;
            int initialCount;
            TKernelItem item;

            //Ensure every token can only be use once as look-ahead
            foreach (ProductionRule production in this.cfgGrammar.ProductionRules)
            {
                used.Add(production, new HashSet<string>());
            }
            result = new Kernel<TKernelItem>();

            //The closure exists of the existing items
            foreach (TKernelItem lr1KernelItem in kernel)
            {
                //TODO: reuse existing kernel item
                result.Add
                    (
                    this.CreateKernelItem
                        (
                        lr1KernelItem.Production,
                        lr1KernelItem.Index,
                        lr1KernelItem.LookAheads
                        )
                    );
            }

            //Keep returning items while relations are being found
            do
            {
                //Save the current coutn to check if new items have been added
                initialCount = result.Count;

                //Iterate over the entire (current) result set. The collection
                //gets modified (appended)!
                for (int i = 0; i < initialCount; i++)
                {
                    item = result[i];

                    //Find the current remaining token strings from the current index

                    //If it's the null "Production"
                    if (item.Production.Equals(ProductionRule.UnSet))
                    {
                        //And the index is 0
                        if (item.Index == 0)
                        {
                            //Only the start symbol remains
                            remaining = new string[] { start };
                        }
                        else
                        {
                            //Else the end has been reached
                            remaining = Array.Empty<string>();
                        }
                    }
                    //Grab the remaining tokens
                    else
                    {
                        remaining = item.Production.Body.Skip(item.Index).ToArray();
                    }

                    if (remaining.Length == 0)
                    {
                        continue;
                    }
                    //Get the first token of the remaining body
                    token = remaining[0];

                    //Ensure this is a non-terminal, so a look-ahead can occur
                    if (!nonterminal.Contains(token))
                    {
                        continue;
                    }

                    /* Compute the first set of all tokens that follow the first
                     * token of the remaining set and the look-ahead of the
                     * current item.
                     * If the second item of remaining is a terminal, return it.
                     * If the second item of remaining is a non-terminal, return
                     * the FIRST of this non-terminal. If this non-terminal is
                     * nullable, continue with the rest of the remaining set. */
                    lookaheads = this.cfgGrammar.GetFirst(remaining.Skip(1).Concat(item.LookAheads)).ToArray();

                    //Find all productions for the current first token of the
                    //remaining set

                    foreach (var Production in this.cfgGrammar.ProductionRules)
                    {
                        if (!Production.Head.Equals(token))
                            continue;

                        //And add the look-aheads found from the remaining
                        //token(s) as look-aheads for that production (if that
                        //look-ahead has not been used for that production
                        //before)
                        foreach (var l in lookaheads)
                        {
                            if (!used[Production].Contains(l))
                            {
                                result.Add(this.CreateKernelItem(Production, 0, l));
                                used[Production].Add(l);
                            }
                        }
                    }
                }
            } while (result.Count > initialCount);

            return result;
        }

        //Move position 1 forward in the current production / look ahead
        protected override TKernelItem CreateTransitionKernelItem(TKernelItem item)
            => this.CreateKernelItem(item.Production, item.Index +1, item.LookAheads);

        public override void Classify()
        => this.ClassifyLR1(this.CreateParsingTable());
    }
}
