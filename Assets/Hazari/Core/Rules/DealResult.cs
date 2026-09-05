using System.Collections.Generic;

namespace Hazari.Core
{
    /// <summary>The outcome of one deal's showdown.</summary>
    public sealed class DealResult
    {
        /// <summary>Points captured this deal, indexed by seat (0..3).</summary>
        public IReadOnlyList<int> PointsBySeat { get; }

        /// <summary>Winning seat of each of the four rounds, in order.</summary>
        public IReadOnlyList<int> RoundWinners { get; }

        /// <summary>Sum of all captured points — always 360 for a full deal.</summary>
        public int TotalPoints { get; }

        public DealResult(int[] pointsBySeat, int[] roundWinners)
        {
            PointsBySeat = pointsBySeat;
            RoundWinners = roundWinners;

            int total = 0;
            foreach (int p in pointsBySeat) total += p;
            TotalPoints = total;
        }
    }
}
