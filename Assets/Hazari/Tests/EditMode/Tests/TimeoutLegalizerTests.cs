using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Hazari.Core;

namespace Hazari.Tests
{
    public class TimeoutLegalizerTests
    {
        private static readonly IReadOnlyList<IReadOnlyList<Card>> Nothing =
            new List<IReadOnlyList<Card>>();

        // A 13-card hand that contains three aces (the only possible trail) plus
        // ten unrelated low/mid cards.
        private static List<Card> HandWithThreeAces() => new List<Card>
        {
            new Card(Rank.Ace, Suit.Spades),
            new Card(Rank.Ace, Suit.Hearts),
            new Card(Rank.Ace, Suit.Diamonds),
            new Card(Rank.Two, Suit.Clubs),
            new Card(Rank.Four, Suit.Diamonds),
            new Card(Rank.Six, Suit.Hearts),
            new Card(Rank.Eight, Suit.Spades),
            new Card(Rank.Nine, Suit.Clubs),
            new Card(Rank.Jack, Suit.Diamonds),
            new Card(Rank.Queen, Suit.Hearts),
            new Card(Rank.King, Suit.Clubs),
            new Card(Rank.Seven, Suit.Spades),
            new Card(Rank.Five, Suit.Clubs)
        };

        [Test]
        public void Legalize_AlwaysReturnsALegalArrangement_AcrossManyDeals()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var hand = Dealer.Deal(seed)[0];
                var arrangement = TimeoutLegalizer.Legalize(hand, Nothing);

                Assert.IsNotNull(arrangement, $"seed {seed}: no arrangement");
                Assert.IsTrue(UpDownValidator.IsLegal(arrangement.Hands),
                    $"seed {seed}: arrangement violates Up-Down");
            }
        }

        [Test]
        public void Legalize_KeepsACompletedTrailIntact()
        {
            var hand = HandWithThreeAces();
            var builtTrail = new List<IReadOnlyList<Card>>
            {
                new List<Card>
                {
                    new Card(Rank.Ace, Suit.Spades),
                    new Card(Rank.Ace, Suit.Hearts),
                    new Card(Rank.Ace, Suit.Diamonds)
                }
            };

            var arrangement = TimeoutLegalizer.Legalize(hand, builtTrail);

            // The player built the trail, so it must survive — as their strongest
            // group it lands in Hand 1.
            Assert.AreEqual(HandCategory.Trail,
                HandEvaluator.Evaluate(arrangement.Hand1).Category);
            Assert.IsTrue(UpDownValidator.IsLegal(arrangement.Hands));
        }

        [Test]
        public void Legalize_DoesNotHandYouATrailYouDidNotBuild()
        {
            // Same three aces, but the player completed nothing. The weakest legal
            // completion splits the aces across hands, so no hand is a trail —
            // timing out does not do the work for you.
            var hand = HandWithThreeAces();

            var arrangement = TimeoutLegalizer.Legalize(hand, Nothing);

            bool anyTrail = arrangement.Hands
                .Any(h => HandEvaluator.Evaluate(h).Category == HandCategory.Trail);
            Assert.IsFalse(anyTrail, "timeout should not build a trail the player didn't");
        }
    }
}
