using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// placeholder shift until the real delivery gameplay exists
// 1 = good delivery to June, 2 = crushed delivery, E = end shift
public class PlayerTestShift : MonoBehaviour
{
    static readonly string[] Days = { "", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
    Text hud;

    void Start()
    {
        Canvas c = UIHelper.MakeCanvas(Color.clear);
        hud = UIHelper.MakeText(c, "", new Vector2(0, 300), 24, Color.white);
        UIHelper.MakeText(c, "WASD = walk   1 = good delivery   2 = crushed   E = end shift", new Vector2(0, -320), 20, Color.white);
    }

    void Update()
    {
        GameState gs = GameState.Get();
        Keyboard k = Keyboard.current;
        if (k != null)
        {
            if (k.digit1Key.wasPressedThisFrame) gs.RecordDelivery("june_14_elm", true, "Sunday dinners at June's");
            if (k.digit2Key.wasPressedThisFrame) gs.RecordDelivery("june_14_elm", false, "A door that stayed closed");
            if (k.eKey.wasPressedThisFrame) SceneManager.LoadScene(SceneNames.Summary);
        }

        SaveData d = gs.data;
        hud.text = Days[d.currentDay] + "   |   Tips $" + d.tips + "   |   Memories " + d.memoryFragments.Count + "/5   |   June's house: " + gs.GetHouseState("june_14_elm");
    }
}
