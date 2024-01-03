using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Parser.Grammar;
using SGSFramework.Core.Parser.LRParseing.ParingInterface;
using SGSFramework.Core.Parser.LRParseing;

namespace SGSFramework.Core.Parser.LRParseing.Display
{
    public static class TableDisplay
    {
        public static string PrintLR1Table<TKernelItem>(IParsingTable<BaseLR1ActionDictionary<TKernelItem>> Table)
            where TKernelItem : BaseLR0KernelItem, IEquatable<TKernelItem>
        {
            string Result = "";
            for (int i = 0; i < Table.Count; i++)
            {
                var followRow = Table.ElementAt(i);
                foreach (var kvp in followRow)
                {
                    var Token = kvp.Key;
                    var actions = kvp.Value;
                    if (!(actions.Reduce is null))
                    {
                        if (actions.Reduce[0].Index >= 0)
                            Result += $" {i} : {Token} -> r{actions.Reduce[0].Index} \n";
                        else
                            Result += $" {i} : {Token} -> acc \n";
                    }
                    else
                    {
                        Result += $" {i} : {Token} -> s{actions.Shift.Index} \n";
                    }
                }

            }
            return Result;
        }
    }
}
