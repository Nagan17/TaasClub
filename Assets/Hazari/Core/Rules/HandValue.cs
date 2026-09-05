using System;

namespace Hazari.Core
{
    /// <summary>Hand categories, low to high. Higher beats lower (rules §4).</summary>
    public enum HandCategory
    {
        HighCard = 0,
        Pair = 1,
        Flush = 2,
        Sequence = 3,
        PureSequence = 4,
        Trail = 5
    }

    /// <summary>
    /// The evaluated strength of a hand, as a totally-ordered value.
    /// Ordering is: <see cref="Category"/> first, then the within-category
    /// <c>tiebreak</c> compared element by element, then <see cref="Kicker"/>
    /// (only non-zero for 4-card hands — the card left over after the best
    /// 3-card combo, rules §5).
    /// </summary>
    public readonly struct HandValue : IComparable<HandValue>, IEquatable<HandValue>
    {
        public HandCategory Category { get; }
        private readonly int[] _tiebreak;
        public int Kicker { get; }

        public HandValue(HandCategory category, int[] tiebreak, int kicker = 0)
        {
            Category = category;
            _tiebreak = tiebreak ?? Array.Empty<int>();
            Kicker = kicker;
        }

        /// <summary>Returns a copy of this value with a kicker attached (rules §5).</summary>
        public HandValue WithKicker(int kicker) => new HandValue(Category, _tiebreak, kicker);

        /// <summary>
        /// Compares by combo only — category then tiebreak — ignoring the kicker.
        /// This is the comparison the Up-Down rule uses (rules §8), so an equal
        /// combo satisfies "Hand n ≥ Hand n+1" even if the 4-card hand has a kicker.
        /// </summary>
        public int CompareComboTo(HandValue other)
        {
            int c = Category.CompareTo(other.Category);
            if (c != 0) return c;

            int len = Math.Min(_tiebreak.Length, other._tiebreak.Length);
            for (int i = 0; i < len; i++)
            {
                c = _tiebreak[i].CompareTo(other._tiebreak[i]);
                if (c != 0) return c;
            }
            return 0;
        }

        /// <summary>
        /// Full comparison — combo first, then kicker. This is the comparison the
        /// showdown uses to rank hands of the same size (rules §6). Exact ties
        /// (return value 0) are resolved by play order outside the evaluator (§7).
        /// </summary>
        public int CompareTo(HandValue other)
        {
            int c = CompareComboTo(other);
            if (c != 0) return c;
            return Kicker.CompareTo(other.Kicker);
        }

        public bool Equals(HandValue other) => CompareTo(other) == 0;
        public override bool Equals(object obj) => obj is HandValue hv && Equals(hv);

        public override int GetHashCode()
        {
            int h = (int)Category * 397 + Kicker;
            foreach (int t in _tiebreak) h = h * 397 + t;
            return h;
        }

        public override string ToString()
            => Kicker == 0
                ? $"{Category} [{string.Join(",", _tiebreak)}]"
                : $"{Category} [{string.Join(",", _tiebreak)}] +{Kicker}";
    }
}
