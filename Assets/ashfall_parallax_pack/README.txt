ASHFALL
Three-layer parallax. A burning coast, seamless.

Firepious
firepious.itch.io


CONTENTS

  layers/           three seamless 960x540 PNGs
    01_sky.png      ember-flecked dusk, banded cloud, drifting cinders
    02_mid.png      cracked terrain, distant cones, lava veins
    03_front.png    grounded silhouettes, standing stones, molten seams

  godot4-demo/      a working Godot 4 project, self-contained

  LICENSE.txt       terms of use
  README.txt        this file


THE LAYERS

  Back to front: 01_sky, then 02_mid, then 03_front.

  Each strip is 960x540 and tiles horizontally with no
  seam. They do not tile vertically.

  The sky layer is opaque edge to edge. The mid and front
  layers carry transparency above their horizon, so they
  must sit over the sky, not beside it.


IN ANY ENGINE

  Anchor each strip top-left, not centered, or the wrap
  seam lands mid-screen instead of off-frame.

  Move the back layers slower than the front. 0.25 / 0.5 /
  1.0 back to front reads well as a starting point.

  Scale by whole numbers only. 2x, 3x, 4x. A non-integer
  scale destroys the pixel grid and the art turns to mush.
  Use nearest-neighbour filtering, never bilinear.

  If you reset your scroll accumulator, reset it at a
  distance where every layer has travelled a whole number
  of strip widths. Wrapping at 960 alone snaps the slower
  layers mid-strip and reads as a hitch every few seconds.


IN GODOT 4

  A ParallaxBackground with three ParallaxLayer children,
  each holding one Sprite2D.

  Per layer:
    sprite.centered        false
    sprite.position        (0, 0)
    motion_mirroring       (960, 0)
    motion_scale           (0.25, 1) / (0.5, 1) / (1.0, 1)

  Drive scroll_base_offset, not scroll_offset. If an
  active Camera2D exists, ParallaxBackground recomputes
  scroll_offset from the camera transform every frame and
  any hand-written value is discarded.

  The included script does all of the above at runtime,
  reading the mirroring width from the texture so a
  swapped strip never desyncs.

  The demo project's viewport is 960x540 with a 1920x1080
  window override, stretch mode "viewport", aspect "keep".
  Texture filter is set to nearest.


THE DEMO

  Open godot4-demo/project.godot in Godot 4.4 or newer and
  run it. Everything is wired.

  Exported on the root node:
    motion_scales     depth per layer, back to front
    layer_offsets_y   vertical placement per layer
    speed             front-plane drift, px/sec
    time_scale        master multiplier, 0.0 to freeze


LICENSE

  See LICENSE.txt. Short version: use it in anything, free
  or commercial, modified or not. Don't resell the layers
  on their own.

  Rough forms generated with a local tool, then reworked by
  hand for palette, banding, and seam consistency.


If Ashfall serves you, a rating helps.
Thanks!

— Firepious