using UnityEngine;

// Yasin: little world-space text that floats up and fades ("Splash!", "HONK!", "~ wind ~").
public class FloatingText : MonoBehaviour
{
    const float Life = 1.1f;

    TextMesh text;
    Color color;
    float age;

    public static void Spawn(string message, Vector2 position, Color color)
    {
        GameObject root = new GameObject("Floating Text");
        root.transform.position = position;
        FloatingText ft = root.AddComponent<FloatingText>();
        ft.color = color;
        ft.text = BellrumArt.MakeText(root.transform, Vector3.zero, message, color, 30);
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.position += Vector3.up * 0.8f * Time.deltaTime;
        Color c = color;
        c.a = 1f - Mathf.Clamp01((age - 0.5f) / (Life - 0.5f));
        text.color = c;
        if (age >= Life) Destroy(gameObject);
    }
}
