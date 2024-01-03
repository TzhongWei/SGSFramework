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
    /// <summary>
    /// It must be only one voxel block voca in a rhino file.
    /// </summary>
    public class VoxelBlockVoca : LabelBlock
    {
        public override string LabelBlockName { get; protected set; } = "TestBlockVoca";
        public VoxelBlock Voxel { get; private set; }
        public VoxelBlockVoca() : base()
        {
            if (BlockTable.HasNamed("TestBlock"))
            {
                this.Voxel = BlockTable.IndexAt(BlockTable.FindBlockID("TestBlock")) as VoxelBlock;
                this.blockTokens.Add("Voxel_I", this.Voxel.BlockName);
            }
            else
            {
                this.Voxel = new VoxelBlock();
                this.Voxel.SetBlock();
                this.blockTokens.Add("Voxel_I", this.Voxel.BlockName);
            }
        }
        public override void SetUpVocabulary()
        {
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

            this.blockTokens.Add("S", S);
            this.blockTokens.Add("R", R);
            this.blockTokens.Add("L", L);
            this.blockTokens.Add("UP", UP);
            this.blockTokens.Add("F", F);
            this.blockTokens.Add("Sf", Sf);
            this.blockTokens.Add("Rf", Rf);
            this.blockTokens.Add("Lf", Lf);
            this.blockTokens.Add("Upf", UPf);
        }
        public override object Clone()
        {
            var NewThis = new VoxelBlockVoca();
            NewThis.blockTokens = (BlockTokenList)this.blockTokens.Clone();
            return NewThis;
        }
    }
}
