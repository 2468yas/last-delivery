# Last Delivery

A top-down 2D pixel art pizza delivery game for CSS 385 at UW Bothell, by Yasin Maow and Shubh Malhotra.

You play a pizza delivery girl balancing a wobbly stack of boxes through the town of Bellrum over five night shifts. But Bellrum isn't real. It's her memory of the last five days before something bad happened, and every delivery decides which memories she keeps and which ones fade.

## Low-code prototype

**[Play the prototype in Figma](https://www.figma.com/proto/d73THswMk5yc7VDYS0WOgU/Last-Delivery-Low-Code-Prototype?node-id=2-3&starting-point-node-id=2%3A3)**

The prototype covers one full delivery on Monday:

1. Pick up the order at Corner Slice (a Large Veggie, no meat, for 14 Elm St).
2. Pick a route. Bell Hill is short but steep. Downtown is long but flat.
3. On Downtown, a car comes. Slow Down to stay safe, or Run Past and risk it.
4. When the stack wobbles, hit Steady within 1.5 seconds.
5. Knock on the door and read the note the resident leaves.
6. See the Order Complete screen with your tips and memories.

If the pizza arrives intact, June's house gets brighter, she leaves a warm note, and you unlock a good memory. If it gets crushed, her house fades, a shadow figure shows up, and the note is bitter.

### Screenshots

| | |
|---|---|
| ![Corner Slice](prototype/screenshots/01_corner_slice.png) | ![Pick a route](prototype/screenshots/02_pick_route.png) |
| Corner Slice: a new order comes in | Pick a route to 14 Elm St |
| ![Car hazard](prototype/screenshots/03_car_hazard.png) | ![Steady check](prototype/screenshots/04_steady_check.png) |
| Downtown: a car is coming | The stack is wobbling, hit Steady |
| ![Clue match](prototype/screenshots/05_clue_match.png) | ![Memory gets stronger](prototype/screenshots/06_memory_stronger.png) |
| Intact: the veggie garden clue matches | The memory gets stronger |
| ![Order intact](prototype/screenshots/07_order_intact.png) | ![Pizza squished](prototype/screenshots/08_pizza_squished.png) |
| Order complete: intact, +$12 | Crushed: "...it's a little squished" |
| ![Memory fades](prototype/screenshots/09_memory_fades.png) | ![Order crushed](prototype/screenshots/10_order_crushed.png) |
| The memory starts to fade | Order complete: crushed, +$2 |

## Docs

- [Game Design Document (PDF)](docs/Last_Delivery_GDD.pdf) and [Word version](docs/Last_Delivery_GDD.docx)
- [Core Game Loop (combined)](docs/Core_Game_Loop_Combined.docx)
- [Playtest Round 1: test plan and survey](docs/Playtest_Round1.md)

### Where the idea came from

Last Delivery combines two earlier concepts:

- **Bellrum (Shubh):** the pizza delivery story, the memory system and the town of Bellrum. [Art Bible](docs/concepts/shubh-bellrum/Game_Art_Bible_Bellrum.pdf), [World Atlas](docs/concepts/shubh-bellrum/World_Building_Atlas_Bellrum.pdf), [Core Game Loop](docs/concepts/shubh-bellrum/Core_Game_Loop_Bellrum.docx)
- **Neon Rain City (Yasin):** the top-down pixel art style, the glowing neon rain, upgrades and randomized runs. [Art Bible](docs/concepts/yasin-neon-rain-city/Game_Art_Bible_Neon_Rain_City.pdf), [World Atlas](docs/concepts/yasin-neon-rain-city/World_Building_Atlas_Neon_Rain_City.pdf), [Core Game Loop](docs/concepts/yasin-neon-rain-city/Core_Game_Loop_Neon_Rain_City.pdf)

## Art

| | |
|---|---|
| ![Monday street](art/mockups/monday_street.png) | ![Friday neon street](art/mockups/friday_neon_street.png) |
| Monday: sunny and calm | Friday: glowing neon rain |
| ![Bellrum map](art/bellrum/bellrum_map.png) | ![Bellrum timeline](art/bellrum/bellrum_timeline.png) |
| Map of Bellrum | Her last five days |

More in the [art folder](art): palettes, the HUD reference, the core loop diagram, and the Neon Rain City concept art.

## What's next

- Build the core loop in Unity 2D: movement, pizza stack balancing and the tilt marker.
- Bellrum map blockout, deliveries, notes and the HUD.
- Houses that brighten or fade, and the five shifts from Monday to Friday.
