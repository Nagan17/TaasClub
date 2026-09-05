using System.Collections.Generic;

namespace Hazari.Core
{
    /// <summary>
    /// Enforces the Up-Down rule (rules §8): a player's four hands must be
    /// arranged so that Hand 1 ≥ Hand 2 ≥ Hand 3 ≥ Hand 4 by combo strength.
    /// Comparison is combo-only, so an equal combo passes ≥ and the 4-card
    /// hand's kicker never makes it "greater" than an equal 3-card Hand 3.
    /// </summary>
    public static class UpDownValidator
    {
        /// <summary>
        /// hands must be four groups sized 3, 3, 3, 4 (Hand 1 → Hand 4).
        /// Returns true only if the sizes are right and the order is legal.
        /// </summary>
        public static bool IsLegal(IReadOnlyList<IReadOnlyList<Card>> hands)
        {
            if (hands == null || hands.Count != 4) return false;
            if (hands[0].Count != 3 || hands[1].Count != 3 ||
                hands[2].Count != 3 || hands[3].Count != 4)
                return false;

            HandValue h1 = HandEvaluator.Evaluate(hands[0]);
            HandValue h2 = HandEvaluator.Evaluate(hands[1]);
            HandValue h3 = HandEvaluator.Evaluate(hands[2]);
            HandValue h4 = HandEvaluator.Evaluate(hands[3]);

            return h1.CompareComboTo(h2) >= 0
                && h2.CompareComboTo(h3) >= 0
                && h3.CompareComboTo(h4) >= 0;
        }
    }
}
