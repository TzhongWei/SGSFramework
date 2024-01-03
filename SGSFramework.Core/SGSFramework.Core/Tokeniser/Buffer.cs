using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace SGSFramework.Core.Tokeniser
{
    public class Buffer : IEquatable<Buffer>
    {
        internal int StripIndex { get; private set; } = -1;
        public Curve Crv { get; private set; }
        public Point3d[] Strip { get; private set; } = new Point3d[0];
        public Point3d Current { get { return this[StripIndex]; } }
        public Point3d Next { get 
            {
                if (StripIndex + 1 < Count) return this[StripIndex + 1];
                else return Point3d.Unset;
            } 
        }
        public Point3d Privous { get 
            { 
            if(StripIndex > 0) return this[StripIndex - 1];
            else return Point3d.Unset;
            } 
        }
        public readonly int Count = 0;
        public double CurveLength { get; }
        public Point3d StartPoint { get { return this[0]; } }
        public Point3d EndPoint { get { return this[Count - 1]; } }
        public Point3d[] CrvStartAndEnd { get; private set; }
        public void Align(Vector3d Vec)
        {
            for (int i = 0; i < Count; i++)
                this.Strip[i] += Vec;
            this.Crv.Translate(Vec);
        }
        public void Reverse()
        {
            var TempStrip = new Point3d[this.Count];
            for (int i = 0; i < Count; i++)
            {
                TempStrip[i] = this.Strip[Count - i-1];
            }
            this.Strip = TempStrip;

            var TempPt = this.CrvStartAndEnd[0];
            CrvStartAndEnd[0] = CrvStartAndEnd[1];
            CrvStartAndEnd[1] = TempPt;
        }
        public Point3d this[int Index]
        {
            get
            {
                if (Index < Strip.Length && Index >= 0)
                    return Strip[Index];
                else
                    throw new IndexOutOfRangeException();
            }
        }
        public Buffer(Line line)
        {
            var LNCrv = new LineCurve(line);
            Strip = LNCrv.DivideEquidistant((double)Util.GeneralSetting.SegUnit).Select(x => Util.Util.Round(x)).ToArray();
            if (Strip.Last().DistanceTo(LNCrv.PointAtEnd) > Util.GeneralSetting.SegUnit * 0.5)
            {
                var TempStripList = Strip.ToList();
                var Vec = Strip.Last() - Strip.First();
                Vec.Unitize();
                TempStripList.Add(Strip.Last() + Vec * Util.GeneralSetting.SegUnit);
                Strip = TempStripList.ToArray();
            }
            this.Crv = LNCrv;
            this.Count = Strip.Length;
            CurveLength = LNCrv.PointAtStart.DistanceTo(LNCrv.PointAtEnd);
            this.CrvStartAndEnd = new Point3d[2] { Util.Util.Round(Crv.PointAtStart), Util.Util.Round(Crv.PointAtEnd) };
        }
        public Buffer(Curve crv)
        {
            Strip = crv.DivideEquidistant((double)Util.GeneralSetting.SegUnit).Select(x => Util.Util.Round(x)).ToArray();
            if (Strip.Last().DistanceTo(crv.PointAtEnd) > Util.GeneralSetting.SegUnit * 0.5)
            {
                var TempStripList = Strip.ToList();
                TempStripList.Add(Strip.Last() + crv.TangentAtEnd * Util.GeneralSetting.SegUnit);
                Strip = TempStripList.ToArray();
            }
            this.Crv = crv;
            this.Count = Strip.Length;
            CurveLength = crv.GetLength();
            this.CrvStartAndEnd = new Point3d[2] { Crv.PointAtStart, Crv.PointAtEnd };
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        public bool MoveNext()
        {
            this.StripIndex++;
            return this.StripIndex < this.Count;
        }
        public void Reset()
        {
            this.StripIndex = -1;
        }
        public void ResetCrvStartAndEnd()
        {
            this.CrvStartAndEnd[0] = this.StartPoint;
            this.CrvStartAndEnd[1] = this.EndPoint;
        }
        public PolylineCurve Draw()
        {
            return new PolylineCurve(Strip);
        }
        public void ExtendAdd()
        {
            var TempStrip = this.Strip.ToList();
            var Vec = this.Crv.TangentAtEnd;
            var TempEnd = this.EndPoint + Vec * Util.GeneralSetting.SegUnit;
            TempStrip.Add(TempEnd);
            this.Strip = TempStrip.ToArray();
        }
        public static bool operator==(Buffer A, Buffer B)
        {
            if (A.Count != B.Count)
                return false;
            for (int i = 0; i < A.Count; i++)
            {
                if (A[i] != B[i])
                    return false;
            }
            return true;
        }
        public static bool operator !=(Buffer A, Buffer B)
        {
            return !(A == B);
        }
        public override bool Equals(object other)
        {
            return other is Buffer && Equals((Buffer)other);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public bool Equals(Buffer other)
        {
            if(this.Count != other.Count)
                return false;
            for (int i = 0; i < other.Count; i++)
            {
                if (this[i] != other[i])
                    return false;
            }
            return true;
        }
    }
}
