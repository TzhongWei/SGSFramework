using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.BlockSetting.BlockBase;
using SGSFramework.Core.Interface;
using SGSFramework.Core.Token;
using Rhino.Geometry;


namespace SGSFramework.Core.BlockSetting.LabelBlockBase_2
{
    public class SLBlockVoca : LabelBlock, IDiverseForms<SLBlock>
    {
        public override string LabelBlockName { get; protected set; } = "SLBlockVoca";
        public HashSet<SLBlock> blockList { get; set; }
        public List<int> Block_IDs { get; set; }

        public bool AddBlock(SLBlock block)
        {
            if (blockList.Contains(block))
                return false;
            if (blockList.Add(block))
            {
                this.blockTokens.Add(block.BlockName + "_I", block.BlockName);
                return true;
            }
            return false;
        }
        public SLBlockVoca(IEnumerable<SLBlock> blocks) : this(blocks.ToArray()) { }
        private static int UnitSet = 0;
        public SLBlockVoca(params SLBlock[] blocks) : base()
        {
            this.blockList = new HashSet<SLBlock>();
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                if (block.Block_Id == -1) continue;
                this.blockList.Add(block);
                this.blockTokens.Add($"{block.BlockName}_I", block.BlockName);
            }
        }
        public override void SetUpVocabulary()
        {
            if (UnitSet == Util.GeneralSetting.SegUnit && this.blockTokens.Count < 1)
                return;
            UnitSet = Util.GeneralSetting.SegUnit;

            string[] Labels = new string[] { "H", "A", "D", "S", "T", "Y" };
            double Stepper = Util.GeneralSetting.SegUnit;
            double[] TranslationX = new double[] {
                Stepper * 2,
                Stepper,
                Stepper * 2,
                Stepper,
                Stepper,
                Stepper
            };
            double[] TranslationY = new double[] {
                0,
                -Stepper,
                0,
                Stepper,
                -Stepper,
                Stepper
            };
            double[] TranslationZ = new double[] {
            Stepper * 4,
                0,
                -Stepper,
                Stepper * 3,
                Stepper *5,
                -Stepper * 2
            };
            double[] RotateX = new double[]
            {
                0, -90, 0, 90, -90, 90
            };
            double[] RotateZ = new double[]
            {
                180, 0, 0, 180, 180, 0
            };
            for (int i = 0; i < 6; i++)
            {
                var MxTranslation = Transform.Translation(new Vector3d(TranslationX[i], TranslationY[i], TranslationZ[i]));
                var MxRotateX = Transform.Rotation(RotateX[i] * Math.PI / 180, Point3d.Origin);
                var MxRotateZ = Transform.Rotation(-RotateZ[i] * Math.PI / 180, Vector3d.XAxis, Point3d.Origin);
                this.blockTokens.Add(Labels[i], MxTranslation * MxRotateX * MxRotateZ);
            }
        }
        public override object Clone()
        {
            var NewThis = new SLBlockVoca(this.blockList);
            NewThis.blockTokens = (BlockTokenList) this.blockTokens.Clone();
            return NewThis;
        }
    }
}
