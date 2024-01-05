using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.DocObjects;
using System.Drawing;
using SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicInterfaces;

namespace SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicBlock
{
    public class OsteomorphicBlock : OsteomorphicBlockBase
    {
        public override Plane ReferencePlane { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
        public override string BlockType { get; protected set; }
        public List<Brep> TestFaces = new List<Brep>();
        public OsteomorphicBlock(OsteoBlockface face, int n = 1, int m = 1, double height = 10) : base(n, m)
        {
            n = n <= 0 ? 1 : n;
            m = m <= 0 ? 1 : m;
            this.Face = new OsteoBlockface[1]{face}; 
            this.HeightSize = height <= face.ZSize? face.ZSize + 5 : height;
            this.BlockType = $"OsteomorphicBlock => {n} x {m} Height {height}";
            double Unit = Util.GeneralSetting.SegUnit;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    var TempFace = (OsteoBlockface)face.Clone();
                    
                    if ((i % 2 == 1 && j % 2 != 1) || (i % 2 != 1 && j % 2 == 1))
                    {
                        TempFace.SetFlip();
                        TempFace.FaceColor = Color.LightBlue;
                    }
                    else
                    {
                        TempFace.FaceColor = Color.SteelBlue;
                    }
                    TempFace.Location = new Plane(new Point3d(Unit * i, Unit * j, height / 2), Vector3d.ZAxis);
                    Topfaces.Add((OsteoBlockface)TempFace.Clone());
                    TempFace = (OsteoBlockface)face.Clone();
                    
                    if ((i % 2 == 1 && j % 2 != 1) || (i%2 != 1 && j % 2 == 1) )
                    {
                        TempFace.SetFlip();
                        TempFace.FaceColor = Color.Green;
                    }
                    else
                    {
                        TempFace.FaceColor = Color.GreenYellow;
                    }
                    TempFace.Location = new Plane(new Point3d(Unit * i, Unit * j, -height / 2), Vector3d.ZAxis);
                    TempFace.Mirror();
                    Bottomfaces.Add(TempFace);
                }
            }
            SetShape();
        }
        protected override void SetShape()
        {
            var TopCrvs = OsteoBlockface.JoinFacesFrame(this.Topfaces);
            var BotCrvs = OsteoBlockface.JoinFacesFrame(this.Bottomfaces);

            List<Brep> Walls = new List<Brep>();

            for (int i = 0; i < TopCrvs.Count; i++)
            {
                Walls.AddRange(Brep.CreateDevelopableLoft(TopCrvs[i], BotCrvs[i], false, false, 5));
            }

            this.BrickSolid.AddRange(Brep.JoinBreps(Walls, 0.1));
            this.BrickSolid.AddRange(TopSrfs);
            this.BrickSolid.AddRange(BottomSrfs);
            var Atts = new ObjectAttributes { ColorSource = ObjectColorSource.ColorFromObject, ObjectColor = Color.Gray };
            this.AddComponent(Brep.JoinBreps(Walls, 0.1)[0], Atts);
            for (int i = 0; i < TopSrfs.Count; i++)
            {
                Atts = new ObjectAttributes { ColorSource = ObjectColorSource.ColorFromObject, ObjectColor = this.Topfaces[i].FaceColor };
                this.AddComponent(TopSrfs[i], Atts);
                Atts = new ObjectAttributes { ColorSource = ObjectColorSource.ColorFromObject, ObjectColor = this.Bottomfaces[i].FaceColor };
                this.AddComponent(BottomSrfs[i], Atts);
            }
        }

        public override bool Equals(OsteomorphicBlockBase other)
         => this.BlockType == other.BlockType;
    }
}
