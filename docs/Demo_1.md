# Demo 1

## How to play
1. Open `Assets/Scenes/StartScreen` and press Play. Click **New Game**.
2. Read the day's intro card (about 15 seconds) and press **SPACE** (or **A** on a controller).
3. At **Corner Slice**, press SPACE to pick up an order. The order card says what it is and gives a **clue**, not an address ("the house with a vegetable garden").
4. Walk along the street, find the house whose art matches the clue, and press SPACE at its door.
   - Wrong house: "Hmm, this isn't it."
   - Stuck for 40 seconds: the right house's address label starts glowing.
5. Read the resident's note. An intact pizza makes the house brighter; a crushed one makes it fade (and its clue gets harder to see next time).
6. After the last order, go back to Corner Slice to **clock out**. The summary screen shows tips and the day's memory fragment, then **Save & Continue** saves and opens the shop.

| Shift | Orders | What changes |
|---|---|---|
| Monday | 5 | Sunny, calm music |
| Tuesday | 6 | Clouds, uneasy music, notes start knowing too much |
| Wednesday | 6 | Rain tint, an order to her own apartment, notes get creepy |

Three shifts plus summaries and the shop run well over 7 minutes. The only non-interactive part is the intro card at the start of each shift (under 30 seconds each).

## Controls
| Action | Keyboard | Controller |
|---|---|---|
| Move | WASD / arrows | Left stick (Yasin) |
| Interact / pick up / deliver / continue | SPACE, ENTER or E | A / Cross |
| Debug: crush the pizza | C | Y / Triangle |
| Open the save folder | O | |

## Polish checklist
- **Graphics:** pixel-art houses with 3 versions each (vivid / normal / faded), each with a visible clue prop; Corner Slice, player, trees, lampposts and Willow Pond.
- **Music and sound:** calm Monday theme, uneasy theme from Tuesday on, and sounds for pick-up, knocking, good and bad deliveries, wrong house, notes, clock-out and crashes.
- **Story in the game:** intro cards, resident notes that get stranger each day, memory fragments, and the order to her own address.
- **Controller:** interact works on a controller; movement and balancing are Yasin's part.
- **Doesn't need a guide:** every step has an on-screen prompt.

## For Yasin: hooking the balancing in
- The player is now a prefab: `Assets/Prefabs/Player.prefab`. Add your balancing / stack scripts to the prefab, not the scene, so we don't both edit `TestStreet.unity`.
- `PizzaStack.HasPizza` is true while she is carrying an order (show the boxes, run the sway).
- Call `PizzaStack.Crush()` when the stack tips over. It plays the crash sound and the delivery counts as crushed.
- Events: `PizzaStack.PickedUp` (reset sway), `PizzaStack.WasCrushed`, `PizzaStack.Delivered`.
- Hazard difficulty for the day: `StoryData.GetDay(GameState.Get().data.currentDay).hazardLevel` (1 = calm ... 5 = storm).
- Sprites ready to use: `Assets/Art/Bellrum/car.png`, `pizza_box.png`. Sounds: `AudioManager.Play("wobble")`, `AudioManager.Play("crash")`.
