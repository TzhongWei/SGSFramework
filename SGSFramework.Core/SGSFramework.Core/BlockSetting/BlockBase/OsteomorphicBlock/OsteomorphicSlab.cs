using Rhino.Geometry;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Rhino.DocObjects;
using SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicInterfaces;

namespace SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicBlock
{
    public class OsteomorphicSlab : OsteomorphicBlockBase
    {
        public override Plane ReferencePlane { get; protected set; }
        public override string BlockType { get; protected set; }

        public OsteomorphicSlab(OsteoBlockface face, int n = 1, int m = 1, double height = 10) : base(n, m)
        {
            this.ReferencePlane = Plane.WorldXY;
            n = n <= 0 ? 1 : n;
            m = m <= 0 ? 1 : m;
            this.Face = new OsteoBlockface[2] { face, new Planarface() };
            this.HeightSize = height <= face.ZSize ? face.ZSize + 5 : height;
            this.BlockType = $"OsteomorphicSlab => {n} x {m} Height {height}";
            double Unit = Util.GeneralSetting.SegUnit;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    var TopFace = (OsteoBlockface)face.Clone();
                    var Botface = new Planarface();
                    if ((i % 2 == 1 && j % 2 != 1) || (i % 2 != 1 && j % 2 == 1))
                    {
                        TopFace.SetFlip();
                        TopFace.FaceColor = Color.YellowGreen;
                        Botface.SetFlip();
                        Botface.FaceColor = Color.OrangeRed;
                    }
                    else
                    {
                        TopFace.FaceColor = Color.LightYellow;
                        Botface.FaceColor = Color.DarkOrange;
                    }
                    TopFace.Location = new Plane(new Point3d(Unit * i, Unit * j, height / 2), Vector3d.ZAxis);
                    Topfaces.Add((OsteoBlockface)TopFace.Clone());
                    Botface.Location = new Plane(new Point3d(Unit * i, Unit * j, -height / 2), Vector3d.ZAxis);
                    Botface.Mirror();
                    Bottomfaces.Add(Botface);
                }
            SetShape();
        }

        public override bool Equals(OsteomorphicBlockBase other)
        => this.BlockType == other.BlockType;

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
    }
}
