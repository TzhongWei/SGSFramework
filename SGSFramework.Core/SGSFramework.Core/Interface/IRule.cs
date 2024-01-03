using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Interface
{
    public interface IRule<TokenType>
    {
        /// <summary>
        /// The head token which is always a non-terminal
        /// </summary>
        string Head { get; }
        /// <summary>
        /// a list of characters substitute the non-terminal
        /// </summary>
        IReadOnlyList<TokenType> Body { get; }
        /// <summary>
        /// The index of this production for easier debugging and table generation
        /// </summary>
        int Index { get; }
    }
}
