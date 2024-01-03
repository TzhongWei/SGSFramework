using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Rhino.Geometry;
using Rhino.Render;
using SGSFramework.Core.Tokeniser.Attributes;

namespace SGSFramework.Core.Tokeniser
{
    internal class NamespaceDoc { }
    public struct PointsReader
    {
        public Point3d Backward { get; }
        public Point3d Current { get; }
        public Point3d Forward { get; }
        public PointsReader(Point3d Previous, Point3d Current, Point3d Next)
        {
            this.Backward = Previous;
            this.Current = Current;
            this.Forward = Next;
            this.IsNode = false;
            this.IsBranch = false;
        }
        public bool IsNode;
        public bool IsBranch;
    }
    public class Tokeniser
    {
//==================================Attribute=================================
        /// <summary>
        /// a table store ID pointers and corresponding attributes, which means different value-types.
        /// </summary>
        public SymbolTable SymbolTable { get; private set; } = new SymbolTable();
        /// <summary>
        /// a token table saves all tokens
        /// </summary>
        public TokenTable tokenTable { get; private set; } = new TokenTable();
        /// <summary>
        /// Stream store a list of buffer read from curves
        /// </summary>
        public BufferStream Stream { get; private set; }
        public List<PointsReader> Readers { get; private set; } = new List<PointsReader>();
        /// <summary>
        /// Constructors
        /// </summary>
        /// <param name="stream"></param>
        internal Tokeniser(BufferStream stream)
        {
            Stream = stream;
        }
        public Tokeniser(IEnumerable<Curve> curves)
        {
            if (Util.Util.TestCurve(curves))
                Stream = new BufferStream(curves);
            else
                throw new Exception("Some curves are shorter than segment unit setting");
            SymbolTable = new SymbolTable();
            tokenTable = new TokenTable();
        }
        public Tokeniser(Curve Crv)
        {
            if (Util.Util.TestCurve(Crv))
                Stream = new BufferStream(Crv);
            else
                throw new Exception("Some curves are shorter than segment unit setting");
            SymbolTable = new SymbolTable();
            tokenTable = new TokenTable();
        }
        /// <summary>
        /// Function to set the first point in the buffer stream and rearrange it.
        /// </summary>
        /// <param name="BufferArray"></param>
        /// <param name="StartOrEnd"></param>
        /// <returns></returns>
        public Point3d StartPointSetting(int BufferArray, bool StartOrEnd)
        {
            #region SET_START_POINT
            BufferArray = BufferArray == -1 ? Stream.Count - 1 :
                (BufferArray < Stream.Count && BufferArray >= 0) ? BufferArray : 0;
            this.Stream.StartPt = (BufferArray, StartOrEnd);
            Point3d Pt = Point3d.Unset;

            if (!StartOrEnd)
            {
                this.Stream[BufferArray].Reverse();
                if (this.Stream[BufferArray].StartPoint.DistanceTo(this.Stream[BufferArray].Crv.PointAtEnd) > 0)
                {
                    var Vec = this.Stream[BufferArray].Crv.PointAtEnd - this.Stream[BufferArray].StartPoint;
                    this.Stream.Align(Vec);
                }
            }
            this.Stream.AlignBuffering();
            return this.Stream[0].StartPoint;
            #endregion
        }
        public List<PolylineCurve> Draw()
        {
            return this.Stream.Draw();
        }
        public bool Tokenisation()
        {
            #region SET_TOKEN
            //Create Reader
            if (!Stream.IsSetReader)
            {
                this.Readers = new List<PointsReader>();
                while (this.Stream.BufferListReader(out Point3d previousPt, out Point3d currentPt, out Point3d nextPt))
                {
                    Readers.Add(new PointsReader(previousPt, currentPt, nextPt));
                }

                if (!this.Stream.ConstructNodeGraph() || this.Readers.Count == 0)
                    return false;
            }
            List<Point3d> Nodes = this.Stream.NodesToBuffer.Keys.ToList();

            if (this.SymbolTable.Count > 0 || this.tokenTable.Count > 0)
            {
                this.SymbolTable.Clear();
                this.tokenTable.Clear();
            }

            Readers = Readers.Select(x =>
            {
                if (Nodes.Contains(x.Current))
                {
                    x.IsNode = true;
                    return x;
                }
                else
                {
                    return x;
                }

            }).ToList();

            Readers = Readers.Select(x =>
            {
                if (x.IsNode)
                {
                    var Index = Nodes.IndexOf(x.Current);
                    if (this.Stream.NodesToBuffer[x.Current].Count > 2)
                        x.IsBranch = true;
                    return x;
                }
                else
                { return x; }
            }).ToList();

            //Create Token
            foreach(var Reader in Readers)
            {
                if (Reader.IsNode && Reader.IsBranch)
                {
                    //Is Branch
                }
                else
                {
                    if (Reader.Backward == Point3d.Unset)
                    {
                        var StartAttr = new StartAttribute(this.SymbolTable, Reader.Current, Reader.Forward);
                        CrvToken Tk = new CrvToken(Token_Name.eS, StartAttr.ID);
                        this.SymbolTable.Push_Back(StartAttr);
                        this.tokenTable.Add(Tk);
                    }
                    else if (Reader.Forward == Point3d.Unset)
                    {
                        var EndAttr = new EndAttribute(this.SymbolTable, Reader.Backward, Reader.Current);
                        CrvToken Tk = new CrvToken(Token_Name.Se, EndAttr.ID);
                        this.SymbolTable.Push_Back(EndAttr);
                        this.tokenTable.Add(Tk);
                    }
                    else
                    {
                        var Attr = new ShapeAttribute2d(this.SymbolTable, Reader.Backward, Reader.Current, Reader.Forward);
                        CrvToken Tk = CrvToken.Unset;
                        switch (Util.Util.Match(Reader))
                        {
                            case "S":
                                Tk = new CrvToken(Token_Name.S, Attr.ID);
                                break;
                            case "R":
                                Tk = new CrvToken(Token_Name.R, Attr.ID);
                                break;
                            case "L":
                                Tk = new CrvToken(Token_Name.L, Attr.ID);
                                break;
                        }

                        this.SymbolTable.Push_Back(Attr);
                        this.tokenTable.Add(Tk);

                    }
                }
            }
            return true;
            #endregion
        }

    }
}
