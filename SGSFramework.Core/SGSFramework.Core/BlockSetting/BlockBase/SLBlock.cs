using Rhino.Geometry;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;

namespace SGSFramework.Core.BlockSetting.BlockBase
{
    public class SLBlock : Block
    {
        public Point3d OriginPoint;
        public override Plane ReferencePlane { get; protected set; }
        public override string BlockType { get; protected set; }
        private GeometryBase[] _slblock;
        public GeometryBase[] SL_Block 
        {
            get 
            {
                if (_slblock is null)
                {
                    this.SetShape();
                    return _slblock;
                }
                else
                    return _slblock;
            } 
            private set
            {
                _slblock = value;
            }
        }
        private SLBlock():base() 
        {

            this.OriginPoint = new Point3d(0, 0, 0);
            this.ReferencePlane = new Plane(this.OriginPoint + new Vector3d(SLBlock.size * 2, 0, 0), Vector3d.ZAxis);
            this.blockAttribute = null;
        }
        public SLBlock(BlockBaseOption Options, int size = 10, bool Flip = true) : this()
        {
            if (!BlockTable.HasNamed(Options.BlockName))
            {
                SLBlock._flip = Flip;
                SLBlock.size = size > 0 ? size : 10;
                this.SetUnit(SLBlock.size);
                this.blockAttribute = Options;
                SetShape();
                this.BlockType = this.BlockName;
            }
        }
        public SLBlock(int size = 10, bool Flip = true) : this(Color.Gray, size, Flip)
        { }
        public SLBlock(Color Colour, int size = 10, bool Flip = true) : base()
        {
            SLBlock._flip = Flip;
            SLBlock.size = size > 0 ? size : 10;
            this.SetUnit(SLBlock.size);
            this.OriginPoint = new Point3d(0, 0, 0);
            this.ReferencePlane = new Plane(this.OriginPoint + new Vector3d(SLBlock.size * 2, 0, 0), Vector3d.ZAxis);
            this.blockAttribute = null;
            this.BlockColours = Colour;
            SetShape();
            this.BlockType = this.BlockName;
        }
        public static SLBlock SetShape(GeometryBase[] SL_Block, int BlockSize, 
            BlockBaseOption blockBaseOption = null)
        {
            var NewSLBlock = new SLBlock();
            SLBlock.size = size > 0 ? size : 10;
            NewSLBlock.SetUnit(SLBlock.size);
            NewSLBlock._slblock = SL_Block;
            ObjectAttributes Att = new ObjectAttributes();
            if (blockBaseOption is null)
            {
                NewSLBlock.blockAttribute = new BlockBaseOption();
                Att.ColorSource = ObjectColorSource.ColorFromObject;
                Att.ObjectColor = Color.Gray;
                NewSLBlock.blockAttribute.BlockName = $"SLBlock({Count})";
                NewSLBlock.blockAttribute.LayerName = "SLBlock";
                NewSLBlock.blockAttribute.Description = "This is an SL block default setting";
                
            }
            else
            {
                if (blockBaseOption.BlockName == $"SLBlock({Count})")
                {
                    Count++;
                    blockBaseOption.BlockName = $"SLBlock({Count})";
                    NewSLBlock.blockAttribute = blockBaseOption;
                }
                else
                {
                    NewSLBlock.blockAttribute = blockBaseOption;
                    Count = 0;
                }
            }
            Count++;
            List<ObjectAttributes> Atts = new List<ObjectAttributes>();
            for (int i = 0; i < SL_Block.Length; i++)
                Atts.Add(Att);
            NewSLBlock.AddRangeComponent(SL_Block, Atts);
            NewSLBlock.BlockType = NewSLBlock.BlockName;
            return NewSLBlock;
        }
        public void SetBlockOption(BlockBaseOption Options)
        {
            this.blockAttribute = Options;
        }
        private static int size = 10;
        public static int Count { get; private set; } = 0;
        private static bool _flip = true;
        private Color BlockColours = Color.Black;
        protected override void SetShape()
        {
            if (!(this._slblock is null)) return;
            ObjectAttributes Att = new ObjectAttributes();
            if (blockAttribute == null)
            {
                this.blockAttribute = new BlockBaseOption();
                Att = new ObjectAttributes()
                {
                    ColorSource = ObjectColorSource.ColorFromObject,
                    ObjectColor = BlockColours
                };
                while (BlockTable.HasNamed($"SLBlock({Count})")) Count++;
                this.blockAttribute.BlockName = $"SLBlock({Count})";
                this.blockAttribute.LayerName = "SLBlock";
                this.blockAttribute.Description = "This is an SL block default setting";
            }
            else
                Att = this.blockAttribute._attributes[0];

            int Size = Util.GeneralSetting.SegUnit;
            List<Point3d> Contour_1 = new List<Point3d>(){OriginPoint,
        OriginPoint + new Vector3d(Size * 3, 0, 0),
        OriginPoint + new Vector3d(Size * 3, 0, Size),
        OriginPoint + new Vector3d(Size, 0, Size),
        OriginPoint + new Vector3d(Size, 0, Size * 2),
        OriginPoint + new Vector3d(0, 0, Size * 2),
        OriginPoint
            };
            PolylineCurve Curve_1 = new PolylineCurve(Contour_1);
            List<Point3d> Contour_2 = new List<Point3d>(){
        OriginPoint + new Vector3d(Size * 2, 0, 0),
        OriginPoint + new Vector3d(Size * 3, 0, 0),
        OriginPoint + new Vector3d(Size * 3, 0, Size),
        OriginPoint + new Vector3d(Size * 4, 0, Size),
        OriginPoint + new Vector3d(Size * 4, 0, Size * 3),
        OriginPoint + new Vector3d(Size * 3, 0, Size * 3),
        OriginPoint + new Vector3d(Size * 3, 0, Size * 2),
        OriginPoint + new Vector3d(Size * 2, 0, Size * 2),
        OriginPoint + new Vector3d(Size * 2, 0, 0)
        };
            PolylineCurve Curve_2 = new PolylineCurve(Contour_2);
            var Extrude_1 = Extrusion.Create(Curve_1, Size, true).ToBrep();
            var Extrude_2 = Extrusion.Create(Curve_2, -Size, true).ToBrep();
            Brep SL_Block = Brep.CreateBooleanUnion(new List<Brep>() { Extrude_1, Extrude_2 }, 0.1)[0];
            SL_Block.MergeCoplanarFaces(0.1, 0.1);
            if (_flip)
            {
                List<int> edgeInt = new List<int>();
                List<double> Radius = new List<double>();
                for (int i = 0; i < SL_Block.Edges.Count; i++)
                {
                    edgeInt.Add(i);
                    Radius.Add(Size * 0.1);
                }
                SL_Block = Brep.CreateFilletEdges(SL_Block, edgeInt, Radius, Radius, BlendType.Fillet, RailType.DistanceFromEdge, 0.1)[0];
                Curve[] OuterCurve = SL_Block.DuplicateNakedEdgeCurves(true, false);
                List<Brep> JoinBreps = new List<Brep>();
                foreach (Curve cur in Curve.JoinCurves(SL_Block.DuplicateNakedEdgeCurves(true, false), 0.1))
                {
                    var Segs = cur.DuplicateSegments();
                    var Cap_Srf = Brep.CreateEdgeSurface(Segs);
                    JoinBreps.Add(Cap_Srf);
                }
                JoinBreps.Add(SL_Block);
                SL_Block = Brep.JoinBreps(JoinBreps, 0.1)[0];
            }

            Point3d TempPt = this.OriginPoint + new Vector3d(Size * 2, 0, 0);
            _slblock = new Brep[2];
            SL_Block.Rotate(Math.PI, Vector3d.ZAxis, TempPt);
            this.AddComponent(_slblock[0] = SL_Block.DuplicateBrep(), Att);
            TempPt = this.OriginPoint + new Vector3d(Size * 2, 0, Size * 2);
            SL_Block.Rotate(Math.PI, Vector3d.YAxis, TempPt);
            this.AddComponent(_slblock[1] = SL_Block.DuplicateBrep(), Att);
        }
        public override string ToString()
        {
            return this.BlockName;
        }
    }
}
