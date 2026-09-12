using System;
using System.Collections.Generic;

namespace Hazari.Core
{
    /// <summary>
    /// Interprets the 13 cards in the order the player physically arranged them
    /// at submit time. Scans left to right for any 3 consecutive cards that
    /// already form Pair-or-better — a combo the player clearly built on
    /// purpose, wherever it sits in the row — locks it in as one hand, and
    /// fills whatever's left into the remaining slots in the order given.
    /// Same "never search leftovers, never invent a combo" principle as
    /// <see cref="TimeoutLegalizer"/>, but without its weakest-possible penalty
    /// search — this never punishes, it just reads what's there.
    /// </summary>
    public static class SubmissionParser
    {
        public static Arrangement Parse(IReadOnlyList<Card> thirteenInOrder)
        {
            if (thirteenInOrder == null || thirteenInOrder.Count != 13)
                throw new ArgumentException("Expected 13 cards.", nameof(thirteenInOrder));

            var remaining = new List<Card>(thirteenInOrder);
            var threes = new List<List<Card>>();

            // Scan for combos the player already built, wherever they sit.
            int i = 0;
            while (i + 3 <= remaining.Count && threes.Count < 3)
            {
                List<Card> window = remaining.GetRange(i, 3);
                if (HandEvaluator.Evaluate(window).Category > HandCategory.HighCard)
                {
                    threes.Add(window);
                    remaining.RemoveRange(i, 3);
                    // Don't advance i — the next three cards just shifted here.
                }
                else
                {
                    i++;
                }
            }

            // Dumb-fill: whatever's left, in order, fills the rest — no searching.
            int cursor = 0;
            while (threes.Count < 3)
            {
                threes.Add(remaining.GetRange(cursor, 3));
                cursor += 3;
            }
            List<Card> four = remaining.GetRange(cursor, remaining.Count - cursor);

            // Sort the three groups strongest-first so Hand1 >= Hand2 >= Hand3
            // is automatic (same trick as StrongArranger and TimeoutLegalizer).
            threes.Sort((a, b) => HandEvaluator.Evaluate(b).CompareTo(HandEvaluator.Evaluate(a)));

            return new Arrangement(threes[0], threes[1], threes[2], four);
        }
    }
}