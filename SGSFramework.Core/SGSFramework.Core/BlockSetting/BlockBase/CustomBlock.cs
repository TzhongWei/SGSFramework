using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.DocObjects;

namespace SGSFramework.Core.BlockSetting.BlockBase
{
    public class CustomBlock : Block
    {
        public override string ToString()
        {
            return this.BlockName;
        }
        public new string BlockName { get 
            {
                return this.blockAttribute.BlockName;
            }
            set
            {
                this.blockAttribute.BlockName = value;
            }
        }
        public override Plane ReferencePlane { get; protected set; }
        public override string BlockType { get { return "CustomBlock"; }
            protected set { BlockType = "CustomBlock"; } }
        private bool IsSet = false;  
        public CustomBlock() : base()
        {
            if (BlockTable.HasNamed($"CustomBlock") || BlockTable.HasNamed(BlockName))
                this.IsSet = true;
        }
        public void SetShape(List<Guid> RhinoGeometry, Guid Axis)
        {
            if (this.IsSet) return;
            if (RhinoGeometry.Count == 0 || RhinoGeometry == null || Axis == Guid.Empty) return;

            var Doc = RhinoDoc.ActiveDoc;
            var Obj = Doc.Objects.FindId(Axis).Geometry as InstanceReferenceGeometry;
            var ReferObj = Doc.InstanceDefinitions.FindId(Obj.ParentIdefId);
            if (ReferObj.Name != "ShapeProgramAxis") return;
            this.ReferencePlane = Util.GeneralSetting.AxisToPlane(Axis);

            this.blockAttribute.BlockName = "CustomBlock";
            this.blockAttribute.LayerName = "CustomBlockLayer";
            this.blockAttribute.Description = "This is a custom Block setting";

            var RhinoObjs = RhinoGeometry.Select(x => Doc.Objects.FindId(x)).ToList();
            var Geoms = RhinoObjs.Select(x => x.Geometry).ToList();
            var Atts = RhinoObjs.Select(x => x.Attributes).ToList();
            this.AddRangeComponent(Geoms, Atts);
            IsSet = true;
        }
        public void SetShape(List<(GeometryBase, ObjectAttributes)> RhinoObj, Guid Axis)
        {
            if (IsSet) return;
            if (RhinoObj.Count == 0 || RhinoObj == null || Axis == Guid.Empty) return;
            var Doc = RhinoDoc.ActiveDoc;
            var Obj = Doc.Objects.FindId(Axis).Geometry as InstanceReferenceGeometry;
            var ReferObj = Doc.InstanceDefinitions.FindId(Obj.ParentIdefId);
            if (ReferObj.Name != "ShapeProgramAxis") return;
            this.ReferencePlane = Util.GeneralSetting.AxisToPlane(Axis);

            this.blockAttribute.BlockName = "CustomBlock";
            this.blockAttribute.LayerName = "CustomBlockLayer";
            this.blockAttribute.Description = "This is a custom Block setting";

            var Geoms = RhinoObj.Select(x => x.Item1).ToList();
            var Atts = RhinoObj.Select(x => x.Item2).ToList();
            this.AddRangeComponent(Geoms, Atts);
            this.IsSet = true;
        }
        protected override void SetShape()
        {
            throw new NotImplementedException();
        }
    }
}
