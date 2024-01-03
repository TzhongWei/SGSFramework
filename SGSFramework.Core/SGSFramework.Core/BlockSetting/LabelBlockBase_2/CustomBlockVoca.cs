using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using SGSFramework.Core.BlockSetting.BlockBase;
using SGSFramework.Core.Token;

namespace SGSFramework.Core.BlockSetting.LabelBlockBase_2
{
    public class CustomBlockVoca : LabelBlock
    {
        public override string LabelBlockName { get; protected set; } = "CustomBlockVoca";
        public Block block { get; private set; }
        private bool IsSetBlock = false;
        /// <summary>
        /// Labels and axes must be unambigious and one by one
        /// </summary>
        private Dictionary<string, Plane> AxisAndLabel;
        public CustomBlockVoca(Block block):base()
        {
            this.LabelBlockName = block.BlockName + "Voca";
            this.block = block;
            if (BlockTable.HasNamed(block.BlockName))
            {
                this.IsSetBlock = true;
            }
            this.blockTokens = new BlockTokenList(this.LabelBlockName);
            this.blockTokens.Add($"{block.BlockName}_I", block.BlockName);
            AxisAndLabel = new Dictionary<string, Plane>();
        }
        private Plane ReferencePlane = new Plane();
        public void SetLabelAndTransform(List<string> Labels, List<Guid> Axes, Guid ReferenceAxis)
        {
            if (Labels.Count != Axes.Count)
                return;
            var PLS = Axes.Select(x => Util.GeneralSetting.AxisToPlane(x)).ToList();
            var RefPL = Util.GeneralSetting.AxisToPlane(ReferenceAxis);
            for (int i = 0; i < Labels.Count; i++)
            {
                if (!AxisAndLabel.ContainsKey(Labels[i]))
                {
                    this.ReferencePlane = RefPL;
                    AxisAndLabel.Add(Labels[i], PLS[i]);
                }
                else
                    throw new Exception("Label Duplicated at CustomBlock Voca setting");
            }
        }
        public override void SetUpVocabulary()
        {
            if (AxisAndLabel.Count == 0 || !this.IsSetBlock)
                return;

            foreach (var KVP in this.AxisAndLabel)
            {
                Plane TempReference = new Plane(this.ReferencePlane);
                TempReference.Origin = Point3d.Origin;
                var BackToOri = Transform.PlaneToPlane(this.ReferencePlane, TempReference);
                var KvpValue = KVP.Value;
                KvpValue.Transform(BackToOri);

                Transform TS = Transform.PlaneToPlane(TempReference, KvpValue);
                this.blockTokens.Add(KVP.Key, TS);
            }
        }
        public override string ToString()
        {
            return this.GetType().Name + $"_{this.LabelBlockName}";
        }
        public override object Clone()
        {
            var NewThis = new CustomBlockVoca(this.block);
            NewThis.blockTokens = (BlockTokenList)this.blockTokens.Clone();
            NewThis.AxisAndLabel = this.AxisAndLabel;
            return NewThis;
        }
    }
}
