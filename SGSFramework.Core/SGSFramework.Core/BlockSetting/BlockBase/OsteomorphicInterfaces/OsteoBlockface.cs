using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Rhino.Geometry;
namespace SGSFramework.Core.BlockSetting.BlockBase.OsteomorphicInterfaces
{
    public abstract class OsteoBlockface : ICloneable, IEquatable<OsteoBlockface>
    {
        public Brep Face => Alignment<Brep>(this.Face);
        public List<Curve> Perphery => this.Perphery.Select(x => Alignment<Curve>(x)).ToList();
        public Plane AlignPlane { get { var Temp = this.AlignPlane.Clone(); Temp.Transform(OrientTS); return Temp; } }
        protected ObjectT Alignment<ObjectT>(ObjectT OrientGeometry) where ObjectT : GeometryBase
        {
            var Temp = OrientGeometry.Duplicate();
            Temp.Transform(OrientTS);
            return (ObjectT) Temp;
        }
        protected Transform OrientTS => Transform.PlaneToPlane(this._AlignPlane, this.Location);
        public static OsteoBlockface Unset { get; }
        public int index;
        static OsteoBlockface()
        {
            Unset._AlignPlane = Plane.Unset;
        }

        protected OsteoBlockface()
        {
            SetShape();
        }
        protected OsteoBlockface(double Height, bool IsFlip) : this()
        {
            this.ZSize = Height;
            this.IsFlip = IsFlip;
        }
        public Plane Location { get; set; } = Plane.WorldXY;
        public Color FaceColor;
        /// <summary>
        /// The orientation of the plane
        /// </summary>
        protected abstract Plane _AlignPlane { get; set; }
        protected Plane _ReverseAlignPlane
        { 
            get
            {
                return new Plane(this._AlignPlane.Origin, -this._AlignPlane.XAxis, -this._AlignPlane.YAxis);
            }
        }
        /// <summary>
        /// To determine the orientation between 0 and 90 degree
        /// </summary>
        public bool IsFlip { get; }
        public double ZSize { get; protected set; }
        public abstract string TypeName { get; }
        protected abstract Brep _Face { get; set; }
        /// <summary>
        /// The Perphery always start from the left most edge
        ///
        ///           (2)    
        ///        - - - - -   
        ///       |         |  
        ///   (3) |         | (1)
        ///       |         |
        ///        - - - - -
        ///           (0)
        /// </summary>
        protected List<Curve> _Perphery { get; set; }
        protected abstract void SetShape();
        public override string ToString()
            => TypeName;
        public virtual object Clone() => this.Clone<OsteoBlockface>();
        public Face Clone<Face>() where Face : OsteoBlockface
        {
            Face Temp = (Face)Activator.CreateInstance(typeof(Face), this.ZSize, this.IsFlip);
            return Temp;
        }
        public abstract bool Equals(OsteoBlockface other);
        public static bool IsSameType(params OsteoBlockface[] faces)
        {
            string FaceType = faces[0].TypeName;
            for (int i = 1; i < faces.Length; i++)
            {
                if (FaceType != faces[i].TypeName)
                    return false;
            }
            return true;
        }
        public virtual bool Mirror()
        {
            if (_Face == null || this._Perphery == null)
                return false;
            var MirrorTS = Transform.Mirror(this._AlignPlane);
            this._Face.Transform(MirrorTS);
            this._Perphery = this._Perphery.Select(x => 
            { 
                x.Transform(MirrorTS);
                return x;
            }).ToList();
            this._AlignPlane.Transform(MirrorTS);
            return true;
        }
        public static List<Curve> JoinFacesFrame(IEnumerable<OsteoBlockface> faces)
        {
            if (!IsSameType(faces.ToArray())) return null;
            var faceList = faces.ToList();
            List<Curve> Edges = new List<Curve>();
            List<Point3d> TestPtsList = new List<Point3d>();
            for (int i = 0; i < faceList.Count; i++)
            {
                var TestPts = EdgeTestPoints(faceList[i]);
                for (int j = 0; j < TestPts.Count; j++)
                {
                    if (TestPtsList.Contains(TestPts[j]))
                    {
                        var Index = TestPtsList.IndexOf(TestPts[j]);
                        TestPtsList.RemoveAt(Index);
                        Edges.RemoveAt(Index);
                    }
                    else
                    {
                        Edges.Add(faceList[i].Perphery[j]);
                        TestPtsList.Add(TestPts[j]);
                    }
                }
            }
            return Edges;

            List<Point3d> EdgeTestPoints(OsteoBlockface face)
            {
                return new List<Point3d>()
                {
                    (face.Perphery[0].PointAtEnd + face.Perphery[0].PointAtStart) / 2,
                    (face.Perphery[1].PointAtEnd + face.Perphery[1].PointAtStart) / 2,
                    (face.Perphery[2].PointAtEnd + face.Perphery[2].PointAtStart) / 2,
                    (face.Perphery[3].PointAtEnd + face.Perphery[3].PointAtStart) / 2
                };
            }
        }
    }
}
