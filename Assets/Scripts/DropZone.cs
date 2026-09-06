using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler
{
    public int maxCards = 13;

    // Update the originalParent as soon as the mouse hovers over this zone
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableCard card = eventData.pointerDrag.GetComponent<DraggableCard>();

        if (card != null && transform.childCount < maxCards)
        {
            card.originalParent = this.transform;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // The Drop is now handled by OnEndDrag in DraggableCard.cs 
        // swapping into the placeholder's sibling index.
    }
}