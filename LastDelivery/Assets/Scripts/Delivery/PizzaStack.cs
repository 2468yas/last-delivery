using System;

// The link between Shubh's delivery code and Yasin's balancing code.
//
// Shubh's ShiftManager calls PickUp() at Corner Slice and Deliver() at the door.
// Yasin's balance code reads HasPizza (to show/hide the stack) and calls Crush()
// when the stack tips over. A crushed pizza still gets delivered, just badly.
public static class PizzaStack
{
    public static bool HasPizza { get; private set; }
    public static bool Crushed { get; private set; }

    public static event Action PickedUp;      // reset the sway, show the boxes
    public static event Action WasCrushed;    // play the fall animation
    public static event Action Delivered;     // hide the boxes

    public static void PickUp()
    {
        HasPizza = true;
        Crushed = false;
        PickedUp?.Invoke();
    }

    // Yasin: call this when the sway goes past the limit.
    public static void Crush()
    {
        if (!HasPizza || Crushed) return;
        Crushed = true;
        AudioManager.Play("crash");
        WasCrushed?.Invoke();
    }

    public static void Deliver()
    {
        HasPizza = false;
        Delivered?.Invoke();
    }

    // called at the start of every shift so nothing carries over between scenes
    public static void Clear()
    {
        HasPizza = false;
        Crushed = false;
    }
}
