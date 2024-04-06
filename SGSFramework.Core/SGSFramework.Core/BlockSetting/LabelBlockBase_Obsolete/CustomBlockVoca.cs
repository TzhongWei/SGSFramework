using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using SGSFramework.Core.BlockSetting.BlockBase;

namespace SGSFramework.Core.BlockSetting.LabelBlockBase
{
    [Obsolete]
    public class CustomBlockVoca : LabelBlock
    {
        public Block block { get; private set; }
        public int Block_ID => block.Block_Id;
        private bool IsSetBlock = false;
        protected override Dictionary<string, string> CompatibleName { get; set; }
        public CustomBlockVoca(Block block) : base()
        {
            this.block = block;
            
            if (BlockTable.HasNamed(block.BlockName) || block.Block_Id == -1)
            {
                IsSetBlock = true;
            }
            this.CompatibleName = new Dictionary<string, string>();
            this.LabelBlockName = this.block.BlockName + "Voca";
            this.CompatibleName.Add($"{block.BlockName}_I", block.BlockName);
            this.AxisAndLabel = new Dictionary<string, Plane>();
        }
        public override object Clone()
        {
            var Copy = new CustomBlockVoca(this.block);
            Copy.CompatibleName = new Dictionary<string, string>(this.CompatibleName);
            Copy.Vocabulary = new Dictionary<string, Transform>(this.Vocabulary);
            Copy.AxisAndLabel = new Dictionary<string, Plane>(this.AxisAndLabel);
            return Copy;
        }
        private Plane ReferencePlane = new Plane();
        /// <summary>
        /// Labels and axes must be unambigious and one by one
        /// </summary>
        private Dictionary<string, Plane> AxisAndLabel = new Dictionary<string, Plane>();
        public void SetLabelAndTransform(List<string> Labels, List<Guid> Axes)
        {
            if (Labels.Count != Axes.Count)
                return;
            var PLS = Axes.Select(x => Util.GeneralSetting.AxisToPlane(x)).ToList();
            for (int i = 0; i < Labels.Count; i++)
            {
                if (!AxisAndLabel.ContainsKey(Labels[i]))
                {
                    AxisAndLabel.Add(Labels[i], PLS[i]);
                    this.ReferencePlane = this.block.ReferencePlane;
                }
                else
                    throw new Exception("Label Duplicated at CustomBlock Voca setting");
            }
        }
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
        public override string LabelBlockName { get; protected set; }
        public override void SetUpVocabulary()
        {
            if (AxisAndLabel.Count == 0 || !IsSetBlock)
                return;

            

            foreach (var KVP in this.AxisAndLabel)
            {
                Plane TempReference = new Plane(this.ReferencePlane);
                TempReference.Origin = Point3d.Origin;
                var BackToOri = Transform.PlaneToPlane(this.ReferencePlane, TempReference);
                var KvpValue = KVP.Value;
                KvpValue.Transform(BackToOri);

                Transform TS = Transform.PlaneToPlane(TempReference, KvpValue);
                this.Vocabulary.Add(KVP.Key, TS);
            }
            
        }
        public override string ToString()
        {
            return this.GetType().Name + $"_{block.BlockName}";
        }
    }
}
