using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.BlockSetting.BlockBase;

namespace SGSFramework.Core.Interface
{
    interface IDiverseForms<TBlock> where TBlock: Block
    {
        HashSet<TBlock> blockList { get; set; }
        List<int> Block_IDs { get; set; }
        bool AddBlock(TBlock block);
    }
}
