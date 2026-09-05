using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Hazari.Core;

namespace Hazari.Tests
{
    public class ShowdownResolverTests
    {
        private static Card C(Rank r, Suit s) => new Card(r, s);

        [Test]
        public void Resolve_DistributesExactly360_AcrossManyDeals()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var hands = Dealer.Deal(seed);
                var arrangements = hands
                    .Select(h => StrongArranger.FindBest(h))
                    .ToList();

                var result = ShowdownResolver.Resolve(arrangements, dealerSeat: seed % 4);

                Assert.AreEqual(360, result.TotalPoints, $"seed {seed}");
                Assert.AreEqual(360, result.PointsBySeat.Sum(), $"seed {seed}");
            }
        }

        // Two players hold an equal A-K-Q in Hand 1 (different suits). The rest of
        // each arrangement is filler — Resolve only reads the hands, so this is a
        // focused test of the tie-break, not a full 52-card deal.
        private static Arrangement TieCandidate(Suit suit) => Build(
            C(Rank.Ace, suit), C(Rank.King, suit), C(Rank.Queen, suit));

        private static Arrangement WeakHand1(Rank high) => Build(
            C(high, Suit.Clubs), C(Rank.Three, Suit.Diamonds), C(Rank.Two, Suit.Spades));

        private static Arrangement Build(params Card[] hand1) => new Arrangement(
            hand1.ToList(),
            new List<Card> { C(Rank.Four, Suit.Clubs), C(Rank.Three, Suit.Clubs), C(Rank.Two, Suit.Clubs) },
            new List<Card> { C(Rank.Four, Suit.Diamonds), C(Rank.Three, Suit.Diamonds), C(Rank.Two, Suit.Diamonds) },
            new List<Card> { C(Rank.Five, Suit.Hearts), C(Rank.Four, Suit.Hearts), C(Rank.Three, Suit.Hearts), C(Rank.Two, Suit.Hearts) });

        [Test]
        public void Resolve_ExactTie_GoesToTheLaterThrower()
        {
            // Seats 0 and 1 tie in round 1; seats 2 and 3 are weaker.
            var arrangements = new List<Arrangement>
            {
                TieCandidate(Suit.Spades), // seat 0
                TieCandidate(Suit.Hearts), // seat 1
                WeakHand1(Rank.Five),      // seat 2
                WeakHand1(Rank.Six)        // seat 3
            };

            // dealer = 0 -> lead is seat 1; throw order 1,2,3,0 -> seat 0 throws last.
            var a = ShowdownResolver.Resolve(arrangements, dealerSeat: 0);
            Assert.AreEqual(0, a.RoundWinners[0], "seat 0 threw last, so it wins the tie");

            // dealer = 3 -> lead is seat 0; throw order 0,1,2,3 -> seat 1 throws after seat 0.
            var b = ShowdownResolver.Resolve(arrangements, dealerSeat: 3);
            Assert.AreEqual(1, b.RoundWinners[0], "seat 1 threw later, so it wins the tie");
        }
    }
}
