using System.Collections;
using UnityEngine;

// Yasin: the pizza stack, the sway meter and the stack falling over.
//
// The stack works like balancing a broom on your hand:
//   - speeding up, stopping and turning make it lean (that's the momentum from PlayerMovement)
//   - her hands pull it back to the middle, but only so hard, so past about 65% it gets away from her
//   - moving the way it leans pulls it back up
//   - holding SHIFT / LB steadies it (stronger hands) but she walks at half speed
//   - cars, splashes and wind (HazardSpawner) shove it with Push()
// At 100% it falls: PizzaStack.Crush() (Shubh's code counts the delivery as crushed).
//
// Added to the player automatically by PlayerMovement, no setup needed.
public class StackBalance : MonoBehaviour
{
    [Header("Sway feel (tested in a quick sim, tweak while playing)")]
    public float handStrength = 6f;        // pull back to the middle
    public float handLimit = 0.45f;        // hands can't pull harder than this much lean
    public float gravity = 4.2f;           // the further it leans, the harder it wants to fall
    public float damping = 1.4f;
    public float accelEffect = 0.16f;      // how much speeding up / braking tips it
    public float walkJitter = 0.35f;       // random wobble while walking, grows each day
    public float steadyStrength = 1.6f;    // hands x this while steadying

    [Header("Look")]
    public int boxCount = 3;
    public float maxAngle = 40f;           // tilt at 100% sway
    public Vector2 handOffset = new Vector2(0f, 0.05f);   // where she holds the bottom box
    public float meterHeight = 1.55f;

    public float Sway => sway;             // -1 (falling left) .. 1 (falling right)
    public const float DangerZone = 0.6f;

    PlayerMovement movement;
    Transform pivot;
    SpriteRenderer[] boxes;
    GameObject meter;
    Transform meterMarker;
    SpriteRenderer meterMarkerRenderer;
    TextMesh hint;

