# Generates the Bellrum pixel-art sprites (houses x vivid/normal/faded, Corner Slice, player, props).
# Run from the repo root:  python tools/generate_sprites.py
# Needs Pillow:  pip install pillow
# After running, re-run Tools > Last Delivery > Build Bellrum Street in Unity if you added new houses.
import colorsys, os, sys
from PIL import Image, ImageDraw

OUT = sys.argv[1] if len(sys.argv) > 1 else "LastDelivery/Assets/Art/Bellrum"
os.makedirs(OUT, exist_ok=True)

INK = (30, 24, 36, 255)
CLEAR = (0, 0, 0, 0)

def rgba(c, a=255): return (c[0], c[1], c[2], a)

def adjust(c, sat=1.0, val=1.0):
    h, s, v = colorsys.rgb_to_hsv(c[0] / 255, c[1] / 255, c[2] / 255)
    r, g, b = colorsys.hsv_to_rgb(h, min(1, s * sat), min(1, v * val))
    return (int(r * 255), int(g * 255), int(b * 255))

def fade(c, amt=0.75, grey=(150, 150, 158)):
    return tuple(int(c[i] * (1 - amt) + grey[i] * amt) for i in range(3))

# ---------------- houses ----------------
W, H = 64, 56
HOUSES = {
    "nate_1_elm":      dict(wall=(240, 200, 90),  roof=(170, 60, 50),  prop="hoop"),
    "june_14_elm":     dict(wall=(80, 205, 195),  roof=(40, 40, 150),  prop="garden"),
    "dana_7_elm":      dict(wall=(250, 160, 110), roof=(110, 60, 120), prop="scooter"),
    "parks_22_elm":    dict(wall=(150, 210, 110), roof=(160, 70, 60),  prop="bikes"),
    "theo_5_willow":   dict(wall=(240, 140, 190), roof=(60, 50, 110),  prop="posters"),
    "kessler_9_river": dict(wall=(150, 165, 230), roof=(70, 60, 70),   prop="nosolicit"),
    "rourke_3_river":  dict(wall=(210, 190, 160), roof=(120, 80, 60),  prop="forrent"),
    "home_12_mill":    dict(wall=(190, 180, 220), roof=(80, 70, 100),  prop="cactus"),
}

