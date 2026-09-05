using System.Collections.Generic;
using System.Linq;

namespace Hazari.Core
{
    /// <summary>
    /// Plays out one deal (rules §6, §7). Four rounds: in round n every player
    /// reveals Hand n; the highest hand wins and captures all point cards among
    /// the four revealed hands. Exact ties go to the player who threw latest in
    /// the round's order, which runs counter-clockwise from the dealer's right and
    /// is the same for all four rounds of the deal.
    /// </summary>
    public static class ShowdownResolver
    {
        /// <summary>
        /// arrangements: one per seat, indexed 0..3.
        /// dealerSeat: the seat that dealt (0..3). The player to its right leads.
        /// </summary>
        public static DealResult Resolve(IReadOnlyList<Arrangement> arrangements, int dealerSeat)
        {
            const int seats = 4;
            const int rounds = 4;

            // Throw order: lead is the seat to the dealer's right (counter-clockwise,
            // = next seat), then round the table. throwPosition[seat] is how late
            // that seat throws — higher wins ties.
            int[] throwPosition = new int[seats];
            for (int pos = 0; pos < seats; pos++)
            {
                int seat = (dealerSeat + 1 + pos) % seats;
                throwPosition[seat] = pos;
            }

            int[] points = new int[seats];
            int[] winners = new int[rounds];

            for (int round = 0; round < rounds; round++)
            {
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
            }

            return new DealResult(points, winners);
        }
    }
}
