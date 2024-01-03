using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Parser.LRParseing
{
    public interface IState<out TKernelItem>
        : IEquatable<IState<BaseLR0KernelItem>>
        where TKernelItem : BaseLR0KernelItem, IEquatable<TKernelItem>
    {
        IKernel<TKernelItem> Kernel { get; }
        IKernel<TKernelItem> Items { get; }

        void AddKernel(BaseLR0KernelItem item);
        void AddItem(BaseLR0KernelItem item);
    }
}
