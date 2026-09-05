using System;

namespace Hazari.Core
{
    /// <summary>
    /// An immutable playing card. This is a value type: two cards are equal
    /// when both rank and suit match.
    /// </summary>
    public readonly struct Card : IEquatable<Card>
    {
        public Rank Rank { get; }
        public Suit Suit { get; }

        public Card(Rank rank, Suit suit)
        {
            Rank = rank;
            Suit = suit;
        }

        /// <summary>
        /// Point value under Hazari scoring (rules §2): A, K, Q, J and 10 are
        /// worth 10 points each; 2 through 9 are worth 5 points each.
        /// </summary>
        public int PointValue => Rank >= Rank.Ten ? 10 : 5;

        public bool Equals(Card other) => Rank == other.Rank && Suit == other.Suit;

        public override bool Equals(object obj) => obj is Card other && Equals(other);

        public override int GetHashCode() => ((int)Rank << 2) | (int)Suit;

        public override string ToString() => $"{RankSymbol()}{SuitSymbol()}";

        private string RankSymbol() => Rank switch
        {
            Rank.Ten => "10",
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            Rank.Ace => "A",
            _ => ((int)Rank).ToString()
        };

        private char SuitSymbol() => Suit switch
        {
            Suit.Clubs => 'C',
            Suit.Diamonds => 'D',
            Suit.Hearts => 'H',
            Suit.Spades => 'S',
            _ => '?'
        };
    }
}
