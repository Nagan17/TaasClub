using System;
using System.Collections.Generic;
using System.Linq;

namespace Hazari.Core
{
    /// <summary>
    /// Interprets the 13 cards in the order the player physically arranged them
    /// at submit time. Finds the strongest 3-card combo among cards that sit
    /// close together in that order (within MaxCardSpread positions of each
    /// other) — close enough that it reads as something the player actually
    /// grouped, not the detector reaching across the whole hand to build a
    /// combo they never assembled. Locks that combo in as one hand, and fills
    /// whatever's left into the remaining slots in the order given.
    /// </summary>
    public static class SubmissionParser
    {
        /// <summary>
        /// Max allowed distance, in original hand-order positions, between the
        /// two farthest-apart cards of a detected combo. 2 means fully
        /// adjacent (3 cards, no gaps) — raise it to allow a stray card or two
        /// sitting between the ones that were actually grouped.
        /// </summary>
        public const int MaxCardSpread = 2;

        public static Arrangement Parse(IReadOnlyList<Card> thirteenInOrder)
        {
            if (thirteenInOrder == null || thirteenInOrder.Count != 13)
                throw new ArgumentException("Expected 13 cards.", nameof(thirteenInOrder));

            // Track each card's original position so "close together" always
            // means close in the hand as the player laid it out, not close in
            // whatever happens to be left after earlier combos were pulled out.
            var remaining = thirteenInOrder
                .Select((card, index) => (card, index))
                .ToList();

            var threes = new List<List<Card>>();

            while (threes.Count < 3)
            {
                var best = BestThreeCardCombo(remaining);
                if (best == null) break;

                threes.Add(best.Value.cards);
                remaining.RemoveAll(x => best.Value.indices.Contains(x.index));
            }

            // Dumb-fill: whatever's left, in its original relative order, fills
            // the rest — no searching, no rearranging.
            var leftoverCards = remaining.Select(x => x.card).ToList();
            int cursor = 0;
            while (threes.Count < 3)
            {
                threes.Add(leftoverCards.GetRange(cursor, 3));
                cursor += 3;
            }
            List<Card> four = leftoverCards.GetRange(cursor, leftoverCards.Count - cursor);

            // Sort the three groups strongest-first so Hand1 >= Hand2 >= Hand3
            // is automatic (same trick as StrongArranger and TimeoutLegalizer).
            threes.Sort((a, b) => HandEvaluator.Evaluate(b).CompareTo(HandEvaluator.Evaluate(a)));

            return new Arrangement(threes[0], threes[1], threes[2], four);
        }

        /// <summary>
        /// The strongest 3-card combo among cards within MaxCardSpread of each
        /// other in original hand order, or null if nothing in range beats High
        /// Card. At most C(13,3)=286 combos to check, most skipped instantly by
        /// the spread filter.
        /// </summary>
        private static (List<Card> cards, int[] indices)? BestThreeCardCombo(
            List<(Card card, int index)> cards)
        {
            (List<Card> cards, int[] indices)? best = null;
            HandValue bestValue = default;

            for (int a = 0; a < cards.Count - 2; a++)
            for (int b = a + 1; b < cards.Count - 1; b++)
            for (int c = b + 1; c < cards.Count; c++)
            {
                if (cards[c].index - cards[a].index > MaxCardSpread) continue;

                var candidate = new List<Card> { cards[a].card, cards[b].card, cards[c].card };
                HandValue value = HandEvaluator.Evaluate(candidate);

                if (value.Category > HandCategory.HighCard &&
                    (best == null || value.CompareTo(bestValue) > 0))
                {
                    best = (candidate, new[] { cards[a].index, cards[b].index, cards[c].index });
                    bestValue = value;
                }
            }

            return best;
        }
    }
}
