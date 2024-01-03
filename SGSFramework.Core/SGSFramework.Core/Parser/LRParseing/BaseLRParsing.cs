using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Parser.Grammar;
using SGSFramework.Core.Parser.LRParseing.ParingInterface;

namespace SGSFramework.Core.Parser.LRParseing
{
    public abstract class BaseLRParsing<TAction, TKernelItem>: BaseParsing<TAction>
        where TAction: class, IAction
        where TKernelItem: BaseLR0KernelItem, IEquatable<TKernelItem>
    {
        public IAutomaton<TKernelItem> Automaton { get; protected set; }
        protected BaseLRParsing(CFGGrammar cfgGrammar) : base(cfgGrammar) { }
        #region abstract implementaion
        /// <summary>
        /// Create the first kernel item for usage in the automaton, this usually
        /// consist of the start token from which all productions are derived
        /// </summary>
        /// <returns></returns>
        protected abstract Kernel<TKernelItem> CreateInitialKernel();
        /// <summary>
        /// Find all possible productions which can be resolved from the current
        /// kernel items in kernel and initialise them as a new kernel
        /// </summary>
        /// <returns></returns>
        protected abstract Kernel<TKernelItem> CreateClosure(Kernel<TKernelItem> kernel);
        /// <summary>
        /// Create a transition kernel item from a kernel item
        /// </summary>
        /// <returns></returns>
        protected abstract TKernelItem CreateTransitionKernelItem(TKernelItem item);
        #endregion
        /// <summary>
        /// An automaton consits of multiple states, where each state is a set of
        /// productions with a position in each of those productions.Each
        /// production in a set is a derivation from the first production in the
        /// set (on that particular postion). Each state can transition into
        /// another state by changing position in a production of a set
        /// </summary>
        /// <returns></returns>
        public virtual IAutomaton<TKernelItem> CreateAutomaton()
        {
            if (!(this.Automaton is null))
            {
                return this.Automaton;
            }

            int s = 0, l, i;
            Automaton<TKernelItem> states = new Automaton<TKernelItem>();
            State<TKernelItem> state;
            IDictionary<string, Kernel<TKernelItem>> transitions;
            Kernel<TKernelItem> kernel;

            //Add the initial kernel as the initial state (which can be mutated)
            states.Add
            (
                new State<TKernelItem>
                (
                    states.Count,
                    this.CreateInitialKernel()
                )
            );

            //while new states have been added
            while (s < states.Count)
            {
                //iterate over all new state
                for (l = states.Count; s < l; s++)
                {
                    state = states[s];

                    //Find the closure for the current kernel
                    state.Items = this.CreateClosure(state.Kernel);

                    //Find the transitions from that closure
                    transitions = this.CreateTransitions(state.Items);

                    state.Transitions
                        = new Dictionary<string, State<TKernelItem>>();

                    //Check each transition token
                    foreach (string token in transitions.Keys)
                    {
                        kernel = transitions[token];

                        //check if a state with the kernel already exists, if so
                        //add it as a transition from the current state
                        for (i = 0; i < states.Count; i++)
                        {
                            if (kernel.Equals(states[i].Kernel))
                            {
                                state.Transitions.Add(token, states[i]);
                                break;
                            }
                        }

                        //if the kernel has not been found, add it as a new
                        //state and add this new state as transition for the
                        //current state
                        if (i == states.Count)
                        {
                            states.Add
                            (
                                new State<TKernelItem>(states.Count, kernel)
                            );

                            state.Transitions.Add
                            (
                                token,
                                states.Last()
                            );
                        }
                    }
                }
            }

            this.Automaton = states;

            return states;
        }
        /// <summary>
        /// Move the index in the current closure (in all productions) forward by 1 position
        /// </summary>
        /// <param name="closure"></param>
        /// <returns></returns>

        protected IDictionary<string, Kernel<TKernelItem>>
            CreateTransitions(Kernel<TKernelItem> closure)
        {
            string start = this.cfgGrammar.ComputeStartNonTerminal();
            Dictionary<string, Kernel<TKernelItem>> result = new Dictionary<string, Kernel<TKernelItem>>();
            string token;

            foreach (TKernelItem item in closure)
            {
                // Find the token at the current index of the item
                if (item.Production == ProductionRule.UnSet)
                {
                    if (item.Index == 0)
                    {
                        token = start;
                    }
                    else
                        token = null;
                }
                //Ensure the token is still within the body of the production
                else if (item.Index < item.Production.Body.Count)
                {
                    token = item.Production.Body[item.Index];
                }
                //Else the end of the production has been found
                else
                {
                    token = null;
                }

                //indicates end of a production (null production included)
                if (!(token != null && token != string.Empty))
                {
                    continue;
                }

                if (!result.ContainsKey(token))
                {
                    result.Add(token, new Kernel<TKernelItem>());
                }

                result[token].Add(this.CreateTransitionKernelItem(item));
            }

            return result;
        }

        /// <summary>
        /// LR(1) allows for only a single action per token per state
        /// </summary>
        /// <typeparam name="T1Action"></typeparam>
        /// <param name="table"></param>
        public void ClassifyLR1<T1Action>(IParsingTable<T1Action> table)
            where T1Action : BaseLR1ActionDictionary<TKernelItem>
        {
            foreach (T1Action action in table)
            {
                foreach (KeyValuePair<string, LR1ActionItem<TKernelItem>> kv in action)
                {
                    if (!(kv.Value.Reduce is null) && kv.Value.Reduce.Count > 1)
                    {
                        throw new LR1ClassificationException
                       (
                           "Table contains a reduce-reduce conflict"
                       );
                    }
                    if (!(kv.Value.Shift is null)
                       && !(kv.Value.Reduce is null)
                       && kv.Value.Reduce.Count > 0)
                    {
                        throw new LR1ClassificationException
                        (
                            "Table contains a shift-reduce conflict"
                        );
                    }
                }
            }
        }
        protected void AddReduceAction(BaseLR1ActionDictionary<TKernelItem> actions,
            ProductionRule production,
            params string[] tokens)
            => this.AddReduceAction(actions, production, (IEnumerable<string>)tokens);
        protected void AddReduceAction(
            BaseLR1ActionDictionary<TKernelItem> actions,
                ProductionRule production,
                IEnumerable<string> tokens)
        {
            LR1ActionItem<TKernelItem> actionItem;
            foreach (var token in tokens)
            {
                if (!actions.TryGetValue(token, out actionItem))
                {
                    actions.Add(
                        token,
                        actionItem = new LR1ActionItem<TKernelItem>()
                        );
                }
                if (actionItem.Reduce is null)
                {
                    actionItem.Reduce = new List<ProductionRule>();
                }
                actionItem.Reduce.Add(production);
            }
        }
    }
}
