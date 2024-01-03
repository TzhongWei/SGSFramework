using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Rhino.DocObjects;

namespace SGSFramework.Core.BlockSetting.BlockBase
{
    /// <summary>
    /// This class provides a block for testing
    /// </summary>
    public class VoxelBlock : Block
    {
        public Point3d CentrePt => new Point3d(VoxelSize * 0.5, VoxelSize * 0.5, VoxelSize * 0.5);
        /// <summary>
        /// To test if this block is set up
        /// </summary>
        private static bool IsSet = false;
        public override Plane ReferencePlane { get { return Plane.WorldXY; } 
            protected set 
            {
                ReferencePlane = Plane.WorldXY;
            } 
        }
        public override string BlockType { get { return "TestVoxel"; }
            protected set {
                BlockType = "TestVoxel";
            } 
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public VoxelBlock() : base()
        {
            if (!BlockTable.HasNamed("TestVoxel"))
                IsSet = false;
            if (!IsSet)
            {
                SetShape();
            }
            else if (VoxelSize != (double)Util.GeneralSetting.SegUnit)
            {
                BlockTable.Remove(this);
                SetShape();
            }

        }
        /// <summary>
        /// The size of block for testing if to rebuild the block
        /// </summary>
        private static double VoxelSize = 0;
        protected override void SetShape()
        {
            var Box = new Box(this.ReferencePlane,
                new Interval(0, (double)Util.GeneralSetting.SegUnit),
                new Interval(0, (double)Util.GeneralSetting.SegUnit),
                new Interval(0, (double)Util.GeneralSetting.SegUnit)).ToBrep();
            var Boxfaces = Box.Faces.ToList();
            var Atts = new List<ObjectAttributes>();
            for (int i = 0; i < 6; i++)
                Atts.Add(new ObjectAttributes() { ColorSource = ObjectColorSource.ColorFromObject });
            Atts[0].ObjectColor = Color.Red;
            Atts[1].ObjectColor = Color.Blue;
            Atts[2].ObjectColor = Color.GreenYellow;
            Atts[3].ObjectColor = Color.White;
            Atts[4].ObjectColor = Color.DarkGreen;
            Atts[5].ObjectColor = Color.Black;
            this.blockAttribute.BlockName = "TestBlock";
            this.blockAttribute.LayerName = "TestLayer";
            this.blockAttribute.Description = "This is a voxel box defined based on the unit";
            VoxelSize = (double)Util.GeneralSetting.SegUnit;
            IsSet = true;
            this.AddRangeComponent(Boxfaces, Atts);
        }
        public override void DisplayGeometries(out List<GeometryBase> Geom, out List<Color> colors)
        {
            if (this.Components.Count != 0)
            {
                base.DisplayGeometries(out Geom, out colors);
            }
            else
            {
                BlockTable.DisplayGeometries("TestBlock", out Geom, out colors);
            }
        }
    }
}
