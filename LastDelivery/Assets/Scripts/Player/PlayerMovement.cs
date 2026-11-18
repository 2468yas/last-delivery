using UnityEngine;
using UnityEngine.InputSystem;

// placeholder top-down player, WASD or arrows to walk
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    Rigidbody2D rb;
    Vector2 move;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;          // top-down, so no falling
        rb.freezeRotation = true;     // don't spin when bumping stuff
    }

    void Update()
    {
        Keyboard k = Keyboard.current;
        if (k == null) return;
        float x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
        float y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0);
        move = new Vector2(x, y).normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = move * speed;
    }
}
