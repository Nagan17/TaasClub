using System.Collections.Generic;

namespace Hazari.Core
{
    /// <summary>
    /// A player's four hands in Up-Down order: Hand 1 and Hand 2 and Hand 3 are
    /// 3 cards each, Hand 4 is 4 cards. Produced by the arrangers and consumed by
    /// <see cref="UpDownValidator"/> and the showdown.
    /// </summary>
    public sealed class Arrangement
    {
        public IReadOnlyList<Card> Hand1 { get; }
        public IReadOnlyList<Card> Hand2 { get; }
        public IReadOnlyList<Card> Hand3 { get; }
        public IReadOnlyList<Card> Hand4 { get; }

        /// <summary>The four hands as one list, indexed 0..3 (Hand 1 → Hand 4).</summary>
        public IReadOnlyList<IReadOnlyList<Card>> Hands { get; }

        public Arrangement(
            IReadOnlyList<Card> hand1,
            IReadOnlyList<Card> hand2,
            IReadOnlyList<Card> hand3,
            IReadOnlyList<Card> hand4)
        {
            Hand1 = hand1;
            Hand2 = hand2;
            Hand3 = hand3;
            Hand4 = hand4;
            Hands = new[] { hand1, hand2, hand3, hand4 };
        }
    }
}
