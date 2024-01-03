using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using SGSFramework.Core.Grammar;
using SGSFramework.Core.Interface;

namespace SGSFramework.Core.Token
{
    //public class OrActionToken : ActionToken, IFuntionAction
    //{
    //    public Dictionary<string, object> UserString { get; set; }
    //    public override string Print { get; protected set; }
    //    public override string _name { get; }
    //    public static OrActionToken UnSet { get; }
    //    public SGSGrammar sgsGrammar { get; set; }

    //    public PreOrPostExecute IsPreOrPostExecute => PreOrPostExecute.PreExecute;

    //    private int[] OrActionIndices;
    //    static OrActionToken()
    //    {
    //        UnSet = new OrActionToken();
    //    }
    //    private OrActionToken(params int[] Elements)
    //    {
    //        this._name = "Or";
    //        this.Print = "Or( " + string.Join(" | ", Elements) + " )";
    //        this.OrActionIndices = Elements;
    //        this._attributePtr = OrLabelAction;
    //    }
    //    private object OrLabelAction(object Any, ref Stack<Transform> TSStack, params object[] CustomAction)
    //    {
    //        Random Rand = new Random();
    //        return this.OrActionIndices[Rand.Next(0, this.OrActionIndices.Length - 1)];
    //    }
    //    public bool Action(out ActionToken actionToken, params object[] Element)
    //    {
    //        if (Element[0].ToString() == "Or")
    //        {
    //            var NewIntList = new List<int>();
    //            for (int i = 1; i < Element.Length; i++)
    //            {
    //                if (int.TryParse(Element[i].ToString(), out int result))
    //                {
    //                    NewIntList.Add(result);
    //                }
    //                else
    //                {
    //                    this.ExceptionNotion = "There are some error in this command list";
    //                }
    //            }
    //            actionToken = new OrActionToken(NewIntList.ToArray());
    //            return true;
    //        }
    //        else
    //        {
    //            actionToken = null;
    //            return false;
    //        }
    //    }

    //    public override bool Equals(IToken<string, LabelAction<object>> other)
    //    => this._name == other._name;

    //    public bool Executable()
    //    {
    //        return false;
    //    }

    //    public override bool Run(ref object ProductionIndex, ref Stack<Transform> TSStack, params object[] CustomAction)
    //    {
    //        ProductionIndex = (int)this._attributePtr(new object(), ref TSStack);
            
    //        return sgsGrammar.ContainIndex((int)ProductionIndex);
    //    }
    //}
}
