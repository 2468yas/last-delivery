using UnityEngine.InputSystem;

// Interact works on keyboard AND controller, so testers can use either.
public static class GameInput
{
    // SPACE / ENTER / E on keyboard, A (Xbox) or Cross (PlayStation) on a controller
    public static bool InteractPressed()
    {
        Keyboard k = Keyboard.current;
        Gamepad g = Gamepad.current;
        bool key = k != null && (k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame || k.eKey.wasPressedThisFrame);
        bool pad = g != null && g.buttonSouth.wasPressedThisFrame;
        return key || pad;
    }

    // debug only: C on keyboard, or Y / Triangle on a controller, crushes the pizza
    public static bool DebugCrushPressed()
    {
        Keyboard k = Keyboard.current;
        Gamepad g = Gamepad.current;
        return (k != null && k.cKey.wasPressedThisFrame) || (g != null && g.buttonNorth.wasPressedThisFrame);
    }

    public static bool OpenSaveFolderPressed()
    {
        Keyboard k = Keyboard.current;
        return k != null && k.oKey.wasPressedThisFrame;
    }

    // shows the right button name in prompts
    public static string InteractName => Gamepad.current != null ? "A" : "SPACE";
}
