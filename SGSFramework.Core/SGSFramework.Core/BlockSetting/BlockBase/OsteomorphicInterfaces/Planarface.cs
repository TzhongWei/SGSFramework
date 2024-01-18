using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicInterfaces
{
    public class Planarface : OsteoBlockface
    {
        public override string TypeName { get; }
        public Planarface(bool IsFlip = false) : base(0, IsFlip) { }
        protected override Plane _AlignPlane { get; set; }
        protected override Brep _Face { get; set; }

        public override bool Equals(OsteoBlockface other)
          => this.ZSize == other.ZSize && this.IsFlip == other.IsFlip && this.Location == other.Location;

        protected override void SetShape()
        {
            double size = Util.GeneralSetting.SegUnit;
            this._Perphery = new List<Curve>()
            {
                new LineCurve(new Point3d(-size / 2, - size / 2, 0), new Point3d(size/2, -size/2, 0)),
                new LineCurve(new Point3d(-size / 2, -size / 2, 0), new Point3d(-size / 2, size / 2, 0)),
                new LineCurve(new Point3d(-size / 2, size / 2, 0), new Point3d(size / 2, size / 2, 0)),
                new LineCurve(new Point3d(size / 2, -size / 2, 0), new Point3d(size / 2, size / 2, 0))
            };
            this._Face = Brep.CreateEdgeSurface(this._Perphery);
            if (this.IsFlip)
            {
                this._AlignPlane = Plane.WorldXY;
                this._AlignPlane.Rotate(Math.PI * 0.25, Vector3d.ZAxis);
            }
            else
            {
                this._AlignPlane = Plane.WorldXY;
                this._AlignPlane.Rotate(-Math.PI * 0.25, Vector3d.ZAxis);
            }

        }

        public override object Clone()
        {
            var Temp = new Planarface(IsFlip);
            Temp.Location = this.Location;
            Temp.FaceColor = this.FaceColor;
            if (Temp.IsMirror)
                Temp.Mirror();

            return Temp;
        }
    }
}
