using System.Collections.Generic;
using System.Threading.Tasks;
using Hazari.Core;

public interface IHazariPlayer
{
    Task<Arrangement> GetArrangementAsync(IReadOnlyList<Card> dealtCards);
}