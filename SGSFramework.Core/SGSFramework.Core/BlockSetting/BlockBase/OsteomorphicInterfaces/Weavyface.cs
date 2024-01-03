using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicInterfaces
{
    public class Weavyface : OsteoBlockface
    {
        public Weavyface(double Height, bool IsFlip = false) : base(Height, IsFlip)
        {
        }
        public override string TypeName => "Weavyface";

        protected override Brep _Face { get ; set ; }
        protected override Plane _AlignPlane { get; set; }

        public override bool Equals(OsteoBlockface other)
        => this.ZSize == other.ZSize && this.IsFlip == other.IsFlip && this.Location == other.Location;
        protected override void SetShape()
        {
            double size = Util.GeneralSetting.SegUnit;
            double[] XYsize = new double[10];
            double[] Zsize = new double[10];
            for (int i = 0; i <= 9; i++)
            {
                XYsize[i] = size / 9 * i - size / 2;
                if (IsFlip)
                    Zsize[i] = this.ZSize - (Math.Sin((i / 4.5 - 1) * Math.PI * 0.5) * this.ZSize);
                else
                    Zsize[i] = Math.Sin((i / 4.5 - 1) * Math.PI * 0.5) * this.ZSize;
            }
            this._Perphery = new List<Curve>()
            {
                Curve.CreateInterpolatedCurve(PointAggragte(0), 3),
                Curve.CreateInterpolatedCurve(PointAggragte(1), 3),
                Curve.CreateInterpolatedCurve(PointAggragte(2), 3),
                Curve.CreateInterpolatedCurve(PointAggragte(3), 3)
            };
            
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

            IEnumerable<Point3d> PointAggragte(int Position)
            {
                for (int i = 0; i <= 9; i++)
                    switch (Position)
                    {
                        case (0):
                            yield return new Point3d(XYsize[i], -size / 2, Zsize[i]);
                            break;
                        case (1):
                            yield return new Point3d(-size / 2, XYsize[i], Zsize[i]);
                            break;
                        case (2):
                            yield return new Point3d(XYsize[i], size / 2, Zsize[i]);
                            break;
                        case (3):
                            yield return new Point3d(size / 2, XYsize[i], Zsize[i]);
                            break;
                        default:
                            break;
                    }
            }
        }
    }
}
