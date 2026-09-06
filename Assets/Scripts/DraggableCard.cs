using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform originalParent = null;
    private CanvasGroup canvasGroup;

    // The invisible object that holds our space in the layout group
    private GameObject placeholder = null;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;

        // 1. Create a dummy placeholder object
        placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(originalParent);

        RectTransform ghostRT = placeholder.AddComponent<RectTransform>();
        RectTransform myRT = GetComponent<RectTransform>();
        ghostRT.sizeDelta = myRT.sizeDelta;
        ghostRT.localScale = myRT.localScale;

        // 3. Copy the image and make it semi-transparent
        Image myImage = GetComponent<Image>();
        if (myImage != null)
        {
            Image ghostImage = placeholder.AddComponent<Image>();
            ghostImage.sprite = myImage.sprite;

            // Set the color to normal white, but with 35% alpha (transparency)
            ghostImage.color = new Color(1f, 1f, 1f, 0.35f);
        }

        placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());

        // 4. Pop the card out so it renders on top
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;

        transform.rotation = placeholder.transform.rotation;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move the actual card to follow the mouse
        transform.position = eventData.position;

        // Calculate where the placeholder should be based on mouse X position
        int newSiblingIndex = originalParent.childCount;

        for (int i = 0; i < originalParent.childCount; i++)
        {
            if (this.transform.position.x < originalParent.GetChild(i).position.x)
            {
                newSiblingIndex = i;

                // If the placeholder is already to the left of our target, offset by 1
                if (placeholder.transform.GetSiblingIndex() < newSiblingIndex)
                {
                    newSiblingIndex--;
                }
                break;
            }
        }

        // Move the placeholder to the new position to force other cards to shift
        placeholder.transform.SetSiblingIndex(newSiblingIndex);

        transform.rotation = placeholder.transform.rotation;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Put the card back into its container
        transform.SetParent(originalParent);

        // Swap the card into the exact index the placeholder was holding
        transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());

        // Destroy the placeholder
        Destroy(placeholder);
    }
}