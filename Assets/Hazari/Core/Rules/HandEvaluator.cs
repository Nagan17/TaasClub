using System;
using System.Collections.Generic;
using System.Linq;

namespace Hazari.Core
{
    /// <summary>
    /// Ranks Hazari hands. A 3-card hand becomes a <see cref="HandValue"/>
    /// directly; a 4-card hand is ranked by its best 3-card combo with the
    /// leftover card as a kicker (rules §5). Pure C# — no Unity, no state — so
    /// it is unit-testable here and reusable on the authoritative server.
    /// </summary>
    public static class HandEvaluator
    {
        public static HandValue Evaluate(IReadOnlyList<Card> cards)
        {
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            if (cards.Count == 3) return EvaluateThree(cards);
            if (cards.Count == 4) return EvaluateFour(cards);
            throw new ArgumentException("A Hazari hand has 3 or 4 cards.", nameof(cards));
        }

        private static HandValue EvaluateThree(IReadOnlyList<Card> cards)
        {
            // Rank values, highest first.
            int[] v = cards.Select(c => (int)c.Rank).OrderByDescending(x => x).ToArray();
            bool flush = cards[0].Suit == cards[1].Suit && cards[1].Suit == cards[2].Suit;

            // Trail — three of a kind.
            if (v[0] == v[1] && v[1] == v[2])
                return new HandValue(HandCategory.Trail, new[] { v[0] });

            // Sequence (pure or mixed).
            if (TryRunStrength(v, out int runStrength))
                return new HandValue(
                    flush ? HandCategory.PureSequence : HandCategory.Sequence,
                    new[] { runStrength });

            // Flush that is not a sequence.
            if (flush)
                return new HandValue(HandCategory.Flush, new[] { v[0], v[1], v[2] });

            // Pair. With v sorted descending, the two equal cards are either the
            // top pair (v0==v1, kicker v2) or the bottom pair (v1==v2, kicker v0).
            if (v[0] == v[1]) return new HandValue(HandCategory.Pair, new[] { v[0], v[2] });
            if (v[1] == v[2]) return new HandValue(HandCategory.Pair, new[] { v[1], v[0] });

            // High card.
            return new HandValue(HandCategory.HighCard, new[] { v[0], v[1], v[2] });
        }

        private static HandValue EvaluateFour(IReadOnlyList<Card> cards)
        {
            // Try every 3-of-4 subset; keep the one giving the strongest hand,
            // with the skipped card as its kicker. CompareTo (combo then kicker)
            // picks the split that maximises the combo first, then the kicker.
            HandValue best = default;
            bool haveBest = false;

            for (int skip = 0; skip < 4; skip++)
            {
                var three = new List<Card>(3);
                for (int i = 0; i < 4; i++)
                    if (i != skip) three.Add(cards[i]);

                HandValue candidate = EvaluateThree(three).WithKicker((int)cards[skip].Rank);
                if (!haveBest || candidate.CompareTo(best) > 0)
                {
                    best = candidate;
                    haveBest = true;
                }
            }

            return best;
        }

        /// <summary>
        /// If the three rank values (sorted descending) form a run, returns its
        /// strength (rules §4): A-K-Q = 12, A-2-3 = 11, K-Q-J = 10, … 4-3-2 = 1.
        /// A-2-3 is the second-highest run — the traditional Hazari ordering.
        /// </summary>
        private static bool TryRunStrength(int[] v, out int strength)
        {
            strength = 0;

            // Runs need three distinct ranks.
            if (v[0] == v[1] || v[1] == v[2]) return false;

            // A-2-3 (Ace=14, Three=3, Two=2): the special second-highest run.
            if (v[0] == 14 && v[1] == 3 && v[2] == 2)
            {
                strength = 11;
                return true;
            }

            // Any other three consecutive ranks.
            if (v[0] - v[1] == 1 && v[1] - v[2] == 1)
            {
                // A-K-Q (top = Ace) sits above A-2-3; every lower run maps
                // top-card 4..13 → strength 1..10.
                strength = v[0] == 14 ? 12 : v[0] - 3;
                return true;
            }

            return false;
        }
    }
}
