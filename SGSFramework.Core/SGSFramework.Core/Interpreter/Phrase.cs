using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Util;

namespace SGSFramework.Core.Interpreter
{
    /*
    =================================Future===============================
    Macro

    input rule type
    //if compactingName or vocabulary don't spell preferable
    #defined CompactingName I Voxel_I
    #defined Vocabulary H H_1
    //if compactingName or vocabulary has duplicated and 
    //list on the output of labelblocktable cmd
    // you can modified them in the label shape or here by redefined it
    #defined CompactingName I Voxel_I CHANGE(BlockName)
    #defined Vocabulary H H_1 CHANGE(BlockName)
    
    =====================Vocabulary and CompactingName=====================

                    Vocabulary is setting transformation
                 CompactingName is a label to place a block
    
    =============================Rule Setting===============================

    //Rule setting
    <H> = H S S           // A statement
    <J> = 2 H S S 3 <H>   // A statement with calling other statements 
    <K> = H 2 <K> | EMPTY // operator "|" means "or" and "EMPTY" is a key word shows empty action
    <L> = K | O | J P     // ambigious is allowed 
    <T> = 3 C [ K P ]     // operator "[" and "]" means push and pop respectively <= Not implemented

    //Error
    <J> = H S S           // Repeat a statement is prohibited
    <U> = HSHS            // Terminal without space is incorrect
    IJ = H H 2 D          // Nonterminal without "<" and ">" is unaccepted
    <G> = <G> H | EMPTY   // Left Recursion isn't allowed
    

    */
    /// <summary>
    /// A class to store the grammars and commands
    /// </summary>
    [Obsolete("The phrase setting for shape interpreter is obsolete, please refer to the ProductionRule and SemanticRule in Grammar")]
    public class Phrase: IPhraseRules<string>
    {
        /// <summary>
        /// All nonterminals
        /// </summary>
        public List<string> Head => _Rules.Keys.ToList();
        /// <summary>
        /// Get All rules for phrase setting
        /// </summary>
        public Dictionary<string, string> Rules => _Rules;
        /// <summary>
        /// This dictionary store the rules
        /// storeing with RuleName, Sequence
        /// </summary>
        protected Dictionary<string, string> _Rules { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// The original rules
        /// </summary>
        protected List<string> InitalRules = new List<string>();
        /// <summary>
        /// The number of rules
        /// </summary>
        public int Count => _Rules.Count;
        /// <summary>
        /// Constructor
        /// </summary>
        public Phrase() { }
        /// <summary>
        /// Test if the Rules has the heading
        /// </summary>
        /// <param name="heading">The heading of command</param>
        /// <returns></returns>
        public virtual bool HasRule(string heading)
        {
            return this._Rules.ContainsKey(heading);
        }
        public bool AddRule(out string cmd, params string[] Lines)
            => this.AddRule(Lines.ToList(), out cmd);
        public bool AddRule(List<string> Lines, out string cmd)
        {
            bool success = true;
            cmd = "";
            for(int i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];
                if (line[0] == '/' && line[1] == '/') continue;
                if (line.Contains('=') && line.Contains("<") && line.Contains(">"))
                {
                    var heading = Util.Util.CleanSequence(line.Split('=')[0]);
                    if (!heading.Contains("<") || !heading.Contains(">"))
                    {
                        cmd += $"Rule Setting Error, {heading} is defined incorrect";
                        return false;
                    }
                    var Sequence = Util.Util.CleanSequence(line.Split('=')[1]);
                    if (!_Rules.ContainsKey(heading))
                        _Rules.Add(heading, Sequence);
                }
                else
                {
                    cmd += $"Rule Setting Error, {line} cannot be recognised";
                    success = false;
                }
            }
            if (success)
                this.InitalRules = new List<string>(Lines);
            return success;
        }
        /// <summary>
        /// Add rules into this phrase
        /// Not only set up the rules, but also set up the bake up "InitalRules"
        /// </summary>
        /// <param name="Heading">Nonterminals </param>
        /// <param name="Sequence">Terminals </param>
        /// <returns></returns>
        public bool AddRule(string Heading, string Sequence)
        {
            if (_Rules.ContainsKey(Heading))
                return false;
            else if (Heading.Contains("<") && Heading.Contains(">"))
            {
                _Rules.Add(Util.Util.CleanSequence(Heading), Util.Util.CleanSequence(Sequence));
                return true;
            }
            this.InitalRules.Add($"{Heading} = {Sequence}");
            return false;
        }
        /// <summary>
        /// Remove certain head in the rule list
        /// </summary>
        /// <param name="Head"></param>
        /// <returns></returns>
        public virtual bool RemoveRule(string Head)
        {
            if (!_Rules.ContainsKey(Head)) return false;
            else
            {
                _Rules.Remove(Util.Util.CleanSequence(Head));
                return true;
            }

        }
        /// <summary>
        /// substitute the labels from newlabels in the rules to old one.
        /// </summary>
        /// <param name="Oldlabel"></param>
        /// <param name="Newlabel"></param>
        /// <returns></returns>
        public virtual bool ChangeLabel(string Oldlabel, string Newlabel)
        {
            bool success = true;
            var TempNewRule = new Dictionary<string, string>();
            foreach (var KVP in this._Rules)
            {
                if (!KVP.Value.Contains(Newlabel))
                {
                    TempNewRule.Add(KVP.Key, KVP.Value);
                    continue;
                }

                var words = KVP.Value.Split(' ').Select(x =>
                {
                    if (x == Newlabel)
                        return Oldlabel;
                    else
                        return x;
                }).ToList();
                string NewRule = string.Join(" ", words);
                
                NewRule = Util.Util.CleanSequence(NewRule);
                if (NewRule == "")
                {
                    success = false;
                    throw new Exception("Label substitute failed. ");
                }
                TempNewRule.Add(KVP.Key, NewRule);
            }
            this._Rules = TempNewRule;
            return success;
        }
        /// <summary>
        /// Find the rule from the list of rule
        /// if the heading is existed in the list return true,
        /// else false
        /// </summary>
        /// <param name="heading"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public virtual bool FindRule(string heading, out string sequence)
        {
            if (!_Rules.ContainsKey(heading))
            {
                sequence = null;
                return false;
            }
            sequence = _Rules[heading];
            return true;
        }
        /// <summary>
        /// Alter the rule in the list
        /// </summary>
        /// <param name="heading"></param>
        /// <param name="newSequence"></param>
        /// <returns></returns>
        public virtual bool ChangeRule(string heading, string newSequence)
        {
            if (!_Rules.ContainsKey(heading)) return false;
            _Rules[heading] = newSequence;
            return true;
        }
        public virtual void Reset()
        {
            this.AddRule(this.InitalRules, out _);
        }
        public override string ToString()
        {
            string Rules = "";
            for (int i = 0; i < this._Rules.Count; i++)
            {
                Rules += $"[{this._Rules.Keys.ToList()[i]},{this._Rules.Values.ToList()[i]}]\n";
            }
            return Rules;
        }
    }
}
