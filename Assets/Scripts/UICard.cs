using UnityEngine;
using UnityEngine.UI;
using Hazari.Core;

public class UICard : MonoBehaviour
{
    [Header("UI References")]
    public Image cardImage;

    // The logical data backing this visual card
    private Card _cardData;

    /// <summary>
    /// Call this right after instantiating the UI card to give it its data.
    /// </summary>
    public void Initialize(Card cardData, Sprite faceSprite)
    {
        _cardData = cardData;
        cardImage.sprite = faceSprite;

        // Optional: Name the GameObject so your hierarchy is easy to read (e.g., "AceSpades")
        gameObject.name = _cardData.ToString();
    }

    // You can add a getter if your DropZones need to read the card's value later
    public Card GetCardData()
    {
        return _cardData;
    }
}