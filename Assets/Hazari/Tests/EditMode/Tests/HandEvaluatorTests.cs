using System.Collections.Generic;
using NUnit.Framework;
using Hazari.Core;

namespace Hazari.Tests
{
    public class HandEvaluatorTests
    {
        // --- helpers -------------------------------------------------------

        private static Card C(Rank r, Suit s) => new Card(r, s);

        private static HandValue Eval(params Card[] cards)
            => HandEvaluator.Evaluate(cards);

        private static void AssertStronger(HandValue stronger, HandValue weaker)
            => Assert.Greater(stronger.CompareTo(weaker), 0);

        // --- category ordering --------------------------------------------

        [Test]
        public void CategoryOrder_TrailBeatsPureSequenceBeatsSequenceBeatsFlushBeatsPairBeatsHighCard()
        {
            var trail = Eval(C(Rank.Nine, Suit.Clubs), C(Rank.Nine, Suit.Hearts), C(Rank.Nine, Suit.Spades));
            var pureSeq = Eval(C(Rank.Nine, Suit.Clubs), C(Rank.Eight, Suit.Clubs), C(Rank.Seven, Suit.Clubs));
            var seq = Eval(C(Rank.Nine, Suit.Clubs), C(Rank.Eight, Suit.Hearts), C(Rank.Seven, Suit.Spades));
            var flush = Eval(C(Rank.King, Suit.Clubs), C(Rank.Nine, Suit.Clubs), C(Rank.Two, Suit.Clubs));
            var pair = Eval(C(Rank.King, Suit.Clubs), C(Rank.King, Suit.Hearts), C(Rank.Two, Suit.Spades));
            var high = Eval(C(Rank.King, Suit.Clubs), C(Rank.Nine, Suit.Hearts), C(Rank.Two, Suit.Spades));

            AssertStronger(trail, pureSeq);
            AssertStronger(pureSeq, seq);
            AssertStronger(seq, flush);
            AssertStronger(flush, pair);
            AssertStronger(pair, high);
        }

        [Test]
        public void PureSequence_BeatsMixedSequence_OfSameRanks()
        {
            var pure = Eval(C(Rank.Ten, Suit.Hearts), C(Rank.Nine, Suit.Hearts), C(Rank.Eight, Suit.Hearts));
            var mixed = Eval(C(Rank.Ten, Suit.Hearts), C(Rank.Nine, Suit.Clubs), C(Rank.Eight, Suit.Hearts));
            AssertStronger(pure, mixed);
        }

        // --- trail ---------------------------------------------------------

        [Test]
        public void Trail_AceHighest_TwoLowest()
        {
            var aaa = Eval(C(Rank.Ace, Suit.Clubs), C(Rank.Ace, Suit.Hearts), C(Rank.Ace, Suit.Spades));
            var kkk = Eval(C(Rank.King, Suit.Clubs), C(Rank.King, Suit.Hearts), C(Rank.King, Suit.Spades));
            var ttt = Eval(C(Rank.Two, Suit.Clubs), C(Rank.Two, Suit.Hearts), C(Rank.Two, Suit.Spades));
            AssertStronger(aaa, kkk);
            AssertStronger(kkk, ttt);
        }

        // --- the A-2-3 rule (second-highest run) ---------------------------

        [Test]
        public void Sequence_AceKingQueen_IsHighest()
        {
            var akq = Eval(C(Rank.Ace, Suit.Clubs), C(Rank.King, Suit.Hearts), C(Rank.Queen, Suit.Spades));
            var a23 = Eval(C(Rank.Ace, Suit.Clubs), C(Rank.Two, Suit.Hearts), C(Rank.Three, Suit.Spades));
            AssertStronger(akq, a23);
        }

        [Test]
        public void Sequence_AceTwoThree_IsSecondHighest_AboveKingQueenJack()
        {
            var a23 = Eval(C(Rank.Ace, Suit.Clubs), C(Rank.Two, Suit.Hearts), C(Rank.Three, Suit.Spades));
            var kqj = Eval(C(Rank.King, Suit.Clubs), C(Rank.Queen, Suit.Hearts), C(Rank.Jack, Suit.Spades));
            AssertStronger(a23, kqj);
        }

