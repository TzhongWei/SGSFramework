using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Rhino.Geometry;
using SGSFramework.Core.BlockSetting.LabelBlockBase;
using SGSFramework.Core.Interpreter.InterpreterReader;
using SGSFramework.Core.Interpreter.SemanticManager;

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
 <H> = H S S           // A statement   <- need Common Action
 <J> = 2 H S S 3 <H>   // A statement with calling other statements <- need Common Action
 <K> = H 2 <K> | EMPTY // operator "|" means "or" and "EMPTY" is a key word shows empty action   <- need Stochastic Action
 <L> = K | O | J P     // ambigious is allowed                                                        
 <T> = 3 C [ K P ]     // operator "[" and "]" means push and pop respectively <= Not implemented  <- need Stach Action

 <R> = H I H H I       // Typically, a list 

 //Error
 <J> = H S S           // Repeat a statement is prohibited
 <U> = HSHS            // Terminal without space is incorrect
 IJ = H H 2 D          // Nonterminal without "<" and ">" is unaccepted
 <G> = <G> H | EMPTY   // Left Recursion isn't allowed


 */

    /// <summary>
    /// The class here is just an interface to set up the grammar and get the corresponding 
    /// "transformation matrix" for syntax-directed translation in parser
    /// </summary>
    [Obsolete("this interpreter process is obsoleted please refer to the interpreter_2 and Grammar", false)]
    public class ShapeInterpreter
    {
        /// <summary>
        /// _cmd is a property to record the data
        /// </summary>
        private StringBuilder _cmd = new StringBuilder();
        /// <summary>
        /// print cmd
        /// </summary>
        public string cmd => _cmd.ToString();
        /// <summary>
        /// The block table for his interpreter
        /// </summary>
        public LabelBlockTable labelblockTable { get; private set; } = LabelBlockTable.Unset;
        /// <summary>
        /// Phrase is a class to manage the production rules 
        /// </summary>
        public Phrase phraseInfo = new Phrase();
        /// <summary>
        /// Actions can be addressed into different rule execution
        /// </summary>
        private List<ActionBase> Actions = new List<ActionBase>();
        /// <summary>
        /// SemanticManager is a struct to store the final transformation
        /// </summary>
        public Semantic<Transform> SemanticManager { get; private set; } = new Semantic<Transform>() { };
        public void AddAction(ActionBase action)
        {
            if (this.Actions.Contains(action)) return;
            action.Interpreter = this;
            this.Actions.Add(action);
            _actionExe.Add(action.ExecuteRule);
            _actionRule.Add(action.Action);
            SemanticManager = new Semantic<Transform>();
        }
        private ShapeInterpreter() { }
        public ShapeInterpreter(LabelBlockTable labelBlockTable)
        {
            if (!labelBlockTable.Isinvalid)
                this.labelblockTable = labelBlockTable;
            phraseInfo = new Phrase();
            this.AddAction(new CommonAction());
        }
        public delegate bool ExecuteRule(string Rule, out string TargetHeading);
        public delegate string RuleAction(ref Phrase phrase, string heading);
        /// <summary>
        /// Action executable of rules
        /// </summary>
        public List<ExecuteRule> _actionExe { get; private set; } = new List<ExecuteRule>();
        /// <summary>
        /// Action implementation of rules
        /// </summary>
        public List<RuleAction> _actionRule { get; private set; } = new List<RuleAction>();
        /// <summary>
        /// Provides a list to save finished command actions
        /// </summary>
        public List<string> CalledBlockList { get; set; }
        /// <summary>
        /// Assemble the grammar into a set of production rule which represent types of block aggregate forms.
        /// </summary>
        public void Run()
        {
            if (this.labelblockTable.Isinvalid) return;
            this.labelblockTable.CalledLabels = new Dictionary<string, List<string>>();
            this.labelblockTable.ResetBlocks();
            for (int i = 0; i < this.CommandLine.Count; i++)
            {
                var line = this.CommandLine[i];
                if (TargetAction(line, out int Index, out string heading))
                {
                    var TS = Transform.Identity;
                    this.CalledBlockList = new List<string>();
                    _cmd.Append($"<<<<<<<<<<<[ Start {heading} ]>>>>>>>>>>>>\n\n\n");
                    _cmd.Append(_actionRule[Index](ref this.phraseInfo, heading) + "\n");
                    _cmd.Append($"<<<<<<<<<<<[ Finish {heading} ]>>>>>>>>>>>>\n\n\n");
                    if (CalledBlockList.Count == 0 && !this.Actions[Index].IsNoCommand)
                    {
                        _cmd.Append($"The {heading} constructed failed \n");
                        continue;
                    }
                    if (this.labelblockTable.CalledLabels.ContainsKey(heading))
                        _cmd.Append($"The {heading} is duplicated. \n");
                    else
                        this.labelblockTable.CalledLabels.Add(heading, this.CalledBlockList);
                    this.CalledBlockList = new List<string>();
                }
                else
                {
                    _cmd.Append($"Rule {line} cannot be compute. \n");
                }
            }
            if (ExecuteTransform())
            {
                _cmd.Append("Transformation matrix set up success \n");
            }
            else
            {
                _cmd.Append("====================================================\n" +
                    "Transformation matrix set up success \n");
            }
        }
        /// <summary>
        /// Get All Block instance setting into Rhino
        /// </summary>
        /// <returns>if false, construct failed, else true</returns>
        public virtual bool GetAllBlocks()
        {
            if (!this.labelblockTable.GetAllBlocks())
            {
                this._cmd.Append("Block Construct Failed \n");
                return false;
            }
            else
            {
                this._cmd.Append("Block arrangement constructed successfully. \n Get the blocks, ObjectAttributes and their color from \n" +
                    "LabelBlockTable.GetBlockRhinoObj(string label) => List<(GeometryBase, ObjectAttributes)>\n " +
                    "GetGeometry(string Label) => List<GeometryBase> \n" +
                    "GetGeometryColor(string Label) => List<Color> \n");
                return true;
            }
        }
        public List<GeometryBase> GetGeometry(string Label)
        {
            if (this.labelblockTable.BlockObject.Count == 0)
                this.GetAllBlocks();
            return this.labelblockTable.GetRhinoObj(Label).Select(x => x.Item1).ToList();
        }
        public List<Color> GetGeometryColor(string Label)
        {
            if (this.labelblockTable.BlockObject.Count == 0)
                this.GetAllBlocks();
            return this.labelblockTable.GetRhinoObj(Label).Select(x => x.Item2.ObjectColor).ToList();
        }
        /// <summary>
        /// Set the stack transformation
        /// </summary>
        private Stack<Transform> StackTS = new Stack<Transform>();
        /// <summary>
        /// Set all transformation matrix
        /// There is only "[" and "]" can be accept in this Commands
        ///which will impact the transformation
        /// </summary>
        /// <returns>if true represents no error, else there are some errors in the program</returns>
        private bool ExecuteTransform()
        {
            this.labelblockTable.Actions = new Dictionary<string, List<object>>();
            bool NoError = true;
            _cmd.Append("<<<<<<<<<<<<<<<[Set Transformation matrix]>>>>>>>>>>>>>>> \n");
            foreach (var KVP in this.labelblockTable.CalledLabels)
            {
                //There is only "[" and "]" can be accept in this Commands
                //which will impact the transformation
                var Commands = KVP.Value;
                Transform CurrentMx = Transform.Identity;

                int PushCount = Commands.Where(x => x == "[").ToList().Count;
                int PopCount = Commands.Where(x => x == "]").ToList().Count;
                _cmd.Append($"Start {KVP.Key}\n");
                if (PushCount != PopCount)
                {
                    NoError = false;
                    _cmd.Append($"The push {"["} => {PushCount} count and the pop " +
                        $"{"]"} => {PopCount} count are not the same \n");
                    continue;
                }

                for (int i = 0; i < Commands.Count; i++)
                {
                    if (Commands[i] == "[")
                    {
                        StackTS.Push(CurrentMx);
                    }
                    else if (Commands[i] == "]")
                    {
                        CurrentMx = StackTS.Pop();
                    }
                    else if (Commands[i] == "Empty")
                    {
                        continue;
                    }
                    else
                    {
                        if (this.labelblockTable.IsCompactingName(Commands[i]))
                        {
                            var Com = this.FindCompactingName(Commands[i]);
                            if (Com == "")
                            {
                                _cmd.Append($"Cannot Find Compacting Name {Commands[i]}");
                                NoError = false;
                            }
                            if (this.labelblockTable.Actions.ContainsKey(KVP.Key))
                            {
                                this.labelblockTable.Actions[KVP.Key].Add(Com);
                            }
                            else
                            {
                                this.labelblockTable.Actions.Add(KVP.Key,
                                    new List<object>() { Com });
                            }
                        }
                        else
                        {
                            var Matrix = this.labelblockTable[Commands[i]];
                            if (Matrix == Transform.Identity)
                            {
                                NoError = false;
                                _cmd.Append(this.labelblockTable.cmd);
                                CurrentMx = Transform.Identity;
                                break;
                            }
                            CurrentMx *= this.labelblockTable[Commands[i]];

                            if (this.labelblockTable.Actions.ContainsKey(KVP.Key))
                            {
                                this.labelblockTable.Actions[KVP.Key].Add(CurrentMx);
                            }
                            else
                            {
                                this.labelblockTable.Actions.Add(KVP.Key, new List<object>() { CurrentMx });
                            }
                        }
                    }
                }
                this.SemanticManager.AddSemantic(KVP.Key, CurrentMx);
                _cmd.Append($"End {KVP.Key}\n");
            }
            if(NoError)
                _cmd.Append("<<<<<<<<<<<<<<<[Transformation matrix Finished]>>>>>>>>>>>>>>> \n");
            return NoError;
        }
        /// <summary>
        /// Find Compacting name with a label
        /// </summary>
        /// <param name="Label"></param>
        /// <returns></returns>
        private string FindCompactingName(string Label)
        {
            return this.labelblockTable.FindCompactingName(Label);
        }
        /// <summary>
        /// Determine the action index based on the provided rule sequence
        /// </summary>
        /// <param name="RuleSequence"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public bool TargetAction(string RuleSequence, out int Index, out string Heading)
        {
            List<int> ContainsIndex = new List<int>();
            List<string> TargetHeading = new List<string>();
            bool ActiveAction = false;
            for (int i = 0; i < this._actionExe.Count; i++)
            {
                if (_actionExe[i](RuleSequence, out string Tar))
                {
                    TargetHeading.Add(Tar);
                    ContainsIndex.Add(i);
                    ActiveAction = true;
                }
            }
            if (ContainsIndex.Count == 1)
            {
                Index = ContainsIndex[0];
                Heading = TargetHeading[0];
                return ActiveAction;
            }
            else if (ContainsIndex.Count > 1)
            {
                int Min = 30;
                Index = -1;
                Heading = "";
                for (int i = 0; i < ContainsIndex.Count; i++)
                {
                    //The lower Precedence Index is the former priority
                    var SelectAction = this.Actions[ContainsIndex[i]];
                    if (SelectAction.GetPrecedence < Min)
                    {
                        Min = SelectAction.GetPrecedence;
                        Index = ContainsIndex[i];
                        Heading = TargetHeading[i];
                    }
                }
                return ActiveAction;
            }
            else
            {
                Index = -1;
                Heading = "";
                return ActiveAction;
            }
        }
        /// <summary>
        /// A copy from Add rules.
        /// </summary>
        private List<string> CommandLine = new List<string>();
        /// <summary>
        /// Reset this class
        /// </summary>
        public void Reset()
        {
            this.AddRule(this.CommandLine);
        }
        /// <summary>
        /// Add rules in to phraseinfo
        /// </summary>
        /// <param name="lines"></param>
        public void AddRule(List<string> lines)
        {
            this.CommandLine = lines;
            List<string> ProductionRules = new List<string>();
            for (int i = 0; i < lines.Count; i++)
                if (lines[i].Contains("=") || (lines[i][0] =='/' && lines[i][1] == '/'))
                    ProductionRules.Add(lines[i]);
            if (!this.phraseInfo.AddRule(ProductionRules, out string _Response))
            {
                throw new Exception(_Response);
            }
        }
        /// <summary>
        /// To print all actions
        /// </summary>
        /// <returns></returns>
        public string ActionList()
        {
            string List = "";
            for (int i = 0; i < Actions.Count; i++)
            {
                List += Actions[i].ToString() + "\n";
            }
            return List;
        }
    }
}
