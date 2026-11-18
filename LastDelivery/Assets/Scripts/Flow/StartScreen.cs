using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// title screen: Continue loads the save, New Game wipes it and starts Monday
public class StartScreen : MonoBehaviour
{
    void Start()
    {
        Canvas c = UIHelper.MakeCanvas(new Color(0.04f, 0.06f, 0.13f));
        UIHelper.MakeText(c, "LAST DELIVERY", new Vector2(0, 180), 64, new Color(1f, 0.54f, 0.16f));

        Button cont = UIHelper.MakeButton(c, "Continue", new Vector2(0, 20), Continue);
        cont.interactable = SaveManager.HasSave(); // greyed out if there's no save yet

        UIHelper.MakeButton(c, "New Game", new Vector2(0, -60), NewGame);

        // show what day the save is on so it's easy to see loading worked
        if (SaveManager.HasSave())
        {
            SaveData d = SaveManager.Load();
            UIHelper.MakeText(c, "Save found: Day " + d.currentDay + "  |  $" + d.tips, new Vector2(0, -150), 22, Color.white);
        }
    }

    void Continue()
    {
        GameState.Get().data = SaveManager.Load();
        SceneManager.LoadScene(SceneNames.Game);
    }

    void NewGame()
    {
        SaveManager.DeleteSave();
        GameState.Get().NewGame();
        SceneManager.LoadScene(SceneNames.Game);
    }
}
