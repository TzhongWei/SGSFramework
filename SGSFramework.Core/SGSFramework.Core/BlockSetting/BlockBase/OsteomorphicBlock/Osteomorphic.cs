using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicInterfaces;

namespace SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicBlock
{
    public abstract class OsteomorphicBlockBase:Block, IEquatable<OsteomorphicBlockBase>
    {
        public List<Brep> BrickSolid { get; protected set; }
        public OsteoBlockface[] Face { get; protected set; }
        protected List<OsteoBlockface> Topfaces;
        protected List<OsteoBlockface> Bottomfaces;
        public double HeightSize;
        public int n;
        public int m;
        protected OsteomorphicBlockBase(int n, int m) 
        {
            n = n <= 0 ? 1 : n;
            m = m <= 0 ? 1 : m;
        }

        public abstract bool Equals(OsteomorphicBlockBase other);
    }
}
