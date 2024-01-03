using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms.ThemedControls;
using Rhino;
using Rhino.Geometry;
using Rhino.PlugIns;

namespace SGSFramework.Core.Tokeniser
{
    public class BufferStream
    {
        private int BufferPointer = -1;
        private Dictionary<int, Buffer> BufferList { get; set; } = new Dictionary<int, Buffer>();
        public (int, bool)? StartPt { get; set; } = null;
        public Buffer Current { get { return this[BufferPointer]; } }
        public Buffer this[int index]
        {
            get {
                if (index >= 0 && index < Count)
                {
                    return BufferList[index];
                }
                else
                    throw new IndexOutOfRangeException();
            }
        }
        public int Count { get; } = 0;
        public Dictionary<Point3d, List<int>> NodesToBuffer { get; private set; }
        public bool NodeContructable { get; private set; } = false;
        private Dictionary<Point3d, List<int>> _ConstructNodeGraph()
        {
            #region GRAPHCONSTRUCT

            var PolyIndices = Enumerable.Range(0, Count).ToList();
            var NodesToBuffer = new Dictionary<Point3d, List<int>>();

            foreach (var Ind in PolyIndices)
            {
                var BufferCrvSt = this[Ind].CrvStartAndEnd[0];
                var BufferCrvEd = this[Ind].CrvStartAndEnd[1];
                if (NodesToBuffer.ContainsKey(BufferCrvSt))
                {
                    NodesToBuffer[BufferCrvSt].Add(Ind);
                }
                else
                {
                    NodesToBuffer.Add(BufferCrvSt, new List<int>() { Ind });
                }
                if (NodesToBuffer.ContainsKey(BufferCrvEd))
                {
                    NodesToBuffer[BufferCrvEd].Add(Ind);
                }
                else
                {
                    NodesToBuffer.Add(BufferCrvEd, new List<int>() { Ind });
                }
            }
            this.NodeContructable = false;
            return NodesToBuffer;
            #endregion
        }
        public BufferStream(Curve PLCrv)
        {
            #region CONSTRUCTOR
            var Segs = PLCrv.DuplicateSegments();
            foreach (var Seg in Segs)
            {
                this.BufferList.Add(Count, new Buffer(Seg));
                Count++;
            }
            this.NodesToBuffer = _ConstructNodeGraph();
            #endregion
        }
        public BufferStream(IEnumerable<Curve> Crvs)
        {
            #region CONSTRUCTOR
            var CrvSegs = Crvs.Select(x => x.DuplicateSegments()).ToList();
            var Segs = new List<Curve>();
            foreach (var CrvSeg in CrvSegs)
                Segs.AddRange(CrvSeg);
            foreach (var Seg in Segs)
            {
                this.BufferList.Add(Count, new Buffer(Seg));
                Count++;
            }
            this.NodesToBuffer = _ConstructNodeGraph();
            #endregion
        }
        public bool AlignBuffering()
        {
            #region ALIGNBUFFERLIST
            if (StartPt == null)
            {
                throw new Exception("The start point hasn't set yet");
            }
            var Indices = Enumerable.Range(0, Count).Where(x => x != StartPt.Value.Item1).ToList();
            var NewSequence = new List<int>() { StartPt.Value.Item1 };

            var NewBufferList = new Dictionary<int, Buffer>();
            NewBufferList.Add(0, this[NewSequence[0]]);
            var CurrentEndPt = this[NewSequence[0]].EndPoint;


            var CurrentBuffer = this[NewSequence[0]];
            var CurrentBufferTestPt = CurrentBuffer.CrvStartAndEnd[1];
            var StackSe = new Stack<int>();
            StackSe.Push(NewSequence[0]);
            int AddCount = 1;

            while (Indices.Count != 0)
            {
                if (NodesToBuffer[CurrentBufferTestPt].Count > 1)
                {
                    double Max = 0;
                    int TargetBufferInd = -1;
                    for (int i = 0; i < NodesToBuffer[CurrentBufferTestPt].Count; i++)
                    {
                        var TestAnswer = NodesToBuffer[CurrentBufferTestPt][i];
                        if (!NewSequence.Contains(NodesToBuffer[CurrentBufferTestPt][i]))
                        {
                            var Len = BufferList[NodesToBuffer[CurrentBufferTestPt][i]].CurveLength;
                            if (Len > Max)
                            {
                                Max = Len;
                                TargetBufferInd = NodesToBuffer[CurrentBufferTestPt][i];
                            }
                        }
                    }
                    
                    if (TargetBufferInd == -1)
                    {
                        NodesToBuffer[CurrentBufferTestPt].RemoveAt(0);
                        Indices.RemoveAt(0);
                        continue;
                    }
                    if (CurrentBuffer == this[TargetBufferInd])
                    {
                        throw new Exception("Buffer list is duplicated");
                    }
                    NewSequence.Add(TargetBufferInd);
                    Indices.Remove(TargetBufferInd);
                    NodesToBuffer[CurrentBufferTestPt].Remove(TargetBufferInd);

                    if (this[TargetBufferInd].CrvStartAndEnd[1] != CurrentBufferTestPt)
                    {
                        CurrentBufferTestPt = this[TargetBufferInd].CrvStartAndEnd[1];
                        CurrentBuffer = this[TargetBufferInd];

                        var Vec = CurrentEndPt - CurrentBuffer.StartPoint;

                        //if (Vec.Length < Util.GeneralSetting.SegUnit)
                            this[TargetBufferInd].Align(Vec);
                        //else
                        //{
                        //    this[TargetBufferInd].ExtendAdd();
                        //    Vec = CurrentEndPt - this[TargetBufferInd].StartPoint;
                        //    this[TargetBufferInd].Align(Vec);
                        //}

                        NewBufferList.Add(AddCount, this[TargetBufferInd]);
                        CurrentEndPt = this[TargetBufferInd].EndPoint;

                        AddCount++;
                    }
                    else
                    {
                        CurrentBufferTestPt = this[TargetBufferInd].CrvStartAndEnd[0];
                        this[TargetBufferInd].Reverse();
                        CurrentBuffer = this[TargetBufferInd];

                        var Vec = CurrentEndPt - CurrentBuffer.StartPoint;
                        
                            this[TargetBufferInd].Align(Vec);
                        
                        NewBufferList.Add(AddCount, this[TargetBufferInd]);
                        CurrentEndPt = this[TargetBufferInd].EndPoint;

                        AddCount++;
                    }
                    StackSe.Push(TargetBufferInd);
                    continue;
                }
                else if (NodesToBuffer[CurrentBufferTestPt].Count == 1)
                {
                    int PopIndex = StackSe.Pop();
                    if (PopIndex == StartPt.Value.Item1)
                    {
                        CurrentBuffer = this[StartPt.Value.Item1];

                        if (CurrentBuffer.CrvStartAndEnd[0] != CurrentBufferTestPt)
                        {
                            CurrentBufferTestPt = CurrentBuffer.CrvStartAndEnd[0];
                            CurrentEndPt = CurrentBuffer.StartPoint;
                        }
                        else
                        {
                            CurrentBufferTestPt = CurrentBuffer.CrvStartAndEnd[1];
                            CurrentEndPt = CurrentBuffer.EndPoint;
                        }

                        if (NodesToBuffer[CurrentBufferTestPt].Count == 1)
                        {
                            break;
                        }
                        else continue;

                    }
                    else 
                    {
                        CurrentBuffer = this[PopIndex];
                        if (CurrentBuffer.CrvStartAndEnd[0] != CurrentBufferTestPt)
                        {
                            CurrentBufferTestPt = CurrentBuffer.CrvStartAndEnd[0];
                            CurrentEndPt = CurrentBuffer.StartPoint;
                        }
                        else
                        {
                            CurrentBufferTestPt = CurrentBuffer.CrvStartAndEnd[1];
                            CurrentEndPt = CurrentBuffer.EndPoint;
                        }

                        continue;
                    }
                }
            }

            this.NodeContructable = true;
            #region TEST
            //var CurrentBuffer = this[StartPt.Value.Item1];
            //var CurrentBufferTestPt = CurrentBuffer.CrvStartAndEnd[1];
            //var StackSe = new Stack<int>();
            //StackSe.Push(StartPt.Value.Item1);

            //while (Indices.Count != 0)
            //{
            //    if (NodesToBuffer[CurrentBufferTestPt].Count > 1)    //Branch or on the way
            //    {
            //        double Min = 0;
            //        int TargetBufferInd = -1;
            //        for (int i = 0; i < NodesToBuffer[CurrentBufferTestPt].Count; i++)
            //        {
            //            if (!NewSequence.Contains(NodesToBuffer[CurrentBufferTestPt][i]))
            //            {
            //                var Len = BufferList[NodesToBuffer[CurrentBufferTestPt][i]].CurveLength;
            //                if (Len > Min)
            //                {
            //                    Min = Len;
            //                    TargetBufferInd = NodesToBuffer[CurrentBufferTestPt][i];
            //                }
            //            }
            //        }
            //        NewSequence.Add(TargetBufferInd);
            //        Indices.Remove(TargetBufferInd);
            //        NodesToBuffer[CurrentBufferTestPt].Remove(TargetBufferInd);
            //        CurrentBuffer = this[TargetBufferInd];
            //        CurrentBufferTestPt = CurrentBuffer.CrvStartAndEnd[1] != CurrentBufferTestPt ?
            //            CurrentBuffer.CrvStartAndEnd[1] : CurrentBuffer.CrvStartAndEnd[0];
            //        StackSe.Push(TargetBufferInd);
            //        continue;
            //    }
            //    else if (NodesToBuffer[CurrentBufferTestPt].Count == 1)  //End Segment
            //    {
            //        int PopIndex = StackSe.Pop();
            //        if (PopIndex == StartPt.Value.Item1)
            //        {
            //            CurrentBuffer = this[StartPt.Value.Item1];
            //            CurrentBufferTestPt = CurrentBuffer.CrvStartAndEnd[0] != CurrentBufferTestPt ?
            //                CurrentBuffer.CrvStartAndEnd[0] : CurrentBuffer.CrvStartAndEnd[1];


            //            if (NodesToBuffer[CurrentBufferTestPt].Count == 1)
            //            {
            //                break;
            //            }
            //            else continue;
            //        }
            //        else
            //        {
            //            CurrentBuffer = this[PopIndex];
            //            CurrentBufferTestPt = CurrentBuffer.CrvStartAndEnd[0] != CurrentBufferTestPt ?
            //                CurrentBuffer.CrvStartAndEnd[0] : CurrentBuffer.CrvStartAndEnd[1];

            //            continue;
            //        }
            //    }

            //}

            //var NewBufferList = new Dictionary<int, Buffer>();
            //NewBufferList.Add(0, this[NewSequence[0]]);

            //var CurrentEndPt = this[NewSequence[0]].EndPoint;
            //StackSe = new Stack<int>();
            //StackSe.Push(0);

            //NewSequence.RemoveAt(0);
            //int AddCount = 1;

            //while (NewSequence.Count != 0)
            //{
            //    var TestStartPt = this[NewSequence[0]].StartPoint;
            //    var TestEndPt = this[NewSequence[0]].EndPoint;
            //    if (CurrentEndPt.DistanceTo(TestStartPt) < Util.GeneralSetting.SegUnit*1.2)
            //    {
            //        var Vec = CurrentEndPt - TestStartPt;
            //        this[NewSequence[0]].Align(Vec);
            //        NewBufferList.Add(AddCount, this[NewSequence[0]]);
            //        StackSe.Push(AddCount);
            //        CurrentEndPt = this[NewSequence[0]].EndPoint;
            //        NewSequence.RemoveAt(0);

            //        AddCount++;
            //    }
            //    else if (CurrentEndPt.DistanceTo(TestEndPt) < Util.GeneralSetting.SegUnit*1.2)
            //    {
            //        this[NewSequence[0]].Reverse();
            //        var Vec = CurrentEndPt - TestEndPt;
            //        this[NewSequence[0]].Align(Vec);
            //        NewBufferList.Add(AddCount, this[NewSequence[0]]);
            //        StackSe.Push(AddCount);
            //        CurrentEndPt = this[NewSequence[0]].EndPoint;
            //        NewSequence.RemoveAt(0);

            //        AddCount++;
            //    }
            //    else
            //    {
            //        CurrentEndPt = NewBufferList.Values.ToList()[StackSe.Pop()].StartPoint;
            //    }
            //}
            #endregion

            if (this.Count != NewBufferList.Count)
                return false;

            this.BufferList = NewBufferList;

            ///Reset Topology
            this.BufferResetCrvStartAndEnd();
            this.NodesToBuffer = this._ConstructNodeGraph();
            this.IsSetReader = false;
            return true;
            #endregion
        }
        public void Align(Vector3d Vec)
        {
            for(int i = 0; i < Count; i++)
                BufferList[i].Align(Vec);
        }
        public List<PolylineCurve> Draw()
        {
            return this.BufferList.Select(x => x.Value.Draw()).ToList();
        }
        public List<Point3d> GetPoints()
        {
            var PtList = new List<Point3d>();
            for (int i = 0; i < Count; i++)
            {
                for (int j = 0; j < this[i].Count; j++)
                {
                    if (!PtList.Contains(this[i][j]))
                    {
                        PtList.Add(this[i][j]); 
                    }
                }
            }
            return PtList;
        }
        private void BufferResetCrvStartAndEnd()
        {
            for (int i = 0; i < Count; i++)
                this[i].ResetCrvStartAndEnd();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        private Stack<Point3d> PrePointsStack = new Stack<Point3d>();
        private Stack<Point3d> EndPointsStack = new Stack<Point3d>();
        public bool IsSetReader { get; private set; } = false;
        public bool BufferListReader(out Point3d PreviousPt, out Point3d CurrentPt, out Point3d NextPt)
        {
            #region BUFFERREADER
            if (BufferPointer == -1) this.MoveNext();
            if(!this.NodeContructable) this.NodeContructable = true;
            if (this.BufferPointer >= Count - 1 && Current.StripIndex >= Current.Count - 1)
            {
                PreviousPt = Current.Strip[Current.Count - 2];
                CurrentPt = Current.EndPoint;
                NextPt = Point3d.Unset;
                IsSetReader = true;
                return false;
            }

            var EndPtInGraph = Current.CrvStartAndEnd[1];
            var NodeCount = this.NodesToBuffer[EndPtInGraph].Count;

            if (this.BufferPointer == 0 && Current.StripIndex == -1 && this.NodesToBuffer[this[0].CrvStartAndEnd[0]].Count != 1)
            {
                Current.MoveNext();
                PreviousPt = this.Current.Privous;
                CurrentPt = this.Current.Current;
                NextPt = this.Current.Next;
                var StartNodeCount = this.NodesToBuffer[this[0].CrvStartAndEnd[0]].Count;
                for (int i = 0; i < StartNodeCount - 1; i++)
                {
                    this.PrePointsStack.Push(NextPt);
                    this.EndPointsStack.Push(EndPtInGraph);
                    this.NodesToBuffer[this[0].CrvStartAndEnd[0]].RemoveAt(0);
                }
                //Stack At Start
                return true;
            }
            if (Current.MoveNext() && !(NodeCount > 2 && Current.StripIndex == Current.Count - 1))
            {
                PreviousPt = this.Current.Privous;
                CurrentPt = this.Current.Current;
                NextPt = this.Current.Next;

                if (NextPt == Point3d.Unset)
                {
                    if (this.PrePointsStack.Count > 0 && NodeCount == 1)
                    {
                        //End of Branch
                        return true;
                    }
                    else if (this.PrePointsStack.Count > 0 && NodeCount > 1 && 
                        this.EndPointsStack.Contains(EndPtInGraph))
                    {
                        var Indices = new List<int>();
                        for (int i = 0; i < this.EndPointsStack.ToList().Count; i++)
                        {
                            if(this.EndPointsStack.ElementAt(i) == EndPtInGraph)
                                Indices.Add(i);
                        }

                        var TempEndPtsStackList = this.EndPointsStack.ToList();
                        var TempPrePtsStackList = this.PrePointsStack.ToList();
                        TempEndPtsStackList.RemoveAt(Indices[0]);
                        TempPrePtsStackList.RemoveAt(Indices[0]);

                        this.PrePointsStack = new Stack<Point3d>();
                        this.EndPointsStack = new Stack<Point3d>();

                        //TempPrePtsStackList.Reverse();
                        //TempEndPtsStackList.Reverse();

                        for (int i = 0; i < TempEndPtsStackList.Count; i++)
                        {
                            this.PrePointsStack.Push(TempPrePtsStackList[i]);
                            this.EndPointsStack.Push(TempEndPtsStackList[i]);
                        }

                        //this.PrePointsStack.Reverse();
                        //this.EndPointsStack.Reverse();

                        //Loop
                        return true;
                    }
                    else if (MoveNext() && NodeCount == 2)
                    {
                        Current.MoveNext();
                        CurrentPt = this.Current.Current;
                        NextPt = this.Current.Next;
                        //Normal Turn
                        return true;
                    }
                    else
                    {
                        BufferPointer--;
                        PreviousPt = this.Current.Privous;
                        CurrentPt = this.Current.Current;
                        NextPt = this.Current.Next;
                        this.IsSetReader = true;
                        //End
                        return true;
                    }
                }
                //Straight
                return true;
            }
            else
            {
                if (Current.StripIndex == Current.Count - 1 && NodeCount != 1)
                {

                    //Must Be Branch
                    PreviousPt = this.Current.Privous;
                    if (MoveNext())
                    {
                        Current.MoveNext();
                        CurrentPt = this.Current.Current;
                        NextPt = this.Current.Next;

                        for (int i = 0; i < NodeCount - 2; i++)
                        {
                            this.PrePointsStack.Push(PreviousPt);
                            this.EndPointsStack.Push(EndPtInGraph);
                            this.NodesToBuffer[EndPtInGraph].RemoveAt(0);
                        }
                        //Stack Push
                        return true;
                    }
                }
                else
                {
                    MoveNext();
                    Current.MoveNext();
                    PreviousPt = this.PrePointsStack.Pop();
                    this.EndPointsStack.Pop();
                    CurrentPt = this.Current.Current;
                    NextPt = this.Current.Next;
                    //Stack Pop
                    return true;
                }
            }
        
            PreviousPt = Point3d.Unset;
            CurrentPt = Point3d.Unset;
            NextPt = Point3d.Unset;
            this.IsSetReader = true;
            //Exception
            return false;
            #endregion
        }
        public bool MoveNext()
        {
            this.BufferPointer++;
            return this.BufferPointer < this.Count;
        }
        public bool ConstructNodeGraph()
        {
            if (this.NodeContructable)
            {
                this._ConstructNodeGraph();
                return true;
            }
            else
                return false;
        }
        public void Reset()
        {
            this.BufferPointer = -1;
        }
    }
}
