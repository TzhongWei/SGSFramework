using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Interface;
using Rhino.Geometry;
using System.Collections;

namespace SGSFramework.Core.Token
{
    /// <summary>
    /// Block token store the transformation and compatible name information with label pair
    /// </summary>
    public class BlockToken : IToken<string, BlockAction>, IComparable<BlockToken>
    {
        public string _name { get; private set; }
        public BlockAction _attributePtr { get;}
        public BlockToken(string name, BlockAction CustomAction)
        {
            this._name = name;
            this._attributePtr = CustomAction;
        }
        public override string ToString()
        {
            return this._name + ", " + this._attributePtr.ActionName;
        }
        public BlockToken(string name, Transform _Transform)
        {
            this._name = name;
            this._attributePtr = BlockAction.SetAction(_Transform);
        }
        public BlockToken(string name, string CompatibleName)
        {
            this._name = name;
            this._attributePtr = BlockAction.SetAction(CompatibleName);
        }
        public BlockToken ResetName(string newName)
        {
            this._name = newName;
            return this;
        }
        public bool Equals(IToken<string, BlockAction> other)
         => this._name == other._name && this._attributePtr == other._attributePtr;
        public int CompareTo(BlockToken other)
        {
            if (other.Type == this.Type)
            {
                return this._name.CompareTo(other._name);
            }
            else
            {
                if (this.Type == "Vocabulary" && other.Type == "CompactibleName")
                    return 1;
                else
                    return -1;
            }
        }
        public object Action => this._attributePtr.Action;
        public string Type => this._attributePtr.ActionName;       
    }

    public abstract class BlockAction : IEquatable<BlockAction>
    {
        public abstract string ActionName { get; }
        public abstract object Action { get; }
        public static BlockAction SetAction(Transform transform)
            => new TransformationAction(transform);
        public static BlockAction SetAction(string Name)
            => new IdentifierAction(Name);
        public override bool Equals(object obj)
         => this.Equals(obj as BlockAction);
        public override int GetHashCode()
            => base.GetHashCode();
        public abstract bool Equals(BlockAction other);
        public static bool operator ==(BlockAction A, BlockAction B)
         => A.Equals(B);
        public static bool operator !=(BlockAction A, BlockAction B)
            => A.Equals(B);
    }
    public class TransformationAction : BlockAction
    {
        private Transform _TS;
        public TransformationAction(Transform transform)
        {
            this._TS = transform;
        }
        public override object Action => _TS;
        public override string ActionName => "Vocabulary";

        public override bool Equals(BlockAction other)
        {
            if (other is TransformationAction)
            {
                return ((Transform)other.Action) == this._TS;
            }
            else
                return false;
        }
    }
    public class IdentifierAction : BlockAction
    {
        public override string ActionName => "CompatibleName";
        private string _CompatibleName;
        public IdentifierAction(string Name)
        {
            this._CompatibleName = Name;
        }
        public override object Action => _CompatibleName;

        public override bool Equals(BlockAction other)
        {
            if (other is IdentifierAction)
            {
                return other.Action.ToString() == _CompatibleName;
            }
            else
                return false;
        }
    }
}
