using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Hazari.Core;

public class UICard : MonoBehaviour
{
    [Header("UI References")]
    public Image cardImage;

    // The logical data backing this visual card
    private Card _cardData;
    private Sprite _faceSprite;

    /// <summary>
    /// Call this right after instantiating the UI card to give it its data.
    /// </summary>
    public void Initialize(Card cardData, Sprite faceSprite, Sprite backSprite = null)
    {
        _cardData = cardData;
        _faceSprite = faceSprite;
        cardImage.sprite = backSprite != null ? backSprite : faceSprite;

        gameObject.name = _cardData.ToString();
    }

    // You can add a getter if your DropZones need to read the card's value later
    public Card GetCardData()
    {
        return _cardData;
    }

    public async Task RevealAsync(float flipDuration = 0.25f)
    {
        RectTransform rt = (RectTransform)transform;
        Vector3 baseScale = rt.localScale;
        float half = flipDuration / 2f;

        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            rt.localScale = new Vector3(Mathf.Lerp(baseScale.x, 0f, t), baseScale.y, baseScale.z);
            await Task.Yield();
        }

        cardImage.sprite = _faceSprite;

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            rt.localScale = new Vector3(Mathf.Lerp(0f, baseScale.x, t), baseScale.y, baseScale.z);
            await Task.Yield();
        }

        rt.localScale = baseScale;
    }
}