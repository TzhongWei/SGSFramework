using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.Geometry;
using SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicInterfaces;

namespace SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicBlock
{
    public abstract class OsteomorphicBlockBase: Block, IEquatable<OsteomorphicBlockBase>
    {
        public List<Brep> BrickSolid { get; protected set; }
        public OsteoBlockface[] Face { get; protected set; }
        protected List<OsteoBlockface> Topfaces;
        protected List<OsteoBlockface> Bottomfaces;
        public List<Brep> TopSrfs => Topfaces.Select(x => x.Face).ToList();
        public List<Plane> TopAlignPlane => Topfaces.Select(x => x.AlignPlane).ToList();
        public List<Brep> BottomSrfs => Bottomfaces.Select(x => x.Face).ToList();
        public List<Plane> BottomAlignPlane => Bottomfaces.Select(x => x.AlignPlane).ToList();
        public double HeightSize;
        public int n;
        public int m;
        protected OsteomorphicBlockBase(int n, int m) :base()
        {
            n = n <= 0 ? 1 : n;
            m = m <= 0 ? 1 : m;
            this.Topfaces = new List<OsteoBlockface>();
            this.Bottomfaces = new List<OsteoBlockface>();
            this.BrickSolid = new List<Brep>();
        }
        public bool Bake
        {
            get
            {
                if (this.BrickSolid.Count == 0)
                    return false;
                else
                {
                    RhinoDoc Doc = RhinoDoc.ActiveDoc;
                    List<Guid> BlockIDs = new List<Guid>();
                    for (int i = 0; i < this.Components.Count; i++)
                        BlockIDs.Add(Doc.Objects.Add(this.Components[i].Duplicate(), this.blockAttribute._attributes[i]));

                    Doc.Groups.Add(this.BlockType, BlockIDs);

                    return true;
                }
            }
        }
        public abstract bool Equals(OsteomorphicBlockBase other);
    }
}
