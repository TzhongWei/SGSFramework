using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Input.Custom;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using SGSFramework.Core.BlockSetting.BlockBase;
using Rhino.DocObjects;
namespace SGSFramework.Core.Util
{
    public static class GeneralSetting
    {
        private static bool _SetAxis = false;
        private static int instanceObjId = -1;
        public static int SegUnit
        {
            get
            {
                return _SetUnit;
            }
            set
            {

                if (value < 1)
                    _SetUnit = 1;
                else
                    _SetUnit = value;
            }
        }
        private static int _SetUnit = 10;
        public static List<(GeometryBase, Color)> DisplayBlock(Block block)
        {
            var ComponentGeoms = block.Components;
            var AttrColor = block.blockAttribute._attributes.Select(x => x.ObjectColor).ToList();
            var GeomColor = new List<(GeometryBase, Color)>();
            for (int i = 0; i < ComponentGeoms.Count; i++)
                GeomColor.Add((ComponentGeoms[i], AttrColor[i]));
            return GeomColor;
        }
        public static void DisplayAxis(IGH_PreviewArgs args, double Length = 1)
        {
            Length = Length >= 1 ? Length : 1;
            var Doc = RhinoDoc.ActiveDoc;
            if (!_SetAxis || instanceObjId == -1)
            {
                SetAxis();
            }
            var RefInsObj = Doc.InstanceDefinitions.Find("ShapeProgramAxis");
            
            var InsObj = Doc.Objects.FindByObjectType(ObjectType.InstanceReference);
            var InstanceObjectList = InsObj.Select(x => x.Geometry).ToList();
            var InstanceObjectAtt = InsObj.Select(x => x.Attributes).ToList();
            for (int i = 0; i < InstanceObjectList.Count; i++)
            {
                var ObjectTS = (InstanceObjectList[i] as Rhino.Geometry.InstanceReferenceGeometry).Xform;
                for (int j = 0; j < RefInsObj.GetObjects().Count(); j++)
                {
                    var Geoms = RefInsObj.GetObjects().Select(x=> x.Geometry).ToList();
                    var Geom = RefInsObj.GetObjects()[j].Geometry;
                    var Att = RefInsObj.GetObjects()[j].Attributes;
                    Geom.Transform(ObjectTS);
                    if (j == 0)
                        args.Display.DrawPoint((Geom as Rhino.Geometry.Point).Location, Att.ObjectColor);
                    else
                    {
                        var AxisLine = (Geom as Rhino.Geometry.LineCurve).Line;
                        AxisLine.Extend(0, Length - 1);
                        args.Display.DrawArrow(AxisLine, Att.ObjectColor);
                    }
                }
            }
        }
        public static void SetAxis()
        {
            var Doc = RhinoDoc.ActiveDoc;
            try
            {
                instanceObjId = Doc.InstanceDefinitions.Find("ShapeProgramAxis").Index;
                if (_SetAxis && instanceObjId != -1)
                {
                    Doc.Objects.AddInstanceObject(instanceObjId, Transform.Identity);
                    return;
                }
            }
            catch
            { }
            Layer RHLayer = null;
            if (Doc.Layers.FindName("ShapeProgramAxis") is null)
            {
                RHLayer = new Layer()
                {
                    Color = System.Drawing.Color.Beige,
                    Name = "ShapeProgramAxis",
                };
                Doc.Layers.Add(RHLayer);
            }
            
            RHLayer = Doc.Layers.FindName("ShapeProgramAxis");

            Point3d Ori = Point3d.Origin;
            Line XAxis = new Line(Ori, Ori + Vector3d.XAxis);
            Line YAxis = new Line(Ori, Ori + Vector3d.YAxis);
            Line ZAxis = new Line(Ori, Ori + Vector3d.ZAxis);
            var OriAtt = new ObjectAttributes();
            OriAtt.SetUserString("Origin", Point3d.Origin.ToString());
            OriAtt.ColorSource = ObjectColorSource.ColorFromObject;
            OriAtt.ObjectColor = System.Drawing.Color.White;
            var XAtt = new ObjectAttributes();
            XAtt.SetUserString("Axis", Vector3d.XAxis.ToString());
            XAtt.ColorSource = ObjectColorSource.ColorFromObject;
            XAtt.ObjectColor = System.Drawing.Color.Red;
            var YAtt = new ObjectAttributes();
            YAtt.SetUserString("Axis", Vector3d.YAxis.ToString());
            YAtt.ColorSource = ObjectColorSource.ColorFromObject;
            YAtt.ObjectColor = System.Drawing.Color.GreenYellow;
            var ZAtt = new ObjectAttributes();
            ZAtt.SetUserString("Axis", Vector3d.ZAxis.ToString());
            ZAtt.ColorSource = ObjectColorSource.ColorFromObject;
            ZAtt.ObjectColor = System.Drawing.Color.Blue;
            var GeoList = new List<GeometryBase>();
            GeoList.Add(new Rhino.Geometry.Point(Ori));
            GeoList.Add(new LineCurve(XAxis));
            GeoList.Add(new LineCurve(YAxis));
            GeoList.Add(new LineCurve(ZAxis));

            instanceObjId = Doc.InstanceDefinitions.Add("ShapeProgramAxis", 
                "This is a setting for axes of shape transformation for shape compiler", 
                Point3d.Origin,
                GeoList, 
                new List<ObjectAttributes>() { OriAtt, XAtt, YAtt, ZAtt });
            Doc.Objects.AddInstanceObject(instanceObjId, Transform.Identity);
        }
        public static Plane AxisToPlane(Guid AxisPlane)
        {
            var Doc = RhinoDoc.ActiveDoc;
            var RhinoObj = Doc.Objects.FindId(AxisPlane).Geometry as InstanceReferenceGeometry;
            var TS = RhinoObj.Xform;

            var DefBlock = Doc.InstanceDefinitions.FindId(RhinoObj.ParentIdefId);
            if (DefBlock.Name != "ShapeProgramAxis")
                throw new Exception("The given guid isn't an axis block, please check the Guid in AxisToPlane()");

            var DefObj = DefBlock.GetObjects().Select(x => x.Geometry).ToList();
            for (int i = 0; i < DefObj.Count; i++)
            {
                DefObj[i].Transform(TS);
            }
            var Ori = (DefObj[0] as Rhino.Geometry.Point).Location;
            var XAxis = (DefObj[1] as LineCurve).PointAtEnd - (DefObj[1] as LineCurve).PointAtStart;
            var YAxis = (DefObj[2] as LineCurve).PointAtEnd - (DefObj[2] as LineCurve).PointAtStart;
            return new Plane(Ori, XAxis, YAxis);
        }
        public static Guid GetInstanceObject()
        {
            var Doc = RhinoDoc.ActiveDoc;
            var rc = Guid.Empty;
            var go = new GetObject();
            go.SetCommandPrompt("Select a instance reference object");
            go.GeometryFilter = ObjectType.InstanceReference;
            go.Get();
            go.EnablePreSelect(false, true);
            go.DeselectAllBeforePostSelect = false;
            if (go.CommandResult() == Rhino.Commands.Result.Success)
            {
                var rh_obj = go.Object(0).Object();
                if (rh_obj != null)
                {
                    rh_obj.Select(true);
                    rc = rh_obj.Id;
                }
                else
                {
                    Rhino.RhinoApp.WriteLine("Selection failed");
                }
            }
            return rc;
        }
        public static List<Guid> GetInstanceObjects()
        {
            var Doc = RhinoDoc.ActiveDoc;
            var rcs = new List<Guid>();
            var go = new GetObject();
            go.SetCommandPrompt("Select instance reference objects");
            go.GeometryFilter = ObjectType.InstanceReference;
            go.GetMultiple(0, 50);
            go.EnablePreSelect(false, true);
            go.DeselectAllBeforePostSelect = false;
            if (go.CommandResult() == Rhino.Commands.Result.Success)
            {
                for (int i = 0; i < go.ObjectCount; i++)
                {
                    var rh_obj = go.Object(i).Object();
                    if (rh_obj != null)
                    {
                        rh_obj.Select(true);
                        rcs.Add(rh_obj.Id);
                    }
                    else
                    {
                        Rhino.RhinoApp.WriteLine("Selection failed");
                    }
                }
            }
            return rcs;
        }
        public static IEnumerable<string> PrintDictionary<Key, Value>(IDictionary<Key, Value> ADictionary) 
            where Value : IEnumerable<object>
        {
            foreach (var kvp in ADictionary)
            {
                string result = $"{kvp.Key} = {"{"}";
                IList<string> valueList = kvp.Value.Select(x => x.ToString()).ToList();
                yield return string.Concat(kvp.Key.ToString(), " = {", valueList.Count > 0 ? string.Join(" ", valueList) : "empty", "}");
            }
        }
    }
}
