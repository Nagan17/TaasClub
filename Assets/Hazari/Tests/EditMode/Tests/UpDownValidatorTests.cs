using System.Collections.Generic;
using NUnit.Framework;
using Hazari.Core;

namespace Hazari.Tests
{
    public class UpDownValidatorTests
    {
        private static Card C(Rank r, Suit s) => new Card(r, s);
        private static List<Card> H(params Card[] cards) => new List<Card>(cards);

        [Test]
        public void LegalDescendingArrangement_IsLegal()
        {
            var hands = new List<IReadOnlyList<Card>>
            {
                H(C(Rank.Ace, Suit.Spades), C(Rank.King, Suit.Spades), C(Rank.Queen, Suit.Spades)),   // pure seq (top)
                H(C(Rank.Queen, Suit.Hearts), C(Rank.Jack, Suit.Hearts), C(Rank.Ten, Suit.Hearts)),   // pure seq (weaker)
                H(C(Rank.Nine, Suit.Clubs), C(Rank.Nine, Suit.Diamonds), C(Rank.Two, Suit.Clubs)),     // pair
                H(C(Rank.King, Suit.Diamonds), C(Rank.Eight, Suit.Spades), C(Rank.Six, Suit.Hearts), C(Rank.Three, Suit.Diamonds)) // high card, no hidden run
            };
            Assert.IsTrue(UpDownValidator.IsLegal(hands));
        }

        [Test]
        public void EqualAdjacentHands_AreLegal()
        {
            // Hand 1 and Hand 2 are equal-strength pure sequences (A-K-Q of two suits).
            var hands = new List<IReadOnlyList<Card>>
            {
                H(C(Rank.Ace, Suit.Spades), C(Rank.King, Suit.Spades), C(Rank.Queen, Suit.Spades)),
                H(C(Rank.Ace, Suit.Hearts), C(Rank.King, Suit.Hearts), C(Rank.Queen, Suit.Hearts)),
                H(C(Rank.Eight, Suit.Clubs), C(Rank.Seven, Suit.Clubs), C(Rank.Six, Suit.Clubs)),
                H(C(Rank.Five, Suit.Diamonds), C(Rank.Four, Suit.Diamonds), C(Rank.Three, Suit.Diamonds), C(Rank.Two, Suit.Diamonds))
            };
            Assert.IsTrue(UpDownValidator.IsLegal(hands));
        }

        [Test]
        public void AscendingArrangement_IsIllegal()
        {
            // Hand 1 (Q-J-10) is weaker than Hand 2 (A-K-Q) -> violates Up-Down.
            var hands = new List<IReadOnlyList<Card>>
            {
                H(C(Rank.Queen, Suit.Hearts), C(Rank.Jack, Suit.Hearts), C(Rank.Ten, Suit.Hearts)),
                H(C(Rank.Ace, Suit.Spades), C(Rank.King, Suit.Spades), C(Rank.Queen, Suit.Spades)),
                H(C(Rank.Nine, Suit.Clubs), C(Rank.Nine, Suit.Diamonds), C(Rank.Two, Suit.Clubs)),
                H(C(Rank.Seven, Suit.Spades), C(Rank.Five, Suit.Hearts), C(Rank.Four, Suit.Diamonds), C(Rank.Three, Suit.Clubs))
            };
            Assert.IsFalse(UpDownValidator.IsLegal(hands));
        }

        [Test]
        public void WrongHandSizes_AreIllegal()
        {
            var hands = new List<IReadOnlyList<Card>>
            {
                H(C(Rank.Ace, Suit.Spades), C(Rank.King, Suit.Spades)),                                // only 2 cards
                H(C(Rank.Queen, Suit.Hearts), C(Rank.Jack, Suit.Hearts), C(Rank.Ten, Suit.Hearts)),
                H(C(Rank.Nine, Suit.Clubs), C(Rank.Nine, Suit.Diamonds), C(Rank.Two, Suit.Clubs)),
                H(C(Rank.Seven, Suit.Spades), C(Rank.Five, Suit.Hearts), C(Rank.Four, Suit.Diamonds), C(Rank.Three, Suit.Clubs))
            };
            Assert.IsFalse(UpDownValidator.IsLegal(hands));
        }

        [Test]
        public void FourCardKicker_IsIgnored_WhenComparingHand3ToHand4()
        {
            // Hand 3 and Hand 4 share the same best-3 combo (pair of 5s, side 9).
            // Hand 4 carries a leftover kicker (2). Since Up-Down ignores the
            // kicker, the equal combo keeps Hand 3 >= Hand 4 and the whole
            // arrangement stays legal.
            var hands = new List<IReadOnlyList<Card>>
            {
                H(C(Rank.Ace, Suit.Spades), C(Rank.King, Suit.Spades), C(Rank.Queen, Suit.Spades)),
                H(C(Rank.Ace, Suit.Hearts), C(Rank.King, Suit.Hearts), C(Rank.Queen, Suit.Hearts)),
                H(C(Rank.Five, Suit.Clubs), C(Rank.Five, Suit.Diamonds), C(Rank.Nine, Suit.Clubs)),
                H(C(Rank.Five, Suit.Spades), C(Rank.Five, Suit.Hearts), C(Rank.Nine, Suit.Diamonds), C(Rank.Two, Suit.Diamonds))
            };
            Assert.IsTrue(UpDownValidator.IsLegal(hands));
        }
    }
}