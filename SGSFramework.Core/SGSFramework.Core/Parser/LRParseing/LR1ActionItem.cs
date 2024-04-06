using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Grammar;

namespace SGSFramework.Core.Parser.LRParseing
{
    public class LR1ActionItem<TKernelItem> 
        where TKernelItem : BaseLR0KernelItem, IEquatable<TKernelItem>
    {
        /// <summary>
        /// During a shift (on a terminal) the input sentence can be shifted 1
        /// token to the left and a transition to a new state can happen.If the
        /// token is a non-terminal, a goto occurs without shifting the input
        /// sentence
        /// </summary>
        public State<TKernelItem> Shift { get; internal set; }
        public List<ProductionRule> Reduce { get; internal set; }
        public LR1ActionItem() { }
        /// <summary>
        /// Initialise a shift as an action
        /// </summary>
        /// <param name="Shift"></param>
        public LR1ActionItem(State<TKernelItem> Shift):this()
        {
            this.Shift = Shift;
        }
    }
}
