using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using SGSFramework.Core.Interface;
using SGSFramework.Core.BlockSetting.LabelBlockBase_2;

namespace SGSFramework.Core.Token
{
    public class TerminalActionTokenList : ITokenList<TerminalActionToken>
    {
        public TerminalActionToken this[int index]
        {
            get => this.TokenList.ToList()[index];
            set
            {
                TokenList[index] = value;
            }
        }

        public List<TerminalActionToken> TokenList { get; private set; }

        public int Count => TokenList.Count;
        public LabelBlockTable LabelTable;
        public bool IsReadOnly => false;

        HashSet<TerminalActionToken> ITokenList<TerminalActionToken>.TokenList => throw new NotImplementedException();

        public TerminalActionTokenList(LabelBlockTable labelBlockTable)
        {
            this.TokenList = new List<TerminalActionToken>();
            this.LabelTable = labelBlockTable;
        }
        public void Add(TerminalActionToken item)
        {
            item.LabelTable = LabelTable;
            this.TokenList.Add(item);  
        }
        void ITokenList<TerminalActionToken>.AddRange(HashSet<TerminalActionToken> Tokens) => throw new NotImplementedException();
        public List<string> AllActions => this.Select(x => string.Join(",",x._name, x._attributePtr.ToString())).ToList();
        public void AddRange(List<TerminalActionToken> Tokens)
        {
            foreach (var item in Tokens)
                this.Add(item);
        }
        public string cmd { get; private set; } = "Success";
        public virtual bool Run(string Label, params object[] CustomAction)
        {
            bool result = false;
            var TS = Transform.Identity as object;
            Stack<Transform> TSStack = new Stack<Transform>();
            foreach (var ActionToken in this.TokenList)
            {
                result = ActionToken.Run(ref TS, ref TSStack, Label, CustomAction);
                if (!result)
                {
                    cmd = ActionToken.ExceptionNotion + "\n";
                    return false;
                }
            }
            if (TSStack.Count != 0)
            {
                cmd += "Stack count isn't currected";
                return false;
            }
            return result;
        }
        public void Clear()
        {
            this.TokenList.Count();
        }

        public bool Contains(TerminalActionToken item)
        => this.TokenList.Contains(item);

        public void CopyTo(TerminalActionToken[] array, int arrayIndex)
        => this.TokenList.CopyTo(array, arrayIndex);

        public IEnumerator<TerminalActionToken> GetEnumerator()
        {
            return this.TokenList.GetEnumerator();
        }

        public int IndexOf(TerminalActionToken item)
        => this.TokenList.IndexOf(item);

        public void Insert(int index, TerminalActionToken item)
        => this.TokenList.Insert(index, item);

        public bool Remove(TerminalActionToken item)
         => this.TokenList.Remove(item);

        public void RemoveAt(int index)
         => this.TokenList.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.TokenList.GetEnumerator();
        }
    }
}
