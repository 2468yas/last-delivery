using UnityEngine;

// The pizza shop. Orders are picked up and the shift ends at its door.
public class CornerSlice : MonoBehaviour
{
    public Vector2 doorOffset = new Vector2(0f, -2.4f);

    public static CornerSlice Instance { get; private set; }

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    public Vector2 DoorPosition => (Vector2)transform.position + doorOffset;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(DoorPosition, 0.4f);
    }
}
