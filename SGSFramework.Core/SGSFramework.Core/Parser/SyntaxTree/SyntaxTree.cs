using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Parser.SyntaxTree
{
    #region OBSOLETE
    //public class SyntaxTree : IComparable<SyntaxTree>
    //{
    //    public string BranchValue => BranchNode.Value;
    //    public readonly Node BranchNode;
    //    public readonly List<SyntaxTree> SubBranches;
    //    public int Count => this._GetAllDerivedNodes.Count;
    //    public Path BranchPath => BranchNode.TreePath;
    //    public static SyntaxTree VoidTree;
    //    public void Shift()
    //    {
    //        this.BranchNode.TreePath.PushFrontElement(0);
    //        foreach (SyntaxTree tree in SubBranches)
    //        {
    //            if (tree.SubBranches.Count > 0)
    //                tree.Shift();
    //            else
    //                tree.BranchNode.TreePath.PushFrontElement(0);
    //        }
    //    }
    //    static SyntaxTree()
    //    {
    //        VoidTree = new SyntaxTree();
    //    }
    //    private SyntaxTree()
    //    {
    //        BranchNode = Node.Unset;
    //        SubBranches = new List<SyntaxTree>();
    //    }
    //    private SyntaxTree(Node LeafNodes)
    //    {
    //        BranchNode = LeafNodes;
    //        this.SubBranches = new List<SyntaxTree>();
    //    }
    //    public bool HasNode(Node Node, out List<Path> BranchID)
    //    {
    //        List<SyntaxTree> Find = this.SubBranches;
    //        if (BranchNode == Node)
    //        {
    //            BranchID = new List<Path>() { this.BranchPath };
    //            return true;
    //        }
    //        else
    //        {
    //            foreach (SyntaxTree tree in Find)
    //            {
    //                if (tree.SubBranches.Count > 0)
    //                    return tree.HasNode(Node, out BranchID);
    //            }
    //            BranchID = null;
    //            return false;
    //        }
    //    }
    //    public bool InsertNode(Path NodePath, params Node[] LeafNodes)
    //    {
    //        var Trees = LeafNodes.Select(x => new SyntaxTree(x)).ToArray();
    //        return InsertTree(NodePath, Trees);
    //    }
    //    public bool InsertTree(Path NodePath, params SyntaxTree[] LeafTree)
    //    {
    //        if (this.BranchPath == NodePath)
    //        {
    //            this.SubBranches.AddRange(LeafTree);
    //            return true;
    //        }
    //        else
    //        {
    //            foreach (var tree in this.SubBranches)
    //            {
    //                if (tree.SubBranches.Count > 0)
    //                    return tree.InsertTree(NodePath, LeafTree);
    //            }
    //            return false;
    //        }
    //    }
    //    public void AddNodeInTree(params Node[] Nodes)
    //    {
    //        foreach (Node node in Nodes)
    //            AddNodeInTree(node);
    //    }
    //    public void AddNodeInTree(Node LeafNodes)
    //    {
    //        var NewNodeBranch = new SyntaxTree(LeafNodes);
    //        if (NewNodeBranch.BranchPath.Length == this.BranchPath.Length)
    //            return;

    //        if (this.GetDeriveNodes.Select(x => x.TreePath).ToList().Contains(LeafNodes.TreePath))
    //            return;

    //        if (SubBranches.Count == 0)
    //            this.SubBranches.Add(NewNodeBranch);

    //        for (int i = 0; i < SubBranches.Count; i++)
    //        {
    //            if (NewNodeBranch.BranchPath < SubBranches[i].BranchPath)
    //            {
    //                if (NewNodeBranch.BranchPath.Length == SubBranches[i].BranchPath.Length)
    //                {
    //                    this.SubBranches.Add(NewNodeBranch);
    //                    this.SubBranches.Sort();
    //                    return;
    //                }
    //                else
    //                {
    //                    this.SubBranches[i].AddNodeInTree(LeafNodes);
    //                    return;
    //                }
    //            }
    //            else if (i < SubBranches.Count - 1)
    //            {
    //                if (SubBranches[i].BranchPath < NewNodeBranch.BranchPath
    //                    && NewNodeBranch.BranchPath < SubBranches[i + 1].BranchPath)
    //                {
    //                    if (NewNodeBranch.BranchPath.Length == SubBranches[i].BranchPath.Length)
    //                    {
    //                        this.SubBranches.Add(NewNodeBranch);
    //                        this.SubBranches.Sort();
    //                        return;
    //                    }
    //                    else
    //                    {
    //                        this.SubBranches[i].AddNodeInTree(LeafNodes);
    //                        return;
    //                    }
    //                }
    //            }
    //            else if (NewNodeBranch.BranchPath > SubBranches[i].BranchPath)
    //            {
    //                if (NewNodeBranch.BranchPath.Length == SubBranches[i].BranchPath.Length)
    //                {
    //                    this.SubBranches.Add(NewNodeBranch);
    //                    this.SubBranches.Sort();
    //                    return;
    //                }
    //                else
    //                {
    //                    this.SubBranches[i].AddNodeInTree(LeafNodes);
    //                    return;
    //                }
    //            }
    //        }
    //    }
    //    public SyntaxTree(Node BranchNode, params Node[] LeafNodes): 
    //        this(BranchNode, LeafNodes.Select(x => new SyntaxTree(x)).ToArray())
    //    {
    //    }
    //    public SyntaxTree(Node BranchNode, SyntaxTree[] LeafTrees)
    //    {
    //        this.BranchNode = BranchNode;
    //        this.SubBranches = LeafTrees.ToList();
    //    }
    //    public List<Node> GetDeriveNodes
    //    {
    //        get
    //        {
    //            var NodeList = _GetAllDerivedNodes;
    //            NodeList.Sort();
    //            return NodeList;
    //        }
    //    }
    //    private List<Node> _GetAllDerivedNodes
    //    {
    //        get
    //        {
    //            var Nodes = new List<Node>();
    //            Nodes.Add(this.BranchNode);
    //            foreach (var tree in this.SubBranches)
    //            {
    //                if (tree.SubBranches.Count > 0)
    //                {
    //                    Nodes.AddRange(tree._GetAllDerivedNodes);
    //                }
    //                else
    //                    Nodes.Add(tree.BranchNode);
    //            }
    //            return Nodes; 
    //        }
    //    }
    //    public override string ToString()
    //    {
    //        string result = "Syntax Tree => \n";
    //        List<Node> nodes = new List<Node>() {};
    //        nodes.AddRange(this._GetAllDerivedNodes);

    //        nodes.Sort();

    //        result += nodes.Count > 0 ? string.Join("\n", nodes) : "";

    //        return result;
    //    }
    //    public int CompareTo(SyntaxTree other)
    //    {
    //        if (!other.BranchNode.Valid || !this.BranchNode.Valid)
    //            return 1;

    //        return this.BranchPath.CompareTo(other.BranchPath);
    //    }
    //}
    #endregion
    public class SyntaxTree
    {
        public string Value { get; set; }
        public List<SyntaxTree> Chlidren { get; }
        public SyntaxTree(string Value, List<SyntaxTree> Chlidren = null)
        {
            this.Value = Value;
            this.Chlidren = Chlidren ?? new List<SyntaxTree>();
        }
        List<string> Result = new List<string>();
        public string PrintTree(SyntaxTree tree, string indent, bool Last)
        {
            string result = $"{indent} +- {tree.Value} \n";
            indent += Last ? "  " : "|  ";
            for (int i = 0; i < tree.Chlidren.Count; i++)
            {
                result += PrintTree(tree.Chlidren[i], indent, i == tree.Chlidren.Count - 1);
            }
            return result;

        }
        public override string ToString()
             => this.PrintTree(this, "", true);
    }
}
