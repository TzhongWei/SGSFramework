using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using SGSFramework.Core.Token;

namespace SGSFramework.Core.BlockSetting.LabelBlockBase_2
{
    /// <summary>
    /// This class is to manage the block with their corrosponding transformations and labels
    /// </summary>
    internal class NamespaceDoc { }
    /// <summary>
    /// The class need to be defined with labels and transformation actions.
    /// </summary>
    public abstract class LabelBlock : IEquatable<LabelBlock>, ICloneable
    {
        /// <summary>
        /// Constructor, it can only be set up with inherited classes
        /// </summary>
        protected LabelBlock()
        {
            this.blockTokens = new BlockTokenList(this.LabelBlockName);
        }
        /// <summary>
        /// This block tokens store and manage all block's vocabulary and compatible name 
        /// [Vocabulary]
        /// The principle of naming vocabulary is a pair of string label and transformation matrix
        /// [Compatible Name]
        /// /// BlockName_I, BlockName =>
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
        public BlockTokenList blockTokens { get; protected set; }
        /// <summary>
        /// Every derived class need to set up their vocabulary
        /// </summary>
        public abstract void SetUpVocabulary();

        public bool Equals(LabelBlock other)
         => this.LabelBlockName == other.LabelBlockName && this.blockTokens == other.blockTokens;

        /// <summary>
        /// The label block name differetiate the label block instance
        /// </summary>
        public abstract string LabelBlockName { get; protected set; }
        /// <summary>
        /// this properity is only for retrieving vocabulary labels
        /// </summary>
        /// <returns></returns>
        public List<string> VocaLabels => this.blockTokens.VocabularyLabel;
        /// <summary>
        /// this properity is only for retrieving CompactingName labels
        /// </summary>
        public List<string> ComLabels => this.blockTokens.CompatibleLabel;
        public List<string> Labels
        {
            get
            {
                var LabelList = ComLabels;
                LabelList.AddRange(VocaLabels);
                return LabelList;
            }
        }
        public override string ToString()
        {
            return this.LabelBlockName;
        }
        public static LabelBlock operator +(LabelBlock A, LabelBlock B)
        {
            if (A.Equals(B)) return A;
            A.blockTokens += B.blockTokens;
            A.LabelBlockName = "ComposeLabelBlock";
            return A;
        }
        /// <summary>
        /// To test if vocabulary label exists in the vocabulary
        /// </summary>
        /// <param name="Label"></param>
        /// <returns></returns>
        public bool HasVocaLabel(string Label) => this.blockTokens.HasVocaLabel(Label);
        /// <summary>
        /// To test if compactingName label exists in the vocabulary
        /// </summary>
        /// <param name="Label"></param>
        /// <returns></returns>
        public bool HasComLabel(string Label) => this.blockTokens.HasComLabel(Label);

        public abstract object Clone();
    }
}
