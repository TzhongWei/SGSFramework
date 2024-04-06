using System;
using System.Collections.Generic;
using System.Drawing;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Runtime;
using Grasshopper.Kernel.Types;
using System.Runtime.CompilerServices;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

//Unfinished list
//Load from Rhino block need to test if can read information from GeometryBase

namespace SGSFramework.Core.BlockSetting.BlockBase
{
    /// <summary>
    /// The geometry namespace contains fundamental types that define commonly-used value types and classes.
    /// </summary>
    internal class NamespaceDoc { }
    /// <summary>
    /// The Block here only deal with Geometry settings
    /// </summary>
    public abstract class Block : IEquatable<Block>
    {
        /// <summary>
        /// Internalise a block from outsource
        /// </summary>
        /// <param name="InstanceFromRhino"></param>
        internal Block(InstanceDefinition InstanceFromRhino)
        {

            //ReadFromRhino(InstanceFromRhino);
        }
        //abstract bool ReadFromRhino(InstanceDefinition RhinoBlock);
        /// <summary>
        /// Constructor, it can only be set up with the inherited classes
        /// </summary>
        protected Block()
        {
            this.blockAttribute = new BlockBaseOption();
        }
        /// <summary>
        /// The shared and optional setting manager, namely Name, layer Name, description
        /// </summary>
        public BlockBaseOption blockAttribute { get; protected set; }
        /// <summary>
        /// Return the block name from block attribute
        /// </summary>
        public virtual string BlockName => blockAttribute.BlockName;
        /// <summary>
        /// The Components store the geometry form for Geometry
        /// </summary>
        public List<GeometryBase> Components { get; private set; } = new List<GeometryBase>();
        /// <summary>
        /// The block id of the geometry, unique with in Rhino, if it's defined please remove the block
        /// </summary>
        public int Block_Id { get; protected set; } = -1;
        /// <summary>
        /// Provide a geometry from the index
        /// </summary>
        /// <param name="index">integer value represent the geomtry</param>
        /// <returns></returns>
        public GeometryBase this[int index]
        {
            get
            {
                if (index > 0 && index < Components.Count)
                    return Components[index];
                else
                    throw new IndexOutOfRangeException();
            }
        }
        /// <summary>
        /// A reference plane for the block geometry.
        /// </summary>
        public abstract Plane ReferencePlane { get; protected set; }
        /// <summary>
        /// Return the name of block
        /// </summary>
        public abstract string BlockType { get; protected set; }
        /// <summary>
        /// Add single geometry and it's objectattrubutes into this block
        /// </summary>
        /// <param name="Shape">the shape into components</param> 
        /// <param name="Att">the attrubutes into this block corresponding to the component</param> 
        protected virtual void AddComponent(GeometryBase Shape, ObjectAttributes Att)
        {
            this.Components.Add(Shape);
            this.blockAttribute._attributes.Add(Att);
        }
        /// <summary>
        /// Add a set of geometry and it's objectattrubutes into this block
        /// </summary>
        /// <param name="Shapes">the shape into components</param> 
        /// <param name="Atts">the attrubutes into this block corresponding to the component</param> 
        protected virtual void AddRangeComponent(IEnumerable<GeometryBase> Shapes, IEnumerable<ObjectAttributes> Atts)
        {
            for(int i = 0; i < Shapes.ToList().Count; i++)
                this.AddComponent(Shapes.ToList()[i], Atts.ToList()[i]);
        }
        /// <summary>
        /// Add a set of geometries with the same objectattributes into this block
        /// </summary>
        /// <param name="Shapes"></param>
        /// <param name="SameAtt"></param>
        public virtual void AddRangeComponent(IEnumerable<GeometryBase> Shapes, ObjectAttributes SameAtt)
        {
            for (int i = 0; i < Shapes.ToList().Count; i++)
                this.AddComponent(Shapes.ToList()[i], SameAtt);
        }
        /// <summary>
        /// Remove component from this block
        /// </summary>
        /// <param name="index"></param>
        protected virtual void RemoveComponentAt(int index)
        {
            try
            {
                if (!(this[index] is null))
                {
                    this.Components.RemoveAt(index);
                    this.blockAttribute._attributes.RemoveAt(index);
                }
            }
            catch
            {
                throw new Exception("The Component index is out of the range or the geometry from index is null");
            }
        }
        /// <summary>
        /// Print all information and geometries of this block
        /// </summary>
        /// <returns></returns>
        public virtual List<object> GetDisplay()
        {
            List<object> Display = new List<object>() { blockAttribute.BlockName, ReferencePlane };
            Display.AddRange(Components);
            return Display;
        }
        /// <summary>
        /// To string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.BlockType;
        }
        /// <summary>
        /// Set the components and object attrubutes into a block
        /// </summary>
        /// <returns></returns>
        public virtual bool SetBlock()
        {
            if (this.Components.Count == 0 || BlockTable.HasNamed(this.BlockName))
                return false;
            var Doc = RhinoDoc.ActiveDoc;
            string Name = blockAttribute.BlockName;
            if (!(Doc.InstanceDefinitions.Find(Name) is null))
                return false;

            int LayerID = 0;

            //Create Layer
            if (Doc.Layers.FindName(blockAttribute.LayerName) is null)
            {
                LayerID = Doc.Layers.Add(blockAttribute.LayerName, Color.Black);
            }
            else
                LayerID = Doc.Layers.FindName(blockAttribute.LayerName).Index;


            string Description = blockAttribute.Description == " " ? blockAttribute.Description : BlockType;

            if (blockAttribute._attributes.Count != this.Components.Count)
                throw new Exception("The block attrubute count doesn't match with component count");

            this.blockAttribute.SetAttrubuteLayer(LayerID);

            if (this.blockAttribute.IsResetColour)
            {
                this.blockAttribute.FinaliseColour();
            }

            this.Block_Id = Doc.InstanceDefinitions.Add(Name, Description,this.blockAttribute.SaveNameURL, 
                "the url for SGSFramework", ReferencePlane.Origin, Components, blockAttribute._attributes);

            //Final Check
            if (this.Block_Id > -1)
            {
                BlockTable.Add(this);
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// Build up the shape
        /// </summary>
        protected abstract void SetShape();
        /// <summary>
        /// Display Geometry output with colours
        /// </summary>
        /// <param name="Geoms"></param>
        /// <param name="Colours"></param>
        public virtual void DisplayGeometries(out List<GeometryBase> Geoms, out List<Color> Colours)
        {
            Geoms = new List<GeometryBase>();
            Colours = new List<Color>();
            var GeomColour = Util.GeneralSetting.DisplayBlock(this);
            Geoms = GeomColour.Select(x => x.Item1).ToList();
            Colours = GeomColour.Select(x => x.Item2).ToList();
        }
        public void SetUnit(int Unit)
        {
            Util.GeneralSetting.SegUnit = Unit;
        }
        public bool Equals(Block other)
        {
            if (this.Block_Id == -1 || other.Block_Id == -1)
                return false;

            return this.blockAttribute == other.blockAttribute &
                this.Components.Equals(other.Components) &
                this.ReferencePlane == other.ReferencePlane &
                this.BlockType == other.BlockType; 
        }
    }
}
