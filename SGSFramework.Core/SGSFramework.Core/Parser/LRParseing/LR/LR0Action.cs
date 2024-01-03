using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Parser.Grammar;

namespace SGSFramework.Core.Parser.LRParseing
{
    public class LR0Action : IAction
    {
        /// <summary>
        /// During a shift (on a terminal) the input sentence can be shifted 1
        /// token to the left and a transition to a new state can happen.If the
        /// token is a non-terminal, a goto occurs without shifting the input
        /// sentence
        /// </summary>
        public Dictionary<string, State<LR0KernelItem>> Shift
        {
            get;
            private set;
        }

        /// <summary>
        /// A reductions reduces one ore more tokens (body of a production) to a
        /// single non terminal (head of a production) 
        /// </summary>
        public List<ProductionRule> Reduce { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public LR0Action()
        {
            this.Shift = new Dictionary<string, State<LR0KernelItem>>();
            this.Reduce = new List<ProductionRule>();
        }
    }
}
