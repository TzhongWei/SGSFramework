using System;
using System.Collections;
using System.Linq;
using Rhino.Geometry;
using Rhino.Input.Custom;
using SGSFramework.Core.Util;

namespace SGSFramework.Core.Tokeniser.Attributes
{
    /// <summary>
    /// Token label all possible behaviours that the block permutation process needs to follow
    /// </summary>
    internal class NamespaceDoc { }
    /// <summary>
    /// A token type must have an ID and placed in a defined symbol table
    /// </summary>
    public interface ITokenAttribute : IEquatable<ITokenAttribute>
    {
        /// <summary>
        /// The ID for the token itself which points to the symbol table
        /// </summary>
        int ID { get; }
        /// <summary>
        /// The symbol table places the token
        /// </summary>
        SymbolTable SymbolTable { get; }
    }
    /// <summary>
    /// The token illustrate a specific transformation
    /// </summary>
    public interface ITokenAttribute_Action : ITokenAttribute
    {
        /// <summary>
        /// This transformation matrix can be addressed to transform object onto the node position.
        /// </summary>
        Transform TransformMt { get; }
        /// <summary>
        /// The Vector Show the next step
        /// </summary>
        /// <returns></returns>
        Vector3d GetVector();
    }

    public struct DisplayRegion
    {
        public Circle Region { get; }
        public PolylineCurve RectangleCell { get; }
        private Point3d CentrePt { get; }
        private int SegUnit;
        public DisplayRegion(Point3d CentrePt, Point3d NextPt)
        {
            this.CentrePt = CentrePt;
            this.SegUnit = GeneralSetting.SegUnit;
            Region = new Circle(CentrePt, this.SegUnit);
            this.RectangleCell = Util.Util.GetRectangle(CentrePt, NextPt - CentrePt);
        }
    }

}
