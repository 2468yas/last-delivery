using UnityEngine;

// Yasin: a puddle. Walking through it makes her slide (PlayerMovement loses grip)
// and gives the stack a little jolt. Cars driving past it splash her (see Car).
// Made by HazardSpawner.
[RequireComponent(typeof(SpriteRenderer))]
public class Puddle : MonoBehaviour
{
    SpriteRenderer sr;
    float splashTimer;
    static readonly Color Normal = Color.white;
    static readonly Color Flash = new Color(0.85f, 0.95f, 1f, 1f);

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = BellrumArt.Puddle;
        sr.sortingOrder = -3;   // on the ground: above the road (-6) and sidewalks (-5), under everything else

        BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(1.7f, 0.55f);
    }

    void Update()
    {
        if (splashTimer > 0f)
        {
            splashTimer -= Time.deltaTime;
            float k = Mathf.Clamp01(splashTimer / 0.4f);
            sr.color = Color.Lerp(Normal, Flash, k);
            transform.localScale = Vector3.one * (1f + 0.25f * k);
        }
    }

    public void Splash()
    {
        splashTimer = 0.4f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = Owner(other);
        if (player == null) return;
        player.EnterPuddle();

        // stepping in at speed jolts the stack the way she's moving
        StackBalance stack = player.GetComponent<StackBalance>();
        float speed = player.Velocity.x;
        if (stack != null && Mathf.Abs(speed) > 1f)
            stack.Push(-Mathf.Sign(speed) * 0.25f);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement player = Owner(other);
        if (player != null) player.ExitPuddle();
    }

    static PlayerMovement Owner(Collider2D c)
    {
        return c.attachedRigidbody != null ? c.attachedRigidbody.GetComponent<PlayerMovement>() : null;
    }
}
