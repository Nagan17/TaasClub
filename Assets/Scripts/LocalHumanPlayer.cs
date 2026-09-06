using System.Collections.Generic;
using System.Threading.Tasks;
using Hazari.Core;

public class LocalHumanPlayer : IHazariPlayer
{
    private TaskCompletionSource<Arrangement> _submitTask;

    public Task<Arrangement> GetArrangementAsync(IReadOnlyList<Card> dealtCards)
    {
        _submitTask = new TaskCompletionSource<Arrangement>();

        return _submitTask.Task;
    }

    public void OnSubmitButtonClicked(Arrangement playerArrangement)
    {
        _submitTask?.TrySetResult(playerArrangement);
    }
}