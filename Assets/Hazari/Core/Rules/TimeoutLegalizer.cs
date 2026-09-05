using System.Collections.Generic;
using System.Linq;

namespace Hazari.Core
{
    /// <summary>
    /// The timeout fallback (rules §9). When a turn timer expires, this submits a
    /// LEGAL arrangement that is deliberately WEAK, so timing out is never better
    /// than finishing by hand. It keeps any hands the player already completed
    /// (their built trail stays a trail) and completes the rest as weakly as
    /// legally possible — it never searches the leftover cards for combos.
    ///
    /// It is the opposite of <see cref="StrongArranger"/>: same search, but the
    /// completed hands are locked and the objective is flipped to minimise.
    /// (The Combinations / PartitionIntoThree helpers mirror StrongArranger's;
    /// a later cleanup could extract them into a shared helper.)
    /// </summary>
    public static class TimeoutLegalizer
    {
        /// <summary>
        /// thirteen: the player's full hand.
        /// completedHands: the groups the player finished before the timer expired
        ///   (each 3 or 4 cards, disjoint, a subset of thirteen). Empty if none.
        /// Returns the weakest legal arrangement that keeps those groups intact.
        /// </summary>
        public static Arrangement Legalize(
            IReadOnlyList<Card> thirteen,
            IReadOnlyList<IReadOnlyList<Card>> completedHands)
        {
            var cards = thirteen.ToArray();
            if (cards.Length != 13)
                throw new System.ArgumentException("Expected 13 cards.", nameof(thirteen));

            int[] locks = (completedHands ?? System.Array.Empty<IReadOnlyList<Card>>())
                .Select(group => MaskOf(group, cards))
                .Where(mask => mask != 0)
                .ToArray();

            // Honour the locks first; if that leaves no legal arrangement, relax them.
            return SearchWeakest(cards, locks) ?? SearchWeakest(cards, System.Array.Empty<int>());
        }

        private static Arrangement SearchWeakest(Card[] cards, int[] locks)
        {
            var cache = new Dictionary<int, HandValue>();
            HandValue Eval(int[] idx)
            {
                int mask = Mask(idx);
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
            Arrangement weakest = null;
            HandValue[] weakestScore = null;

            foreach (int[] four in Combinations(all, 4))
            {
                int fourMask = Mask(four);
                HandValue h4 = Eval(four);
                int[] nine = all.Where(i => (fourMask & (1 << i)) == 0).ToArray();

                foreach ((int[] g1, int[] g2, int[] g3) in PartitionIntoThree(nine))
                {
                    // Every locked group must sit entirely inside one of the hands.
                    if (!LocksRespected(locks, fourMask, Mask(g1), Mask(g2), Mask(g3)))
                        continue;

                    var threes = new List<(HandValue v, int[] idx)>
                    {
                        (Eval(g1), g1), (Eval(g2), g2), (Eval(g3), g3)
                    };
                    threes.Sort((x, y) => y.v.CompareTo(x.v)); // Hand 1..3

                    if (threes[2].v.CompareComboTo(h4) < 0) continue; // Up-Down seam

                    var score = new[] { threes[0].v, threes[1].v, threes[2].v, h4 };
                    if (weakest == null || IsWeaker(score, weakestScore))
                    {
                        weakestScore = score;
                        weakest = new Arrangement(
                            ToCards(cards, threes[0].idx),
                            ToCards(cards, threes[1].idx),
                            ToCards(cards, threes[2].idx),
                            ToCards(cards, four));
                    }
                }
            }

            return weakest;
        }

        private static bool LocksRespected(int[] locks, int h1, int h2, int h3, int h4)
        {
            foreach (int lm in locks)
            {
                bool inside = (lm & h1) == lm || (lm & h2) == lm
                           || (lm & h3) == lm || (lm & h4) == lm;
                if (!inside) return false;
            }
            return true;
        }

        /// <summary>Candidate wins if it is weaker, compared Hand 1 → Hand 4.</summary>
        private static bool IsWeaker(HandValue[] candidate, HandValue[] best)
        {
            for (int i = 0; i < candidate.Length; i++)
            {
                int c = candidate[i].CompareTo(best[i]);
                if (c != 0) return c < 0;
            }
            return false;
        }

        private static int Mask(int[] idx)
        {
            int m = 0;
            foreach (int i in idx) m |= 1 << i;
            return m;
        }

        private static int MaskOf(IReadOnlyList<Card> group, Card[] cards)
        {
            int m = 0;
            foreach (Card c in group)
            {
                int idx = System.Array.IndexOf(cards, c);
                if (idx >= 0) m |= 1 << idx;
            }
            return m;
        }

        private static List<Card> ToCards(Card[] cards, int[] idx)
        {
            var list = new List<Card>(idx.Length);
            foreach (int i in idx) list.Add(cards[i]);
            return list;
        }

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
