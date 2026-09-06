using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazari.Core;

public class BotPlayer : IHazariPlayer
{
    // Use standard C# Random instead of Unity's Random
    private static readonly Random _rng = new Random();

    public async Task<Arrangement> GetArrangementAsync(IReadOnlyList<Card> dealtCards)
    {
        // 1. Fake "thinking" delay using System.Random (between 2000ms and 5000ms)
        int thinkingTimeMs = _rng.Next(2000, 5000);
        await Task.Delay(thinkingTimeMs);

        // 2. Ask your core logic for the strongest legal arrangement[cite: 10]
        Arrangement bestPlay = StrongArranger.FindBest(dealtCards);

        return bestPlay;
    }
}