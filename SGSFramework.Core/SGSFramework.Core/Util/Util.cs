using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using SGSFramework.Core.Tokeniser;

namespace SGSFramework.Core.Util
{
    internal static partial class Util
    {
        /// <summary>
        /// Clean the space in a sequence
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static string CleanSequence(string sequence)
        {
            while (sequence[0] == ' ')
                sequence = sequence.Remove(0, 1);
            while (sequence[sequence.Length - 1] == ' ')
                sequence = sequence.Remove(sequence.Length - 1);
            return sequence;
        }
        /// <summary>
        /// The seed for random setting for a gibberish
        /// </summary>
        private static int Seed = 0;
        /// <summary>
        /// Set a gibberish string for BlockName or LayerName
        /// </summary>
        /// <returns></returns>
        public static string SetGibberish()
        {
            char[] alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
            string gibberish = string.Empty;
            for (int i = 0; i < 8; i++)
            {
                Random random = new Random(Seed + System.DateTime.Now.Millisecond);
                int ind = random.Next(0, alphabets.Length);
                gibberish += alphabets[ind];
                Seed++;
            }
            return gibberish;
        }
        /// <summary>
        /// Test if all curves or segments are longer than SegUnit setting
        /// if true, all curves longer than the unit
        /// else false.
        /// </summary>
        /// <param name="curves"></param> Curves for test
        /// <returns></returns>
        public static bool TestCurve(IEnumerable<Curve> curves)
        {
            List<Curve> Crvs = new List<Curve>();
            foreach (var curve in curves) 
            {
                Crvs.AddRange(curve.DuplicateSegments());
            }
            for (int i = 0; i < Crvs.Count; i++)
            {
                if (Crvs[i].GetLength() < GeneralSetting.SegUnit * 1.2)
                    return false;
            }
            return true;
        }
        public static bool TestCurve(Curve Crv)
        {
            var Crvs = Crv.DuplicateSegments();
            return TestCurve(Crvs);
           
        }
        public static PolylineCurve GetRectangle(Point3d SetCentre, Vector3d Dir)
        {
            var Rec = SetRectanle();
            if (!Dir.IsUnitVector)
                Dir.Unitize();
            Rec.Rotate(Vector3d.VectorAngle(Vector3d.XAxis, Dir), Vector3d.ZAxis, Point3d.Origin);
            Rec.Translate(new Vector3d(SetCentre));
            return Rec;
        }
        private static PolylineCurve SetRectanle()
        {
            var FirstPt = new Point3d((double)-GeneralSetting.SegUnit * 0.5, (double)-GeneralSetting.SegUnit * 0.5, 0);
            var SecondPt = new Point3d((double)-GeneralSetting.SegUnit * 0.5, (double)GeneralSetting.SegUnit * 0.5, 0);
            var ThirdPt = new Point3d((double)GeneralSetting.SegUnit * 0.5, (double)GeneralSetting.SegUnit * 0.5, 0);
            var ForthPt = new Point3d((double)GeneralSetting.SegUnit * 0.5, (double)-GeneralSetting.SegUnit * 0.5, 0);
            return new PolylineCurve(new[] { FirstPt, SecondPt, ThirdPt, ForthPt, FirstPt });
        }
        public static Point3d Round(Point3d Pt, int tolerance = 5)
        {
            return new Point3d(Math.Round(Pt.X, tolerance), Math.Round(Pt.Y, tolerance), Math.Round(Pt.Z, tolerance));
        }
        public static Vector3d Round(Vector3d Vec, int tolerance = 5)
        {
            return new Vector3d(Math.Round(Vec.X, tolerance), Math.Round(Vec.Y, tolerance), Math.Round(Vec.Z, tolerance));
        }
        public static string Match(PointsReader Reader)
        {
            var Previous = Reader.Backward;
            var Current = Reader.Current;
            var Next = Reader.Forward;

            var V1 = Round(Current - Previous);
            var V2 = Round(Next - Current);

            if (V1 == V2)
                return "S";
            else
            {
                V1.Unitize();
                V2.Unitize();
                //Only 90 degree
                double Angle = Vector3d.VectorAngle(V1, V2, Plane.WorldXY);
                if (Angle < Math.PI)
                {
                    return "L";
                }
                else
                {
                    return "R";
                }
            }
        }
        public static string CharSpace(string words, int totalLength = 15)
        {
            if (words.Length > totalLength)
                return words;
            string result = "";
            for (int i = 0; i < totalLength; i++)
            {
                if (i < words.Length)
                    result += words[i].ToString();
                else
                    result += " ";
            }
            return result;
        }
        public static string CharSpace(IEnumerable<string> words, int totalLength = 50)
        {
            return CharSpace(string.Join(" ", words), totalLength);
        }
    }

#if X64
    [Obsolete("The version test", true)]
    class foo
    { 
    }
#endif
}
