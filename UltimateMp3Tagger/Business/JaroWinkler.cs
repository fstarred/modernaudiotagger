using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltimateMusicTagger.Business
{
    public class JaroWinklerDistance
    {
        private readonly double mWeightThreshold;
        private readonly int mNumChars;

        public JaroWinklerDistance() : this(Double.PositiveInfinity, 0)
        {
            
        }

        public JaroWinklerDistance(double weightThreshold, int numChars)
        {
            mNumChars = numChars;
            mWeightThreshold = weightThreshold;
        }

        public double Distance(IEnumerable<char> cSeq1, IEnumerable<char> cSeq2)
        {
            return 1.0 - Proximity(cSeq1, cSeq2);
        }

        public double Proximity(IEnumerable<char> cSeq1, IEnumerable<char> cSeq2)
        {
            int len1 = cSeq1.Count();
            int len2 = cSeq2.Count();
            if (len1 == 0)
                return len2 == 0 ? 1.0 : 0.0;

            int searchRange = Math.Max(0, Math.Max(len1, len2) / 2 - 1);

            bool[] matched1 = new bool[len1];
            matched1 = Enumerable.Repeat(false, matched1.Length).ToArray<bool>();
            //Arrays.fill(matched1, false);
            bool[] matched2 = new bool[len2];
            //Arrays.fill(matched2, false);
            matched2 = Enumerable.Repeat(false, matched2.Length).ToArray<bool>();

            int numCommon = 0;
            for (int i = 0; i < len1; ++i)
            {
                int start = Math.Max(0, i - searchRange);
                int end = Math.Min(i + searchRange + 1, len2);
                for (int j = start; j < end; ++j)
                {
                    if (matched2[j]) continue;
                    if (cSeq1.ElementAt(i) != cSeq2.ElementAt(j))
                        continue;
                    matched1[i] = true;
                    matched2[j] = true;
                    ++numCommon;
                    break;
                }
            }
            if (numCommon == 0) return 0.0;

            int numHalfTransposed = 0;
            int z = 0;
            for (int i = 0; i < len1; ++i)
            {
                if (!matched1[i]) continue;
                while (!matched2[z]) ++z;
                if (cSeq1.ElementAt(i) != cSeq2.ElementAt(z))
                    ++numHalfTransposed;
                ++z;
            }
            // System.out.println("numHalfTransposed=" + numHalfTransposed);
            int numTransposed = numHalfTransposed / 2;

            // System.out.println("numCommon=" + numCommon
            // + " numTransposed=" + numTransposed);
            double numCommonD = numCommon;
            double weight = (numCommonD / len1
                             + numCommonD / len2
                             + (numCommon - numTransposed) / numCommonD) / 3.0;

            if (weight <= mWeightThreshold) return weight;
            int max = Math.Min(mNumChars, Math.Min(cSeq1.Count(), cSeq2.Count()));
            int pos = 0;
            while (pos < max && cSeq1.ElementAt(pos) == cSeq2.ElementAt(pos))
                ++pos;
            if (pos == 0) return weight;
            return weight + 0.1 * pos * (1.0 - weight);

        }

        //public readonly JaroWinklerDistance JARO_DISTANCE = new JaroWinklerDistance();

        //public readonly JaroWinklerDistance JARO_WINKLER_DISTANCE = new JaroWinklerDistance(0.70, 4);
    }
}
