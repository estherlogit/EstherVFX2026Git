# SH_Particles.shadergraph, explained

Reference notes for the NamuFX Stylized Water Effects particle shader.
The same explanations are inside the graph itself as sticky notes, one per group.

---

## What it is

An **unlit, transparent** particle shader (graph path: NAMU/MasterShader).
It produces only two things: a **Base Color** and an **Alpha**. There is no
lighting, so what you see on screen is exactly what those two outputs say.

Render settings: Surface Transparent, Alpha blending, Unlit, casts no shadows,
renders both faces. Targets are URP and Built-In.

## One texture, four jobs

`Main Tex` is a **packed** texture. Each channel is a separate mask, and the
graph samples the same texture several times, each time pulling one channel:

| Channel | Used for |
|---------|----------|
| R | the coloured shape (MainColor group) |
| G | the dissolve / erosion pattern (Dissolve, Dissolve Edge, Outline) |
| B | where the secondary colour appears (Secondary Color) |
| A | the silhouette / opacity (Alpha) |

If the shape looks wrong, check which channel of your texture you actually
painted. A texture with only an alpha channel gives you a shape with no
dissolve pattern and no secondary colour.

## Driven by the Particle System, not the material

Most of the look comes per particle from the Particle System. In the Renderer
module you must add Custom Vertex Streams:

```
Color         -> Vertex Colour  (RGB tint, A fade)
Custom1.xyzw  -> TEXCOORD1      (read in the graph as "UV1")
Custom2.xyzw  -> TEXCOORD2      (read in the graph as "UV2")
```

| Value | Meaning |
|-------|---------|
| UV1.x | Intensity (brightness, above 1 it blooms) |
| UV1.y | Dissolve amount (0 = whole, 1 = gone) |
| UV1.z | Dissolve falloff (0 = hard burn line, higher = soft fade) |
| UV1.w | Soft particle fade distance |
| UV2.xyz | Secondary colour |
| UV2.w | How much of the secondary colour to blend in |

Animate these with Custom Data curves, so every particle dissolves and glows
on its own timing from a single material.

## Material properties

`Main Tex`, `OutlineColor`, `OutlineThickness`, `DIstortionNoiseTex`,
`DistortionScroll`, `DIstortion Str`, `Use Soft Particle?` (toggle).

---

## The groups, left to right

### Distortion
Warps the UVs so a flat sprite looks like moving liquid. Its output feeds the
UV input of **every** Main Tex sample, so the whole particle wobbles together.

1. Time x `DistortionScroll`, added to UV0. That scrolls the noise. X and Y set
   speed and direction.
2. That scrolling UV samples `DIstortionNoiseTex`. Only the R channel is used.
3. Noise R x (`DIstortion Str` x 0.1) becomes a small offset, copied into both
   X and Y of a Vector2.
4. That offset is added to the clean UV0. The result is the distorted UV.

The Sampler State is set to **Clamp**, so the pushed UVs do not repeat the
sprite at the borders.

Note: the extra Sample Texture 2D with nothing plugged into its Texture slot
returns white (1), so it multiplies by 1 and changes nothing. It looks like a
leftover hook for a second mask. Safe to ignore or delete.

### Intensity
Reads UV1.x and multiplies the finished colour by it. Above 1 pushes into HDR
so it blooms, below 1 dims it.

### MainColor
1. Vertex Colour RGB (the particle Start Color) x Main Tex R = a tinted shape.
2. A Lerp blends that with the Secondary Colour.
3. The result is multiplied by Intensity.

The output does not go straight to Base Color. It travels into Dissolve Edge,
where the dissolve mask cuts it and the rim colour is added on top.

### Secondary Color
Lets each particle carry a second colour that appears only in certain parts of
the texture. Good for hot cores, foam tips, gradients over lifetime.

- UV2.xyz = the colour, UV2.w = how much to blend, Main Tex B = where.
- B x UV2.w becomes the T input of the Lerp in MainColor.

