using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGSFramework.Core.Parser.LRParseing;
using SGSFramework.Core.Parser.LRParseing.LALR;
using SGSFramework.Core.Parser.LRParseing.ParingInterface;

namespace SGSFramework.Core.Parser.SyntaxTree
{
    public interface LR1ParsingString<TKernelItem> : IParsingString where TKernelItem : BaseLR1KernelItem, IEquatable<TKernelItem>
    { }
    public class LALR1ParsingString: LR1ParsingString<LALR1KernelItem>
    {
        public string cmd => _cmd;
        private string _cmd = "================SLRParsingString============\n";
        public IParsingTable<BaseLR1ActionDictionary<LALR1KernelItem>> parsingTable { get; }
        public string Sentence;
        public SyntaxTree Tree { get; set; }
        public LALR1ParsingString(string S, IParsingTable<BaseLR1ActionDictionary<LALR1KernelItem>> parsingTable)
        {
            Sentence = S;
            this.parsingTable = parsingTable;
        }
        public LALR1ParsingString(string S, LALR1Parsing Parser):this(S, Parser.ParsingTable)
        {
        }
        public SyntaxTree StartParsing()
        {
            if (!(Tree is null))
                return Tree;

            var Tokens = Sentence.Split(' ').ToList();
            Tokens.Add("$");
            string input;
            int stateRow = 0;
            int productionIndex = 0;
            int GridLength = this.Sentence.Length * 2 + 15;
            //The token stack must be a number with a token, which is start from 0,
            //until accept
            Stack<object> TokenStack = new Stack<object>();
            TokenStack.Push(stateRow);
            BaseLR1ActionDictionary<LALR1KernelItem> foundRow;

            var StackPrint = TokenStack.Select(x => x.ToString()).ToList();
            StackPrint.Reverse();
            var TokenPrint = new List<string>(Tokens);
            List<SyntaxTree> _Syntaxtree = new List<SyntaxTree> { new SyntaxTree("$") };
            while (productionIndex != -1)
            {
                input = Tokens.First();
                foundRow = parsingTable.ElementAt(stateRow);
                if (foundRow.TryGetValue(input, out var Action))
                {
                    if (!(Action.Reduce is null))
                    {
                        var Rules = Action.Reduce.First(); //LALR parser only accept one reduce
                        productionIndex = Rules.Index;
                        if (productionIndex == -1 && Tokens.Count > 1)
                            throw new Exception("This sentence cannot be accepted.");
                        else if (productionIndex == -1)
                        {
                            _cmd += Util.Util.CharSpace(StackPrint, GridLength) + "|" +
                                       Util.Util.CharSpace(TokenPrint, this.Sentence.Length + 5) + "|" +
                                       "accept" + "\n";
                            this.Tree = new SyntaxTree("<Start>", _Syntaxtree);
                            _Syntaxtree = null;
                            continue;
                        }
                        else
                        {
                            int BodyCount = Rules.Body.Count;
                            if (TokenStack.Count < BodyCount)
                            {
                                throw new Exception("Token stack has errors.");
                            }
                            else
                            {
                                //Construct tree during reduction
                                List<SyntaxTree> TokensFromStack = new List<SyntaxTree>();
                                for (int i = 0; i < BodyCount * 2; i++)
                                {
                                    var Item = TokenStack.Pop();
                                    if (i % 2 == 1)
                                    {
                                        TokensFromStack.Add(new SyntaxTree(Item.ToString()));
                                    }
                                }
                                TokensFromStack.Reverse();
                                var Nonterminal = new SyntaxTree(Rules.Head, TokensFromStack);
                                if (TokensFromStack.Count == 0)
                                {
                                    Nonterminal = new SyntaxTree(Rules.Head, new List<SyntaxTree>() { new SyntaxTree("empty") });
                                    _Syntaxtree.Add(Nonterminal);
                                }
                                else if (Nonterminal.Chlidren.Last().Value == _Syntaxtree.Last().Value)
                                {
                                    Nonterminal.Chlidren[Nonterminal.Chlidren.Count - 1] = _Syntaxtree.Last();
                                    _Syntaxtree.RemoveAt(_Syntaxtree.Count - 1);

                                    _Syntaxtree.Add(Nonterminal);
                                }
                                else
                                    _Syntaxtree.Add(Nonterminal);

                                Tokens.Insert(0, Rules.Head);
                                stateRow = (int)TokenStack.First();
                                _cmd += Util.Util.CharSpace(StackPrint, GridLength) + "|" +
                                        Util.Util.CharSpace(TokenPrint, this.Sentence.Length + 5) + "|" +
                                        $"r{Rules.Index.ToString()}" + "\n";
                                StackPrint = TokenStack.Select(x => x.ToString()).ToList();
                                StackPrint.Reverse();
                                TokenPrint = new List<string>(Tokens);
                            }
                        }
                    }
                    else
                    {
                        stateRow = Action.Shift.Index;
                        TokenStack.Push(input);
                        TokenStack.Push(stateRow);
                        Tokens.RemoveAt(0);
                        _cmd += Util.Util.CharSpace(StackPrint, GridLength) + "|" +
                            Util.Util.CharSpace(TokenPrint, this.Sentence.Length + 5) + "|" +
                            $"s{stateRow.ToString()}" + "\n";
                        StackPrint = TokenStack.Select(x => x.ToString()).ToList();
                        StackPrint.Reverse();
                        TokenPrint = new List<string>(Tokens);
                    }
                }
                else
                {
                    _cmd += "Sentance cannot be accepted \n" +
                        "=======================Sentence Failed=======================\n";
                    break;
                }
            }

            _cmd += "=================Parsing Table finished==================\n";
            return this.Tree;

        }
    }
}
