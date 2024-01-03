using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Parser.SyntaxTree
{
    [Obsolete]
    public class Node : IEquatable<Node>, IComparable<Node>
    {
        public readonly string Value;
        public readonly Path TreePath;
        public static Node Unset;
        public bool Valid => TreePath.Valid;
        public Node(string Value, Path TreePath)
        {
            this.Value = Value;
            this.TreePath = TreePath;
        }
        public bool Equals(Node other)
        {
            if (!other.Valid || !this.Valid) return false;
            else
            {
                return this.Value == other.Value & this.TreePath == other.TreePath;
            }
        }
        
        public override string ToString()
        {
            return this.TreePath.ToString() + " : " + Value;
            
        }

        public int CompareTo(Node other)
        {
            if (!other.Valid || !this.Valid)
                return 1;
            return this.TreePath.CompareTo(other.TreePath);
        }
    }
}
