using UnityEngine;
using UnityEngine.InputSystem;

// Yasin: movement + steadying input, works on keyboard AND controller.
// (Interact / pick up / deliver is in Shubh's GameInput.)
//
//   Move:   WASD / arrows,  left stick / d-pad
//   Steady: SHIFT,          LB / RB / either trigger   (walk slower, stack stops swaying)
public static class PlayerControls
{
    const float StickDeadzone = 0.2f;

    // -1..1 on each axis. Half tilt on the stick = half speed.
    public static Vector2 Move()
    {
        Vector2 v = Vector2.zero;

        Keyboard k = Keyboard.current;
        if (k != null)
        {
            v.x += (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
            v.y += (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0);
        }

        Gamepad g = Gamepad.current;
        if (g != null)
        {
            Vector2 stick = g.leftStick.ReadValue();
            if (stick.magnitude > StickDeadzone) v += stick;   // ignore a stick that's drifting
            v += g.dpad.ReadValue();
        }

        return Vector2.ClampMagnitude(v, 1f);   // diagonals aren't faster
    }

    public static bool SteadyHeld()
    {
        Keyboard k = Keyboard.current;
        Gamepad g = Gamepad.current;
        bool key = k != null && (k.leftShiftKey.isPressed || k.rightShiftKey.isPressed);
        bool pad = g != null && (g.leftShoulder.isPressed || g.rightShoulder.isPressed ||
                                 g.leftTrigger.ReadValue() > 0.5f || g.rightTrigger.ReadValue() > 0.5f);
        return key || pad;
    }

    // shows the right button name in hints
    public static string SteadyName => Gamepad.current != null ? "LB" : "SHIFT";

    // controller rumble (does nothing on keyboard). PlayerMovement stops it after `seconds`.
    public static void Rumble(float low, float high)
    {
        Gamepad g = Gamepad.current;
        if (g != null) g.SetMotorSpeeds(low, high);
    }

    public static void StopRumble()
    {
        Gamepad g = Gamepad.current;
        if (g != null) g.SetMotorSpeeds(0f, 0f);
    }
}
