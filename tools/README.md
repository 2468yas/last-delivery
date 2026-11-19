# Tools

Scripts that generate the game's art and audio. Run them from the repo root (the folder with `LastDelivery` and `docs`).

```
pip install pillow numpy
python tools/generate_sprites.py    # -> LastDelivery/Assets/Art/Bellrum/*.png
python tools/generate_audio.py      # -> LastDelivery/Assets/Resources/Audio/*.wav
```

Both overwrite the existing files, so commit or back up anything you edited by hand first. Unity reimports the files when you switch back to it.

## Sprites (`generate_sprites.py`)
- **Houses:** edit the `HOUSES` dictionary (wall color, roof color, clue prop). Each house is drawn three times: `vivid`, `normal`, `faded`. Prop options: `garden`, `bikes`, `posters`, `nosolicit`, `forrent`, `scooter`, `hoop`, `cactus` (add new ones in `draw_house`).
- **A new house** also needs an entry in `TestStreetSetup.cs` (`Houses` list), its clue and notes in `StoryData.cs`, then **Tools > Last Delivery > Build Bellrum Street**.
- Sizes are in real pixels at 16 pixels per unit: houses 64x56, player 16x32, Corner Slice 80x60.

## Audio (`generate_audio.py`)
- **Sound effects** are built from square/triangle waves and noise. Each `save("name", ...)` line makes `name.wav`.
- **Music:** `song(bpm, chords, melody, ...)`. Chords are lists of note offsets, melody notes are MIDI numbers (60 = middle C), `None` = rest, a negative number = held twice as long.
  - `music_calm`: C major, 118 BPM (Monday)
  - `music_uneasy`: A minor, 92 BPM, detuned lead (Tuesday onward)
- Which track plays on which day is set by `music` in `StoryData.cs`.
