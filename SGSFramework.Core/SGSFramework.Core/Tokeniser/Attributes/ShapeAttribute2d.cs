using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Tokeniser.Attributes
{
    public struct ShapeAttribute2d : ITokenAttribute_Action
    {
        public int ID { get; }
        public Point3d CurrentPoint { get; }
        private Point3d PreviousPoint { get; }
        private Point3d NextPoint { get; }
        public SymbolTable SymbolTable { get; }
        public Transform TransformMt { get; private set; }
        public ShapeAttribute2d(SymbolTable symbolTable, Point3d PreviousPoint, Point3d CurrentPt, Point3d NextPoint)
        {
            this.ID = symbolTable.GetID();
            this.SymbolTable = symbolTable;
            this.CurrentPoint = CurrentPt;
            this.PreviousPoint = PreviousPoint;
            this.NextPoint = NextPoint;
            this.TransformMt = new Transform();
            SetUpTransform();
        }
        private void SetUpTransform()
        {
            Vector3d V1 = CurrentPoint - PreviousPoint;
            Vector3d V2 = NextPoint - CurrentPoint;

            if (V1 == V2)   //Straight
                this.TransformMt = Transform.Translation(V1);
            else
            {
                double angle = Vector3d.VectorAngle(V1, V2, Plane.WorldXY);
                this.TransformMt = Transform.Rotation(angle, Vector3d.ZAxis, Point3d.Origin) * Transform.Translation(V1);
            }
        }
        /// <summary>
        /// The Vector Show the next step
        /// </summary>
        /// <returns></returns>
        public Vector3d GetVector()
        {
            return this.NextPoint - this.CurrentPoint;
        }
        public bool Equals(ITokenAttribute other)
        {
            if (!(other is ShapeAttribute2d))
                return false;
            ShapeAttribute2d _Att = (ShapeAttribute2d)other;
            if (_Att.CurrentPoint == this.CurrentPoint && _Att.PreviousPoint == this.PreviousPoint && _Att.NextPoint == this.NextPoint)
                return true;
            else
                return false;

        }
    }
}
