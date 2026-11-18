using System;
using System.Collections.Generic;

// everything the game remembers between sessions (just these 5 things)
// this is the "contract" between our two halves, so change it together!
[Serializable]
public class SaveData
{
    // 1. which day she's on (1 = Monday ... 5 = Friday)
    public int currentDay = 1;

    // 2. how well she remembers each house (score goes up or down with deliveries)
    public List<HouseMemory> houseMemories = new List<HouseMemory>();

    // 3. money she earned from tips
    public int tips = 0;

    // 4. upgrades she owns (ids like "better_bag")
    public List<string> ownedUpgrades = new List<string>();

    // 5. memory fragments she collected (ids like "june_sunday_dinners")
    public List<string> memoryFragments = new List<string>();
}

// one house and its memory score
// JsonUtility can't save dictionaries, so we use a list of these instead
[Serializable]
public class HouseMemory
{
    public string houseId;
    public int score; // good delivery = +1, crushed = -1
}

// what the score turns into for the art
public enum MemoryState { Faded, Normal, Vivid }
