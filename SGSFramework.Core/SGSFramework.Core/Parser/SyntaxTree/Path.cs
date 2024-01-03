using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Parser.SyntaxTree
{
    [Obsolete]
    public class Path : IEquatable<Path>, IComparable<Path>, ICloneable
    {
        private readonly int[] _Path;
        public int Length => _Path.Length;
        public bool Valid => _Path != null;
        public int[] indices => _Path;
        public Path(IEnumerable<int> path):this(path.ToArray())
        {
        }
        public Path(params int[] index)
        {
            this._Path = index;
        }
        public Path(Path path)
        {
            this._Path = path._Path;
        }
        public Path(int path)
        {
            this._Path = new int[1] { path };
        }
        public Path PushBackElement(int index)
        {
            var TempPath = ((int[])_Path.Clone()).ToList();
            TempPath.Add(index);
            return new Path(TempPath);
        }
        public Path PushFrontElement(int index)
        {
            var TempPath = ((int[])_Path.Clone()).ToList();
            TempPath.Insert(0, index);
            return new Path(TempPath);
        }
        public bool Equals(Path other)
         => this._Path == other._Path;
        public Path ShiftPath(int index = 1)
        {
            if (this.Length == 1)
            {
                return new Path(_Path[0]++);
            }
            var NewPath = new int[this.Length - index];
            for (int i = 0; i < NewPath.Length; i++)
                NewPath[i] = _Path[i];
            return new Path(NewPath);
        }
        public override bool Equals(object obj)
        {
            return this == obj as Path;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator== (Path A, Path B)
            => A._Path == B._Path;
        public static bool operator <(Path A, Path B)
            => A.CompareTo(B) == -1 ? true : false;
        public static bool operator >(Path A, Path B)
            => A.CompareTo(B) == 1 ? true : false;
        public static bool operator !=(Path A, Path B)
            => !(A == B);
        public int CompareTo(Path other)
        {
            if (this._Path is null || other is null)
            {
                return 1; // Null paths are considered greater.
            }
            int minLength = Math.Min(this.Length, other.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (this._Path[i] < other._Path[i])
                {
                    return -1; // This path is smaller
                }
                else if (this._Path[i] > other._Path[i])
                {
                    return 1; // This path is greater
                }
            }

            // If all common elements are equal, the shorter path is smaller
            return this.Length.CompareTo(other.Length);
        }
        public static implicit operator Path(string str)
        {
            str = Util.Util.CleanSequence(str);
            List<int> indicesPath = new List<int>();
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != '{' && str[i] != '}' && str[i] != ' '  && str[i] != ',' )
                {
                    if (int.TryParse(str[i].ToString(), out int result))
                    {
                        indicesPath.Add(result);
                    }
                }
            }
            return new Path(indicesPath);
        }   //Conversion  Obj => Obj "as  => obj | null" is a
        public static implicit operator string(Path Path)     //Parse       Language => Language    3dm  => dwg
             => Path.ToString();
        public object Clone()
        {
            return new Path(this);
        }
        public override string ToString()
        {
            return "{" + string.Join(", ", _Path) + "}";
        }
    }
}
