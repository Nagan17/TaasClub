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

    [Header("Make-Way Animation")]
    public float positionSmoothTime = 0.12f; // Lower = snappier, higher = floatier slide
    public float rotationSpeed = 12f;

    private Vector3[] velocities = new Vector3[0]; // SmoothDamp needs per-card velocity state

    void Update()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        if(velocities.Length != childCount)
        {
            velocities = new Vector3[childCount];
        }

        float midIndex = (childCount - 1) / 2f;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);

            float distanceFromCenter = i - midIndex;

            float targetX = distanceFromCenter * spacing;

            // Apply the parabolic drop, then add the vertical offset
            float targetY = -(distanceFromCenter * distanceFromCenter) * yDropPerCard + verticalOffset;

            float targetZRotation = -distanceFromCenter * anglePerCard;

            Vector3 targerPos = new Vector3(targetX, targetY, 0);
            Quaternion tragetRot = Quaternion.Euler(0, 0, targetZRotation);

            if (Application.isPlaying)
            {
                child.localPosition = Vector3.SmoothDamp(child.localPosition, targerPos, ref velocities[i], positionSmoothTime);

                child.localRotation = Quaternion.RotateTowards(child.localRotation, tragetRot, rotationSpeed * 360.0f * Time.deltaTime);
            }
            else
            {

                child.localPosition = targerPos;
                child.localRotation = tragetRot;
            }
        }
    }
}