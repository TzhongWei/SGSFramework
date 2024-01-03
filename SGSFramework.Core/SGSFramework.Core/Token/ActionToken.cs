using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Interface;
using Rhino.Geometry;

namespace SGSFramework.Core.Token
{
    public abstract class ActionToken : IActionToken<object>
    {
        public string ExceptionNotion { get; set; } = "";
        public abstract string Print { get; protected set; }
        /// <summary>
        /// The Key of the token pair <_name, _attributePtr>
        /// </summary>
        public abstract string _name { get; }
        /// <summary>
        /// The Value of the token pair <_name, _attributePtr>
        /// </summary>
        public LabelAction<object> _attributePtr { get; protected set; }

        public abstract bool Equals(IToken<string, LabelAction<object>> other);

        public abstract bool Run(ref object TS, ref Stack<Transform> TSStack, params object[] CustomAction);

        public static LabelAction<object> ConvertLabel<T>(LabelAction<T> transformDelegate)
        {
            return ConvertLabelImplementation;

            // Separate method to perform the conversion
            object ConvertLabelImplementation(object input, ref Stack<Transform> stack, params object[] customAction)
            {
                if (input is T transformInput)
                {
                    return transformDelegate(transformInput, ref stack, customAction);
                }
                else
                {
                    return input;
                }
            }
        }
        public override string ToString()
         => this.Print;
    }
}
