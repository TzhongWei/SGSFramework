using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Parser.SyntaxTree;

namespace SGSFramework.Core.Parser.LRParseing.ParingInterface
{
    public interface IParsingString
    {
        string cmd { get; }
        SyntaxTree.SyntaxTree Tree { get; set; }
        SyntaxTree.SyntaxTree StartParsing();
    }
}
