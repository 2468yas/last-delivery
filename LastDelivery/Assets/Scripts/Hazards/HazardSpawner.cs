using System.Collections.Generic;
using UnityEngine;

// Yasin: cars, puddles and wind on Bellrum street, scaled by the day's hazardLevel
// (StoryData: 1 = calm Monday ... 5 = storm).
//
//            cars max   car every   speed    puddles   wind
//   Monday   1  ->  2   ~8 s        ~4       3         -
//   Tuesday  2  ->  3   ~6.5 s      ~5       5         -
//   Wed      3  ->  4   ~5 s        ~5.5     7         light gusts
//   ...      up to 6    ~2.5 s      ~7       11        strong gusts
//
// PlayerMovement creates this on the delivery street, so TestStreet.unity doesn't need editing.
// Everything freezes while the player can't move (intro card, notes).
public class HazardSpawner : MonoBehaviour
{
    [Header("Street layout (matches TestStreetSetup)")]
    public float streetLeft = -8f;
    public float streetRight = 62f;
    public float northLaneY = 0.8f;       // drives west (right to left)
    public float southLaneY = -0.8f;      // drives east (left to right)
    public float northSidewalkY = 2.05f;
    public float southSidewalkY = -2.05f;
    public float northGutterY = 1.3f;     // edge of the road next to the houses

    public int Level { get; private set; }
    public bool Paused => player == null || !player.enabled;
    public PlayerMovement Player => player;
    public StackBalance Stack => stack;
    public readonly List<Puddle> Puddles = new List<Puddle>();

    PlayerMovement player;
    StackBalance stack;
    readonly List<Car> cars = new List<Car>();
    float carTimer;
    float windTimer;

    void Start()
    {
        player = PlayerMovement.Instance != null ? PlayerMovement.Instance : FindAnyObjectByType<PlayerMovement>();
        if (player != null) stack = player.GetComponent<StackBalance>();
        Level = Mathf.Clamp(StoryData.GetDay(GameState.Get().data.currentDay).hazardLevel, 1, 5);

        SpawnPuddles(1 + Level * 2);
        carTimer = 3f;   // first car shortly after the shift starts
        windTimer = NextWindDelay();
    }

    void Update()
    {
        if (Paused) return;
        cars.RemoveAll(c => c == null);

        // cars
        carTimer -= Time.deltaTime;
        int maxCars = Level + 1;
        if (carTimer <= 0f && cars.Count < maxCars)
        {
            SpawnCar();
            float every = Mathf.Lerp(8f, 2.5f, (Level - 1) / 4f);
            carTimer = every * Random.Range(0.7f, 1.3f);
        }

        // wind from Wednesday on (hazardLevel 3+)
        if (Level >= 3)
        {
            windTimer -= Time.deltaTime;
            if (windTimer <= 0f)
            {
                Gust();
                windTimer = NextWindDelay();
            }
        }
    }

    float NextWindDelay()
    {
        return Random.Range(6f, 10f) / Mathf.Max(1, Level - 2);
    }

    void Gust()
    {
        if (stack == null || !PizzaStack.HasPizza || PizzaStack.Crushed) return;
        float dir = Random.value < 0.5f ? -1f : 1f;
        stack.Push(dir * 0.15f * Level);
        FloatingText.Spawn(dir > 0 ? "~ wind ~ >>" : "<< ~ wind ~", (Vector2)player.transform.position + new Vector2(0f, 2.3f),
            new Color(0.75f, 0.85f, 1f));
        AudioManager.Play("wobble", 0.5f);
    }

    void SpawnCar()
    {
        bool westbound = Random.value < 0.5f;
        float y = westbound ? northLaneY : southLaneY;
        float x = westbound ? streetRight + 4f : streetLeft - 4f;
        float speed = 3.4f + 0.7f * Level + Random.Range(-0.4f, 0.4f);

        Car car = new GameObject("Car").AddComponent<Car>();
        car.transform.SetParent(transform, false);
        car.transform.position = new Vector3(x, y, 0f);
        car.Init(this, westbound ? -1f : 1f, speed, westbound ? streetLeft - 6f : streetRight + 6f);
        cars.Add(car);
    }

    void SpawnPuddles(int count)
    {
        // keep doors clear so delivering is never impossible
        List<float> doors = new List<float>();
        foreach (HouseVisual h in HouseVisual.All) doors.Add(h.DoorPosition.x);
        if (CornerSlice.Instance != null) doors.Add(CornerSlice.Instance.DoorPosition.x);

        int placed = 0, tries = 0;
        while (placed < count && tries < 300)
        {
            tries++;
            float x = Random.Range(streetLeft + 4f, streetRight - 4f);

            // mostly on the north side where she walks: sidewalk or the gutter next to it
            float roll = Random.value;
            float y = roll < 0.45f ? northSidewalkY : roll < 0.8f ? northGutterY : Random.Range(southLaneY, northLaneY);

            bool blocked = false;
            if (y > northGutterY - 0.1f)
                foreach (float d in doors) if (Mathf.Abs(d - x) < 1.6f) { blocked = true; break; }
            foreach (Puddle p in Puddles) if (Vector2.Distance(p.transform.position, new Vector2(x, y)) < 3f) { blocked = true; break; }
            if (blocked) continue;

            Puddle puddle = new GameObject("Puddle").AddComponent<Puddle>();
            puddle.transform.SetParent(transform, false);
            puddle.transform.position = new Vector3(x, y, 0f);
            Puddles.Add(puddle);
            placed++;
        }
    }
}
