using System.Collections.Generic;
using System.Linq;

namespace Hazari.Core
{
    /// <summary>
    /// Plays out one deal (rules §6, §7). Four rounds: in round n every player
    /// reveals Hand n; the highest hand wins and captures all point cards among
    /// the four revealed hands. Round 1 is led by the dealer's right; every round
    /// after that is led by whoever won the previous round (winner-leads-next).
    /// Exact ties go to whoever threw latest in that round's order, which starts
    /// at that round's lead and runs counter-clockwise.
    /// </summary>
    public static class ShowdownResolver
    {
        /// <summary>
        /// arrangements: one per seat, indexed 0..3.
        /// dealerSeat: the seat that dealt (0..3). The player to its right leads round 1.
        /// </summary>
        public static DealResult Resolve(IReadOnlyList<Arrangement> arrangements, int dealerSeat)
        {
            const int seats = 4;
            const int rounds = 4;

            int[] points = new int[seats];
            int[] winners = new int[rounds];

            // Round 1's lead is the dealer's right; every round after that is led
            // by whoever won the previous round (rules §7).
            int leadSeat = (dealerSeat + 1) % seats;

            for (int round = 0; round < rounds; round++)
            {
                // Throw order for this round: lead, then round the table.
                // throwPosition[seat] is how late that seat throws — higher wins ties.
                int[] throwPosition = new int[seats];
                for (int pos = 0; pos < seats; pos++)
                {
                    int seat = (leadSeat + pos) % seats;
                    throwPosition[seat] = pos;
                }

                int winnerSeat = -1;
                HandValue winnerValue = default;
                int roundPoints = 0;

                for (int seat = 0; seat < seats; seat++)
                {
                    IReadOnlyList<Card> hand = arrangements[seat].Hands[round];
                    roundPoints += hand.Sum(c => c.PointValue);

                    HandValue value = HandEvaluator.Evaluate(hand);
                    if (winnerSeat < 0)
                    {
                        winnerSeat = seat;
                        winnerValue = value;
                        continue;
                    }

                    int cmp = value.CompareTo(winnerValue);
                    bool beats = cmp > 0
                        || (cmp == 0 && throwPosition[seat] > throwPosition[winnerSeat]);
                    if (beats)
                    {
                        winnerSeat = seat;
                        winnerValue = value;
                    }
                }

                winners[round] = winnerSeat;
                points[winnerSeat] += roundPoints;
                leadSeat = winnerSeat; // winner leads the next round
            }

            return new DealResult(points, winners);
        }
    }
}
