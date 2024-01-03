using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Parser.LRParseing.ParingInterface
{
    public interface IParsingTable<out TAction> : IEnumerable<TAction> 
        where TAction : class, IAction
    {
        int Count { get; }
    }
}
