﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Rhino.Geometry;
using Rhino;
using SGSFramework.Core.BlockSetting.BlockBase;

namespace SGSFramework.Core.BlockSetting
{
    /// <summary>
    /// The table store all blocks
    /// </summary>
    public static class BlockTable
    {
        /// <summary>
        /// All Blocks
        /// </summary>
        private static HashSet<Block> Blocks = new HashSet<Block>();
        /// <summary>
        /// Find the block with block reference ID in Rhino, it's not the index in this blocktable,
        /// if seeking for the corrosponding sequence please use Find(BlockName);
        /// </summary>
        /// <param name="ReferenceIndex"></param>
        /// <returns></returns>
        public static Block IndexAt(int ReferenceIndex)
        {
            if (Blocks.Count == 0)
                return null;
            if (ReferenceIndex == -1)
                throw new Exception("The Index is invalid");
            for (int i = 0; i < Blocks.Count; i++)
            {
                if (Blocks.ToList()[i].Block_Id == ReferenceIndex)
                    return Blocks.ToList()[i];
            }
            throw new Exception("The ReferenceIndex is invalid");
        }
        /// <summary>
        /// The number blocks in block table
        /// </summary>
        public static int Count => Blocks.Count;
        /// <summary>
        /// To test if the name is applied
        /// </summary>
        /// <param name="ANewName"></param>
        /// <returns></returns>
        public static bool HasNamed(string ANewName)
        {
            if (Blocks.Count == 0) return false;
            foreach (Block Block in Blocks)
            {
                if (Block.blockAttribute.BlockName == ANewName)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Add the defined block into table, the blocks here are all set into rhino instance definition
        /// </summary>
        /// <param name="block">A block</param>
        public static void Add(Block block)
        {
            if (block.Block_Id != -1 && !Contains(block))
                Blocks.Add(block);
            else
                throw new Exception("The block is defined");
        }
        /// <summary>
        /// Test if the block contains in this table.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static bool Contains(Block block)
        {
            if (Find(block.BlockName) == -1)
                return false;
            else
                return true;
        }
        /// <summary>
        /// Find the block reference ID in Rhino
        /// </summary>
        /// <param name="BlockName">The Name of the block</param>
        /// <returns></returns>
        public static int FindBlockID(string BlockName)
        {
            if (Blocks.Count == 0)
                return -1;
            for (int i = 0; i < Blocks.Count; i++)
            {
                if (Blocks.ToList()[i].BlockName == BlockName)
                    return Blocks.ToList()[i].Block_Id;
            }
            return -1;
        }
        /// <summary>
        /// Find the index of block in this table
        /// </summary>
        /// <param name="BlockName">The Name of the block</param>
        /// <returns>Return the block's index in the sequence</returns>
        public static int Find(string BlockName)
        {
            if (Blocks.Count == 0)
                return -1;
            for (int i = 0; i < Blocks.Count; i++)
            {
                if (Blocks.ToList()[i].BlockName == BlockName)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// GetAllBlocks
        /// </summary>
        public static List<Block> AllBlocks => Blocks.ToList();
        /// <summary>
        /// Get Block by name
        /// </summary>
        /// <param name="BlockName"></param>
        /// <returns></returns>
        public static Block FindByName(string BlockName)
        {
            if (Blocks.Count == 0)
                return null;
            for (int i = 0; i < Blocks.Count; i++)
            {
                if (Blocks.ToList()[i].BlockName == BlockName)
                    return Blocks.ToList()[i];
            }
            return null;
        }
        /// <summary>
        /// Find a block with block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static Block Find(Block block)
        => FindByName(block.BlockName);
        /// <summary>
        /// Remove block
        /// </summary>
        /// <param name="block"></param>
        public static void Remove(Block block)
        {
            if (Blocks.Count == 0)
                throw new Exception("No blocks are set up");
            Block Ablock;
            if ((Ablock = Find(block)) == null) return;
            else
            {
                var Doc = RhinoDoc.ActiveDoc;
                Doc.InstanceDefinitions.Delete(Ablock.Block_Id, true, true);
                Blocks.Remove(Ablock);
            }
        }
        /// <summary>
        /// Remove block from the block name
        /// </summary>
        /// <param name="BlockName"></param>
        public static void Remove(string BlockName)
        {
            if (HasNamed(BlockName))
            {
                Blocks.Remove(FindByName(BlockName));
            }
            var Doc = RhinoDoc.ActiveDoc;
            int ID;
            if ((ID = Doc.InstanceDefinitions.Find(BlockName).Index) != -1)
            {
                Doc.InstanceDefinitions.Delete(ID, true, false);
            }

        }
        /// <summary>
        /// Remove All the block instance in Rhino
        /// </summary>
        public static void Clear()
        {
            var Doc = RhinoDoc.ActiveDoc;
            foreach (var Block in Blocks)
            {
                var InstanceObj = Doc.InstanceDefinitions.Find(Block.BlockName);
                if (!(InstanceObj is null))
                {
                    Doc.InstanceDefinitions.Delete(InstanceObj);
                }
            }
            Blocks.Clear();
        }
        /// <summary>
        /// Load blocks from Rhino with 
        /// </summary>
        /// <param name="SaveNameID"></param>
        /// <returns></returns>
        public static bool LoadBlock(string SaveNameID = "SGSFramework") 
        {
            var Doc = RhinoDoc.ActiveDoc;
            foreach (var block in Doc.InstanceDefinitions)
            {
                  
            }
             
            return BlockTable.AllBlocks.Count == 0;
        }
        public static void DisplayGeometries(string blockName, out List<GeometryBase> Geom, out List<Color> colors)
        {
            if ((Find(blockName) != -1 ))
            {
                var block = FindByName(blockName);
                block.DisplayGeometries(out Geom, out colors);
            }
            else
            {
                Geom = new List<GeometryBase>();
                colors = new List<Color>();
            }
        }
    }
}
