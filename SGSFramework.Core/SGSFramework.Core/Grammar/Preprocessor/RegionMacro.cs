using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Grammar.Preprocessor
{
    public class RegionMacro : Macro
    {
        public override string Key => "#region";
        public RegionMacro() : base() { }
        public override bool Preprocessing(ref List<string> Rules)
        {
            bool Result = true;
            var RuleSnap = new List<string>();
            var ReturnRule = new List<string>();
            bool Record = false;
            foreach (var rule in Rules)
            {
                if (rule.Contains("#region") && !Record)
                {
                    cmd += "Snap started";
                    Record = true;
                }
                else if (rule.Contains("#endregion") && Record)
                {
                    cmd += "Snap finished";
                    Result |= sgsGrammar.Preprocessor(ref RuleSnap);
                    ReturnRule.AddRange(RuleSnap);
                    Record = false;
                }
                else if (Record)
                {
                    RuleSnap.Add(rule);
                }
                else
                    ReturnRule.Add(rule);
            }
            if (Record || !Result)
            {
                return false;
            }
            else
            {
                Rules = ReturnRule;
                return true;
            }
        }
        public override string Example => "This macro can limit which snaps of grammar rules need to " +
                                          "follow certain compile patterns \n" +
                                          "EG: \n" +
                                          "#region \n" +
                                          "#define Vo_I Voxel_I \n" +
                                          "<P> = H Vo_I H \n" +
                                          "#endregion \n" +
                                          "<P> = H Voxel_I H <= cannot use the macro in the #region";
    }
}
