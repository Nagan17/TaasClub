using System;
using System.Collections.Generic;

namespace Hazari.Core
{
    /// <summary>
    /// Builds and deals a standard 52-card deck. The shuffle is <em>seeded</em>
    /// so deals are reproducible in tests (rules §1). In production the server
    /// swaps <see cref="System.Random"/> for a CSPRNG behind the same call —
    /// nothing else in the engine changes.
    /// </summary>
    public static class Dealer
    {
        public const int Players = 4;
        public const int CardsPerPlayer = 13;

        /// <summary>Returns a full, ordered 52-card deck (unshuffled).</summary>
        public static List<Card> BuildDeck()
        {
            var deck = new List<Card>(52);
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    deck.Add(new Card(rank, suit));
            return deck;
        }

        /// <summary>
        /// Deals 13 cards to each of the 4 players from a deck shuffled with
        /// <paramref name="seed"/>. The same seed always produces the same deal.
        /// Returns one list per player, indexed by seat.
        /// </summary>
        public static List<Card>[] Deal(int seed)
        {
            var deck = BuildDeck();
            Shuffle(deck, new Random(seed));

            var hands = new List<Card>[Players];
            for (int p = 0; p < Players; p++)
                hands[p] = new List<Card>(CardsPerPlayer);

            // Round-robin deal, mirroring how cards are dealt at a real table.
            for (int i = 0; i < deck.Count; i++)
                hands[i % Players].Add(deck[i]);

            return hands;
        }

        /// <summary>In-place Fisher–Yates shuffle.</summary>
        private static void Shuffle(IList<Card> cards, Random rng)
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }
    }
}