    float sway, swayVelocity;
    float noiseSeed;
    float dayFactor = 1f;
    float bagFactor = 1f;     // Better Bag upgrade: less sway
    float holderFactor = 1f;  // Box Holder upgrade: stronger hands
    float wobbleCooldown;
    bool falling;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        noiseSeed = Random.value * 100f;
        BuildVisuals();
        ShowStack(false);
    }

    void OnEnable()
    {
        PizzaStack.PickedUp += OnPickedUp;
        PizzaStack.WasCrushed += OnCrushed;
        PizzaStack.Delivered += OnDelivered;
    }

    void OnDisable()
    {
        PizzaStack.PickedUp -= OnPickedUp;
        PizzaStack.WasCrushed -= OnCrushed;
        PizzaStack.Delivered -= OnDelivered;
    }

    void Start()
    {
        SaveData d = GameState.Get().data;
        int level = StoryData.GetDay(d.currentDay).hazardLevel;
        dayFactor = 1f + 0.25f * (level - 1);                          // Monday 1x ... storm 2x wobble
        bagFactor = d.ownedUpgrades.Contains("better_bag") ? 0.75f : 1f;
        holderFactor = d.ownedUpgrades.Contains("box_holder") ? 1.25f : 1f;
    }

    // other scripts shove the stack with this (positive = to the right)
    public void Push(float amount)
    {
        if (!Active) return;
        swayVelocity += amount * bagFactor;
    }

    // swaying only while she's carrying an intact stack and allowed to move
    bool Active => PizzaStack.HasPizza && !PizzaStack.Crushed && !falling && movement != null && movement.enabled;

    // ---------------- sway ----------------

    void FixedUpdate()
    {
        if (!Active) return;
        float dt = Time.fixedDeltaTime;
        bool steady = movement.Steadying;

        float hands = handStrength * holderFactor * (steady ? steadyStrength : 1f);
        float handPull = -Mathf.Clamp(sway, -handLimit, handLimit) * hands;
        float fall = gravity * sway;

        // speeding up to the right leaves the stack behind (it leans left), like a real stack
        float fromMomentum = -accelEffect * bagFactor * movement.Acceleration.x;

        // a little wobble while walking (more on later days, none when standing still)
        float walk = movement.Velocity.magnitude / Mathf.Max(0.01f, movement.carryingSpeed);
        float noise = (Mathf.PerlinNoise(Time.time * 1.7f, noiseSeed) - 0.5f) * 2f;
        float jitter = noise * walkJitter * walk * dayFactor * bagFactor * (steady ? 0.3f : 1f);
        if (movement.InPuddle) jitter *= 2f;   // slippery

        swayVelocity += (handPull + fall + fromMomentum + jitter) * dt;
        swayVelocity /= 1f + damping * (steady ? 2f : 1f) * dt;
        sway += swayVelocity * dt;

        if (Mathf.Abs(sway) >= 1f)
        {
            sway = Mathf.Sign(sway);
            PizzaStack.Crush();   // fires WasCrushed -> OnCrushed plays the fall
        }
    }

    void Update()
    {
        // keep the boxes in sync even if a shift starts with PizzaStack.Clear() (no event)
        if (!falling)
        {
            bool shouldShow = PizzaStack.HasPizza;
            if (pivot.gameObject.activeSelf != shouldShow) ShowStack(shouldShow);
        }

        if (wobbleCooldown > 0f) wobbleCooldown -= Time.deltaTime;
        if (Active && Mathf.Abs(sway) > DangerZone && wobbleCooldown <= 0f)
        {
            AudioManager.Play("wobble", 0.8f);
            movement.RumbleFor(0.15f, 0.35f, 0.25f);
            wobbleCooldown = 1.2f;
        }

        UpdateVisuals();
    }

    // ---------------- events from Shubh's PizzaStack ----------------

    void OnPickedUp()
    {
        sway = 0f;
        swayVelocity = 0f;
        falling = false;
        StopAllCoroutines();
        ResetBoxes();
        ShowStack(true);
    }

    void OnCrushed()
    {
        if (!falling) StartCoroutine(FallOver(Mathf.Sign(sway == 0f ? 1f : sway)));
        if (movement != null) movement.RumbleFor(1f, 1f, 0.5f);
    }

    void OnDelivered()
    {
        StopAllCoroutines();
        falling = false;
        ShowStack(false);
    }

    // boxes fly off the way it was leaning, then she picks up one squashed box
    IEnumerator FallOver(float dir)
    {
        falling = true;
        meter.SetActive(false);
        hint.gameObject.SetActive(false);

        Vector3[] vel = new Vector3[boxes.Length];
        float[] spin = new float[boxes.Length];
        for (int i = 0; i < boxes.Length; i++)
        {
            boxes[i].transform.SetParent(null, true);   // let them fly free in the world
            vel[i] = new Vector3(dir * Random.Range(1.5f, 3.5f), Random.Range(2f, 4f), 0f);
            spin[i] = -dir * Random.Range(200f, 500f);
        }

        float t = 0f;
        const float duration = 0.9f;
        while (t < duration)
        {
            float dt = Time.deltaTime;
            t += dt;
            for (int i = 0; i < boxes.Length; i++)
            {
                vel[i].y -= 12f * dt;   // fake gravity
                boxes[i].transform.position += vel[i] * dt;
                boxes[i].transform.Rotate(0f, 0f, spin[i] * dt);
                Color c = boxes[i].color;
                c.a = 1f - Mathf.Clamp01((t - 0.5f) / (duration - 0.5f));
                boxes[i].color = c;
            }
            yield return null;
        }

        // she grabs what's left: one dented, darker box
        ResetBoxes();
        for (int i = 1; i < boxes.Length; i++) boxes[i].gameObject.SetActive(false);
        boxes[0].color = new Color(0.7f, 0.55f, 0.45f);
        boxes[0].transform.localScale = new Vector3(1f, 0.6f, 1f);
        boxes[0].transform.localRotation = Quaternion.Euler(0f, 0f, -8f * dir);
        pivot.localRotation = Quaternion.identity;
        falling = false;
    }

    // ---------------- visuals ----------------

    void BuildVisuals()
    {
        pivot = new GameObject("Pizza Stack").transform;
        pivot.SetParent(transform, false);
        pivot.localPosition = handOffset;

        Sprite box = BellrumArt.PizzaBox;
        boxes = new SpriteRenderer[boxCount];
        for (int i = 0; i < boxCount; i++)
        {
            SpriteRenderer sr = new GameObject("Pizza Box " + (i + 1)).AddComponent<SpriteRenderer>();
            sr.sprite = box;
            sr.sortingOrder = 6 + i;   // player is 5
            boxes[i] = sr;
        }
        ResetBoxes();

        // sway meter: dark bar with green / yellow / red zones and a white marker
        meter = new GameObject("Sway Meter");
        meter.transform.SetParent(transform, false);
        meter.transform.localPosition = new Vector3(0f, meterHeight, 0f);
        Bar("Back", 0f, 1.5f, 0.2f, new Color(0f, 0f, 0f, 0.7f), 20);
        Bar("Green", 0f, 1.4f * DangerZone, 0.1f, new Color(0.35f, 0.85f, 0.4f), 21);
        Bar("Yellow L", -0.7f * (DangerZone + 0.85f) / 2f, 0.7f * (0.85f - DangerZone), 0.1f, new Color(1f, 0.8f, 0.25f), 21);
        Bar("Yellow R", 0.7f * (DangerZone + 0.85f) / 2f, 0.7f * (0.85f - DangerZone), 0.1f, new Color(1f, 0.8f, 0.25f), 21);
        Bar("Red L", -0.7f * (0.85f + 1f) / 2f, 0.7f * 0.15f, 0.1f, new Color(0.95f, 0.3f, 0.25f), 21);
        Bar("Red R", 0.7f * (0.85f + 1f) / 2f, 0.7f * 0.15f, 0.1f, new Color(0.95f, 0.3f, 0.25f), 21);
        meterMarkerRenderer = Bar("Marker", 0f, 0.07f, 0.26f, Color.white, 22);
        meterMarker = meterMarkerRenderer.transform;

        hint = BellrumArt.MakeText(transform, new Vector3(0f, meterHeight + 0.35f, 0f), "", new Color(1f, 0.9f, 0.4f), 23);
    }

    SpriteRenderer Bar(string name, float x, float width, float height, Color color, int order)
    {
        SpriteRenderer sr = new GameObject(name).AddComponent<SpriteRenderer>();
        sr.transform.SetParent(meter.transform, false);
        sr.transform.localPosition = new Vector3(x, 0f, 0f);
        sr.transform.localScale = new Vector3(width, height, 1f);
        sr.sprite = BellrumArt.White;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    // put every box back on the stack, upright and solid
    void ResetBoxes()
    {
        float boxHeight = boxes.Length > 0 && boxes[0].sprite != null ? boxes[0].sprite.bounds.size.y : 0.3125f;
        for (int i = 0; i < boxes.Length; i++)
        {
            Transform t = boxes[i].transform;
            t.SetParent(pivot, false);
            t.localPosition = new Vector3(0f, boxHeight * (i + 0.5f), 0f);
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            boxes[i].color = Color.white;
            boxes[i].gameObject.SetActive(true);
        }
        pivot.localRotation = Quaternion.identity;
    }

    void ShowStack(bool on)
    {
        if (!on)
        {
            // boxes that flew off are no longer children, so hide them directly
            foreach (SpriteRenderer b in boxes) b.gameObject.SetActive(false);
            ResetBoxes();
            sway = 0f;
            swayVelocity = 0f;
        }
        pivot.gameObject.SetActive(on);
        foreach (SpriteRenderer b in boxes) b.gameObject.SetActive(on);
        meter.SetActive(on);
        hint.gameObject.SetActive(on);
    }

    void UpdateVisuals()
    {
        if (falling || !pivot.gameObject.activeSelf) return;

        bool intact = !PizzaStack.Crushed;
        meter.SetActive(intact);
        hint.gameObject.SetActive(intact);
        if (!intact) return;

        // whole stack tilts at her hands, top boxes slide a bit further (looks floppier)
        pivot.localRotation = Quaternion.Euler(0f, 0f, -sway * maxAngle);
        float boxHeight = boxes[0].sprite != null ? boxes[0].sprite.bounds.size.y : 0.3125f;
        for (int i = 0; i < boxes.Length; i++)
            boxes[i].transform.localPosition = new Vector3(sway * 0.06f * i * i, boxHeight * (i + 0.5f), 0f);

        meterMarker.localPosition = new Vector3(Mathf.Clamp(sway, -1f, 1f) * 0.7f, 0f, 0f);
        float a = Mathf.Abs(sway);
        meterMarkerRenderer.color = a > 0.85f ? new Color(1f, 0.4f, 0.35f) : a > DangerZone ? new Color(1f, 0.9f, 0.4f) : Color.white;

        if (a > DangerZone && !movement.Steadying)
            hint.text = "Hold " + PlayerControls.SteadyName + " to steady!";
        else if (movement.Steadying)
            hint.text = "steady...";
        else
            hint.text = "";
    }
}
