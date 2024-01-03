using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace SGSFramework.Core.Interface
{
    public delegate T LabelAction<T>(T TS, ref Stack<Transform> TSStack, params object[] CustomAction);
    /// <summary>
    /// ActionToken is a toke with semantic actions where the "T" represent the input and return type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IActionToken<T> : IToken<string, LabelAction<T>>
    {
        bool Run(ref T TS, ref Stack<Transform> TSStack, params object[] CustomAction);
        /// <summary>
        /// The form is represented when it convert to string
        /// </summary>
        string Print { get; }
    }

}
