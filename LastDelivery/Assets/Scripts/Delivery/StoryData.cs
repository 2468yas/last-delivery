using System.Collections.Generic;
using UnityEngine;

// All the writing for the shifts in one place: intro cards, orders, resident notes
// and the memory fragment each shift gives. Edit the text here, no scene changes needed.
public class OrderInfo
{
    public string houseId;
    public string food;
    public string clue;   // what the player has to spot on the house

    public OrderInfo(string houseId, string food)
    {
        this.houseId = houseId;
        this.food = food;
        clue = StoryData.Clue(houseId);
    }
}

public class DayInfo
{
    public string name;
    public string[] intro;          // lines on the intro card (keep it short, < 30 seconds)
    public List<OrderInfo> orders;
    public string goodMemory;       // fragment if most pizzas arrived intact
    public string badMemory;        // fragment if most were crushed
    public string music;            // file in Resources/Audio
    public Color grass;             // camera background
    public Color tint;              // weather overlay on the screen (alpha = strength)
    public int hazardLevel;         // for Yasin's hazards: 1 = calm ... 5 = storm
}

public static class StoryData
{
    // ---------------- houses ----------------
    static readonly Dictionary<string, string> clues = new Dictionary<string, string>
    {
        { "nate_1_elm",      "a basketball hoop" },
        { "june_14_elm",     "a vegetable garden" },
        { "dana_7_elm",      "a red scooter out front" },
        { "parks_22_elm",    "kids' bikes on the lawn" },
        { "theo_5_willow",   "band posters in the windows" },
        { "kessler_9_river", "a NO SOLICITING sign" },
        { "rourke_3_river",  "a FOR RENT sign" },
        { "home_12_mill",    "a cactus in the window" },
    };

    public static string Clue(string houseId)
    {
        return clues.TryGetValue(houseId, out string c) ? c : "the right address";
    }

    // default note for each house: { intact, crushed }
    static readonly Dictionary<string, string[]> notes = new Dictionary<string, string[]>
    {
        { "nate_1_elm",      new[] { "Perfect, still hot! Come shoot hoops sometime, like old times.",
                                     "Why is my pizza smashed?! Did you guys DROP IT OR SOMETHING???" } },
        { "june_14_elm",     new[] { "Still warm! Just like when you were little and came over every Sunday. :)",
                                     "Cold and squashed. You never visit anymore either." } },
        { "dana_7_elm",      new[] { "Okay, fine, that was a clean delivery. Don't let it go to your head.",
                                     "Ha! And Sal says YOU'RE the careful one?" } },
        { "parks_22_elm",    new[] { "The twins say thank you!! They drew you a picture. (It's a pizza with legs.)",
                                     "The kids cried. Maybe someone else should babysit next time." } },
        { "theo_5_willow",   new[] { "You remembered pineapple! Text me back sometime, okay?",
                                     "...you didn't even knock. You never answer anymore." } },
        { "kessler_9_river", new[] { "Hmph. Acceptable. Don't park on my lawn.",
                                     "This is EXACTLY why I complain about you." } },
        { "rourke_3_river",  new[] { "Good. Now about that rent you owe me...",
                                     "Squashed, like your excuses. Rent is due FRIDAY." } },
        { "home_12_mill",    new[] { "You left the light on for yourself. Why does this door feel so familiar?",
                                     "Nobody answers. The note on the door is in your handwriting." } },
    };

    // notes that only appear on certain days, so the town gets stranger: "houseId|day|intact"
    static readonly Dictionary<string, string> specialNotes = new Dictionary<string, string>
    {
        { "june_14_elm|2|True",      "Thank you, sweetheart. Why do you look so tired? You should rest before Friday." },
        { "june_14_elm|2|False",     "You don't have to keep coming. I know about Friday." },
        { "parks_22_elm|2|True",     "The twins asked why you were outside our window last night. Silly kids." },
        { "kessler_9_river|2|True",  "You again. You've delivered here every day for years. Haven't you?" },
        { "theo_5_willow|3|True",    "I keep calling. It goes straight to voicemail. Where ARE you?" },
        { "theo_5_willow|3|False",   "This isn't real, you know. None of us are." },
        { "nate_1_elm|3|True",       "Hey... be careful on the bridge tonight. It's slick when it rains." },
        { "rourke_3_river|3|True",   "Your apartment is still empty. Funny. I could have sworn I saw you go in." },
    };

    public static string Note(string houseId, int day, bool intact)
    {
        if (specialNotes.TryGetValue(houseId + "|" + day + "|" + intact, out string special)) return special;
        if (notes.TryGetValue(houseId, out string[] pair)) return intact ? pair[0] : pair[1];
        return intact ? "Thanks!" : "...";
    }

    // ---------------- days ----------------
    static DayInfo[] days;

    public static DayInfo GetDay(int day)
    {
        if (days == null) Build();
        return days[Mathf.Clamp(day, 1, 5) - 1];
    }

