using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Tokeniser.Attributes
{
    public struct EndAttribute : ITokenAttribute_Action
    {
        public int ID { get; }
        public Point3d PreviousPt, CurrentPt;
        public Transform TransformMt { get; }
        public SymbolTable SymbolTable { get; }
        public EndAttribute(SymbolTable symbolTable, Point3d PreviousPt, Point3d CurrentPt)
        {
            this.SymbolTable = symbolTable;
            this.ID = SymbolTable.GetID();
            this.PreviousPt = PreviousPt;
            this.CurrentPt = CurrentPt;
            this.TransformMt = Transform.Translation(new Vector3d((CurrentPt - PreviousPt)));
        }
        public Vector3d GetVector()
        {
            return new Vector3d();
        }
        public bool Equals(ITokenAttribute other)
        {
            if (!(other is EndAttribute))
                return false;
            var otherSymbol = (EndAttribute)other;
            if (otherSymbol.PreviousPt == this.PreviousPt &&
                this.CurrentPt == otherSymbol.CurrentPt &&
                this.TransformMt == otherSymbol.TransformMt)
                return true;
            else
                return false;
        }
    }
}
