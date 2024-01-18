using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SGSFramework.Core.BlockSetting.BlockBase;
using SGSFramework.Core.Token;

namespace SGSFramework.Core.BlockSetting.LabelBlockBase_2
{
    /// <summary>
    /// This class compacts with the compiling classes which is to deal with the geometry storages, 
    /// and the acumulate the label block and provide the information compiling classes need. 
    /// </summary>
    public class LabelBlockTable
    {
        /// <summary>
        /// Unset the class 
        /// </summary>
        public static LabelBlockTable Unset => new LabelBlockTable();
        /// <summary>
        /// Table Name
        /// </summary>
        public string TableName = "Unnamed";
        /// <summary>
        /// This dictionary save all instanceobject guid in rhino
        /// </summary>
        public Dictionary<string, List<Guid>> RhinoReferenceBlockGuid;
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
        /// The label shape set is ready to be called out
        /// </summary>
        public List<LabelBlock> _LabelBlocks { get; private set; }
        /// <summary>
        /// to check if there is duplicated labels between label blocks
        /// if true, there are some labels in label blocks dictionary is the same.
        /// Then, the system will return a new dictionary named with different labels. 
        /// </summary>
        /// <returns> if true, there are duplicated labels, else false</returns>
        private bool CheckDuplicatedLabel(IEnumerable<LabelBlock> IElabelBlocks)
        {
            var labelBlocks = IElabelBlocks.ToList();
            for (int i = 0; i < labelBlocks.Count; i++)
                for (int j = 0; j < labelBlocks.Count; j++)
                {
                    if (i != j)
                    {
                        if (BlockTokenList.IsLabelDuplicated(
                            labelBlocks[i].blockTokens, 
                            labelBlocks[j].blockTokens, 
                            out List<(int, int)> DuList))
                        {
                            for (int k = 0; k < DuList.Count; k++)
                                cmd += $"LabelBlock _ {i} and {j} has duplicated";
                            return true;
                        }
                    }
                }
            return false;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="labelBlocks"></param>
        public LabelBlockTable(params LabelBlock[] labelBlocks)
        {
            this.Isinvalid = CheckDuplicatedLabel(labelBlocks);
            this._LabelBlocks = labelBlocks.ToList();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="labelBlocks"></param>
        public LabelBlockTable(IEnumerable<LabelBlock> labelBlocks) : this(labelBlocks.ToArray())
        {
        }
        /// <summary>
        /// This type store the transform geometry object from guids
        /// </summary>
        public Dictionary<string, List<(GeometryBase, ObjectAttributes)>> BlockObject { get; private set; }
        /// <summary>
        /// Storing all the transformation after commanding from compiling process.
        /// </summary>
        public Dictionary<string, TerminalActionTokenList> Actions = new Dictionary<string, TerminalActionTokenList>();
        /// <summary>
        /// Storing all the called out block name in compacting name
        /// in a list which is used to permute
        /// the component into the rhino environment
        /// the list store the terminal token name
        /// </summary>
        public Dictionary<string, List<string>> CalledLabels = new Dictionary<string, List<string>>();
        /// <summary>
        /// Try to find compacting name from label
        /// </summary>
        /// <param name="Label"></param>
        /// <returns></returns>
        public string FindCompactingName(string Label)
        {
            for (int i = 0; i < this._LabelBlocks.Count; i++)
            {
                string Name;
                if (this._LabelBlocks[i].blockTokens.TryGetCompactible(Label, out Name))
                    return Name;

            }
            return "";
        }
        public List<string> Terminals
        {
            get
            {
                List<string> terminals = new List<string>();
                var TempLabel = (LabelBlock)_LabelBlocks[0].Clone();
                for (int i = 1; i < _LabelBlocks.Count; i++)
                {
                    TempLabel += _LabelBlocks[i];
                }
                TempLabel.blockTokens.Sort();
                return TempLabel.Labels;
            }
        }
        /// <summary>
        /// Clear blockObject
        /// </summary>
        public void ResetBlocks()
        {
            this.BlockObject = null;
        }
        /// <summary>
        /// bake all blocks reference instances from nonterminals into rhino
        /// </summary>
        /// <returns></returns>
        public virtual bool GetAllBlocks()
        {
            this.RhinoReferenceBlockGuid = new Dictionary<string, List<Guid>>();
            bool Result = true;
            foreach (var kvp in Actions)
            {
                Result |= kvp.Value.Run(kvp.Key);
                this.cmd += kvp.Key + " => " + kvp.Value.cmd;
            }
            return Result;
        }
        /// <summary>
        /// bake blocks reference from the given nonterminal label
        /// </summary>
        /// <param name="Label"></param>
        /// <returns></returns>
        public virtual bool GetBlocks(string Label)
        {
            if (this.RhinoReferenceBlockGuid == null)
                this.RhinoReferenceBlockGuid = new Dictionary<string, List<Guid>>();

            if (this.RhinoReferenceBlockGuid.ContainsKey(Label))
                this.RhinoReferenceBlockGuid[Label] = new List<Guid>();
            else
                this.RhinoReferenceBlockGuid.Add(Label, new List<Guid>());

            if (!Actions.ContainsKey(Label))
            {
                cmd += $"There isn't {Label} in production rules";
                return false;
            }
            bool Result = Actions[Label].Run(Label);
            this.cmd += Label + " => " + Actions[Label].cmd;
            return Result;
        }
        private void GetObjFromGuid(string Label)
        {
            this.GetBlocks(Label);
            RhinoDoc Doc = RhinoDoc.ActiveDoc;
            if(BlockObject is null)
                this.BlockObject = new Dictionary<string, List<(GeometryBase, ObjectAttributes)>>();
            BlockObject.Add(Label, new List<(GeometryBase, ObjectAttributes)>());
            foreach (var ID in this.RhinoReferenceBlockGuid[Label])
            {
                InstanceReferenceGeometry InsObj = null;
                try
                {
                    InsObj = Doc.Objects.FindId(ID).Geometry as InstanceReferenceGeometry;
                }
                catch
                {
                    throw new Exception($"InstanceReferenceGeometry of {Label} cannot be found in Rhino");
                }
                BlockObject[Label].AddRange(GetRhinoObj(InsObj));
                Doc.Objects.Delete(ID, true); 
            }
        }
        /// <summary>
        /// Get the blocks and attribute after GetAllBlocks() with the input label
        /// </summary>
        /// <param name="Label"> the non-terminal Labels of block arrangement</param>
        /// <returns></returns>
        public List<(GeometryBase, ObjectAttributes)> GetRhinoObj(string Label)
        {
            if (BlockObject is null)
                GetObjFromGuid(Label);
            if (!BlockObject.ContainsKey(Label))
                GetObjFromGuid(Label);
            return BlockObject[Label];
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
        public bool TryGetTransform(string Label, out Transform TS)
        {
            foreach (var item in _LabelBlocks)
            {
                if (item.blockTokens.TryGetTransform(Label, out TS))
                    return true;
            }
            TS = Transform.Identity;
            return false;
        }
        public bool TryGetCompatibleName(string Label, out string CompatibleName)
        {
            foreach (var item in _LabelBlocks)
            {
                if (item.blockTokens.TryGetCompactible(Label, out CompatibleName))
                    return true;
            }
            CompatibleName = "";
            return false;
        }
        public bool TryGetBlockToken(string Label, out BlockToken blockToken)
        {
            foreach (var labelblock in this._LabelBlocks)
            {
                foreach (var Token in labelblock.blockTokens)
                {
                    if (Token._name == Label)
                    {
                        blockToken = Token;
                        return true;
                    }
                }
            }
            blockToken = null;
            return false;
        }
    }
}
