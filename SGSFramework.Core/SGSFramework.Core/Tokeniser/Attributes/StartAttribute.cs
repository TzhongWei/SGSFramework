using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Tokeniser.Attributes
{
    public struct StartAttribute : ITokenAttribute_Action
    {
        public int ID { get; }
        public SymbolTable SymbolTable { get; }
        public Point3d StartPt { get; }
        public Point3d NextPt { get; }
        public Transform TransformMt { get; }
        public StartAttribute(SymbolTable symbolTable, Point3d StartPt, Point3d NextPt)
        {
            this.SymbolTable = symbolTable;
            this.ID = SymbolTable.GetID();
            this.StartPt = StartPt;
            this.NextPt = NextPt;
            Vector3d Vec = NextPt - StartPt;
            Vec.Unitize();
            double Angle = Vector3d.VectorAngle(Vector3d.XAxis, Vec);
            TransformMt = (Transform.Rotation(Angle, StartPt)) * Transform.Translation(new Vector3d(StartPt));
        }
        public Vector3d GetVector()
        {
            return NextPt - StartPt;
        }
        public bool Equals(ITokenAttribute other)
        {
            if (!(other is StartAttribute))
                return false;
            var otherSymbol = (StartAttribute)other;
            if (otherSymbol.StartPt == this.StartPt &&
                this.NextPt == otherSymbol.NextPt &&
                this.TransformMt == otherSymbol.TransformMt)
                return true;
            else
                return false;
        }
    }
}
