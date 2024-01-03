using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Interface;

namespace SGSFramework.Core.Interface
{
    public interface ITokenList<IToken>:IList<IToken>
    {
        HashSet<IToken> TokenList { get;}
        void AddRange(HashSet<IToken> Tokens);
    }
}
