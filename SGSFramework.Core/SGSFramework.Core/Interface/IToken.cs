using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Interface
{
    public interface IToken<Key, Value> : IEquatable<IToken<Key, Value>>
    {
        /// <summary>
        /// The Key of the token pair <_name, _attributePtr>
        /// </summary>
        Key _name { get; }
        /// <summary>
        /// The Value of the token pair <_name, _attributePtr>
        /// </summary>
        Value _attributePtr { get;}
    }
}
