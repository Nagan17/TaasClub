namespace Hazari.Core
{
    /// <summary>
    /// Card ranks. The numeric values (2..14) give a natural high-card order
    /// with Ace high. Ace's special <em>low</em> role in the A-2-3 run is a
    /// concern of the hand evaluator, not of this enum.
    /// </summary>
    public enum Rank
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }
}
