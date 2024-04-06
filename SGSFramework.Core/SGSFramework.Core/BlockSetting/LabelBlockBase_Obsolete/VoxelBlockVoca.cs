using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using SGSFramework.Core.BlockSetting.BlockBase;

namespace SGSFramework.Core.BlockSetting.LabelBlockBase
{
    [Obsolete]
    public class VoxelBlockVoca : LabelBlock
    {
        /// <summary>
        /// Voxel block
        /// </summary>
        public VoxelBlock Voxel { get; private set; }
        private Dictionary<string, string> _compactingName = new Dictionary<string, string>();
        protected override Dictionary<string, string> CompatibleName {
            get => _compactingName;
            set { _compactingName = value; }
        }
        /// <summary>
        /// Return the block id
        /// </summary>
        public int Block_ID => Voxel.Block_Id;
        public override void SetUpVocabulary()
        {
            if (this.Vocabulary.Count != 0) return;
            double size = (double)Util.GeneralSetting.SegUnit;

            Transform S = Transform.Translation(new Vector3d(size, 0, 0));
            Transform R = Transform.Rotation(Math.PI * 0.5, Voxel.CentrePt);
            Transform L = Transform.Rotation(-Math.PI * 0.5, Voxel.CentrePt);
            Transform UP = Transform.Translation(new Vector3d(0, 0, size));
            Transform F = Transform.Rotation(Math.PI, Vector3d.YAxis, Voxel.CentrePt);
            Transform Sf = F * S;
            Transform Rf = F * R;
            Transform Lf = F * L;
            Transform UPf = F * UP;

            this.Vocabulary.Add("S", S);
            this.Vocabulary.Add("R", R);
            this.Vocabulary.Add("L", L);
            this.Vocabulary.Add("UP", UP);
            this.Vocabulary.Add("F", F);
            this.Vocabulary.Add("Sf", Sf);
            this.Vocabulary.Add("Rf", Rf);
            this.Vocabulary.Add("Lf", Lf);
            this.Vocabulary.Add("Upf", UPf);
            
        }

        public override object Clone()
        {
            var Copy = new VoxelBlockVoca();
            Copy.CompatibleName = new Dictionary<string, string>(this.CompatibleName);
            Copy.Vocabulary = new Dictionary<string, Transform>(this.Vocabulary);
            return Copy;
        }

        public VoxelBlockVoca():base()
        {
            this.LabelBlockName = "TestBlockVoca";
            if (BlockTable.HasNamed("TestBlock"))
            {
                this.Voxel = BlockTable.IndexAt(BlockTable.FindBlockID("TestBlock")) as VoxelBlock;
                _compactingName = new Dictionary<string, string>();
                _compactingName.Add("Voxel_I", this.Voxel.BlockName);
            }
            else
            {
                this.Voxel = new VoxelBlock();
                this.Voxel.SetBlock();
                _compactingName = new Dictionary<string, string>();
                _compactingName.Add("Voxel_I", this.Voxel.BlockName);
            }
        }
        public override string LabelBlockName { get; protected set; }
        public override string ToString()
        {
            return "VoxelBlockVoca";
        }
    }
}
