using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Parser.LRParseing
{
    public interface IAutomaton<out TKernelItem>
        : IEnumerable<IState<TKernelItem>>
        where TKernelItem : BaseLR0KernelItem, IEquatable<TKernelItem>
    {
        int Count { get; }

        int IndexOf(IState<BaseLR0KernelItem> item);
    }
}
