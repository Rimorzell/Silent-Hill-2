# Silent-Hill-2 Style Prototype (Unity)

This repo now contains a modular prototype for:

- **Cinematic fixed camera zones** with smooth blends.
- **Player movement aligned to active camera direction**.
- **Interaction detection + pickup flow** using the **Unity Input System**.
- **Edge-case handling** (null references, disabled zones, duplicate hit filtering, LOS checks, interaction target scoring).

> This is intentionally built as a clean foundation so we can add inventory, animations, and advanced interactions later without rewriting core systems.

---

## Folder structure

- `Assets/Scripts/Core/PlayerInputBridge.cs`
- `Assets/Scripts/Player/PlayerMotor.cs`
- `Assets/Scripts/Camera/FixedCameraDirector.cs`
- `Assets/Scripts/Camera/CameraZoneVolume.cs`
- `Assets/Scripts/Camera/CameraZoneTriggerRelay.cs`
- `Assets/Scripts/Interaction/IInteractable.cs`
- `Assets/Scripts/Interaction/InteractionContext.cs`
- `Assets/Scripts/Interaction/InteractionSensor.cs`
- `Assets/Scripts/Interaction/PickupInteractable.cs`
- `Assets/Scripts/Interaction/PickupInventory.cs`
- `Assets/Scripts/Interaction/InteractionPromptDebug.cs`

---

## Quick setup (minimal manual work)

## 1) Project prerequisites

1. Install/enable Unity's **Input System** package.
2. In Player settings, set active input handling to **Input System Package (New)** or **Both**.

## 2) Player setup (your cylinder)

On your player cylinder:

1. Add a **CharacterController**.
2. Add scripts:
   - `PlayerMotor`
   - `InteractionSensor`
   - `PickupInventory`
   - `PlayerInputBridge`
   - (optional) `InteractionPromptDebug`
3. Add a **PlayerInput** component.
4. Create/assign an Input Actions asset with:
   - `Move` (Vector2)
     - bindings: WASD / left stick
   - `Interact` (Button)
     - bindings: E / gamepad south button
5. Make sure `PlayerInputBridge` action names match your action names (`Move`, `Interact`).

## 3) Camera setup (your main camera)

On the main camera:

1. Add `FixedCameraDirector`.
2. (Optional but recommended) Drag your player transform into the `player` field.
3. Keep fallback offsets as-is initially; tune later.

## 4) Camera zone setup (for SH2-like room/angle switches)

For each camera area:

1. Create an empty object, e.g. `CamZone_Hall_A`.
2. Add a **BoxCollider** and check **Is Trigger**.
3. Add scripts:
   - `CameraZoneVolume`
   - `CameraZoneTriggerRelay`
4. Create a child transform named `Pose` and place/rotate it to your desired cinematic angle.
5. Assign that child into `CameraZoneVolume.cameraPose`.
6. Set `priority` when overlaps exist (higher wins).

## 5) Pickup setup (your boxes)

On each pickup box:

1. Add a collider (not trigger).
2. Add `PickupInteractable`.
3. Optionally create a child `InteractionPoint` near the object center and assign it.
4. Optionally set `destroyOnPickup`.

## 6) Layer masks (recommended)

For robust behavior:

1. Put interactables on an `Interactable` layer.
2. In `InteractionSensor`:
   - `interactionMask`: Interactable layer(s)
   - `obstructionMask`: World + dynamic blockers (exclude player, optionally exclude interactables)

---

## How it works

## Camera flow

- `CameraZoneTriggerRelay` registers/unregisters zones with `FixedCameraDirector` when player enters/exits triggers.
- `FixedCameraDirector` picks the highest-priority active zone.
- Camera smoothly blends to zone pose (`blendTime`).
- If no zone is active, camera falls back to a follow-look behavior.

## Movement flow

- `PlayerInputBridge` reads `Move` from `PlayerInput` every frame.
- `PlayerMotor` converts input into camera-relative world motion.
- Rotation smooths toward movement direction.

## Interaction flow

- `InteractionSensor` casts from camera center to find nearby interactables.
- Candidates are filtered by availability, distance, angle, and optional line-of-sight.
- Best candidate is selected with score favoring closer + centered targets.
- `Interact` input calls `TryInteract()`, invoking `IInteractable.Interact(...)`.

---

## Edge cases already covered

- Missing references fallback (`Camera.main`, auto component lookups).
- Camera zones being disabled/destroyed while active.
- Multiple colliders hitting same interactable (deduplicated).
- Interaction blocked by world geometry (line-of-sight check).
- Stale target after pickup/disable (target clears automatically).
- Overlapping camera zones with deterministic priority resolution.

---

## Suggested tuning values (starting point)

- `PlayerMotor.moveSpeed`: 2.1–2.6
- `PlayerMotor.rotationSpeed`: 10–14
- `InteractionSensor.maxDistance`: 1.8–2.5
- `InteractionSensor.maxViewAngle`: 40–60
- `CameraZoneVolume.blendTime`: 0.35–0.75

---

## Next steps (future iteration)

- Plug pickups into inventory UI data model.
- Add contextual interaction types (inspect, open, push, read).
- Add animation hooks + root-motion aware movement mode.
- Add camera cut constraints and one-way transition gates.
- Add interaction prompt UI with world/canvas styling (replace debug label).

