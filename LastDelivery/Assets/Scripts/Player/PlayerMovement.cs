using UnityEngine;

// Yasin: top-down movement with momentum. She speeds up and slows down instead of
// snapping to full speed, so quick turns throw the pizza stack around (StackBalance).
// Puddles make her slide, cars knock her back. Keyboard or controller (PlayerControls).
//
// ShiftManager turns this component off during intro cards and notes. That also
// pauses the sway and the cars, so nothing happens while you're reading.
//
// No setup needed: this adds StackBalance to the player, and on the delivery street
// (anywhere with a ShiftManager) it also creates the HazardSpawner.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    [Header("Top speed")]
    public float speed = 5f;                    // empty-handed
    public float carryingSpeed = 4.2f;          // with a stack of pizzas
    public float steadySpeedMultiplier = 0.5f;  // holding SHIFT / LB

    [Header("Momentum (units per second, per second)")]
    public float acceleration = 16f;
    public float deceleration = 14f;
    public float carryingAcceleration = 9f;
    public float carryingDeceleration = 7f;
    [Range(0f, 1f)] public float puddleGrip = 0.2f;        // how much control is left in a puddle
    [Range(0f, 1f)] public float grippyShoesGrip = 0.5f;   // with the Grippy Shoes upgrade

    // read by StackBalance and the hazards
    public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
    public Vector2 Acceleration { get; private set; }   // how hard she's speeding up / braking / turning
    public bool InPuddle => puddleCount > 0;
    public bool Steadying { get; private set; }
    public bool IsStunned => stunTimer > 0f;
    public Vector2 FeetPosition => feet != null ? (Vector2)feet.bounds.center : (Vector2)transform.position;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D feet;
    Vector2 input;
    Vector2 lastVelocity;
    int puddleCount;
    float stunTimer;
    float rumbleTimer;
    bool grippyShoes;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;          // top-down, so no falling
        rb.freezeRotation = true;     // don't spin when bumping stuff
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        sr = GetComponent<SpriteRenderer>();
        feet = GetComponent<Collider2D>();

        if (GetComponent<StackBalance>() == null) gameObject.AddComponent<StackBalance>();

        // hazards only on the real delivery street, not in the old PlayerTest scene
        if (FindAnyObjectByType<ShiftManager>() != null && FindAnyObjectByType<HazardSpawner>() == null)
            new GameObject("Hazards (Yasin)").AddComponent<HazardSpawner>();
    }

    void OnEnable()
    {
        grippyShoes = GameState.Get().data.ownedUpgrades.Contains("grippy_shoes");
        lastVelocity = rb != null ? rb.linearVelocity : Vector2.zero;
    }

    void OnDisable()
    {
        Acceleration = Vector2.zero;
        Steadying = false;
        stunTimer = 0f;
        rumbleTimer = 0f;
        PlayerControls.StopRumble();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        PlayerControls.StopRumble();
    }

    void Update()
    {
        input = PlayerControls.Move();
        Steadying = PlayerControls.SteadyHeld() && PizzaStack.HasPizza && !PizzaStack.Crushed;

        if (sr != null && Mathf.Abs(input.x) > 0.1f) sr.flipX = input.x < 0;   // face where she's walking

        if (rumbleTimer > 0f)
        {
            rumbleTimer -= Time.deltaTime;
            if (rumbleTimer <= 0f) PlayerControls.StopRumble();
        }
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        bool carrying = PizzaStack.HasPizza && !PizzaStack.Crushed;

        float top = carrying ? carryingSpeed : speed;
        if (Steadying) top *= steadySpeedMultiplier;
        float accel = carrying ? carryingAcceleration : acceleration;
        float decel = carrying ? carryingDeceleration : deceleration;
        if (grippyShoes) decel *= 1.25f;

        // puddles: she keeps sliding the way she was going
        if (InPuddle)
        {
            float grip = grippyShoes ? grippyShoesGrip : puddleGrip;
            accel *= grip;
            decel *= grip;
        }

        Vector2 target = input * top;
        if (stunTimer > 0f)
        {
            stunTimer -= dt;
            target = Vector2.zero;   // knocked back, no control for a moment
            decel *= 0.4f;
        }

        // rb velocity already includes bumps into walls and car knockback
        Vector2 v = rb.linearVelocity;
        float rate = target.sqrMagnitude > 0.0001f ? accel : decel;

        // pushing against the way she's moving brakes first (feels less floaty)
        if (Vector2.Dot(v, target) < 0f) rate = Mathf.Max(rate, decel);

        v = Vector2.MoveTowards(v, target, rate * dt);
        rb.linearVelocity = v;

        // measured from last step, so wall bumps and knockback count too
        Acceleration = (v - lastVelocity) / dt;
        lastVelocity = v;
    }

    // ---------------- called by hazards ----------------

    // a car hit: shove her and take control away for `stunSeconds`
    public void Knockback(Vector2 push, float stunSeconds)
    {
        if (!enabled) return;
        rb.linearVelocity += push;
        stunTimer = stunSeconds;
        RumbleFor(0.7f, 1f, 0.4f);
    }

    public void EnterPuddle() { puddleCount++; }
    public void ExitPuddle() { puddleCount = Mathf.Max(0, puddleCount - 1); }

    public void RumbleFor(float low, float high, float seconds)
    {
        if (!enabled) return;
        PlayerControls.Rumble(low, high);
        rumbleTimer = Mathf.Max(rumbleTimer, seconds);
    }
}
