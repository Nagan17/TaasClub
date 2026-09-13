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

        private static Arrangement TwoRoundArrangement(List<Card> hand1, List<Card> hand2) => new Arrangement(
            hand1,
            hand2,
            new List<Card> { C(Rank.Four, Suit.Clubs), C(Rank.Three, Suit.Clubs), C(Rank.Two, Suit.Clubs) },
            new List<Card> { C(Rank.Five, Suit.Diamonds), C(Rank.Four, Suit.Diamonds), C(Rank.Three, Suit.Diamonds), C(Rank.Two, Suit.Diamonds) });

        [Test]
        public void Resolve_Round2Lead_IsRound1Winner_NotFixedByDealer()
        {
            // Round 1: seat 0 holds the unique A-A-A trail, so seat 0 wins round 1
            // outright. Round 2: seats 1 and 2 hold an equal A-K-Q (tie).
            // Winner-leads-next means round 2's order starts at seat 0 (round 1's
            // winner) -> 0,1,2,3 -> seat 2 throws later and wins the tie. Under the
            // old fixed-order rule (dealer=1 -> lead=2 every round), round 2's order
            // would instead be 2,3,0,1 -> seat 1 would wrongly win.
            var arrangements = new List<Arrangement>
            {
                TwoRoundArrangement(
                    hand1: new List<Card> { C(Rank.Ace, Suit.Clubs), C(Rank.Ace, Suit.Diamonds), C(Rank.Ace, Suit.Hearts) },
                    hand2: new List<Card> { C(Rank.Nine, Suit.Clubs), C(Rank.Three, Suit.Diamonds), C(Rank.Two, Suit.Spades) }),
                TwoRoundArrangement(
                    hand1: new List<Card> { C(Rank.King, Suit.Clubs), C(Rank.Nine, Suit.Diamonds), C(Rank.Two, Suit.Spades) },
                    hand2: new List<Card> { C(Rank.Ace, Suit.Spades), C(Rank.King, Suit.Spades), C(Rank.Queen, Suit.Spades) }),
                TwoRoundArrangement(
                    hand1: new List<Card> { C(Rank.Eight, Suit.Clubs), C(Rank.Nine, Suit.Hearts), C(Rank.Two, Suit.Diamonds) },
                    hand2: new List<Card> { C(Rank.Ace, Suit.Hearts), C(Rank.King, Suit.Hearts), C(Rank.Queen, Suit.Hearts) }),
                TwoRoundArrangement(
                    hand1: new List<Card> { C(Rank.Seven, Suit.Clubs), C(Rank.Nine, Suit.Spades), C(Rank.Two, Suit.Hearts) },
                    hand2: new List<Card> { C(Rank.Eight, Suit.Diamonds), C(Rank.Three, Suit.Clubs), C(Rank.Two, Suit.Clubs) })
            };

            var result = ShowdownResolver.Resolve(arrangements, dealerSeat: 1);

            Assert.AreEqual(0, result.RoundWinners[0], "seat 0's unique trail wins round 1");
            Assert.AreEqual(2, result.RoundWinners[1],
                "round 2 must lead from seat 0 (round 1's winner), not dealer+1 - seat 2 throws later and wins the tie");
        }
    }
}