        [Test]
        public void Sequence_FourThreeTwo_IsLowest()
        {
            var a23 = Eval(C(Rank.Ace, Suit.Clubs), C(Rank.Two, Suit.Hearts), C(Rank.Three, Suit.Spades));
            var kqj = Eval(C(Rank.King, Suit.Clubs), C(Rank.Queen, Suit.Hearts), C(Rank.Jack, Suit.Spades));
            var fourThreeTwo = Eval(C(Rank.Four, Suit.Clubs), C(Rank.Three, Suit.Hearts), C(Rank.Two, Suit.Spades));
            AssertStronger(kqj, fourThreeTwo);
            AssertStronger(a23, fourThreeTwo);
        }

        // --- flush ---------------------------------------------------------

        [Test]
        public void Flush_ComparesHighestThenNextThenLowest()
        {
            // J-9-2 beats J-8-7 because the second card 9 outranks 8.
            var j92 = Eval(C(Rank.Jack, Suit.Spades), C(Rank.Nine, Suit.Spades), C(Rank.Two, Suit.Spades));
            var j87 = Eval(C(Rank.Jack, Suit.Hearts), C(Rank.Eight, Suit.Hearts), C(Rank.Seven, Suit.Hearts));
            AssertStronger(j92, j87);
        }

        // --- pair ----------------------------------------------------------

        [Test]
        public void Pair_HigherPairWins_RegardlessOfKicker()
        {
            var kingsLowKicker = Eval(C(Rank.King, Suit.Clubs), C(Rank.King, Suit.Hearts), C(Rank.Two, Suit.Spades));
            var queensAceKicker = Eval(C(Rank.Queen, Suit.Clubs), C(Rank.Queen, Suit.Hearts), C(Rank.Ace, Suit.Spades));
            AssertStronger(kingsLowKicker, queensAceKicker);
        }

        [Test]
        public void Pair_EqualPair_HigherSideCardWins()
        {
            var kk9 = Eval(C(Rank.King, Suit.Clubs), C(Rank.King, Suit.Hearts), C(Rank.Nine, Suit.Spades));
            var kk4 = Eval(C(Rank.King, Suit.Diamonds), C(Rank.King, Suit.Spades), C(Rank.Four, Suit.Clubs));
            AssertStronger(kk9, kk4);
        }

        // --- 4-card hand: best 3 + kicker ----------------------------------

        [Test]
        public void FourCard_RankedByBestThreeCardCombo()
        {
            // A-A-A-2 -> best three is the Ace trail; beats K-K-K-Q's King trail.
            var acesPlusTwo = Eval(
                C(Rank.Ace, Suit.Clubs), C(Rank.Ace, Suit.Hearts),
                C(Rank.Ace, Suit.Spades), C(Rank.Two, Suit.Diamonds));
            var kingsPlusQueen = Eval(
                C(Rank.King, Suit.Clubs), C(Rank.King, Suit.Hearts),
                C(Rank.King, Suit.Spades), C(Rank.Queen, Suit.Diamonds));
            AssertStronger(acesPlusTwo, kingsPlusQueen);
        }

        [Test]
        public void FourCard_EqualBestThree_KickerBreaksTie()
        {
            // Both best-threes are pair-of-Kings-with-nine; the leftover card
            // (5 vs 4) is the kicker and decides it.
            var withFive = Eval(
                C(Rank.King, Suit.Spades), C(Rank.King, Suit.Hearts),
                C(Rank.Nine, Suit.Clubs), C(Rank.Five, Suit.Diamonds));
            var withFour = Eval(
                C(Rank.King, Suit.Spades), C(Rank.King, Suit.Hearts),
                C(Rank.Nine, Suit.Clubs), C(Rank.Four, Suit.Diamonds));
            AssertStronger(withFive, withFour);
        }
    }
}
