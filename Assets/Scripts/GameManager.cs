using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazari.Core;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject uiCardPrefab;
    public Transform mainHandZone;

    [Header("UI Controls")]
    public GameObject readyButtonObject;

    [Header("Table Play Zones")]
    public Transform myPlayZone;      // Seat 0
    public Transform rightPlayZone;   // Seat 1
    public Transform topPlayZone;     // Seat 2
    public Transform leftPlayZone;    // Seat 3
    public float seatOffsetDistance = 180f;

    private Transform[] playZones;

    [Header("Game State")]
    public LocalHumanPlayer humanPlayer;
    private IHazariPlayer[] players = new IHazariPlayer[4];
    private int[] totalScores = new int[4];
    private int currentDealerSeat;
    private const int WINNING_SCORE = 1000;

    [Header("Deal Animation")]
    //public Transform dealOrigin;      // an empty Transform marking the deck's screen position
    public Sprite cardBackSprite;
    public float dealStagger = 0.06f; // delay between successive cards being dealt
    public float revealDelay = 0.15f; // pause after a human card is dealt before it flips

    void Start()
    {
        playZones = new Transform[] { myPlayZone, rightPlayZone, topPlayZone, leftPlayZone };

        humanPlayer = new LocalHumanPlayer();
        players[0] = humanPlayer;
        players[1] = new BotPlayer();
        players[2] = new BotPlayer();
        players[3] = new BotPlayer();

        currentDealerSeat = UnityEngine.Random.Range(0, 4);

        _ = PlayMatchLoop();
    }

    private async Task PlayMatchLoop()
    {
        bool matchOver = false;

        while (!matchOver)
        {
            if (!Application.isPlaying) return;

            Debug.Log($"--- New Deal! Dealer is Seat {currentDealerSeat} ---");

            // 1. Ensure the submit button is active and ready for the new hand
            if (readyButtonObject != null) readyButtonObject.SetActive(true);

            int seed = UnityEngine.Random.Range(0, int.MaxValue);
            List<Card>[] dealtHands = Dealer.Deal(seed);

            await DealAllHandsAnimated(dealtHands, currentDealerSeat);

            Task<Arrangement>[] arrangementTasks = new Task<Arrangement>[4];
            for (int i = 0; i < 4; i++)
            {
                arrangementTasks[i] = players[i].GetArrangementAsync(dealtHands[i]);
            }

            Arrangement[] finalArrangements = await Task.WhenAll(arrangementTasks);
            if (!Application.isPlaying) return;

            // 2. Hide the button the exact moment the player hits submit and the showdown starts
            if (readyButtonObject != null) readyButtonObject.SetActive(false);

            DealResult result = ShowdownResolver.Resolve(finalArrangements, currentDealerSeat);

            await VisualizeShowdown(finalArrangements, result);
            if (!Application.isPlaying) return;

            for (int i = 0; i < 4; i++)
            {
                totalScores[i] += result.PointsBySeat[i];
            }

            if (totalScores.Any(score => score >= WINNING_SCORE))
            {
                matchOver = true;
                break;
            }

            currentDealerSeat = (currentDealerSeat + 1) % 4;
            await Task.Delay(2000);
        }
    }

    private async Task VisualizeShowdown(Arrangement[] finalArrangements, DealResult result)
    {
        // DO NOT hide the main hand anymore! Let the player see their remaining cards.
        Vector3 tableCenter = playZones[0].parent.position;

        for (int round = 0; round < 4; round++)
        {
            if (!Application.isPlaying) return;

            // Counter-clockwise throw order starting from dealer's right[cite: 5]
            int leadSeat = (currentDealerSeat + 1) % 4;
            List<Transform> cardsInCenter = new List<Transform>();

            for (int i = 0; i < 4; i++)
            {
                int seat = (leadSeat + i) % 4;
                IReadOnlyList<Card> handToPlay = finalArrangements[seat].Hands[round];
                List<Transform> playedCards;

                if (seat == 0)
                {
                    // Human: Pull actual cards from the visible fan
                    playedCards = await PullHumanCardsAndSlide(handToPlay, playZones[0], tableCenter);
                }
                else
                {
                    // Bots: Spawn new cards at their respective edges
                    playedCards = await SpawnBotCardsAndSlide(handToPlay, playZones[seat], tableCenter);
                }

                cardsInCenter.AddRange(playedCards);

                await Task.Delay(500);
                if (!Application.isPlaying) return;
            }

            int winnerSeat = result.RoundWinners[round];
            Debug.Log($"Seat {winnerSeat} wins this round!");

            await Task.Delay(2000);
            if (!Application.isPlaying) return;

            await AnimateCardsToWinner(cardsInCenter, playZones[winnerSeat].position);
            await Task.Delay(500);
        }
    }

    private async Task<List<Transform>> PullHumanCardsAndSlide(IReadOnlyList<Card> logicCards, Transform spawnZone, Vector3 centerTarget)
    {
        List<Transform> myCards = new List<Transform>();
        UICard[] allHandCards = mainHandZone.GetComponentsInChildren<UICard>();

        // Find the physical UI cards that match the logical cards we need to play
        foreach (Card logicCard in logicCards)
        {
            foreach (UICard uiCard in allHandCards)
            {
                Card uiData = uiCard.GetCardData();
                if (uiData.Rank == logicCard.Rank && uiData.Suit == logicCard.Suit)
                {
                    if (!myCards.Contains(uiCard.transform))
                    {
                        myCards.Add(uiCard.transform);
                        break;
                    }
                }
            }
        }

        return await AnimateZoneToCenter(myCards, spawnZone, centerTarget);
    }

    private async Task<List<Transform>> SpawnBotCardsAndSlide(IReadOnlyList<Card> logicCards, Transform spawnZone, Vector3 centerTarget)
    {
        List<Transform> spawnedCards = new List<Transform>();

        foreach (Card logicCard in logicCards)
        {
            GameObject newCardObj = Instantiate(uiCardPrefab);
            UICard uiCard = newCardObj.GetComponent<UICard>();
            uiCard.Initialize(logicCard, GetCardSprite(logicCard));
            spawnedCards.Add(newCardObj.transform);
        }

        return await AnimateZoneToCenter(spawnedCards, spawnZone, centerTarget);
    }

    private async Task<List<Transform>> AnimateZoneToCenter(List<Transform> cards, Transform spawnZone, Vector3 centerTarget)
    {
        Quaternion[] startRotations = new Quaternion[cards.Count];
        Vector3[] startScales = new Vector3[cards.Count]; // Track starting scale

        for (int i = 0; i < cards.Count; i++)
        {
            startRotations[i] = cards[i].rotation;
            startScales[i] = cards[i].localScale;

            cards[i].SetParent(spawnZone, false);
            cards[i].position = spawnZone.position;
            cards[i].localScale = Vector3.one;
            if (cards[i].TryGetComponent(out DraggableCard drag)) drag.enabled = false;
        }

        await Task.Yield();
        if (!Application.isPlaying) return cards;

        Vector3[] startPositions = new Vector3[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            startPositions[i] = cards[i].position;
            cards[i].SetParent(spawnZone.parent, true);
        }

        float seatOffsetDistance = 180f;
        Vector3 directionFromCenter = (spawnZone.position - centerTarget).normalized;
        Vector3 finalGroupCenter = centerTarget + (directionFromCenter * seatOffsetDistance);

        float duration = 0.35f;
        float elapsed = 0f;
        float tightSpacing = 45f;

        // Target scale for table showdown cards
        Vector3 targetScale = Vector3.one * 0.75f;

        while (elapsed < duration)
        {
            if (!Application.isPlaying) return cards;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothStep = Mathf.SmoothStep(0, 1, t);

            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null)
                {
                    float xOffset = (i - (cards.Count - 1) / 2f) * tightSpacing;
                    Vector3 tightTargetPos = finalGroupCenter + new Vector3(xOffset, 0, 0);

                    cards[i].position = Vector3.Lerp(startPositions[i], tightTargetPos, smoothStep);
                    cards[i].rotation = Quaternion.Lerp(startRotations[i], Quaternion.identity, smoothStep);

                    // Smoothly shrink down to 0.75 scale as they move to the center
                    cards[i].localScale = Vector3.Lerp(startScales[i], targetScale, smoothStep);
                }
            }
            await Task.Yield();
        }

        return cards;
    }

    private async Task AnimateCardsToWinner(List<Transform> cards, Vector3 targetPosition)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Vector3[] startPositions = new Vector3[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null) startPositions[i] = cards[i].position;
        }

        // Animation Loop: Sweep to winner
        while (elapsed < duration)
        {
            if (!Application.isPlaying) return;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothStep = Mathf.SmoothStep(0, 1, t);

            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null)
                {
                    cards[i].position = Vector3.Lerp(startPositions[i], targetPosition, smoothStep);
                    // Shrink them down as they get collected
                    cards[i].localScale = Vector3.Lerp(Vector3.one, Vector3.zero, smoothStep);
                }
            }
            await Task.Yield();
        }

        // Destroy them once they reach the winner
        foreach (Transform card in cards)
        {
            if (card != null) Destroy(card.gameObject);
        }
    }

    private void SpawnCardsForHuman(List<Card> myHand)
    {
        foreach (Transform child in mainHandZone)
        {
            Destroy(child.gameObject);
        }

        foreach (Card card in myHand)
        {
            GameObject newCardObj = Instantiate(uiCardPrefab, mainHandZone);
            UICard uiCard = newCardObj.GetComponent<UICard>();

            Sprite faceImage = GetCardSprite(card);
            uiCard.Initialize(card, faceImage);
        }
    }

    public void OnSubmitButtonClicked()
    {
        // 1. Gather all 13 cards from left to right
        List<Card> allCards = new List<Card>();
        foreach (Transform child in mainHandZone)
        {
            allCards.Add(child.GetComponent<UICard>().GetCardData());
        }

        if (allCards.Count != 13) return;

        // 2. Slice them into groups based on their visual order
        List<Card> chunkA = allCards.GetRange(0, 3);
        List<Card> chunkB = allCards.GetRange(3, 3);
        List<Card> chunkC = allCards.GetRange(6, 3);
        List<Card> hand4 = allCards.GetRange(9, 4);

        // Auto-detect which physical group is strongest — the groupings the player
        // made are never touched, only which named slot (Hand1/2/3) each one fills.
        List<List<Card>> threeCardGroups = new List<List<Card>> { chunkA, chunkB, chunkC };
        threeCardGroups.Sort((a, b) => HandEvaluator.Evaluate(b).CompareTo(HandEvaluator.Evaluate(a)));

        Arrangement playerArrangement = SubmissionParser.Parse(allCards);

        // 3. Check if the player's left-to-right sorting is legal[cite: 12]
        if (UpDownValidator.IsLegal(playerArrangement.Hands))
        {
            Debug.Log("Valid manual arrangement submitted.");
            humanPlayer.OnSubmitButtonClicked(playerArrangement);
        }
        else
        {
            // 4. AUTO-DETECT (WEAK FALLBACK)
            Debug.LogWarning("Invalid sorting! Applying weak auto-arrangement penalty.");

            // Pass an empty list for "completedHands" since they didn't properly finish any
            List<IReadOnlyList<Card>> noCompletedHands = new List<IReadOnlyList<Card>>();

            Arrangement weakFallback = TimeoutLegalizer.Legalize(allCards, noCompletedHands);

            if (weakFallback != null)
            {
                humanPlayer.OnSubmitButtonClicked(weakFallback);
            }
        }
    }

    private List<Card> GetCardsFromZone(Transform zone)
    {
        List<Card> cards = new List<Card>();
        foreach (Transform child in zone)
        {
            cards.Add(child.GetComponent<UICard>().GetCardData());
        }
        return cards;
    }

    public Sprite GetCardSprite(Card card)
    {
        string rankString;
        switch (card.Rank)
        {
            case Rank.Ace: rankString = "A"; break;
            case Rank.King: rankString = "K"; break;
            case Rank.Queen: rankString = "Q"; break;
            case Rank.Jack: rankString = "J"; break;
            default: rankString = ((int)card.Rank).ToString(); break;
        }

        string spriteName = $"{rankString}_{card.Suit}";
        return Resources.Load<Sprite>($"CardFaces/{spriteName}");
    }

    private async Task DealAllHandsAnimated(List<Card>[] dealtHands, int dealerSeat)
    {
        foreach (Transform child in mainHandZone) Destroy(child.gameObject);

        Vector3 deckPosition = playZones[dealerSeat].position;

        for (int round = 0; round < Dealer.CardsPerPlayer; round++)
        {
            for (int seat = 0; seat < 4; seat++)
            {
                Card card = dealtHands[seat][round];

                if (seat == 0)
                    DealHumanCard(card, deckPosition);
                else
                    _ = DealBotCard(card, seat, deckPosition); // visual only, fire-and-forget

                await Task.Delay((int)(dealStagger * 1000));
                if (!Application.isPlaying) return;
            }
        }

        await Task.Delay(250); // let the fan settle before anyone can act
    }

    private void DealHumanCard(Card card, Vector3 deckPosition)
    {
        GameObject cardObj = Instantiate(uiCardPrefab, mainHandZone);
        UICard uiCard = cardObj.GetComponent<UICard>();
        uiCard.Initialize(card, GetCardSprite(card), cardBackSprite);

        // Spawn at the deck; CardHandFan eases every child toward its fan slot
        // every frame already, so it carries this one into place on its own.
        cardObj.transform.position = deckPosition;

        _ = RevealAfterDelay(uiCard);
    }

    private async Task RevealAfterDelay(UICard uiCard)
    {
        await Task.Delay((int)(revealDelay * 1000));
        if (!Application.isPlaying || uiCard == null) return;
        await uiCard.RevealAsync();
    }

    private async Task DealBotCard(Card card, int seat, Vector3 deckPosition)
    {
        GameObject cardObj = Instantiate(uiCardPrefab);
        UICard uiCard = cardObj.GetComponent<UICard>();
        uiCard.Initialize(card, GetCardSprite(card), cardBackSprite);

        cardObj.transform.SetParent(playZones[0].parent, true); // table root
        cardObj.transform.position = deckPosition;
        cardObj.transform.localScale = Vector3.one * 0.75f;

        Vector3 targetPos = playZones[seat].position;
        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!Application.isPlaying) return;
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            cardObj.transform.position = Vector3.Lerp(deckPosition, targetPos, t);
            await Task.Yield();
        }

        // Bots' hands stay hidden — this was just a dealt-card visual beat.
        Destroy(cardObj);
    }
}