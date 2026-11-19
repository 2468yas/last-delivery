using UnityEngine;

// Side-scrolls the camera with the player along the street, without showing past the ends.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float minX = -8f;      // left edge of the street
    public float maxX = 62f;      // right edge of the street
    public float fixedY = 1.2f;
    public float smooth = 6f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null) return;
        float halfWidth = cam != null ? cam.orthographicSize * cam.aspect : 10f;
        float x = Mathf.Clamp(target.position.x, minX + halfWidth, maxX - halfWidth);
        Vector3 goal = new Vector3(x, fixedY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