    static void Build()
    {
        days = new DayInfo[5];

        days[0] = new DayInfo
        {
            name = "Monday",
            intro = new[] { "BELLRUM.  MONDAY, 5:00 PM.",
                            "Your shift at Corner Slice starts now.",
                            "Read each order, find the house that matches the clue, and keep the stack steady.",
                            "Every pizza you deliver decides what this town remembers." },
            orders = new List<OrderInfo> {
                new OrderInfo("june_14_elm", "Large Veggie, no meat"),
                new OrderInfo("parks_22_elm", "2 Kids' Cheese"),
                new OrderInfo("rourke_3_river", "Meat Lovers"),
                new OrderInfo("nate_1_elm", "Large Pepperoni"),
                new OrderInfo("theo_5_willow", "Medium Hawaiian") },
            goodMemory = "Her first day at Corner Slice. Sal let her keep all the tips.",
            badMemory = "Something about Monday she'd rather not remember...",
            music = "music_calm",
            grass = new Color(0.42f, 0.66f, 0.31f),
            tint = new Color(1f, 0.95f, 0.7f, 0.05f),
            hazardLevel = 1,
        };

        days[1] = new DayInfo
        {
            name = "Tuesday",
            intro = new[] { "TUESDAY.  CLOUDS OVER BELLRUM.",
                            "Funny... the street feels a little longer today.",
                            "Some of the houses look like they've been here forever. Some look new." },
            orders = new List<OrderInfo> {
                new OrderInfo("kessler_9_river", "Plain Cheese, extra napkins"),
                new OrderInfo("dana_7_elm", "Large Pepperoni"),
                new OrderInfo("june_14_elm", "Large Veggie, no meat"),
                new OrderInfo("rourke_3_river", "Meat Lovers"),
                new OrderInfo("theo_5_willow", "Medium Hawaiian"),
                new OrderInfo("parks_22_elm", "2 Kids' Cheese") },
            goodMemory = "Riding past Mill Bridge in the sun, before they closed it.",
            badMemory = "An argument on the phone. Whose voice was it?",
            music = "music_uneasy",
            grass = new Color(0.36f, 0.55f, 0.30f),
            tint = new Color(0.35f, 0.38f, 0.45f, 0.18f),
            hazardLevel = 2,
        };

        days[2] = new DayInfo
        {
            name = "Wednesday",
            intro = new[] { "WEDNESDAY.  IT'S STARTING TO RAIN.",
                            "The puddles glow, just a little.",
                            "One of tonight's orders is going to an address you know too well." },
            orders = new List<OrderInfo> {
                new OrderInfo("theo_5_willow", "Medium Hawaiian"),
                new OrderInfo("june_14_elm", "Large Veggie, no meat"),
                new OrderInfo("nate_1_elm", "Large Pepperoni"),
                new OrderInfo("kessler_9_river", "Plain Cheese"),
                new OrderInfo("rourke_3_river", "Meat Lovers"),
                new OrderInfo("home_12_mill", "Small Cheese, for one") },
            goodMemory = "Grandma June teaching her to stretch dough. Flour everywhere.",
            badMemory = "A key under the mat of an apartment she won't go back to.",
            music = "music_uneasy",
            grass = new Color(0.27f, 0.42f, 0.30f),
            tint = new Color(0.15f, 0.22f, 0.40f, 0.30f),
            hazardLevel = 3,
        };

        // Thursday and Friday: placeholder until the story is written
        days[3] = new DayInfo
        {
            name = "Thursday",
            intro = new[] { "THURSDAY.  HEAVY RAIN.", "The streets don't stay where you left them." },
            orders = new List<OrderInfo> {
                new OrderInfo("dana_7_elm", "Large Pepperoni"),
                new OrderInfo("june_14_elm", "Large Veggie, no meat"),
                new OrderInfo("parks_22_elm", "2 Kids' Cheese"),
                new OrderInfo("kessler_9_river", "Plain Cheese"),
                new OrderInfo("home_12_mill", "Small Cheese, for one") },
            goodMemory = "Theo's voicemail: \"Call me back. Please.\"",
            badMemory = "Sirens somewhere across the river.",
            music = "music_uneasy",
            grass = new Color(0.20f, 0.32f, 0.30f),
            tint = new Color(0.08f, 0.15f, 0.35f, 0.40f),
            hazardLevel = 4,
        };

        days[4] = new DayInfo
        {
            name = "Friday",
            intro = new[] { "FRIDAY.  A GLOWING NEON STORM.", "Last shift. Last delivery." },
            orders = new List<OrderInfo> {
                new OrderInfo("june_14_elm", "Large Veggie, no meat"),
                new OrderInfo("theo_5_willow", "Medium Hawaiian"),
                new OrderInfo("home_12_mill", "The last order of the night") },
            goodMemory = "Headlights on Mill Bridge. She remembers now.",
            badMemory = "Headlights on Mill Bridge. Then nothing.",
            music = "music_uneasy",
            grass = new Color(0.12f, 0.18f, 0.28f),
            tint = new Color(0.05f, 0.10f, 0.30f, 0.50f),
            hazardLevel = 5,
        };
    }
}
