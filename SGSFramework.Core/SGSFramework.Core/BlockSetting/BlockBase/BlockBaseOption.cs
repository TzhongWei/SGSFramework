using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// Customdata setting for a block
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
                    this._BlockName = Util.Util.SetGibberish();
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
                    this._LayerName = Util.Util.SetGibberish();
            }
        }
        public void SetAttrubuteLayer(int LayerID)
        {
            for (int i = 0; i < this._attributes.Count; i++)
            {
                this._attributes[i].LayerIndex = LayerID;
            }
        }
    }
}
