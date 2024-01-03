using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Parser
{
    public class LR1ClassificationException : Exception
    {
        public LR1ClassificationException(string message)
            : base(message)
        { }
    }
}
