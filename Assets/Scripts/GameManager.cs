using UnityEngine;
using System.Collections.Generic;
using Hazari.Core;

public class GameManager : MonoBehaviour
{
    public GameObject UICardPrefab;
    public Transform mainHandZone;

    public Sprite GetCardSprite(Hazari.Core.Card card)
    {
        string rankString;
        switch (card.Rank)
        {
            case Hazari.Core.Rank.Ace: rankString = "A"; break;
            case Hazari.Core.Rank.King: rankString = "K"; break;
            case Hazari.Core.Rank.Queen: rankString = "Q"; break;
            case Hazari.Core.Rank.Jack: rankString = "J"; break;
            default: rankString = ((int)card.Rank).ToString(); break;
        }

        string spriteName = $"{rankString}_{card.Suit}";
        string fullPath = $"CardFaces/{spriteName}";

        Sprite loadedSprite = Resources.Load<Sprite>(fullPath);

        if (loadedSprite == null)
        {
            Debug.LogError($"Could not find sprite at path: Resources/{fullPath}. Check spelling and folder structure!");
        }

        return loadedSprite;
    }

    void Start()
    {
        // 1. Get the logical hands using your Dealer
        int randomSeed = Random.Range(0, 100000);
        List<Card>[] allHands = Dealer.Deal(randomSeed);

        // 2. Get Player 1's hand (the first 13 cards)
        List<Card> myHand = allHands[0];

        // 3. Spawn the visual UI cards
        foreach (Card card in myHand)
        {
            // Create the UI object inside the hand zone
            GameObject newCardObj = Instantiate(UICardPrefab, mainHandZone);

            // Get the bridge script
            UICard uiCard = newCardObj.GetComponent<UICard>();

            // Give it the data and the picture
            Sprite faceImage = GetCardSprite(card);
            uiCard.Initialize(card, faceImage);
        }
    }
}