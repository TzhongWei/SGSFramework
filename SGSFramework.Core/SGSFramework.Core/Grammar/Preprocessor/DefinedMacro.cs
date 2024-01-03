using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Grammar.Preprocessor
{
    /// <summary>
    /// Redefine the token with this preprocessor
    /// </summary>
    public class DefinedMacro : Macro
    {
        public DefinedMacro() : base() { }
        public override string Key =>"#define";
        public string OriginalToken;
        public string ConvertToken;
        public override string Example => "Redefined the token into another form facilitating grammar compiling \n" +
                                          "EG: \n" +
                                          "Syntax : #defined <OriginalToken> <ConvertToken> \n Input is \n" +
                                          "#defined I_Vo Voxel_I \n" +
                                          "<P> = H I_Vo H \n ";
        public override bool Execute(string Line)
        {
            if (Line.Contains(Key))
            {
                Line = Util.Util.CleanSequence(Line);
                var Command = Line.Split(' ');
                if (Command.Length != 3)
                    return false;
                else
                {
                    OriginalToken = Command[1];
                    ConvertToken = Command[2];
                    return true;
                }
            }
            else
                return false;
        }
        public override bool Preprocessing(ref List<string> Rules)
        {
            bool Result = true;
            var Lines = new List<string>();
            foreach (var rule in Rules)
            {
                if (rule.Contains("#") && !rule.Contains("#define"))
                {
                    Lines.Add(rule);
                }
                else if (rule.Contains("#define"))
                {
                    
                }
                else
                {
                    var words = rule.Split(' ');
                    for (int i = 0; i < words.Length; i++)
                    {
                        if (words[i] == this.OriginalToken)
                        {
                            words[i] = this.ConvertToken;

                        }
                    }
                    Lines.Add(string.Join(" ", words));
                }
            }
            Rules = Lines;
            return Result;
        }
    }
}
