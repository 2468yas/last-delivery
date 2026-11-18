using UnityEngine;
using UnityEngine.SceneManagement;

// end of shift screen: tips, memory fragment, then Save & Continue
public class ShiftSummary : MonoBehaviour
{
    static readonly string[] Days = { "", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

    void Start()
    {
        GameState gs = GameState.Get();
        Canvas c = UIHelper.MakeCanvas(new Color(0.09f, 0.13f, 0.29f));

        UIHelper.MakeText(c, Days[gs.data.currentDay] + " shift complete", new Vector2(0, 200), 48, new Color(1f, 0.76f, 0.37f));
        UIHelper.MakeText(c, "Tips this shift: $" + gs.shiftTips, new Vector2(0, 110), 30, Color.white);
        UIHelper.MakeText(c, "Total tips: $" + gs.data.tips, new Vector2(0, 65), 30, Color.white);
        UIHelper.MakeText(c, "Memories: " + gs.data.memoryFragments.Count + " / 5", new Vector2(0, 20), 30, new Color(0.31f, 0.82f, 1f));

        string frag = gs.shiftFragment == "" ? "No new memory this shift..." : "New memory: " + gs.shiftFragment;
        UIHelper.MakeText(c, frag, new Vector2(0, -30), 24, new Color(0.8f, 0.8f, 0.8f));

        if (gs.data.currentDay >= 5)
            UIHelper.MakeText(c, "That was the last shift of the week.", new Vector2(0, -75), 24, Color.white);

        UIHelper.MakeButton(c, "Save & Continue", new Vector2(0, -150), SaveAndContinue);
    }

    void SaveAndContinue()
    {
        GameState gs = GameState.Get();
        bool weekOver = gs.data.currentDay >= 5;
        gs.NextDay();
        SaveManager.Save(gs.data);

        // after Friday go back to the title, otherwise shop then next shift
        SceneManager.LoadScene(weekOver ? SceneNames.Start : SceneNames.Shop);
    }
}
