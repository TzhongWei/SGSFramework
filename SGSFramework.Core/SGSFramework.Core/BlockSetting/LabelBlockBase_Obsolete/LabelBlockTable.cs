using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SGSFramework.Core.BlockSetting.BlockBase;
using SGSFramework.Core.BlockSetting.LabelBlockBase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SGSFramework.Core.BlockSetting.LabelBlockBase
{
    /// <summary>
    /// This class compacts with the compiling classes which is to deal with the geometry storages, 
    /// and the acumulate the label block and provide the information compiling classes need. 
    /// </summary>
    [Obsolete("This class is Obsolete, please refer to the new namespace and new class", false)]
    public class LabelBlockTable
    {
        public static LabelBlockTable Unset => new LabelBlockTable();
        /// <summary>
        /// Table Name
        /// </summary>
        public string TableName = "unnamed";
        /// <summary>
        /// the results and error messages of the class
        /// </summary>
        public string cmd { get; private set; }
        /// <summary>
        /// Check if the table can be computed
        /// </summary>
        public bool Isinvalid { get; private set; } = false;
        ///// <summary>
        ///// Call labelblocksystem
        ///// </summary>
        //public static LabelBlockTable Unset => null;
        /// <summary>
        /// Not allow to use labelblocktable default constructor
        /// </summary>
        private LabelBlockTable() { Isinvalid = true; }
        /// <summary>
        /// to check if there is duplicated labels between label blocks
        /// if true, there are some labels in label blocks dictionary is the same.
        /// Then, the system will return a new dictionary named with different labels. 
        /// </summary>
        /// <returns> if true, there are duplicated labels, else false</returns>
        private bool CheckDuplicatedLabel()
        {
            #region CheckDuplicated
            bool IsSame = false;
            int DuplicatedEventCount = 0;
            List<string> DuplicatedMessages = new List<string>();
            for (int i = 0; i < this._LabelBlocks.Count; i++)
            {
                bool SelfOverlap = LabelBlock.IsVocabularyAndCompatibleNameSame(_LabelBlocks[i], out List<(int, int)> SameComAndVoc);
                if (SelfOverlap)
                {
                    IsSame = true;
                    for (int k = 0; k < SameComAndVoc.Count; k++)
                    {
                        string Message = $"{DuplicatedEventCount}, {_LabelBlocks[i]} has the same vocabulary and " +
                            $"compacting name at {SameComAndVoc[k].Item1} and {SameComAndVoc[k].Item2} \n";
                        DuplicatedEventCount++;
                        DuplicatedMessages.Add(Message);
                    }
                }
                for (int j = 0; j < this._LabelBlocks.Count; j++)
                {
                    if (i != j)
                    {
                        List<(int, int)> SameLabelIndex;
                        List<(int, int)> SameComIndex;
                        bool SameLabel = LabelBlock.IsVocabularySame(_LabelBlocks[i], _LabelBlocks[j], out SameLabelIndex);
                        bool SameCom = LabelBlock.IsCompatibleNameSame(_LabelBlocks[i], _LabelBlocks[j], out SameComIndex);
                        if (SameLabel)
                        {
                            for (int k = 0; k < SameLabelIndex.Count; k++)
                            {
                                string Message = $"{DuplicatedEventCount}, {_LabelBlocks[i]} and {_LabelBlocks[j]} " +
                                    $"vocabulary at index {SameLabelIndex[k]}, respectively," +
                                    $" have the same labels \n";
                                DuplicatedEventCount++;
                                DuplicatedMessages.Add(Message);
                            }
                            IsSame = true;
                        }
                        if (SameCom)
                        {
                            for (int k = 0; k < SameComIndex.Count; k++)
                            {
                                string Message = $"{DuplicatedEventCount}, {_LabelBlocks[i]} and {_LabelBlocks[j]} " +
                                    $"CompactingName at index {SameComIndex[k]}, respectively," +
                                    $" have the same labels \n";
                                DuplicatedEventCount++;
                                DuplicatedMessages.Add(Message);
                            }
                            IsSame = true;
                        }
                    }
                }
            }
            if (IsSame)
            {
                cmd = "=====================================\n" +
                    "Label blocks integrated failed \n" +
                    "=====================================\n";
            }
            else
            {
                cmd = "Label Blocks integrated successed\n";
            }
            for (int i = 0; i < DuplicatedMessages.Count; i++)
                cmd += DuplicatedMessages[i];
            #endregion
            return IsSame; //if true, there are duplicated labels, else false
        }
        /// <summary>
        /// Constructor to place the blocks 
        /// </summary>
        /// <param name="labelBlocks"></param>
        public LabelBlockTable(List<LabelBlock> labelBlocks)
        {
            this._LabelBlocks = labelBlocks;
            this._InitialLabelBlocks = labelBlocks.Select(x => x.Clone() as LabelBlock).ToList();
            if (CheckDuplicatedLabel())
            {
                Isinvalid = true;
            }
        }
        /// <summary>
        /// Reset this class
        /// </summary>
        public void Reset()
        {
            ///Reset with deep clone
            this._LabelBlocks = this._InitialLabelBlocks.Select(x => x.Clone() as LabelBlock).ToList();
            if (CheckDuplicatedLabel())
            {
                Isinvalid = true;
            }
        }
        /// <summary>
        /// The label shape set is ready to be called out
        /// </summary>
        public List<LabelBlock> _LabelBlocks { get; private set; }
        /// <summary>
        /// The Initial label blocks setting is for resetting the class
        /// </summary>
        private List<LabelBlock> _InitialLabelBlocks = new List<LabelBlock>();
        /// <summary>
        /// This type store the transform geometry object from guids
        /// </summary>
        public Dictionary<string, List<(GeometryBase, ObjectAttributes)>> BlockObject { get; private set; } = new Dictionary<string, List<(GeometryBase, ObjectAttributes)>>();
        /// <summary>
        /// Storing all the transformation after commanding from compiling process.
        /// </summary>
        public Dictionary<string, List<object>> Actions = new Dictionary<string, List<object>>();
        /// <summary>
        /// Storing all the called out block name in compacting name
        /// in a list which is used to permute
        /// the component into the rhino environment
        /// </summary>
        public Dictionary<string, List<string>> CalledLabels = new Dictionary<string, List<string>>();
        /// <summary>
        /// Find the block from the list of block in this LabelBlockSystem
        /// if find, return the block
        /// else, return null
        /// </summary>
        /// <param name="id"></param> the block ID in the rhino instance
        /// <returns></returns>
        public Block FindBlock(int id)
        {
            for (int i = 0; i < this._LabelBlocks.Count; i++)
            {
                var Names = _LabelBlocks[i].CompactingBlockName;
                for (int j = 0; j < Names.Count; j++)
                {
                    //Index is reference ID in 
                    int Index = BlockTable.FindBlockID(Names[i]);
                    if (Index == -1) continue;
                    if (BlockTable.IndexAt(Index).Block_Id != id) continue;
                    return BlockTable.IndexAt(Index);
                }
            }
            return null;
        }
        /// <summary>
        /// Try to find compacting name from label
        /// </summary>
        /// <param name="Label"></param>
        /// <returns></returns>
        public string FindCompactingName(string Label)
        {
            for (int i = 0; i < this._LabelBlocks.Count; i++)
            {
                var Name = this._LabelBlocks[i].GetCompatibleName(Label);
                if (Name != "")
                    return Name;
            }
            return "";
        }
        /// <summary>
        /// Terminals contains all vocabulary labels and Compacting name labels
        /// </summary>
        public List<string> Terminals
        {
            get
            {
                List<string> terminals = new List<string>();
                for (int i = 0; i < _LabelBlocks.Count; i++)
                {
                    terminals.AddRange(_LabelBlocks[i].VocaLabels);
                    terminals.AddRange(_LabelBlocks[i].ComLabels);
                }
                return terminals;
            }
        }
        /// <summary>
        /// Clear blockObject
        /// </summary>
        public void ResetBlocks()
        {
            this.BlockObject = new Dictionary<string, List<(GeometryBase, ObjectAttributes)>>();
        }
        /// <summary>
        /// Get All Block instance into Rhino
        /// </summary>
        /// <returns></returns>
        public virtual bool GetAllBlocks()
        {
            #region GETALLBLOCKS
            bool Result = true;
            if (this.BlockObject.Count > 0)
            {
                ResetBlocks();
            }
            foreach (var KVP in Actions)
            {
                if (KVP.Value.Count == 0)
                {
                    Result = false;
                    continue;
                }

                BlockObject.Add(KVP.Key, new List<(GeometryBase, ObjectAttributes)>());
                Transform CurrentTS = Transform.Identity;
                var Doc = RhinoDoc.ActiveDoc;
                var ActionValue = KVP.Value;

                for (int i = 0; i < ActionValue.Count; i++)
                {
                    var CurrentAction = ActionValue[i];
                    if (CurrentAction is Transform)
                    {
                        CurrentTS = (Transform)CurrentAction;
                    }
                    else
                    {
                        var Exiting_idef = Doc.InstanceDefinitions.Find((string)CurrentAction);
                        var ObjectAtt = new ObjectAttributes();
                        Layer layer = new Layer();
                        if (Doc.Layers.FindName("Generative_Blocks") is null)
                        {
                            layer = new Layer() { Name = "Generative_Blocks" };
                            Doc.Layers.Add(layer);
                        }
                        else
                        {
                            layer = Doc.Layers.FindName("Generative_Blocks");
                        }
                        ObjectAtt.LayerIndex = layer.Index;
                        Guid insertBlock = Doc.Objects.AddInstanceObject(Exiting_idef.Index, CurrentTS, ObjectAtt);
                        InstanceReferenceGeometry InsObj = null;
                        try
                        {
                            InsObj = Doc.Objects.FindId(insertBlock).Geometry as InstanceReferenceGeometry;
                        }
                        catch
                        {
                            throw new Exception($"InstanceReferenceGeometry of {CurrentAction} cannot be found in Rhino");
                        }
                        BlockObject[KVP.Key].AddRange(GetRhinoObj(InsObj));
                        Doc.Objects.Delete(insertBlock, false);
                    }
                }
            }
            #endregion
            return Result;
        }
        /// <summary>
        /// Get the blocks and attribute after GetAllBlocks() with the input label
        /// </summary>
        /// <param name="Label"> the non-terminal Labels of block arrangement</param>
        /// <returns></returns>
        public List<(GeometryBase, ObjectAttributes)> GetRhinoObj(string Label)
        {
            if (BlockObject.ContainsKey(Label))
            {
                return BlockObject[Label];
            }
            else
            {
                return new List<(GeometryBase, ObjectAttributes)>();
            }
        }
        /// <summary>
        /// Test if the compacting name is the same 
        /// </summary>
        /// <param name="TestLabel"></param>
        /// <returns></returns>
        public bool IsCompactingName(string TestLabel)
        {
            for (int i = 0; i < _LabelBlocks.Count; i++)
            {
                if (_LabelBlocks[i].HasComLabel(TestLabel))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Get transformation matrix from the label block with vocabulary
        /// </summary>
        /// <param name="VocaLabel"></param>
        /// <returns></returns>
        [Obsolete("This action is integrated into ActionToken", true)]
        public Transform this[string VocaLabel]
        {
            get
            {
                for (int i = 0; i < this._LabelBlocks.Count; i++)
                {
                    if (this._LabelBlocks[i].HasVocaLabel(VocaLabel))
                        return this._LabelBlocks[i][VocaLabel];
                }
                this.cmd += $"Error at {VocaLabel}, there is no such label in the label block table.\n";
                return Transform.Identity;
            }
        }
        /// <summary>
        /// Return class name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.GetType().Name;
        }
        /// <summary>
        /// Return geometries and colours
        /// </summary>
        /// <param name="Symbol">The Nonterminal Symbol</param>
        /// <returns>List<(GeometryBase, Color)> Geometries, and their colours </returns>
        public List<(GeometryBase, Color)> DisplayGeometry(string Symbol)
        {
            var display = this.GetRhinoObj(Symbol);
            var GeomCol = new List<(GeometryBase, Color)>();

            for (int i = 0; i < display.Count; i++)
            {
                GeomCol.Add((display[i].Item1, display[i].Item2.ObjectColor));
            }
            return GeomCol;
        }
        /// <summary>
        /// Return geometries and their attributes
        /// </summary>
        /// <param name="InsObj"></param>
        /// <returns></returns>
        private static List<(GeometryBase, ObjectAttributes)> GetRhinoObj(InstanceReferenceGeometry InsObj)
        {
            var Doc = RhinoDoc.ActiveDoc;
            var result = new List<(GeometryBase, ObjectAttributes)>();
            Transform TS = InsObj.Xform;
            var Refer = Doc.InstanceDefinitions.FindId(InsObj.ParentIdefId);
            
            var ReferObjs = Refer.GetObjects();
            
            for (int i = 0; i < ReferObjs.Length; i++)
            {
                if (ReferObjs[i].Geometry is InstanceReferenceGeometry)
                {
                    var NewInsObj = ReferObjs[i].Geometry as InstanceReferenceGeometry;
                    result.AddRange(GetRhinoObj(NewInsObj));
                }
                else
                {
                    var GHGeam = ReferObjs[i].Geometry;
                    GHGeam.Transform(TS);
                    result.Add((GHGeam, ReferObjs[i].Attributes));
                }
            }
            return result;
        }
    }
}
