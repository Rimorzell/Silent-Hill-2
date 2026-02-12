# Silent-Hill-2 (Unity Prototype)

This repository now contains a **modular, production-leaning prototype** for:

- Silent Hill 2 style camera behavior (zone-based cinematic camera + robust fallback follow camera)
- Third-person character movement for your cylinder player
- A clean interaction pipeline using Unity's **new Input System**
- Box pickup interactions (for now, no inventory dependency)

---

## Included Scripts

### Core
- `Assets/Scripts/Core/SH2InputReader.cs`
  - Centralized input source (Move, Look, Interact, Run)
  - Uses Unity Input System actions created in code (minimal scene setup)

### Character
- `Assets/Scripts/Character/SH2CharacterMotor.cs`
  - CharacterController-based movement
  - Camera-relative navigation
  - Smooth facing rotation
  - Walk / run support

### Camera
- `Assets/Scripts/Camera/SH2CameraController.cs`
  - Picks best camera zone based on trigger overlap and priority
  - Smooth blend between zones
  - Fallback follow camera when outside zones
- `Assets/Scripts/Camera/SH2CameraZone.cs`
  - Trigger volume + virtual camera point data (position/rotation/FOV/priority/blend)

### Interaction
- `Assets/Scripts/Interaction/ISH2Interactable.cs`
  - Interface contract for all interactables
- `Assets/Scripts/Interaction/SH2InteractionController.cs`
  - Camera-forward sphere cast focus detection
  - Robust focus switching
  - Prompt UI support
  - Calls interact action
- `Assets/Scripts/Interaction/SH2PickupItem.cs`
  - Pickup behavior for your boxes
  - Optional visual highlight support

---

## Setup Guide (Minimal Manual Work)

## 1) Project prerequisites

1. Open Unity project.
2. Install/enable **Input System** package.
3. In Player Settings, set Active Input Handling to **Input System Package** or **Both**.

## 2) Player (Cylinder)

On your cylinder GameObject:

1. Add a `CharacterController` component.
2. Tag object as `Player`.
3. Add scripts:
   - `SH2InputReader`
   - `SH2CharacterMotor`
   - `SH2InteractionController`
4. `SH2CharacterMotor` auto-finds camera and input reader; override references only if needed.
5. Keep collider dimensions sensible for floor traversal.

## 3) Main Camera

On your Main Camera:

1. Add `SH2CameraController`.
2. Assign `Player Target` to your player transform (optional if player is tagged correctly).
3. Tune fallback offset for your desired framing.

## 4) Camera zones (Silent-Hill-like fixed angles)

For each camera angle area:

1. Create empty GameObject (e.g. `CamZone_Corridor_01`).
2. Add a `BoxCollider` and enable **Is Trigger**.
3. Add `SH2CameraZone`.
4. Place/rotate this GameObject as your virtual camera point (or assign a child transform as `virtualCameraPoint`).
5. Set:
   - `Priority` (higher wins when overlapping zones)
   - `Field Of View`
   - `Blend Time`

This gives you room-based cinematic cuts/blends similar to classic survival-horror camera logic.

## 5) Pickups (boxes)

For each pickup box:

1. Ensure it has a Collider.
2. Add `SH2PickupItem`.
3. (Optional) Assign renderers to highlight list (or leave default; script auto-fills on reset).
4. Make sure layer is included in `interactableMask` on `SH2InteractionController`.

## 6) Optional Prompt UI

1. Create Canvas (Screen Space Overlay).
2. Add a `Text` UI element.
3. Assign that Text to `promptText` in `SH2InteractionController`.

If omitted, interaction still works; only prompt display is skipped.

---

## Edge Cases Covered

- No input reader found -> character/interaction safely no-op.
- No camera found -> interaction and movement degrade safely.
- Multiple interactables in front -> nearest valid interactable wins.
- Interactable becomes invalid between focus and press -> re-check in interaction call.
- Leaving all camera zones -> smooth fallback follow camera.
- Overlapping zones -> deterministic priority-based resolution.
- Repeated interaction press on already picked item -> ignored safely.
- Missing optional UI text -> interaction still functional.

---

## Recommended Next Step (later)

When you’re ready, we can add:

- Inventory backend + item definitions (ScriptableObjects)
- Different interaction types (inspect, push, unlock, read)
- James-like turning acceleration/strafe variants
- Camera collision handling and occlusion-aware shot correction
- Animation hooks (Animator + state machine + root-motion-compatible motor)

