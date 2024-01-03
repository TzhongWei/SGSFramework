using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Tokeniser.Attributes;

namespace SGSFramework.Core.Tokeniser
{
    public class SymbolTable
    {
        private int ID = 0;
        private Dictionary<int, ITokenAttribute> _Attributes = new Dictionary<int, ITokenAttribute>();
        public bool Push_Back(ITokenAttribute tokenAttribute)
        {
            if (_Attributes.ContainsValue(tokenAttribute) || _Attributes.ContainsKey(tokenAttribute.ID))
                return false;
            else
            {
                this._Attributes.Add(tokenAttribute.ID, tokenAttribute);
                return true;
            }
        }
        public int Count => _Attributes.Count;  
        public ITokenAttribute this[int Index]
        {
            get
            {
                try
                {
                    return this._Attributes[Index];
                }
                catch 
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }
        public SymbolTable() 
        {
            this.ID = 0;
        }
        public int GetID()
        {
            ID++;
            return this.ID - 1;
        }
        public void Clear()
        {
            this._Attributes = new Dictionary<int, ITokenAttribute>();
        }
    }
}