def draw_house(info, state):
    img = Image.new("RGBA", (W, H), CLEAR)
    d = ImageDraw.Draw(img)
    wall, roof = info["wall"], info["roof"]
    win, door = (235, 245, 255), (110, 62, 34)
    if state == "vivid":
        wall, roof, win = adjust(wall, 1.25, 1.08), adjust(roof, 1.2, 1.1), (255, 214, 120)
    elif state == "faded":
        wall, roof, win, door = fade(wall), fade(roof), (190, 190, 196), fade(door, 0.7)

    def P(c): return c if state != "faded" else fade(c, 0.7)

    # roof with eave
    d.polygon([(2, 14), (10, 2), (54, 2), (62, 14)], fill=rgba(roof), outline=INK)
    d.rectangle([4, 14, 59, 55], fill=rgba(wall), outline=INK)
    d.line([(5, 15), (58, 15)], fill=rgba(adjust(wall, 1, 0.8)))
    # windows
    for x in (10, 41):
        d.rectangle([x, 21, x + 12, 31], fill=rgba(win), outline=INK)
        d.line([(x + 6, 21), (x + 6, 31)], fill=INK)
        if state == "vivid":
            d.rectangle([x + 1, 22, x + 2, 30], fill=(220, 60, 90, 255))
            d.rectangle([x + 10, 22, x + 11, 30], fill=(220, 60, 90, 255))
    # door + step
    d.rectangle([27, 36, 37, 55], fill=rgba(door), outline=INK)
    d.point((35, 46), fill=(240, 210, 90, 255))
    d.rectangle([25, 54, 39, 55], fill=(160, 160, 160, 255))

    prop = info["prop"]
    if prop == "garden":
        for i in range(4):
            gx = 6 + i * 5
            d.rectangle([gx, 47, gx + 3, 54], fill=rgba(P((40, 130, 40))))
            if state != "faded":
                d.point((gx + 1, 48), fill=(230, 50, 40, 255)); d.point((gx + 2, 51), fill=(250, 140, 30, 255))
    elif prop == "bikes":
        for bx in (44, 53):
            d.ellipse([bx, 49, bx + 4, 53], outline=rgba(P((40, 40, 40))))
            d.ellipse([bx + 5, 49, bx + 9, 53], outline=rgba(P((40, 40, 40))))
            d.line([(bx + 2, 51), (bx + 5, 47), (bx + 7, 51)], fill=rgba(P((220, 40, 40) if bx == 44 else (40, 120, 230))))
    elif prop == "posters":
        for x, c in ((10, (250, 70, 70)), (41, (60, 60, 230))):
            d.rectangle([x + 2, 22, x + 10, 30], fill=rgba(P(c)))
            d.line([(x + 3, 24), (x + 9, 24)], fill=rgba(P((250, 230, 80))))
            d.point((x + 6, 27), fill=rgba(P((255, 255, 255))))
    elif prop == "nosolicit":
        d.rectangle([40, 39, 54, 46], fill=rgba(P((250, 250, 245))), outline=INK)
        d.line([(42, 41), (52, 41)], fill=rgba(P((220, 30, 30))))
        d.line([(42, 44), (50, 44)], fill=rgba(P((60, 60, 60))))
    elif prop == "forrent":
        d.line([(51, 42), (51, 55)], fill=rgba(P((110, 80, 50))), width=2)
        d.rectangle([44, 40, 60, 48], fill=rgba(P((230, 40, 40))), outline=INK)
        d.rectangle([46, 42, 58, 46], fill=rgba(P((250, 250, 250))))
    elif prop == "scooter":
        d.ellipse([6, 50, 11, 55], fill=INK); d.ellipse([16, 50, 21, 55], fill=INK)
        d.polygon([(8, 50), (19, 50), (17, 45), (11, 45)], fill=rgba(P((220, 40, 40))), outline=INK)
        d.line([(19, 50), (20, 42)], fill=INK)
        d.rectangle([12, 41, 17, 44], fill=rgba(P((240, 200, 60))), outline=INK)
    elif prop == "hoop":
        d.rectangle([48, 34, 58, 40], fill=rgba(P((250, 250, 250))), outline=INK)
        d.line([(50, 41), (56, 41)], fill=rgba(P((250, 120, 30))), width=2)
        d.line([(51, 42), (52, 46)], fill=rgba(P((230, 230, 230)))); d.line([(55, 42), (54, 46)], fill=rgba(P((230, 230, 230))))
        d.ellipse([8, 49, 13, 54], fill=rgba(P((240, 120, 30))), outline=INK)
    elif prop == "cactus":
        d.rectangle([14, 25, 17, 31], fill=rgba(P((60, 150, 70))))
        d.rectangle([12, 26, 13, 28], fill=rgba(P((60, 150, 70)))); d.rectangle([18, 25, 19, 27], fill=rgba(P((60, 150, 70))))
        d.rectangle([13, 30, 18, 31], fill=rgba(P((180, 90, 50))))
        d.rectangle([22, 37, 24, 40], fill=(60, 60, 60, 255))  # porch light, always off

    if state == "vivid":   # flowers along the front
        for i in range(5):
            fx = 40 + i * 4
            if prop in ("bikes", "forrent", "nosolicit"): fx = 6 + i * 4
            d.rectangle([fx, 53, fx + 1, 54], fill=[(255, 90, 160, 255), (255, 220, 60, 255)][i % 2])
    if state == "faded":   # cracks + a missing chunk
        d.line([(7, 18), (12, 30), (9, 40)], fill=(120, 120, 128, 255))
        d.line([(52, 33), (57, 48)], fill=(120, 120, 128, 255))
        d.polygon([(54, 2), (62, 14), (50, 14), (50, 6)], fill=CLEAR)
        d.line([(50, 6), (50, 14)], fill=INK)
    return img

for hid, info in HOUSES.items():
    for st in ("vivid", "normal", "faded"):
        draw_house(info, st).save(f"{OUT}/house_{hid}_{st}.png")

# ---------------- Corner Slice ----------------
img = Image.new("RGBA", (80, 60), CLEAR); d = ImageDraw.Draw(img)
d.rectangle([3, 10, 76, 59], fill=(235, 225, 205, 255), outline=INK)
d.rectangle([0, 0, 79, 10], fill=(200, 50, 45, 255), outline=INK)          # sign board
for x in range(4, 76, 8):                                                    # awning stripes
    d.rectangle([x, 11, x + 3, 17], fill=(210, 40, 40, 255))
    d.rectangle([x + 4, 11, x + 7, 17], fill=(250, 250, 250, 255))
