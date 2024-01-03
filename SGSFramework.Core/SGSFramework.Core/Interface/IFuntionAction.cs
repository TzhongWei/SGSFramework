using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Grammar;
using SGSFramework.Core.Token;

namespace SGSFramework.Core.Interface
{
    /// <summary>
    /// Determine the semantic need to be excuted before or after SeqVal()
    /// </summary>
    public enum PreOrPostExecute
    {
        /// <summary>
        /// Excute before SeqVal()
        /// </summary>
        PreExecute,
        /// <summary>
        /// Execute after Seqval()
        /// </summary>
        PostExecute
    }
    public interface IFuntionAction
    {
        string FunctionName { get;}
        /// <summary>
        /// Set SGS grammar into the class
        /// </summary>
        SGSGrammar sgsGrammar { get; set; }
        Dictionary<string, object> UserString { get; set; }
        /// <summary>
        /// if this is a pre_execute class, and only if executable() returns false, it wouldn't execute the SeqVal().
        /// Or, this is a post_execute class, and only if executable() returns false, it wouldn't return any semantic action
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        bool Executable();
        /// <summary>
        /// Determine the semantic need to be excuted before or after SeqVal()
        /// </summary>
        PreOrPostExecute IsPreOrPostExecute { get; }
        /// <summary>
        /// Action trigger
        /// </summary>
        /// <param name="actionToken"></param>
        /// <param name="Element"></param>
        /// <returns></returns>
        bool Action(out ActionToken actionToken, object[] Element);
    }
}
