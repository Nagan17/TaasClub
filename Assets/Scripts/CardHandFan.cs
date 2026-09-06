using UnityEngine;

[ExecuteInEditMode] // This lets you preview the curve in the editor without hitting Play
public class CardHandFan : MonoBehaviour
{
    [Header("Arc Settings")]
    public float spacing = 60f;        // Horizontal distance between cards
    public float anglePerCard = 4f;    // How much each card rotates
    public float yDropPerCard = 1.5f;  // How fast the arc drops off on the sides

    [Header("Positioning")]
    public float verticalOffset = 0f; // Move the whole fan up (positive) or down (negative)

    void Update()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        float midIndex = (childCount - 1) / 2f;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);

            float distanceFromCenter = i - midIndex;

            float targetX = distanceFromCenter * spacing;

            // Apply the parabolic drop, then add the vertical offset
            float targetY = -(distanceFromCenter * distanceFromCenter) * yDropPerCard + verticalOffset;

            float targetZRotation = -distanceFromCenter * anglePerCard;

            child.localPosition = new Vector3(targetX, targetY, 0);
            child.localRotation = Quaternion.Euler(0, 0, targetZRotation);
        }
    }
}