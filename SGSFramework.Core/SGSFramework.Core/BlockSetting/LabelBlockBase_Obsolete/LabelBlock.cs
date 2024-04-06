using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;

namespace SGSFramework.Core.BlockSetting.LabelBlockBase
{
    /// <summary>
    /// This class is to manage the block with their corrosponding transformations and labels
    /// </summary>
    internal class NamespaceDoc { }
    /// <summary>
    /// The class need to be defined with labels and transformation actions.
    /// </summary>
    [Obsolete("This class and it's derived classes are obsolete, please refer to the namespace LabelBlockBase_2", false)]
    public abstract class LabelBlock : IEquatable<LabelBlock>, ICloneable
    {
        /// <summary>
        /// Constructor, it can only be set up with inherited classes
        /// </summary>
        protected LabelBlock()
        {
            Vocabulary = new Dictionary<string, Transform>();
            CompatibleName = new Dictionary<string, string>();
        }
        /// <summary>
        /// Voca, Transform =>
        /// The dictionary stores all the transformation action for the block
        /// The principle of naming vocabulary is a pair of string label and transformation matrix
        /// </summary>
        public Dictionary<string, Transform> Vocabulary { get; protected set; } = new Dictionary<string, Transform>();
        /// <summary>
        /// BlockName_I, BlockName =>
        /// The dictionary store all the match blockname from each label,
        /// Typically, "I" represents placing block into the aggregations with production rules, 
        /// if only one block is set.
        /// Therefore, if we only have one block, each keyvaluepair would be like 
        /// [I, BLOCKNAME], one pair only
        /// But if we have a lot of block, the keyvaluepair suggest to be set like
        /// [BLOCK_1_I, BLOCKNAME_1], and [BLOCK_2_I, BLOCKNAME_2]
        /// Or you can use 
        /// #defined I1 BLOCK_1_I
        /// to relink the BLOCKNAME_1 in grasshopper panel
        /// </summary>
        protected abstract Dictionary<string, string> CompatibleName { get; set; }
        /// <summary>
        /// Every derived class need to set up their vocabulary
        /// </summary>
        public abstract void SetUpVocabulary();
        /// <summary>
        /// Get all compacting name into a list
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetCompatibleName()
        {
            List<string> outName = new List<string>();
            foreach (KeyValuePair<string, string> kvp in CompatibleName)
            {
                outName.Add(kvp.Key + ", " + kvp.Value);
            }
            return outName;
        }
        public abstract string LabelBlockName { get; protected set; }
        /// <summary>
        /// Get the compacting name of this block
        /// </summary>
        /// <param name="label">A terminal label</param>
        /// <param name="Silent">To run out error messge</param>
        /// <returns></returns>
        public virtual string GetCompatibleName(string label, bool Silent = true)
        {
            if (CompatibleName.ContainsKey(label))
                return CompatibleName[label];
            else
            {
                if (!Silent)
                    throw new Exception("There is not such compacting name");
                else
                    return "";
            }
        }
        /// <summary>
        /// A method to get the transformation matrix from a given label
        /// </summary>
        /// <param name="label">the label for the vocabulary</param>
        /// <returns></returns>
        public Transform this[string label]
        {
            get
            {
                if (this.Vocabulary.ContainsKey(label))
                    return this.Vocabulary[label];
                else
                    throw new Exception($"there is no {label} in the setting");
            }
        }
        /// <summary>
        /// this properity is only for retrieving vocabulary labels
        /// </summary>
        /// <returns></returns>
        public List<string> VocaLabels => Vocabulary.Keys.ToList();
        /// <summary>
        /// this properity is only for retrieving CompactingName labels
        /// </summary>
        public List<string> ComLabels => CompatibleName.Keys.ToList();
        /// <summary>
        /// Replace a vocabulary label name with oldlabel
        /// </summary>
        /// <param name="OldLabel"></param>
        /// <param name="NewLabel"></param>
        /// <returns></returns>
        public bool ResetVocabulary(string OldLabel, string NewLabel)
        {
        
            var AllTransform = this.Vocabulary.Values.ToList();
            var AllLabel = this.Vocabulary.Keys.ToList();
            int IndexOfOld = AllLabel.IndexOf(OldLabel);
            if(IndexOfOld == -1) return false;
            AllLabel.RemoveAt(IndexOfOld);
            AllLabel.Insert(IndexOfOld, NewLabel);
            this.Vocabulary = new Dictionary<string, Transform>();
            for (int i = 0; i < AllLabel.Count; i++)
            {
                this.Vocabulary.Add(AllLabel[i], AllTransform[i]);
            }

            return true;
        }
        /// <summary>
        /// Replace a vocabulary label name with index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="NewLabel"></param>
        /// <returns></returns>
        public bool ResetVocabulary(int index, string NewLabel)
        {
            if (index < 0 || index > this.Vocabulary.Count)
                return false;
            var AllTransform = this.Vocabulary.Values.ToList();
            var AllLabel = this.Vocabulary.Keys.ToList();
            AllLabel.RemoveAt(index);
            AllLabel.Insert(index, NewLabel);
            this.Vocabulary = new Dictionary<string, Transform>();
            for (int i = 0; i < AllLabel.Count; i++)
            {
                this.Vocabulary.Add(AllLabel[i], AllTransform[i]);
            }
            return true;
        }
        /// <summary>
        /// Reset all the vocabulary label name;
        /// </summary>
        /// <param name="Labels"></param>
        /// <returns></returns>
        public bool ResetVocabulary(List<string> Labels)
        {
            List<Transform> AllTransforms = Vocabulary.Values.ToList();
            if (AllTransforms.Count != Labels.Count) return false;
            this.Vocabulary = new Dictionary<string, Transform>();
            for (int i = 0; i < Labels.Count; i++)
            {
                this.Vocabulary.Add(Labels[i], AllTransforms[i]);
            }
            return true;
        }
        /// <summary>
        /// This function is to check if the vocabulary of two blocks are the same
        /// if true, there is at least one label the same
        /// else, the labels are different
        /// </summary>
        /// <param name="block1">first Labelblock </param> 
        /// <param name="block2">second Labelblock </param>
        /// <param name="DuplicatedLabels"> The duplicated indices of label, first and second labelblock</param>
        /// <returns></returns>
        public static bool IsVocabularySame(LabelBlock block1, LabelBlock block2, out List<(int, int)> DuplicatedLabels)
        {
            List<string> Label_1 = block1.Vocabulary.Keys.ToList();
            List<string> Label_2 = block2.Vocabulary.Keys.ToList();
            DuplicatedLabels = new List<(int, int)>();
            bool overlap = false;
            for (int i = 0; i < Label_1.Count; i++)
            {
                for (int j = 0; j < Label_2.Count; j++)
                {
                    if (Label_1[i] == Label_2[j])
                    {
                        DuplicatedLabels.Add((i, j));
                        overlap = true;
                    }
                }
            }
            return overlap;
        }
        /// <summary>
        /// This function is to check if the vocabulary of two blocks are the same
        /// if true, there is at least one label the same
        /// else, the labels are different
        /// </summary>
        /// <param name="other">label block to test </param>
        /// <param name="DuplicatedLabels">The duplicated indices of label, this and other labelblock</param>
        /// <returns></returns>
        public bool IsVocabularySame(LabelBlock other, out List<(int, int)> DuplicatedLabels)
        {
            return IsVocabularySame(this, other, out DuplicatedLabels);
        }
        /// <summary>
        /// To check if the vocabulary and compactingName are repeated.
        /// </summary>
        /// <param name="labelblock">A labelblock </param>
        /// <param name="DuplicatedLabels">The index of Duplicated labels</param>
        /// <returns></returns>
        public static bool IsVocabularyAndCompatibleNameSame(LabelBlock labelblock, out List<(int, int)> DuplicatedLabels)
        {
            DuplicatedLabels = new List<(int, int)>();
            bool IsOverlap = false;
            var Voca = labelblock.Vocabulary.Keys.ToList();
            var Com = labelblock.CompatibleName.Keys.ToList();
            for (int i = 0; i < Voca.Count; i++)
                for (int j = 0; j < Com.Count; j++)
                    if (Voca[i] == Com[j])
                    {
                        DuplicatedLabels.Add((i, j));
                        IsOverlap = true;
                    }
            return IsOverlap;
        }
        /// <summary>
        /// to check if the compacting name is duplicated
        /// </summary>
        /// <param name="block1"></param>
        /// <param name="block2"></param>
        /// <param name="DuplicatedName"></param>
        /// <returns></returns>
        public static bool IsCompatibleNameSame(LabelBlock block1, LabelBlock block2, out List<(int, int)> DuplicatedName)
        {
            DuplicatedName = new List<(int, int)>();
            bool overlap = false;
            for (int i = 0; i < block1.CompatibleName.Count; i++)
                for (int j = 0; j < block2.CompatibleName.Count; j++)
                {
                    if (block1.CompatibleName.Keys.ToList()[i] == block2.CompatibleName.Keys.ToList()[j])
                    {
                        DuplicatedName.Add((i, j));
                        overlap = true;
                    }
                }
            return overlap;
        }
        /// <summary>
        /// This function is to check if the CompactingName of two blocks are the same
        /// if true, there is at least one label the same
        /// else, the labels are different
        /// </summary>
        /// <param name="other"></param>
        /// <param name="DuplicatedName"></param>
        /// <returns></returns>
        public bool IsCompatibleNameSame(LabelBlock other, out List<(int, int)> DuplicatedName)
        {
            DuplicatedName = new List<(int, int)>();
            bool overlap = false;
            for (int i = 0; i < this.CompatibleName.Count; i++)
                for (int j = 0; j < other.CompatibleName.Count; j++)
                {
                    if (this.CompatibleName.Keys.ToList()[i] == other.CompatibleName.Keys.ToList()[j])
                    {
                        DuplicatedName.Add((i, j));
                        overlap = true;
                    }
                }
            return overlap;
        }
        /// <summary>
        /// Replace a CompactingName with an index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="NewName"></param>
        public void ResetCompatibleName(int index, string NewName)
        {
            var CompactingNameKey = this.CompatibleName.Keys.ToList();
            var CompactingNameValue = this.CompatibleName.Values.ToList();
            CompactingNameKey.RemoveAt(index);
            CompactingNameKey.Insert(index, NewName);
            this.CompatibleName.Clear();
            for (int i = 0; i < CompactingNameKey.Count; i++)
            {
                CompatibleName.Add(CompactingNameKey[i], CompactingNameValue[i]);
            }
        }
        /// <summary>
        /// Replace a CompactingName with an OldName
        /// </summary>
        /// <param name="index"></param>
        /// <param name="NewName"></param>
        public void ResetCompatibleName(string OldName, string NewName)
        {
            if (this.CompatibleName.ContainsKey(OldName))
            {
                var TempDic = new Dictionary<string, string>();
                var TempKep = this.CompatibleName.Keys.Select(x =>
                {
                    if (x == OldName)
                        return NewName;
                    else
                        return x;
                }).ToList();
                var TempValue = this.CompatibleName.Values.ToList();
                this.CompatibleName.Clear();
                for (int i = 0; i < TempValue.Count; i++)
                {
                    this.CompatibleName.Add(TempKep[i], TempValue[i]);
                }
            }
        }
        /// <summary>
        /// To test if vocabulary label exists in the vocabulary
        /// </summary>
        /// <param name="Label"></param>
        /// <returns></returns>
        public bool HasVocaLabel(string Label) => this.Vocabulary.ContainsKey(Label);
        /// <summary>
        /// To test if compactingName label exists in the vocabulary
        /// </summary>
        /// <param name="Label"></param>
        /// <returns></returns>
        public bool HasComLabel(string Label) => this.ComLabels.Contains(Label);
        /// <summary>
        /// Check equevilent
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool Equals(LabelBlock other)
        {
            if (this.Vocabulary != other.Vocabulary || this.CompatibleName != other.CompatibleName || this.LabelBlockName != other.LabelBlockName)
                return false;
            else
                return true;
        }
        /// <summary>
        /// Deep clone in derived class
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();
        /// <summary>
        /// Adding label blocks together
        /// </summary>
        /// <param name="labelBlock1">First Label block</param>
        /// <param name="labelBlock2">Second Label block</param>
        /// <returns></returns>
        public static LabelBlock operator +(LabelBlock labelBlock1, LabelBlock labelBlock2)
        {
            if (labelBlock1 == labelBlock2 || labelBlock1.LabelBlockName == labelBlock2.LabelBlockName) return labelBlock1;
            List<(int, int)> DuplicatedVoca;
            List<(int, int)> DuplicatedCom;
            if (IsVocabularySame(labelBlock1, labelBlock2, out DuplicatedVoca))
            {
                var DuplicatedVoca2 = DuplicatedVoca.Select(x => x.Item1).ToList();
                var labels = labelBlock2.Vocabulary.Keys.ToList();
                for (int i = 0; i < labelBlock2.Vocabulary.Count; i++)
                {
                    if (DuplicatedVoca2.Contains(i))
                    {
                        labelBlock2.ResetVocabulary(i, $"{labelBlock2}_" + labels[i] + i.ToString());
                    }
                }
                foreach (var Label in labelBlock2.Vocabulary)
                {
                    labelBlock1.Vocabulary.Add(Label.Key, Label.Value);
                }
            }
            else
            {
                foreach (var Label in labelBlock2.Vocabulary)
                {
                    labelBlock1.Vocabulary.Add(Label.Key, Label.Value);
                }
            }
            if (IsCompatibleNameSame(labelBlock1, labelBlock2, out DuplicatedCom))
            {
                var DuplicatedCom2 = DuplicatedCom.Select(x => x.Item2).ToList();
                var Com2 = labelBlock2.CompatibleName.Keys.ToList();
                for (int i = 0; i < labelBlock2.CompatibleName.Count; i++)
                {
                    if (DuplicatedCom2.Contains(i))
                    {
                        labelBlock2.ResetCompatibleName(i, "Temp_" + Com2[i] + i.ToString());
                    }
                }
                foreach (var Com in labelBlock2.CompatibleName)
                {
                    labelBlock1.CompatibleName.Add(Com.Key, Com.Value);
                }
            }
            else
            {
                foreach (var Com in labelBlock2.CompatibleName)
                {
                    labelBlock1.CompatibleName.Add(Com.Key, Com.Value);
                }
            }
            return labelBlock1;
        }
        /// <summary>
        /// Get All containing blocks' name
        /// </summary>
        public List<string> CompactingBlockName
        {
            get
            {
                var NameList = new List<string>();
                var Names = CompatibleName.Values.ToList();
                for (int i = 0; i < Names.Count; i++)
                {
                    if (!NameList.Contains(Names[i]) && BlockTable.HasNamed(Names[i]))
                        NameList.Add(Names[i]);
                }
                return NameList;
            }
        }
        /// <summary>
        /// Turn this block into a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.GetType().Name;
        }
    }
}
