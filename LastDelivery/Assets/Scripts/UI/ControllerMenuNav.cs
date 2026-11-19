using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Yasin: controller support for the menus (Start, Summary, Shop).
// Unity's buttons only respond to a controller once one of them is selected, and our
// screens are built in code with nothing selected. So when a controller is used and
// nothing is selected, this picks the top button. D-pad / stick moves, A presses.
// The selected button grows a bit so you can see where you are.
//
// Runs by itself on every scene (RuntimeInitializeOnLoadMethod), no setup needed.
public class ControllerMenuNav : MonoBehaviour
{
    const float SelectedScale = 1.1f;
    Button lastSelected;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        GameObject go = new GameObject("Controller Menu Nav (Yasin)");
        DontDestroyOnLoad(go);
        go.AddComponent<ControllerMenuNav>();
    }

    void Update()
    {
        EventSystem es = EventSystem.current;
        if (es == null) return;

        Gamepad g = Gamepad.current;
        if (g != null && PadUsed(g) && !IsUsable(es.currentSelectedGameObject))
        {
            Button first = TopButton();
            if (first != null) es.SetSelectedGameObject(first.gameObject);
        }

        HighlightSelected(es.currentSelectedGameObject);
    }

    static bool PadUsed(Gamepad g)
    {
        return g.leftStick.ReadValue().magnitude > 0.5f || g.dpad.ReadValue().magnitude > 0.5f ||
               g.buttonSouth.wasPressedThisFrame || g.startButton.wasPressedThisFrame;
    }

    static bool IsUsable(GameObject go)
    {
        if (go == null || !go.activeInHierarchy) return false;
        Selectable s = go.GetComponent<Selectable>();
        return s != null && s.IsInteractable();
    }

    // the highest interactable button on screen (menus are laid out top to bottom)
    static Button TopButton()
    {
        Button best = null;
        foreach (Button b in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            if (!b.isActiveAndEnabled || !b.IsInteractable()) continue;
            if (best == null || b.transform.position.y > best.transform.position.y) best = b;
        }
        return best;
    }

    void HighlightSelected(GameObject selected)
    {
        Button now = selected != null ? selected.GetComponent<Button>() : null;
        if (now == lastSelected) return;
        if (lastSelected != null) lastSelected.transform.localScale = Vector3.one;
        if (now != null) now.transform.localScale = Vector3.one * SelectedScale;
        lastSelected = now;
    }
}
