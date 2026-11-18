# Save System

Last Delivery saves the player's progress at the end of every shift to a JSON file on their own computer.

## What is saved (`SaveData.cs`)

| Field | Meaning |
|---|---|
| `currentDay` | 1 = Monday ... 5 = Friday |
| `houseMemories` | each house's memory score, -3 to +3 (vivid at +1 or more, faded at -1 or less) |
| `tips` | money for upgrades |
| `ownedUpgrades` | upgrade ids, e.g. `better_bag` |
| `memoryFragments` | memories collected this week |

## Where the file lives

`Application.persistentDataPath/lastdelivery_save.json`

On Windows that is `C:\Users\<you>\AppData\LocalLow\<Company>\<Product>\lastdelivery_save.json`.
Press **O** in the test street to open the folder, or read the path at the bottom of the screen.

## How it works

- **Export:** `SaveManager.Save()` turns `SaveData` into pretty-printed JSON (`JsonUtility.ToJson`) and writes it. It writes a temp file first, then swaps it in, so a crash mid-save can't wipe the old save.
- **Import:** `SaveManager.Load()` reads the file back into `SaveData` (`JsonUtility.FromJson`). A missing file gives a fresh game. A broken file is backed up as `lastdelivery_save.corrupt.json` and the game starts fresh instead of crashing. Values are clamped (day 1-5, scores -3..3, tips >= 0).
- **GameState** keeps the current `SaveData` while playing and survives scene changes. Houses (`HouseVisual`) listen to it and update as soon as a delivery changes their score.

| Who calls it | When |
|---|---|
| `ShiftSummary` "Save & Continue" | `gs.NextDay()` then `SaveManager.Save(gs.data)` |
| `StartScreen` "Continue" | `GameState.Get().data = SaveManager.Load()` |
| `StartScreen` "New Game" | `SaveManager.DeleteSave()` then `GameState.Get().NewGame()` |
| `TestStreetShift` delivery | `GameState.Get().RecordDelivery(houseId, intact, fragment)` |

## Test checklist

1. **New Game** from StartScreen. You start on Monday in the test street with $0.
2. Walk to June's door and press **SPACE** (intact). Her house brightens, tips go up by $12.
3. Walk to Mr. Kessler's door and press **C** (crushed). His house fades, tips go up by $2.
4. Press **E**, check the summary, click **Save & Continue**. Buy an upgrade in the shop.
5. Press **O** (or open the path shown) and open `lastdelivery_save.json`. Day is 2, tips and houses match.
6. **Stop Play mode.** Press Play again on StartScreen. "Save found: Day 2" shows. Click **Continue**.
7. June's house is still bright, Kessler's still faded, tips and the upgrade are still there.
8. **New Game** resets everything and deletes the file.
9. Corrupt test: type garbage into the JSON file, then Continue. The game starts fresh and a `.corrupt.json` backup appears.
