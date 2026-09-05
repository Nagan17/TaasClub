namespace Hazari.Core
{
    /// <summary>
    /// The four suits. Hazari has no suit ranking — exact ties are broken by
    /// play order (rules §7), never by suit — so the ordering of these values
    /// carries no meaning.
    /// </summary>
    public enum Suit
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }
}
