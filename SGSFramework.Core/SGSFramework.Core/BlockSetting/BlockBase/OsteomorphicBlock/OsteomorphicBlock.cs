using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicInterfaces;

namespace SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicBlock
{
    public class OsteomorphicBlock : OsteomorphicBlockBase
    {
        public override Plane ReferencePlane { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
        public override string BlockType { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
        public OsteomorphicBlock(OsteoBlockface face, int n = 1, int m = 1, double height = 10) : base(n, m)
        {
            this.Face = new OsteoBlockface[1]{face}; 
            this.HeightSize = height <= face.ZSize? face.ZSize + 5 : height;
        }
        protected override void SetShape()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(OsteomorphicBlockBase other)
        {
            throw new NotImplementedException();
        }
    }
}
