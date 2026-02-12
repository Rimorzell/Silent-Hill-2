# Silent Hill 2 Style Camera + Interaction Prototype (Unity)

This repository now contains a modular, production-style foundation for:

- **Fixed/zone-based cinematic camera behavior** inspired by classic survival horror framing.
- **Player movement that remains controllable across camera cuts**.
- **Interaction + pickup flow** for your test boxes.
- **Low manual setup** through an optional bootstrap component.

> Note: this recreates *style and behavior patterns* (fixed shots, blending, interaction cadence), not copyrighted assets/content.

---

## Architecture (not one giant script)

### Camera system
- `SH2CameraAnchor`: Defines a shot (position/rotation/FOV, blend time, priority, optional follow yaw offset, LOS collision rules).
- `SH2CameraZone`: Trigger volume that groups camera anchors and chooses the highest-priority active anchor.
- `SH2CameraZoneTrigger`: Registers player enter/exit with the camera brain.
- `SH2CameraBrain`: Global camera controller on the Main Camera. Resolves active anchor, blends, hard-cuts long jumps, and pulls camera forward if occluded.

### Player system
- `SH2PlayerController`: Camera-relative or tank controls, acceleration smoothing, gravity, turning.

### Interaction system
- `IInteractable`: Interface for all interactables.
- `InteractorContext`: Context passed into interaction (actor transform now, expandable later).
- `SH2InteractionSensor`: Spherecast/overlap-based detection + interact key handling.
- `PickupInteractable`: Basic pickup behavior (destroy/disable, event hook).

### Bootstrap
- `SH2SceneBootstrap`: Optional one-click runtime wiring to reduce manual setup for prototypes.

---

## Fast setup (minimum manual work)

1. **Create / use your cylinder as player**.
   - Add `CharacterController` (or let bootstrap add it).
   - Keep it tagged `Player` (or let bootstrap assign it).

2. **Main Camera**
   - Add `SH2CameraBrain` to your Main Camera (or let bootstrap add it).

3. **Add bootstrap (optional but recommended right now)**
   - Create an empty GameObject named `SH2_Bootstrap`.
   - Add `SH2SceneBootstrap`.
   - Drag your player + main camera references if you want explicit setup.

4. **Create camera zones**
   - For each area/corridor/room, add an empty GameObject with a `BoxCollider` set as trigger.
   - Add `SH2CameraZone` + `SH2CameraZoneTrigger`.
   - Add one or more child objects with `SH2CameraAnchor`.
   - Tune anchor `Priority` when overlaps occur.

5. **Add pickup interactions to your boxes**
   - Add collider if missing.
   - Add `PickupInteractable`.
   - Optional: configure `displayName` and `onPickedUp` UnityEvent.

6. **Player interaction**
   - Ensure player has `SH2InteractionSensor` (bootstrap can add it).
   - Press `E` near a box to pick it up.

---

## Recommended initial tuning values

- `SH2PlayerController`
  - Walk speed: `2.4`
  - Run speed: `4.3`
  - Turn speed: `420`
  - Input style: `CameraRelative` for now
- `SH2CameraBrain`
  - Default blend: `0.25`
  - Hard cut distance: `40`
- `SH2InteractionSensor`
  - Distance: `2.0`
  - Radius: `0.45`

---

## Edge cases covered

- **Multiple overlapping zones**: highest-priority anchor wins.
- **Missing camera zone anchors**: camera falls back to optional fallback anchor (or holds previous camera safely).
- **Huge camera jumps**: auto hard-cut for readability (prevents smeared transitions).
- **Camera occlusion**: spherecast LOS correction prevents walls from fully blocking player.
- **No main camera found**: interaction sensor and bootstrap gracefully fallback/warn.
- **Interactables with nested colliders**: sensor searches own + parent interactable.
- **Double pickup spam**: pickup has internal `picked` guard.
- **Analog diagonal over-speeding**: clamped input magnitude.

---

## Professional extension points for later

When you’re ready for next phase (inventory/animation), this structure is ready for:

- replacing `PickupInteractable` with data-driven item definitions,
- adding interaction prompts/UI from `onFocusChanged`,
- adding animator state machine hooks in `SH2PlayerController`,
- adding context-sensitive interact priority rules,
- adding room graph tooling to auto-generate zones.

---

## Folder layout

- `Assets/Scripts/Camera`
- `Assets/Scripts/Player`
- `Assets/Scripts/Interaction`
- `Assets/Scripts/Core`

