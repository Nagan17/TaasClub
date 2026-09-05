using System.Linq;
using NUnit.Framework;
using Hazari.Core;

namespace Hazari.Tests
{
    public class DealerTests
    {
        [Test]
        public void FullDeck_Has52UniqueCards()
        {
            var deck = Dealer.BuildDeck();
            Assert.AreEqual(52, deck.Count);
            Assert.AreEqual(52, deck.Distinct().Count());
        }

        [Test]
        public void FullDeck_TotalsThreeSixtyPoints()
        {
            Assert.AreEqual(360, Dealer.BuildDeck().Sum(c => c.PointValue));
        }

        [Test]
        public void Deal_GivesFourHandsOfThirteen()
        {
            var hands = Dealer.Deal(seed: 1);
            Assert.AreEqual(4, hands.Length);
            foreach (var hand in hands)
                Assert.AreEqual(13, hand.Count);
        }

        [Test]
        public void Deal_UsesEveryCardExactlyOnce()
        {
            var all = Dealer.Deal(seed: 1).SelectMany(h => h).ToList();
            Assert.AreEqual(52, all.Count);
            Assert.AreEqual(52, all.Distinct().Count());
        }

        [Test]
        public void Deal_IsDeterministicForAGivenSeed()
        {
            var a = Dealer.Deal(seed: 42);
            var b = Dealer.Deal(seed: 42);
            for (int p = 0; p < a.Length; p++)
                CollectionAssert.AreEqual(a[p], b[p]);
        }

        [Test]
        public void Deal_DiffersBetweenSeeds()
        {
            var a = Dealer.Deal(seed: 1).SelectMany(h => h).ToList();
            var b = Dealer.Deal(seed: 2).SelectMany(h => h).ToList();
            CollectionAssert.AreNotEqual(a, b);
        }
    }
}
