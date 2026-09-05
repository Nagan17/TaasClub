using System.Collections.Generic;
using System.Linq;

namespace Hazari.Core
{
    /// <summary>
    /// Finds a strong, legal Up-Down arrangement of 13 cards. Used by bots (to
    /// play to win) and by the hint assistant (to suggest groupings) — rules §11.
    ///
    /// Approach: the Up-Down rule is about order, so for any grouping we place the
    /// 4-card group in Hand 4 and sort the three 3-card groups strongest-first.
    /// Hand 1 ≥ Hand 2 ≥ Hand 3 is then automatic; the only remaining check is
    /// Hand 3 ≥ Hand 4. We try every such grouping and keep the strongest legal one.
    ///
    /// "Strongest" here means: best Hand 1, then best Hand 2, then Hand 3 — stack
    /// your power up top. This is a tunable heuristic, not proven-optimal play
    /// (rules §12); making bots smarter later means changing only the scoring.
    /// </summary>
    public static class StrongArranger
    {
        /// <summary>
        /// Returns the strongest legal arrangement of the 13 cards, or null in the
        /// (not expected) event that no legal arrangement exists.
        /// </summary>
        public static Arrangement FindBest(IReadOnlyList<Card> thirteen)
        {
            var cards = thirteen.ToArray();
            if (cards.Length != 13)
                throw new System.ArgumentException("Expected 13 cards.", nameof(thirteen));

            // Memoized evaluation: rank each group of cards once, keyed by a 13-bit
            // mask of which cards it holds. The same trio shows up in many splits.
            var cache = new Dictionary<int, HandValue>();
            HandValue Eval(int[] idx)
            {
                int mask = 0;
                foreach (int i in idx) mask |= 1 << i;
                if (!cache.TryGetValue(mask, out HandValue v))
                {
                    var group = new Card[idx.Length];
                    for (int k = 0; k < idx.Length; k++) group[k] = cards[idx[k]];
                    v = HandEvaluator.Evaluate(group);
                    cache[mask] = v;
                }
                return v;
            }

            int[] all = Enumerable.Range(0, 13).ToArray();

            Arrangement best = null;
            HandValue[] bestScore = null;

            // 1) Choose the four cards for Hand 4.
            foreach (int[] four in Combinations(all, 4))
            {
                HandValue h4 = Eval(four);
                int[] nine = all.Where(i => !four.Contains(i)).ToArray();

                // 2) Split the other nine into three groups of 3.
                foreach ((int[] g1, int[] g2, int[] g3) in PartitionIntoThree(nine))
                {
                    // 3) Sort the three groups strongest-first -> Hand 1, 2, 3.
                    var threes = new List<(HandValue v, int[] idx)>
                    {
                        (Eval(g1), g1), (Eval(g2), g2), (Eval(g3), g3)
                    };
                    threes.Sort((x, y) => y.v.CompareTo(x.v)); // descending

                    // 4) Up-Down is satisfied except for the Hand 3 >= Hand 4 seam.
                    if (threes[2].v.CompareComboTo(h4) < 0) continue;

                    // 5) Score: strongest Hand 1, then Hand 2, then Hand 3, then Hand 4.
                    var score = new[] { threes[0].v, threes[1].v, threes[2].v, h4 };
                    if (best == null || IsStronger(score, bestScore))
                    {
                        bestScore = score;
                        best = new Arrangement(
                            ToCards(cards, threes[0].idx),
                            ToCards(cards, threes[1].idx),
                            ToCards(cards, threes[2].idx),
                            ToCards(cards, four));
                    }
                }
            }

            return best;
        }

        /// <summary>True if candidate beats best, compared Hand 1 → Hand 4.</summary>
        private static bool IsStronger(HandValue[] candidate, HandValue[] best)
        {
            for (int i = 0; i < candidate.Length; i++)
            {
                int c = candidate[i].CompareTo(best[i]);
                if (c != 0) return c > 0;
            }
            return false;
        }

        private static List<Card> ToCards(Card[] cards, int[] idx)
        {
            var list = new List<Card>(idx.Length);
            foreach (int i in idx) list.Add(cards[i]);
            return list;
        }

        /// <summary>Every k-card subset of the given indices, in lexicographic order.</summary>
        private static IEnumerable<int[]> Combinations(int[] items, int k)
        {
            int n = items.Length;
            int[] pick = Enumerable.Range(0, k).ToArray();
            while (true)
            {
                var combo = new int[k];
                for (int i = 0; i < k; i++) combo[i] = items[pick[i]];
                yield return combo;

                int p = k - 1;
                while (p >= 0 && pick[p] == n - k + p) p--;
                if (p < 0) yield break;
                pick[p]++;
                for (int i = p + 1; i < k; i++) pick[i] = pick[i - 1] + 1;
            }
        }

        /// <summary>
        /// Splits nine indices into three unordered groups of 3. Canonical form:
        /// each group is anchored by its smallest still-unused index, so every
        /// distinct split is produced exactly once (280 of them).
        /// </summary>
        private static IEnumerable<(int[], int[], int[])> PartitionIntoThree(int[] nine)
        {
            int[] rest8 = nine.Skip(1).ToArray();
            foreach (int[] pick1 in Combinations(rest8, 2))
            {
                var g1 = new[] { nine[0], pick1[0], pick1[1] };
                int[] remaining6 = nine.Where(x => !g1.Contains(x)).ToArray();

                int[] rest5 = remaining6.Skip(1).ToArray();
                foreach (int[] pick2 in Combinations(rest5, 2))
                {
                    var g2 = new[] { remaining6[0], pick2[0], pick2[1] };
                    int[] g3 = remaining6.Where(x => !g2.Contains(x)).ToArray();
                    yield return (g1, g2, g3);
                }
            }
        }
    }
}
