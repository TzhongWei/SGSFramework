using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Tokeniser.Attributes
{
    public struct PointerAttribute : ITokenAttribute_Action  //Pop and push
    {
        public int ID => throw new NotImplementedException();
        public SymbolTable SymbolTable => throw new NotImplementedException();

        public Transform TransformMt => throw new NotImplementedException();

        public bool Equals(ITokenAttribute other)
        {
            throw new NotImplementedException();
        }

        public Vector3d GetVector()
        {
            throw new NotImplementedException();
        }
    }
}
