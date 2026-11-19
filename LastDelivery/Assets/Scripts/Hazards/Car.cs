using UnityEngine;

// Yasin: a car driving along one lane of Bellrum street.
//   - hits her if she's on the road: knockback + a big shove to the stack (usually knocks it over)
//   - passing close by: a gust that shoves the stack a little
//   - driving past a puddle near her: splash, a bigger shove
// Made by HazardSpawner.
[RequireComponent(typeof(SpriteRenderer))]
public class Car : MonoBehaviour
{
    const float GustRange = 2.6f;      // how far (up/down) a passing car still pushes the stack
    const float SplashRange = 1.7f;    // how close to a puddle she has to be to get splashed

    HazardSpawner spawner;
    Rigidbody2D rb;
    SpriteRenderer sr;
    float dir;          // +1 east, -1 west
    float speed;
    float endX;
    bool hitPlayer;

    public void Init(HazardSpawner owner, float direction, float carSpeed, float despawnX)
    {
        spawner = owner;
        dir = direction;
        speed = carSpeed;
        endX = despawnX;

        sr = GetComponent<SpriteRenderer>();
        sr.sprite = BellrumArt.Car;
        sr.flipX = dir < 0;   // car art faces right
        sr.sortingOrder = 4;
        // a random paint job so they don't all look the same
        Color[] paint = { Color.white, new Color(1f, 0.75f, 0.75f), new Color(0.8f, 1f, 0.8f), new Color(1f, 0.95f, 0.7f), new Color(0.85f, 0.85f, 0.85f) };
        sr.color = paint[Random.Range(0, paint.Length)];

        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        Vector2 size = sr.sprite != null ? (Vector2)sr.sprite.bounds.size : new Vector2(2f, 1.1f);
        box.size = new Vector2(size.x * 0.9f, size.y * 0.6f);   // a bit smaller than the art, so near misses feel fair
    }

    void FixedUpdate()
    {
        if (spawner == null || spawner.Paused) return;

        Vector2 before = rb.position;
        Vector2 after = before + new Vector2(dir * speed * Time.fixedDeltaTime, 0f);
        rb.MovePosition(after);

        PlayerMovement player = spawner.Player;
        StackBalance stack = spawner.Stack;
        if (player == null || stack == null) return;

        Vector2 feet = player.FeetPosition;

        // gust: the moment the car passes her
        if (Crossed(before.x, after.x, feet.x))
        {
            float dy = Mathf.Abs(feet.y - after.y);
            if (dy < GustRange && !hitPlayer)
            {
                float closeness = 1f - dy / GustRange;
                stack.Push(dir * 0.45f * closeness * (speed / 5f));
                if (closeness > 0.4f) player.RumbleFor(0.2f, 0.1f, 0.2f);
            }
        }

        // splash: driving past a puddle she's standing near
        foreach (Puddle p in spawner.Puddles)
        {
            if (p == null || !Crossed(before.x, after.x, p.transform.position.x)) continue;
            if (Mathf.Abs(p.transform.position.y - after.y) > 1.5f) continue;   // puddle not near this lane
            if (Vector2.Distance(feet, p.transform.position) > SplashRange) continue;

            p.Splash();
            if (PizzaStack.HasPizza && !PizzaStack.Crushed)
            {
                stack.Push(dir * 0.7f);
                FloatingText.Spawn("Splash!", feet + new Vector2(0f, 2.6f), new Color(0.6f, 0.8f, 1f));
                AudioManager.Play("wobble", 0.7f);
            }
        }

        if ((dir > 0 && after.x > endX) || (dir < 0 && after.x < endX)) Destroy(gameObject);
    }

    void Update()
    {
        if (spawner == null || spawner.Player == null) return;
        // draw in front of her when the car is lower on screen (top-down depth)
        sr.sortingOrder = transform.position.y < spawner.Player.FeetPosition.y ? 6 : 4;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hitPlayer || spawner == null || spawner.Paused) return;
        PlayerMovement player = other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<PlayerMovement>() : null;
        if (player == null) return;

        hitPlayer = true;
        float away = Mathf.Sign(player.FeetPosition.y - transform.position.y);
        if (away == 0f) away = 1f;
        player.Knockback(new Vector2(dir * 3f, away * 5f), 0.35f);
        AudioManager.Play("wrong");

        if (PizzaStack.HasPizza && !PizzaStack.Crushed && spawner.Stack != null)
        {
            spawner.Stack.Push(dir * 1.6f);
            FloatingText.Spawn("HONK!", (Vector2)transform.position + new Vector2(0f, 1.2f), new Color(1f, 0.5f, 0.4f));
        }
        else
        {
            FloatingText.Spawn("HONK!", (Vector2)transform.position + new Vector2(0f, 1.2f), new Color(1f, 0.85f, 0.4f));
        }
    }

    static bool Crossed(float from, float to, float x)
    {
        return (from - x) * (to - x) <= 0f && from != to;
    }
}
