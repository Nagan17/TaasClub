using NUnit.Framework;
using Hazari.Core;

namespace Hazari.Tests
{
    public class CardTests
    {
        [Test]
        public void FaceCardsAndTen_AreWorthTenPoints()
        {
            Assert.AreEqual(10, new Card(Rank.Ace, Suit.Spades).PointValue);
            Assert.AreEqual(10, new Card(Rank.King, Suit.Hearts).PointValue);
            Assert.AreEqual(10, new Card(Rank.Queen, Suit.Diamonds).PointValue);
            Assert.AreEqual(10, new Card(Rank.Jack, Suit.Clubs).PointValue);
            Assert.AreEqual(10, new Card(Rank.Ten, Suit.Spades).PointValue);
        }

        [Test]
        public void TwoThroughNine_AreWorthFivePoints()
        {
            for (Rank r = Rank.Two; r <= Rank.Nine; r++)
                Assert.AreEqual(5, new Card(r, Suit.Clubs).PointValue, $"rank {r}");
        }

        [Test]
        public void Equality_MatchesRankAndSuit()
        {
            Assert.AreEqual(new Card(Rank.Ace, Suit.Spades), new Card(Rank.Ace, Suit.Spades));
            Assert.AreNotEqual(new Card(Rank.Ace, Suit.Spades), new Card(Rank.Ace, Suit.Hearts));
            Assert.AreNotEqual(new Card(Rank.Ace, Suit.Spades), new Card(Rank.King, Suit.Spades));
        }
    }
}
