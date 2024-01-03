using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using SGSFramework.Core.Grammar;
using SGSFramework.Core.Interface;
using Rhino.Geometry;

namespace SGSFramework.Core.Token
{
    public class GroupActionToken : TerminalActionToken, IFuntionAction
    {
        public SGSGrammar sgsGrammar { get; set; }
        public Dictionary<string, object> UserString { get;set; }
        public PreOrPostExecute IsPreOrPostExecute { get; private set; }
        public static GroupActionToken Unset { get; }
        static GroupActionToken()
        {
            Unset = new GroupActionToken();
        }
        private GroupActionToken()
        {
            UserString = new Dictionary<string, object>();
            IsPreOrPostExecute = PreOrPostExecute.PreExecute;
        }
        public bool Action(out ActionToken actionToken, object[] Element)
        {
            if (Element[0].ToString() == "Group" && Element.Length == 2)
            {
                var Action = new GroupActionToken(Element[1].ToString());
                Action.sgsGrammar = this.sgsGrammar;
                Action.UserString = this.UserString;
                actionToken = Action;
                return true;
            }
            else
            {
                actionToken = null;
                return false;
            }
        }
        public override bool Run(ref object TS, ref Stack<Transform> TSStack, params object[] CustomAction)
        {
            if (TS == new object())
            {
                if (this.IsPreOrPostExecute == PreOrPostExecute.PreExecute)
                {
                    this.IsPreOrPostExecute = PreOrPostExecute.PostExecute;
                    TS = this;
                    return true;
                }
                else if (this.IsPreOrPostExecute == PreOrPostExecute.PostExecute)
                {
                    this._attributePtr = EndGroup;
                    TS = this;
                    return true;
                }
                else
                {
                    this.ExceptionNotion = "The Group action is failed \n";
                    TS = null;
                    return false;
                }
            }
            return base.Run(ref TS, ref TSStack, CustomAction);
        }
        public GroupActionToken(string GroupName):base("Group", $"Group({GroupName})")
        {
                this.IsPreOrPostExecute = PreOrPostExecute.PreExecute;
                this._attributePtr = StartGroup;
            this.GroupName = GroupName;
        }
        public string GroupName { get; }
        public string FunctionName { get => "GroupAction";}

        private Transform StartGroup(Transform TS, ref Stack<Transform> TSStack, params object[] CustomAction)
        {
            if (this.UserString.ContainsKey(this.GroupName))
            {
                (this.UserString[GroupName] as List<Guid>).Add(
                    this.LabelTable.RhinoReferenceBlockGuid[CustomAction[0].ToString()].Last()
                    );
            }
            else
            {
                this.UserString.Add(GroupName, new List<Guid>() { 
                    this.LabelTable.RhinoReferenceBlockGuid[CustomAction[0].ToString()].Last() }
                );
            }
            return TS;
        }
        private Transform EndGroup(Transform TS, ref Stack<Transform> TSStack, params object[] CustomAction)
        {
            RhinoDoc Doc = RhinoDoc.ActiveDoc;
            var GuidList = this.UserString[GroupName] as List<Guid>;
            Doc.Groups.Add(this.GroupName, GuidList);
            return TS;
        }
        public bool Executable()
        {
            return true;
        }
    }
}
