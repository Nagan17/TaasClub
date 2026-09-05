using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Hazari.Core;

namespace Hazari.Tests
{
    public class StrongArrangerTests
    {
        [Test]
        public void FindBest_AlwaysReturnsALegalArrangement_AcrossManyDeals()
        {
            // The headline guarantee: whatever 13 cards a player is dealt, the
            // arranger returns a valid Up-Down arrangement.
            for (int seed = 0; seed < 30; seed++)
            {
                var hand = Dealer.Deal(seed)[0];
                var arrangement = StrongArranger.FindBest(hand);

                Assert.IsNotNull(arrangement, $"seed {seed}: no arrangement found");
                Assert.IsTrue(UpDownValidator.IsLegal(arrangement.Hands),
                    $"seed {seed}: arrangement violates Up-Down");
            }
        }

        [Test]
        public void FindBest_UsesEachDealtCardExactlyOnce()
        {
            var hand = Dealer.Deal(7)[0];
            var arrangement = StrongArranger.FindBest(hand);

            var used = arrangement.Hands.SelectMany(h => h).ToList();
            Assert.AreEqual(13, used.Count);
            CollectionAssert.AreEquivalent(hand, used);
        }

        [Test]
        public void FindBest_StacksStrongestComboInHand1()
        {
            // Three aces are the strongest possible group, so "power up top"
            // must place that trail in Hand 1.
            var hand = new List<Card>
            {
                new Card(Rank.Ace, Suit.Spades),
                new Card(Rank.Ace, Suit.Hearts),
                new Card(Rank.Ace, Suit.Diamonds),
                new Card(Rank.Two, Suit.Clubs),
                new Card(Rank.Three, Suit.Diamonds),
                new Card(Rank.Four, Suit.Hearts),
                new Card(Rank.Five, Suit.Spades),
                new Card(Rank.Six, Suit.Clubs),
                new Card(Rank.Seven, Suit.Diamonds),
                new Card(Rank.Eight, Suit.Hearts),
                new Card(Rank.Nine, Suit.Spades),
                new Card(Rank.Ten, Suit.Clubs),
                new Card(Rank.Jack, Suit.Diamonds)
            };

            var arrangement = StrongArranger.FindBest(hand);

            Assert.AreEqual(HandCategory.Trail,
                HandEvaluator.Evaluate(arrangement.Hand1).Category);
        }
    }
}