d.line([(3, 17), (76, 17)], fill=INK)
d.polygon([(34, 2), (46, 2), (40, 9)], fill=(250, 200, 80, 255), outline=INK)  # pizza slice icon
for px, py in ((38, 4), (42, 4), (40, 6)): d.point((px, py), fill=(200, 40, 30, 255))
d.rectangle([8, 22, 30, 40], fill=(180, 220, 240, 255), outline=INK)       # window
d.rectangle([50, 22, 72, 40], fill=(180, 220, 240, 255), outline=INK)
d.rectangle([52, 25, 70, 29], fill=(30, 30, 40, 255))                       # neon OPEN
d.line([(54, 27), (68, 27)], fill=(255, 90, 170, 255))
d.ellipse([14, 30, 24, 38], fill=(240, 190, 80, 255), outline=INK)          # pizza in window
d.rectangle([35, 36, 45, 59], fill=(110, 62, 34, 255), outline=INK)
d.rectangle([33, 58, 47, 59], fill=(160, 160, 160, 255))
img.save(f"{OUT}/corner_slice.png")

# ---------------- player (16x32) ----------------
img = Image.new("RGBA", (16, 32), CLEAR); d = ImageDraw.Draw(img)
d.rectangle([4, 2, 11, 5], fill=(200, 40, 40, 255))                          # cap
d.rectangle([10, 5, 14, 6], fill=(200, 40, 40, 255))                         # brim
d.rectangle([5, 6, 10, 12], fill=(232, 180, 140, 255))                       # face
d.point((9, 8), fill=INK)
d.rectangle([3, 6, 4, 13], fill=(90, 50, 30, 255))                           # ponytail
d.rectangle([3, 13, 12, 23], fill=(200, 40, 40, 255))                        # uniform
d.rectangle([6, 15, 9, 16], fill=(250, 250, 250, 255))                       # name tag
d.rectangle([12, 15, 14, 18], fill=(232, 180, 140, 255))                     # arm forward
d.rectangle([4, 24, 6, 30], fill=(40, 40, 60, 255)); d.rectangle([9, 24, 11, 30], fill=(40, 40, 60, 255))
d.rectangle([3, 30, 7, 31], fill=(20, 20, 20, 255)); d.rectangle([8, 30, 12, 31], fill=(20, 20, 20, 255))
img.save(f"{OUT}/player.png")

# ---------------- props ----------------
img = Image.new("RGBA", (16, 5), CLEAR); d = ImageDraw.Draw(img)            # pizza box (for Yasin's stack)
d.rectangle([0, 0, 15, 4], fill=(214, 170, 110, 255), outline=(150, 110, 70, 255))
d.line([(3, 2), (12, 2)], fill=(200, 50, 45, 255))
img.save(f"{OUT}/pizza_box.png")

img = Image.new("RGBA", (32, 18), CLEAR); d = ImageDraw.Draw(img)           # car (for Yasin's hazards)
d.rectangle([1, 6, 30, 14], fill=(60, 110, 200, 255), outline=INK)
d.polygon([(7, 6), (10, 1), (22, 1), (25, 6)], fill=(140, 190, 240, 255), outline=INK)
d.ellipse([4, 12, 10, 17], fill=INK); d.ellipse([21, 12, 27, 17], fill=INK)
d.rectangle([28, 8, 30, 10], fill=(255, 240, 150, 255))
img.save(f"{OUT}/car.png")

img = Image.new("RGBA", (24, 32), CLEAR); d = ImageDraw.Draw(img)           # tree
d.rectangle([10, 20, 13, 31], fill=(110, 70, 40, 255), outline=INK)
d.ellipse([1, 1, 22, 23], fill=(50, 130, 60, 255), outline=INK)
d.ellipse([5, 4, 13, 11], fill=(80, 160, 80, 255))
img.save(f"{OUT}/tree.png")

img = Image.new("RGBA", (8, 30), CLEAR); d = ImageDraw.Draw(img)            # lamppost
d.rectangle([3, 6, 4, 29], fill=(60, 60, 70, 255))
d.rectangle([1, 2, 6, 6], fill=(250, 230, 150, 255), outline=INK)
d.rectangle([1, 28, 6, 29], fill=(60, 60, 70, 255))
img.save(f"{OUT}/lamppost.png")

img = Image.new("RGBA", (96, 40), CLEAR); d = ImageDraw.Draw(img)           # Willow Pond
d.ellipse([0, 2, 95, 38], fill=(110, 170, 200, 255), outline=(60, 110, 150, 255), width=2)
d.ellipse([10, 8, 85, 32], fill=(150, 200, 225, 255))
for x, y in ((25, 15), (60, 22), (45, 12)):
    d.ellipse([x, y, x + 6, y + 4], fill=(70, 150, 70, 255))
img.save(f"{OUT}/pond.png")

print(len(os.listdir(OUT)), "sprites")
