using System;
using System.Collections.Generic;
using Rhino;
using Rhino.DocObjects;
using SGSFramework.Core.Interface;
using SGSFramework.Core.BlockSetting.LabelBlockBase_2;
using Rhino.Geometry;
using SGSFramework.Core.BlockSetting;

namespace SGSFramework.Core.Token
{
    public enum PopAndPush
    {
        Pop, Push
    }
    /// <summary>
    /// TerminalActionToken can only accept terminal actions
    /// operator also included in this terminalActionToken
    /// Eg: 
    /// pop and push "[" "]"
    /// I shall assume we will not have more operator definitions
    /// </summary>
    public class TerminalActionToken : ActionToken
    {
        public LabelBlockTable LabelTable { get; set; }
        public override string Print { get; protected set; }
        public override string _name { get; }
        public new LabelAction<Transform> _attributePtr { get; protected set; }
        /// <summary>
        /// Run the token
        /// </summary>
        /// <param name="TS"></param>
        /// <param name="TSStack"></param>
        /// <param name="CustomAction"></param>
        /// <returns> if returns true, the action function correctly, else false</returns>
        public override bool Run(ref object TS, ref Stack<Transform> TSStack, params object[] CustomAction)
        {
            if (TS is Transform TTS)
            {
                TS = _attributePtr(TTS, ref TSStack, CustomAction);
                if (this.ExceptionNotion == "")
                    return true;
                else
                    return false;
            }
            else
            {
                this.ExceptionNotion = $"The input {TS.ToString()} isn't a transform";
                return false;
            }
        }
        public TerminalActionToken(BlockToken blockToken)
        {
            this._name = blockToken._name;
            if (blockToken.Type == "Vocabulary")
            {
                this._attributePtr = TransformationAction;
                this.Print = $"TS({_name})";
            }
            else
            {
                this._attributePtr = CompatibleNameAction;
                this.Print = $"ID({_name})";
            }
        }
        public TerminalActionToken(string _name, string PrintFormat = "UnKnow")
        {
            this._name = _name;
            if (PrintFormat == "UnKnow")
                this.Print = $"Unknow{_name}";
            else
                this.Print = PrintFormat;
        }
        public TerminalActionToken(string _name, LabelAction<Transform> CustomAction, string PrintFormat = "Unknow")
        {
            this._name = _name;
            this._attributePtr = CustomAction;
            if (PrintFormat == "UnKnow")
                this.Print = $"Unknow{_name}";
            else
                this.Print = PrintFormat;
        }
        public TerminalActionToken(PopAndPush popAndpush)
        {
            if (popAndpush == PopAndPush.Pop)
            {
                this._name = "]";
                this._attributePtr = PopAction;
                this.Print = "Pop";
            }
            else
            {
                this._name = "[";
                this._attributePtr = PushAction;
                this.Print = "Push";
            }
        }
        public TerminalActionToken()
        {
            _name = "empty";
            this._attributePtr = Epsilon;
            this.Print = "empty";
        }
        private Transform Epsilon(Transform TS, ref Stack<Transform> TSStack, params object[] CustomAction)
            => TS;
        /// <summary>
        /// Pop a transformation from teh TSStack
        /// </summary>
        /// <param name="TS"></param>
        /// <param name="TSStack"></param>
        /// <param name="CustomAction"></param>
        /// <returns></returns>
        private Transform PopAction(Transform TS, ref Stack<Transform> TSStack,
            params object[] CustomAction)
        {
            if (TSStack.Count == 0)
            {
                throw new Exception();
            }
            return TSStack.Pop();
        }
        /// <summary>
        /// Push a transformation into the TSStack
        /// </summary>
        /// <param name="TS"></param>
        /// <param name="TSStack"></param>
        /// <param name="CustomAction"></param>
        /// <returns></returns>
        private Transform PushAction(Transform TS, ref Stack<Transform> TSStack,
            params object[] CustomAction)
        {
            TSStack.Push(TS);
            return TS;
        }
        /// <summary>
        /// Concatenate Transformation with the label action
        /// </summary>
        /// <param name="TS">current transformation</param>
        /// <param name="TSStack"></param>
        /// <param name="CustomAction"></param>
        /// <returns>return transformation multiply with vocabulary transformation</returns>
        private Transform TransformationAction(Transform TS, ref Stack<Transform> TSStack,
            params object[] CustomAction)
        {
            Transform TryGetTS;
            if (!LabelTable.TryGetTransform(_name, out TryGetTS))
                ExceptionNotion = $"vocabulary {_name} is undefined in blocks";
            TS *= TryGetTS;
            return TS;
        }
        /// <summary>
        /// Place the block with TS
        /// </summary>
        /// <param name="TS"></param>
        /// <param name="TSStack"></param>
        /// <param name="CustomAction"></param>
        /// <returns></returns>
        private Transform CompatibleNameAction(Transform TS, ref Stack<Transform> TSStack,
            params object[] CustomAction)
        {
            RhinoDoc Doc = RhinoDoc.ActiveDoc;
            string CompatibelName;
            if (!LabelTable.TryGetCompatibleName(_name, out CompatibelName))
                ExceptionNotion = $"Compatible Name {_name} is undefined in blocks";
            if (!BlockTable.HasNamed(CompatibelName))
                ExceptionNotion = $"{CompatibelName} isn't existed in blocktable";
            else
                ExceptionNotion = "";
            var Exiting_idef = Doc.InstanceDefinitions.Find(CompatibelName);
            var ObjectAtt = new ObjectAttributes();
            Layer layer = new Layer();
            if (Doc.Layers.FindName("Generative_Blocks") is null)
            {
                layer = new Layer() { Name = "Generative_Blocks" };
                Doc.Layers.Add(layer);
            }
            else
            {
                layer = Doc.Layers.FindName("Generative_Blocks");
            }
            ObjectAtt.LayerIndex = layer.Index;
            Guid insertBlock = Doc.Objects.AddInstanceObject(Exiting_idef.Index, TS, ObjectAtt);
            this.LabelTable.RhinoReferenceBlockGuid[CustomAction[0].ToString()].Add(insertBlock);
            return TS;
        }

        public bool Equals(IToken<string, LabelAction<Transform>> other)
        => this._name == other._name && this._attributePtr == other._attributePtr;
        public override bool Equals(IToken<string, LabelAction<object>> other)
        {
            var ConvertLabelAction = ActionToken.ConvertLabel<Transform>(this._attributePtr);
            return this._name == other._name && ConvertLabelAction == other._attributePtr;
        }
    }
}