If Custom2 is not sent, w is 0, the Lerp stays at the main colour and this
group does nothing. That is the normal fallback, not a bug.

### Dissolve Sharpness
No maths, it just splits UV1 and hands values to the two Dissolve Functions:
UV1.y -> Dissolve Amount for the Dissolve Edge copy, UV1.z -> Falloff for
**both**. Falloff is the width of the transition band: near 0 is a hard crisp
burn line, higher is a soft misty fade. The group name is slightly misleading,
it carries the amount as well as the sharpness.

### Dissolve
The main erosion. Output is a 0 to 1 mask that multiplies into Alpha.

- Pattern = Main Tex G, Amount = UV1.y (saturated), Falloff = UV1.z.
- These go into the shared `Dissolve Function` subgraph.

Inside the subgraph: it remaps the amount into the range (-Falloff, 1), then
does `Smoothstep(remapped, remapped + Falloff, pattern)`. The remap is what
guarantees the ends behave: amount 0 gives a mask of 1 everywhere (fully
visible), amount 1 gives 0 everywhere (fully gone).

### Dissolve Edge
Draws a bright rim exactly on the dissolve boundary, like burning paper.

1. Main Tex G minus (`OutlineThickness` x 0.1). Lowering the pattern makes it
   dissolve earlier, so this is a **shrunken** copy of the shape.
2. That shrunken pattern goes through a second Dissolve Function, same amount
   and falloff.
3. Big mask minus small mask = a thin band sitting on the edge.
   Band x `OutlineColor` = the rim.
4. The main colour is multiplied by the small mask, so the body is cut by the
   dissolve too.
5. Body + rim are added and passed to Outline.

### Outline
A fixed border that does not move with the dissolve.

1. `Step(edge = OutlineThickness x 0.1, in = Main Tex G)` gives a hard on/off
   inner shape.
2. Main Tex A (full silhouette) minus that inner shape leaves a ring.
3. Ring x `OutlineColor`, added on top of the Dissolve Edge result. That sum is
   the final **Base Color**.

Outline and Dissolve Edge share `OutlineColor` and `OutlineThickness`, so one
setting changes the border and the burn rim together.

### Alpha
Multiplies together everything that can fade the particle, then clamps:

```
Main Tex A  x  Dissolve mask  x  Soft particle fade  x  Vertex Colour A
   -> Saturate -> Alpha
```

Because they are multiplied, any single one going to 0 hides the particle. If
your particles are invisible, check them in that order.

Small note: this group samples Main Tex again instead of reusing the earlier
sample. Same texture, same UV, so the compiler normally folds them into one
read. Harmless.

### SoftParticles
Fades the particle where it intersects solid geometry, so there is no hard
slicing line where the sprite crosses the floor or a wall.

1. Scene Depth (Eye mode) = distance to the surface behind the particle.
2. Screen Position (Raw) .w = distance to the particle itself.
3. Depth minus particle depth = the gap between them.
4. Divide by UV1.w and Saturate. Result: 0 right at the surface, rising to 1 as
   the particle moves away.
5. The Branch node checks `Use Soft Particle?`. On = use this fade, Off = 1.

**Requires the Depth Texture enabled in the URP Asset** (or on the camera),
otherwise Scene Depth returns nothing useful and particles can vanish or stop
fading.

---

## Troubleshooting quick list

| Symptom | Look at |
|---------|---------|
| Particles invisible | Alpha chain: texture A, dissolve mask, soft fade, vertex colour A. Any zero hides it. |
| No dissolve happening | Custom1.y not sent from the Particle System, or Main Tex has no G channel pattern. |
| Dissolve edge too hard or too soft | Custom1.z (falloff). |
| Second colour never shows | Custom2 not sent (w = 0), or Main Tex has no B channel mask. |
| Hard line where particles hit the floor | Depth Texture off in the URP Asset, or Custom1.w is 0. |
| No wobble | `DIstortion Str` at 0, `DistortionScroll` at 0, or no noise texture assigned. |
| Border and burn rim change together | Expected. They share OutlineColor and OutlineThickness. |
