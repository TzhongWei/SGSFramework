using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino;
using SGSFramework.Core.BlockSetting;
using Grasshopper.Kernel.Types;
using System.Runtime.CompilerServices;
using GH_IO.Serialization;
using Rhino.Geometry;
using GH_IO.Types;
using Rhino.DocObjects;

namespace SGSFramework.Core.BlockSetting
{
    /// <summary>
    /// The Options setting for block which deal with shared and optional settings
    /// </summary>
    public class BlockBaseOption
    {
        /// <summary>
        /// To notify if the colour is reset
        /// </summary>
        public bool IsResetColour { get; private set; } = false;
        /// <summary>
        /// This is a ID name to save in the block userString in objectAttribute.
        /// </summary>
        private string _SaveNameURL = "SGSFrameworkID";
        /// <summary>
        /// The URL for this SGS framework
        /// </summary>
        public string SaveNameURL { get { return _SaveNameURL; } set { _SaveNameURL = value; } }
        /// <summary>
        /// Custom data setting for a block
        /// </summary>
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();
        /// <summary>
        /// The attributes for geometries in a block
        /// </summary>
        public List<ObjectAttributes> _attributes;
        /// <summary>
        /// Return an attribute from the given index
        /// </summary>
        /// <param name="index">integer value represent the geomtry</param>
        /// <returns></returns>
        public ObjectAttributes this[int index]
        {
            get
            {
                if (index > 0 && index < this._attributes.Count)
                    return this._attributes[index];
                else
                    throw new IndexOutOfRangeException();
            }
        }
        /// <summary>
        /// Test if the class is called
        /// </summary>
        public bool IsActive { get; } = false;
        /// <summary>
        /// Name of the block component
        /// </summary>
        private string _BlockName = Util.Util.SetGibberish();
        /// <summary>
        /// The layer name for the component
        /// </summary>
        private string _LayerName = Util.Util.SetGibberish();
        /// <summary>
        /// Description of block
        /// </summary>
        public string Description = " ";
        /// <summary>
        /// constructor
        /// </summary>
        public BlockBaseOption()
        {
            IsActive = true;
            this._attributes = new List<ObjectAttributes>();
        }
        private RhinoDoc Doc = RhinoDoc.ActiveDoc;
        /// <summary>
        /// Set a Block Name
        /// </summary>
        public string BlockName
        {
            get
            {
                return this._BlockName;
            }
            set
            {

                if (value != " " || value != string.Empty)
                    this._BlockName = value;
                else
                    this._BlockName = "SGSFrameBlock_" + Util.Util.SetGibberish();
            }
        }
        /// <summary>
        /// Set block's layer
        /// </summary>
        public string LayerName
        {
            get
            {
                return this._LayerName;
            }
            set
            {
                var Doc = RhinoDoc.ActiveDoc;
                if (value != " " || value != string.Empty)
                    this._LayerName = value;
                else
                    this._LayerName = "SGSFrameworkLayer_" + Util.Util.SetGibberish();
            }
        }
        public void SetAttrubuteLayer(int LayerID)
        {
            for (int i = 0; i < this._attributes.Count; i++)
            {
                this._attributes[i].LayerIndex = LayerID;
            }
        }
        private List<Color> _Colours = new List<Color>();
        public List<Color> Colours { get {
                if (this._attributes == null || !this.IsResetColour)
                    return new List<Color> { Color.Black };
                if (_Colours.Count < this._attributes.Count)
                {
                    for (int i = _Colours.Count - 1; i < this._attributes.Count; i++)
                        _Colours.Add(_Colours.Last());
                    return _Colours;
                }
                else
                    return _Colours;
            } }
        public void SetColours(List<Color> Colours)
        {
            this.IsResetColour = true;
            this._Colours = Colours;
        }
        /// <summary>
        /// Only used in Block.cs
        /// </summary>
        internal void FinaliseColour()
        {
            for (int i = 0; i < this._attributes.Count; i++)
            {
                this._attributes[i].ColorSource = ObjectColorSource.ColorFromObject;
                this._attributes[i].ObjectColor = this.Colours[i];
            }
        }
    }
}
