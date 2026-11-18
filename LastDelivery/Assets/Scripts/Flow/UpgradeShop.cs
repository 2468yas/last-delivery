using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// placeholder shop between shifts: spend tips on upgrades
public class UpgradeShop : MonoBehaviour
{
    // id, name shown, cost
    struct Upgrade { public string id, name; public int cost; }

    static readonly Upgrade[] Upgrades =
    {
        new Upgrade { id = "better_bag", name = "Better Bag (less sway)", cost = 20 },
        new Upgrade { id = "grippy_shoes", name = "Grippy Shoes", cost = 15 },
        new Upgrade { id = "box_holder", name = "Box Holder", cost = 30 },
    };

    Text moneyText;

    void Start()
    {
        Canvas c = UIHelper.MakeCanvas(new Color(0.06f, 0.08f, 0.19f));
        UIHelper.MakeText(c, "CORNER SLICE SHOP", new Vector2(0, 230), 48, new Color(1f, 0.54f, 0.16f));
        moneyText = UIHelper.MakeText(c, "", new Vector2(0, 170), 30, Color.white);

        // one button per upgrade
        for (int i = 0; i < Upgrades.Length; i++)
        {
            Upgrade u = Upgrades[i];
            Button b = UIHelper.MakeButton(c, "", new Vector2(0, 80 - i * 80), () => Buy(u));
            b.GetComponent<RectTransform>().sizeDelta = new Vector2(520, 60);
            b.name = u.id;
        }

        UIHelper.MakeButton(c, "Start Next Shift", new Vector2(0, -200), () => SceneManager.LoadScene(SceneNames.Game));
        Refresh();
    }

    void Buy(Upgrade u)
    {
        SaveData d = GameState.Get().data;
        if (d.ownedUpgrades.Contains(u.id) || d.tips < u.cost) return; // already have it or too broke
        d.tips -= u.cost;
        d.ownedUpgrades.Add(u.id);
        Refresh();
    }

    // update the money and button labels
    void Refresh()
    {
        SaveData d = GameState.Get().data;
        moneyText.text = "Tips: $" + d.tips;
        foreach (Upgrade u in Upgrades)
        {
            GameObject b = GameObject.Find(u.id);
            if (b == null) continue;
            bool owned = d.ownedUpgrades.Contains(u.id);
            b.GetComponentInChildren<Text>().text = owned ? u.name + " - OWNED" : u.name + " - $" + u.cost;
            b.GetComponent<Button>().interactable = !owned && d.tips >= u.cost;
        }
    }
}
