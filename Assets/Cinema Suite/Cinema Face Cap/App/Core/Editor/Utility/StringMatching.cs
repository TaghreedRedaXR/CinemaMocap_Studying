using System.Linq;

namespace CinemaSuite.Core.Utility
{
    public static class StringMatching
    {
        public static double DiceCoefficient(this string input, string comparedTo)
        {
            var ngrams = input.ToBiGrams();
            var compareToNgrams = comparedTo.ToBiGrams();
            return ngrams.DiceCoefficient(compareToNgrams);
        }

        public static double DiceCoefficient(this string[] nGrams, string[] compareToNGrams)
        {
            int matches = 0;
            foreach (var nGram in nGrams)
            {
                if (compareToNGrams.Any(x => x == nGram)) matches++;
            }
            if (matches == 0) return 0.0d;
            double totalBigrams = nGrams.Length + compareToNGrams.Length;
            return (2 * matches) / totalBigrams;
        }

        public static string[] ToBiGrams(this string input)
        {
            input = SinglePercent + input + SinglePound;
            return ToNGrams(input, 2);
        }

        public static string[] ToTriGrams(this string input)
        {
            input = DoublePercent + input + DoublePount;
            return ToNGrams(input, 3);
        }

        private static string[] ToNGrams(string input, int nLength)
        {
            int itemsCount = input.Length - 1;
            string[] ngrams = new string[input.Length - 1];
            for (int i = 0; i < itemsCount; i++) ngrams[i] = input.Substring(i, nLength);
            return ngrams;
        }

        private const string SinglePercent = "%";
        private const string SinglePound = "#";
        private const string DoublePercent = "&&";
        private const string DoublePount = "##";




        /// <summary>
        /// Levenshtein Distance algorithm with transposition. <br />
        /// A value of 1 or 2 is okay, 3 is iffy and greater than 4 is a poor match
        /// </summary>
        /// <param name="input"></param>
        /// <param name="comparedTo"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        public static int LevenshteinDistance(this string input, string comparedTo, bool caseSensitive = false)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(comparedTo)) return -1;
            if (!caseSensitive)
            {
                input = input.ToLower();
                comparedTo = comparedTo.ToLower();
            }
            int inputLen = input.Length;
            int comparedToLen = comparedTo.Length;

            int[,] matrix = new int[inputLen, comparedToLen];

            //initialize
            for (int i = 0; i < inputLen; i++) matrix[i, 0] = i;
            for (int i = 0; i < comparedToLen; i++) matrix[0, i] = i;

            //analyze
            for (int i = 1; i < inputLen; i++)
            {
                var si = input[i - 1];
                for (int j = 1; j < comparedToLen; j++)
                {
                    var tj = comparedTo[j - 1];
                    int cost = (si == tj) ? 0 : 1;

                    var above = matrix[i - 1, j];
                    var left = matrix[i, j - 1];
                    var diag = matrix[i - 1, j - 1];
                    var cell = FindMinimum(above + 1, left + 1, diag + cost);

                    //transposition
                    if (i > 1 && j > 1)
                    {
                        var trans = matrix[i - 2, j - 2] + 1;
                        if (input[i - 2] != comparedTo[j - 1]) trans++;
                        if (input[i - 1] != comparedTo[j - 2]) trans++;
                        if (cell > trans) cell = trans;
                    }
                    matrix[i, j] = cell;
                }
            }
            return matrix[inputLen - 1, comparedToLen - 1];
        }

        private static int FindMinimum(params int[] p)
        {
            if (null == p) return int.MinValue;
            int min = int.MaxValue;
            for (int i = 0; i < p.Length; i++)
            {
                if (min > p[i]) min = p[i];
            }
            return min;
        }
    }

}
