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
        /// <summary>
        /// Clone this interface
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var Temp = new Weavyface(this.ZSize, this.IsFlip);
            Temp.Location = this.Location;
            Temp.FaceColor = this.FaceColor;
            return Temp;
        }
        protected override void SetShape()
        {
            double size = Util.GeneralSetting.SegUnit;
            double[] XYsize = new double[10];
            double[] Zsize = new double[10];
            for (int i = 0; i <= 9; i++)
            {
                XYsize[i] = size / 9 * i - size / 2;
                if (IsFlip)
                    Zsize[i] = -(Math.Sin((i / 4.5 - 1) * Math.PI * 0.5) * this.ZSize);
                else
                    Zsize[i] = Math.Sin((i / 4.5 - 1) * Math.PI * 0.5) * this.ZSize;
            }
            var PointList1 = PointAggragte(0).ToList();
            var PointList2 = PointAggragte(1).ToList();
            var PointList3 = PointAggragte(2).ToList();
            var PointList4 = PointAggragte(3).ToList();
            this._Perphery = new List<Curve>()
            {
                Curve.CreateInterpolatedCurve(PointAggragte(0), 3),
                Curve.CreateInterpolatedCurve(PointAggragte(1), 3),
                Curve.CreateInterpolatedCurve(PointAggragte(2), 3),
                Curve.CreateInterpolatedCurve(PointAggragte(3), 3)
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
                            yield return new Point3d(XYsize[i], size / 2, Zsize[9 - i]);
                            break;
                        case (3):
                            yield return new Point3d(size / 2, XYsize[i], Zsize[9 - i]);
                            break;
                        default:
                            break;
                    }
            }
        }
    }
}
