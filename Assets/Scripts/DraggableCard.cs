using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform originalParent = null; //[cite: 1]
    private CanvasGroup canvasGroup; //[cite: 1]
    private GameObject placeholder = null; //[cite: 1]
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector3 pointerOffset;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>(); //[cite: 1]
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; //[cite: 1]

        // 1. Create a dummy placeholder object[cite: 1]
        placeholder = new GameObject("Placeholder"); //[cite: 1]
        placeholder.transform.SetParent(originalParent); //[cite: 1]

        RectTransform ghostRT = placeholder.AddComponent<RectTransform>(); //[cite: 1]
        ghostRT.sizeDelta = rectTransform.sizeDelta; //[cite: 1]
        ghostRT.localScale = rectTransform.localScale; //[cite: 1]

        // 3. Copy the image and make it semi-transparent[cite: 1]
        Image myImage = GetComponent<Image>(); //[cite: 1]
        if (myImage != null) //[cite: 1]
        {
            Image ghostImage = placeholder.AddComponent<Image>(); //[cite: 1]
            ghostImage.sprite = myImage.sprite; //[cite: 1]
            ghostImage.color = new Color(1f, 1f, 1f, 0.35f); //[cite: 1]
        }

        placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex()); //[cite: 1]

        // Calculate world-space pointer offset to prevent the 1-frame jump/snap
        Camera worldCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, worldCamera, out Vector3 globalMousePos))
        {
            pointerOffset = transform.position - globalMousePos;
        }

        // 4. Pop the card out so it renders on top[cite: 1]
        transform.SetParent(transform.root); //[cite: 1]
        canvasGroup.blocksRaycasts = false; //[cite: 1]

        transform.rotation = placeholder.transform.rotation; //[cite: 1]
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Smoothly follow the mouse using the calculated offset instead of raw screen position
        Camera worldCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, worldCamera, out Vector3 globalMousePos))
        {
            transform.position = globalMousePos + pointerOffset;
        }

        // Calculate where the placeholder should be based on mouse X position[cite: 1]
        int newSiblingIndex = originalParent.childCount; //[cite: 1]

        for (int i = 0; i < originalParent.childCount; i++) //[cite: 1]
        {
            if (this.transform.position.x < originalParent.GetChild(i).position.x) //[cite: 1]
            {
                newSiblingIndex = i; //[cite: 1]

                // If the placeholder is already to the left of our target, offset by 1[cite: 1]
                if (placeholder.transform.GetSiblingIndex() < newSiblingIndex) //[cite: 1]
                {
                    newSiblingIndex--; //[cite: 1]
                }
                break; //[cite: 1]
            }
        }

        // Move the placeholder to the new position to force other cards to shift[cite: 1]
        placeholder.transform.SetSiblingIndex(newSiblingIndex); //[cite: 1]

        transform.rotation = placeholder.transform.rotation; //[cite: 1]
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true; //[cite: 1]

        // Put the card back into its container[cite: 1]
        transform.SetParent(originalParent); //[cite: 1]

        // Swap the card into the exact index the placeholder was holding[cite: 1]
        transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex()); //[cite: 1]

        // Destroy the placeholder[cite: 1]
        Destroy(placeholder); //[cite: 1]
    }
}