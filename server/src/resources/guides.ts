import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";

function makeGuideResource(title: string, content: string) {
  return async (uri: URL) => ({
    contents: [
      {
        uri: uri.href,
        mimeType: "text/plain",
        text: `# ${title}\n\n${content}`,
      },
    ],
  });
}

export function registerGuideResources(server: McpServer) {

  server.resource(
    "guide-physics",
    "unity://guides/physics",
    makeGuideResource("Unity Physics Workflow", `## Setup Order
1. Add Collider first, then Rigidbody. Rigidbody without collider = no collision.
2. Use MeshCollider only when primitive colliders (Box/Sphere/Capsule) can't approximate shape. MeshCollider is expensive.
3. Set Collision Detection to Continuous for fast-moving objects (bullets, projectiles) to prevent tunneling.

## Rigidbody Rules
- Never move Rigidbody with Transform. Use rb.MovePosition(), AddForce(), or velocity.
- Physics code goes in FixedUpdate(), not Update(). FixedUpdate runs at fixed timestep (default 0.02s).
- Use Kinematic rigidbody for objects that move but shouldn't be affected by physics (moving platforms, doors).
- Interpolate = Interpolate for player characters (smooths visual jitter between fixed steps).

## Collision Matrix
- Use Layer Collision Matrix (Physics > Settings) to disable unnecessary collision checks.
- Example: "Bullet" layer shouldn't collide with other bullets. "UI" layer shouldn't collide with anything.
- Each disabled pair = free performance.

## Common Pitfalls
- Scaled colliders are slower. Prefer uniform scale (1,1,1) on physics objects.
- Don't nest Rigidbodies. Child rigidbody under parent rigidbody = unpredictable.
- Trigger colliders (isTrigger=true) still need at least one Rigidbody in the pair.
- Physics.Raycast default maxDistance is Infinity — always set a reasonable max.

## Physics Materials
- Create PhysicMaterial for bounciness/friction. Without it, defaults are applied.
- Combine mode: Average (default), Multiply, Minimum, Maximum — set per-material.`)
  );

  server.resource(
    "guide-ui",
    "unity://guides/ui",
    makeGuideResource("Unity UI Workflow", `## Canvas Setup
1. Use Screen Space - Overlay for HUD (health bars, score). Renders on top of everything.
2. Use Screen Space - Camera for in-world UI that respects post-processing.
3. Use World Space for 3D UI (floating name tags, in-game screens).
4. ONE canvas per major UI section (HUD, Menu, Dialog). Multiple canvases = multiple draw calls but independent rebuilds.

## Performance
- Canvas rebuild is expensive. Split static UI (background, frames) and dynamic UI (text, bars) into separate canvases.
- Disable Raycast Target on all non-interactive elements (images, text labels).
- Use TextMeshPro over legacy Text. Always.
- Avoid Layout Groups on frequently-changing UI. Cache layouts manually if needed.

## Anchoring & Scaling
- Set anchors to match element's expected position relative to screen edges.
- Stretch anchors (min != max) for elements that should resize with screen.
- Use Canvas Scaler: Scale With Screen Size, Reference Resolution 1920x1080, Match 0.5.
- Test at multiple aspect ratios: 16:9, 18:9, 4:3.

## Event System
- Only ONE EventSystem per scene. Multiple = input bugs.
- For Input System package: replace StandaloneInputModule with InputSystemUIInputModule.
- Use Button.onClick.AddListener() in code, UnityEvent in inspector for designer-friendly setup.

## Scroll & Mask
- ScrollRect needs: Viewport (with Mask or RectMask2D), Content (with Layout Group or manual sizing).
- RectMask2D is cheaper than Mask (no extra draw call). Use RectMask2D unless you need non-rectangular masks.
- Set Content size via ContentSizeFitter or manual RectTransform size.`)
  );

  server.resource(
    "guide-animation",
    "unity://guides/animation",
    makeGuideResource("Unity Animation Workflow", `## Animator Controller Setup
1. Create Animator Controller asset > assign to GameObject's Animator component.
2. States = animations. Transitions = conditions to switch between them.
3. Default state = orange. Entry always goes to default state.
4. Use "Any State" for global transitions (damage, death) that can happen from any state.

## Parameters & Transitions
- Use Trigger for one-shot actions (attack, jump). Resets automatically after transition.
- Use Bool for persistent states (isRunning, isGrounded).
- Use Float/Int for blend trees (speed, direction).
- Disable "Has Exit Time" for responsive gameplay transitions (idle→run should be instant).
- Set Transition Duration to 0.1-0.15 for snappy gameplay, 0.25+ for cinematic.

## Blend Trees
- 1D blend: speed (idle → walk → run) with single float parameter.
- 2D Freeform Directional: movement (forward/back/left/right) with X and Y floats.
- Threshold values should match animation clip root motion speed if using Root Motion.

## Animation Events
- Add events in Animation window timeline to call methods at specific frames.
- Use for: footstep sounds, attack hitbox activation, VFX spawn timing.
- Method must exist on same GameObject or children.

## Layers
- Base Layer: locomotion (idle, walk, run, jump).
- Upper Body Layer (Avatar Mask): aiming, shooting — plays on top of locomotion.
- Set layer weight: 0 = inactive, 1 = full override.
- Use Additive blending for layers that modify base (breathing, leaning).

## Performance
- Disable Animator on off-screen objects: animator.enabled = false.
- Use Animator.StringToHash() for parameter names, not raw strings.
- Culling Mode: Cull Update Transforms for off-screen characters (saves CPU).`)
  );

  server.resource(
    "guide-lighting",
    "unity://guides/lighting",
    makeGuideResource("Unity Lighting Workflow", `## Lighting Modes
- Realtime: dynamic shadows, expensive. Use for moving lights (flashlights, fire).
- Baked: pre-calculated into lightmaps. Free at runtime. Use for static lights.
- Mixed: baked indirect + realtime direct. Best balance for most games.

## Baking Workflow
1. Mark static objects as "Contribute GI" (Static flags > Contribute GI).
2. Set lights to Mixed or Baked mode.
3. Open Lighting window > Generate Lighting.
4. Lightmap Resolution: 10-20 for mobile, 20-40 for desktop, 40+ for archviz.
5. Lightmap Padding: 2+ to avoid bleeding.

## Light Probes
- Place Light Probes in grid around areas where dynamic objects move.
- Denser near light transitions (doorways, shadow edges).
- Dynamic objects (characters, pickups) sample light probes for indirect lighting.
- Without light probes, dynamic objects only get direct light (look flat/dark in baked scenes).

## Reflection Probes
- Place in each distinct environment area (rooms, outdoor zones).
- Box Projection = true for indoor environments (more accurate reflections).
- Baked for static scenes, Realtime for dynamic (expensive).
- Resolution: 128-256 usually sufficient.

## Common Pitfalls
- Lightmap UV overlap = artifacts. Check "Generate Lightmap UVs" in model import settings.
- Light bleeding through walls = increase lightmap resolution or add geometry thickness.
- Mixed lighting Shadow Mask mode limited to 4 overlapping lights per object.
- Always bake lighting before final build. Don't ship auto-generated lightmaps.

## Environment Lighting
- Skybox affects ambient color. Set in Lighting > Environment.
- Gradient mode for more control: Sky/Equator/Ground colors.
- Ambient Intensity controls indirect light strength.`)
  );

  server.resource(
    "guide-audio",
    "unity://guides/audio",
    makeGuideResource("Unity Audio Workflow", `## Audio Source Setup
- 3D Sound: set Spatial Blend = 1. For world sounds (footsteps, explosions).
- 2D Sound: set Spatial Blend = 0. For UI sounds, music, narration.
- Min/Max Distance defines falloff. Min = full volume, Max = silent.
- Rolloff: Logarithmic (realistic) or Linear (game-friendly, easier to tune).

## Audio Mixer
1. Create AudioMixer asset > create groups: Master, Music, SFX, UI, Ambient.
2. Assign AudioSource output to mixer group.
3. Expose volume parameters for settings menu: mixer.SetFloat("MusicVolume", dB).
4. Convert slider 0-1 to dB: Mathf.Log10(value) * 20. (0.0001 minimum, not 0).
5. Use Snapshots for state changes (underwater = muffled, pause = music only).

## Performance
- Compress audio: Vorbis for music/long clips, ADPCM for short SFX.
- Load Type: Decompress on Load for short SFX, Streaming for music/ambient.
- Max 32 simultaneous AudioSources is practical limit before quality degrades.
- Use AudioSource.PlayOneShot() for overlapping SFX (gunfire, footsteps).

## Spatial Audio
- Attach AudioListener to main camera (one per scene, required).
- Use Audio Reverb Zones for room acoustics (cave, bathroom, outdoor).
- Doppler Factor: 0 for most games (realistic doppler is distracting).

## Common Patterns
- Music manager: singleton, DontDestroyOnLoad, crossfade between tracks.
- SFX pooling: pre-instantiate AudioSources, reuse for one-shots.
- Footstep system: array of clips + random selection to avoid repetition.`)
  );

  server.resource(
    "guide-scene",
    "unity://guides/scene-management",
    makeGuideResource("Unity Scene Management Workflow", `## Multi-Scene Setup
- Split project into scenes: Persistent (managers), Gameplay, UI, Environment.
- Persistent scene loads first, stays loaded. Contains: AudioManager, GameManager, EventSystem.
- Load other scenes additively: SceneManager.LoadSceneAsync("Level1", LoadSceneMode.Additive).
- Set active scene for new object placement: SceneManager.SetActiveScene(scene).

## Loading Patterns
- Use AsyncOperation for non-blocking loads. Show progress bar with asyncOp.progress.
- Unload scenes when not needed: SceneManager.UnloadSceneAsync("Level1").
- Loading screen pattern: Load "Loading" scene > async load target > unload "Loading".

## DontDestroyOnLoad
- Use for: Player data, Audio manager, Network manager.
- Create empty "DontDestroyOnLoad" GameObject as parent to organize.
- Beware duplication: check if instance exists before creating.
- Alternative: additive Persistent scene (better organization, no DDOL quirks).

## Scene Organization
- Use empty GameObjects as folders: —Environment—, —Gameplay—, —Lighting—.
- Prefix with — or _ so they sort to top of hierarchy.
- Keep root object count low. Nested hierarchies are fine.

## Build Settings
- Scene index 0 = first loaded scene. Usually splash/boot scene.
- Only scenes in Build Settings are included in builds.
- Editor-only scenes (test/debug) can be excluded from build.

## Common Pitfalls
- References break across scenes. Use events, scriptable objects, or service locators.
- Lighting bake is per-scene. Multi-scene lighting needs manual probe/lightmap management.
- EventSystem must be in only one loaded scene at a time.`)
  );

  server.resource(
    "guide-performance",
    "unity://guides/performance",
    makeGuideResource("Unity Performance Optimization", `## Rendering
- Use Static Batching for non-moving objects (enable in Player Settings + mark Static).
- Use GPU Instancing for repeated objects (trees, grass). Enable on material.
- Reduce draw calls: atlas textures, merge meshes, use LOD Groups.
- Occlusion Culling: bake for indoor/complex scenes. Free culling of hidden objects.

## LOD (Level of Detail)
1. Create 3 mesh LODs: LOD0 (full), LOD1 (50%), LOD2 (10%).
2. Add LOD Group component, assign renderers to each level.
3. Set transition distances: LOD0 60%, LOD1 30%, LOD2 10%, Culled below.
4. Cross-fade mode for smoother transitions (costs extra draw call during fade).

## CPU Optimization
- Cache GetComponent results. Never call GetComponent in Update.
- Use object pooling for frequently spawned/destroyed objects (bullets, particles, enemies).
- Avoid Find/FindObjectOfType at runtime. Cache references in Start/Awake.
- Use Jobs + Burst for heavy computation (pathfinding, procedural generation).

## Memory
- Compress textures: ASTC (mobile), BC7 (desktop). Never use uncompressed in builds.
- Sprite Atlas for 2D: reduces texture swaps and memory fragmentation.
- Addressables for large projects: load/unload asset bundles on demand.
- Profile with Memory Profiler package for leak detection.

## Physics
- Simplify colliders. Use primitives over MeshCollider.
- Reduce Fixed Timestep if physics doesn't need 50Hz (0.02 → 0.04 for 25Hz).
- Layer collision matrix: disable unnecessary pairs.

## Profiler Usage
1. Profile on target device, not editor (editor adds overhead).
2. Deep Profile only when needed (massive overhead).
3. Look for: GC.Alloc spikes, long frame times, excessive draw calls.
4. Frame Debugger for rendering issues: Window > Analysis > Frame Debugger.

## Mobile Specific
- Target 30fps for battery life, 60fps for action games.
- Reduce shader variants: strip unused. Use Shader Variant Collection.
- Bake lighting, avoid realtime shadows on mobile.`)
  );

  server.resource(
    "guide-prefab",
    "unity://guides/prefab-workflow",
    makeGuideResource("Unity Prefab Workflow", `## Prefab Basics
- Prefab = reusable template. Changes to prefab propagate to all instances.
- Create: drag GameObject from hierarchy to Project window.
- Edit: double-click prefab in Project to open Prefab Mode (isolated editing).
- Always edit in Prefab Mode for clean changes. Scene overrides get messy.

## Nested Prefabs
- Nest prefabs for modular design: Room prefab contains Furniture prefabs.
- Inner prefab changes propagate through outer prefab automatically.
- Override nested prefab properties per-instance for variation.

## Prefab Variants
- Variant = prefab that inherits from base prefab.
- Use for: Enemy_Base → Enemy_Fast, Enemy_Tank, Enemy_Flying.
- Base changes propagate to variants. Variant overrides persist.
- Max 1 level of variant (no variant-of-variant).

## Override Management
- Blue marker in Inspector = property overridden from prefab.
- Right-click override > Apply to save back to prefab, Revert to discard.
- Apply to: choose which prefab in nested hierarchy receives the change.
- Overrides panel (top of Inspector) shows all overrides at once.

## Common Patterns
- Composition over inheritance: small prefabs combined, not one mega-prefab.
- Prefab palette: folder of small building blocks (walls, floors, props).
- Scriptable Object config: prefab references SO for tuning values. Change SO = all instances update.

## Pitfalls
- Don't break prefab connection by restructuring hierarchy in scene.
- Renaming prefab asset = fine. Renaming instance root in scene = override.
- Prefab instances in inactive scenes don't update until scene is loaded.`)
  );

  server.resource(
    "guide-materials",
    "unity://guides/materials-shaders",
    makeGuideResource("Unity Materials & Shaders Workflow", `## Render Pipeline Choice
- URP (Universal): mobile + console + PC. Best for most projects.
- HDRP (High Definition): AAA quality, PC/console only. Heavy.
- Built-in: legacy. Only use if migrating old project.
- Choose BEFORE starting. Pipeline migration is painful.

## Material Setup (URP)
- Base shader: Lit (PBR), Unlit (no lighting), Simple Lit (cheaper).
- Surface Type: Opaque (solid), Transparent (glass, particles).
- Rendering Face: Both for foliage/thin objects, Front for solid objects.

## Texture Maps
- Albedo (Base Map): color/diffuse texture.
- Normal Map: surface detail without geometry. Mark as "Normal Map" in import settings.
- Metallic/Smoothness: R = metallic, A = smoothness (packed in one texture).
- Emission: self-illumination. Enable "Emission" checkbox on material.

## Material Instances
- Renderer.material creates unique instance (causes draw call break).
- Renderer.sharedMaterial modifies the asset (affects all users).
- Use MaterialPropertyBlock for per-instance changes without instancing break.
- GPU Instancing: enable on material for repeated objects with same material.

## Shader Variants
- Each keyword combination = separate shader variant = compile time + memory.
- Strip unused variants: Project Settings > Graphics > Shader Stripping.
- Use Shader Variant Collection to preload needed variants.
- Avoid excessive multi_compile pragmas in custom shaders.

## Common Pitfalls
- Pink material = missing shader. Upgrade materials after pipeline change.
- Transparent + transparent sorting issues: use alpha cutout when possible.
- Don't duplicate materials unnecessarily. Share materials across objects for batching.
- Normal map import: set Texture Type to "Normal map" or it looks wrong.`)
  );

  server.resource(
    "guide-2d",
    "unity://guides/2d-game",
    makeGuideResource("Unity 2D Game Workflow", `## Project Setup
- Set project to 2D mode (or switch in Edit > Project Settings > Editor > Default Behavior Mode).
- Camera: Orthographic projection. Size = half the vertical world units visible.
- Pixels Per Unit: consistent across all sprites (100 is default, 16 for pixel art).

## Sorting & Layers
- Sorting Layers: Background, Default, Foreground, UI (order matters).
- Sorting Order: within same layer, higher number renders on top.
- Use Sorting Group component for grouped objects (character + weapon + effects).
- Transparent sprites: Z-fighting solved by sorting order, not Z position.

## Sprite Atlas
- Create Sprite Atlas for related sprites (all UI icons, all character sprites).
- Reduces draw calls by batching sprites into one texture.
- Include in Addressables if using asset bundles.
- Allow Rotation = false for pixel art (rotation causes sub-pixel artifacts).

## Tilemap Workflow
1. Create Tilemap: GameObject > 2D Object > Tilemap > Rectangular.
2. Create Tile Palette: Window > 2D > Tile Palette > Create New Palette.
3. Drag sprites into palette to create tiles.
4. Layer tilemaps: Ground (base), Decoration (flowers), Collision (invisible, for colliders).
5. Add TilemapCollider2D + CompositeCollider2D for efficient collision.

## 2D Physics
- Use Rigidbody2D + Collider2D (BoxCollider2D, CircleCollider2D, etc.).
- Gravity Scale: 0 for top-down games, 1+ for platformers.
- CompositeCollider2D: merge multiple colliders into one for performance (tilemaps).
- For platformers: use Rigidbody2D with constraints (freeze Z rotation).

## 2D Animation
- Sprite Animation: slice spritesheet, create Animation clip, drag frames.
- Skeletal (2D Animation package): bones + mesh deformation for complex characters.
- Sprite Swap: change body parts at runtime (equipment, expressions).

## Pixel Art Specific
- Filter Mode: Point (no filter). Compression: None.
- Anti-aliasing OFF in camera/URP settings.
- Pixel Perfect Camera component (2D Pixel Perfect package).
- Snap movement to pixel grid for clean rendering.`)
  );

  server.resource(
    "guide-cinemachine",
    "unity://guides/cinemachine",
    makeGuideResource("Unity Cinemachine Workflow", `## Setup
1. Add CinemachineBrain to Main Camera (one per camera).
2. Create Virtual Cameras for each view (follow, orbit, static, etc.).
3. Brain blends between active virtual cameras based on Priority.

## Camera Types
- Follow Camera: Body = Transposer, Aim = Composer. For 3rd person.
- FreeLook: 3 orbital rigs (top/mid/bottom). For full orbit around target.
- State-Driven: tie cameras to Animator states (walk cam, sprint cam, combat cam).
- Dolly Track: camera moves along path. For cutscenes, cinematic reveals.

## Follow & Look At
- Follow = camera position target (player transform).
- Look At = camera rotation target (can differ from Follow).
- Damping: higher = smoother/slower follow. 0 = locked on.

## Noise (Camera Shake)
- Add Noise profile to virtual camera for handheld feel.
- Basic Multi Channel Perlin: Amplitude = 0.5-1 for subtle, 2+ for action.
- Trigger shake via impulse: CinemachineImpulseSource.GenerateImpulse().

## Confiner
- CinemachineConfiner2D (2D) / Confiner (3D): restrict camera to bounds.
- Use PolygonCollider2D as bounding shape for 2D games.
- Set Damping for smooth stop at boundaries.

## Common Patterns
- Priority system: set higher priority on combat camera, auto-switches.
- Blend list: define custom blend curves between specific camera pairs.
- Target Group: multiple targets, camera frames all of them (split-screen alternative).

## Pitfalls
- Only one CinemachineBrain per camera. Multiple brains = conflicts.
- Virtual cameras at Priority 0 = disabled. Use 10+ for active.
- Cinemachine v3 (Unity 6): API changed significantly from v2. Check version first.`)
  );

  server.resource(
    "guide-navmesh",
    "unity://guides/navmesh",
    makeGuideResource("Unity NavMesh Workflow", `## Baking
1. Mark walkable objects: Static > Navigation Static.
2. Open Navigation window: Window > AI > Navigation.
3. Set Agent settings: radius, height, step height, slope angle.
4. Click Bake. Blue overlay = walkable area.

## NavMesh Agent
- Add NavMeshAgent component to AI characters.
- Set destination: agent.SetDestination(targetPosition).
- Agent properties: speed, angular speed, acceleration, stopping distance.
- Auto-braking: true for destinations, false for patrol paths.

## NavMesh Obstacles
- Use for dynamic blockers (doors, barrels, moving walls).
- Carve = true: cuts hole in NavMesh. Agents path around.
- Carve = false: agents try to avoid but don't update NavMesh.
- Carve Only Stationary: reduces cost for moving obstacles.

## NavMesh Links
- Connect disconnected NavMesh areas (jump gaps, ladders, teleporters).
- OffMeshLink: manual placement between two points.
- Auto-generate: enable in Navigation > Bake > Generated Off Mesh Links.

## Common Patterns
- Patrol: array of waypoints, cycle through with SetDestination.
- Chase: SetDestination(player.position) each frame in Update.
- Flee: calculate direction away from threat, SetDestination to flee point.
- Check arrival: agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending.

## Pitfalls
- NavMesh is 2.5D (surface-based). No true 3D navigation (flying AI needs custom solution).
- Large dynamic obstacles with Carve = expensive. Minimize.
- NavMesh bake is per-scene. Multi-scene: bake each, or use NavMesh Components package for runtime baking.
- Agent radius must match baked radius or pathfinding breaks at narrow passages.`)
  );

  server.resource(
    "guide-terrain",
    "unity://guides/terrain",
    makeGuideResource("Unity Terrain Workflow", `## Creation
1. GameObject > 3D Object > Terrain.
2. Set size first: Terrain Settings > Mesh Resolution > Terrain Width/Length/Height.
3. Heightmap Resolution: 513 (small), 1025 (medium), 2049 (large). Power of 2 + 1.

## Sculpting
- Raise/Lower: left click raise, ctrl+click lower.
- Smooth: flatten rough areas after sculpting.
- Stamp: apply height pattern (crater, hill) from brush texture.
- Set Height: paint to specific height. Useful for flat areas (roads, building foundations).

## Painting
- Create Terrain Layers (material per texture): grass, dirt, rock, sand.
- Assign normal maps and mask maps for PBR quality.
- Blend at edges with brush opacity. Max 4-8 layers for mobile, 16 for desktop.
- First layer = default coverage. Additional layers paint on top.

## Trees & Details
- Add tree prototypes: use LOD prefabs for distance optimization.
- Density: mass place trees, then adjust per-area.
- Terrain details (grass, flowers): use GPU Instancing for performance.
- Billboard Distance: far trees render as 2D billboards automatically.

## Performance
- Enable Draw Instanced for GPU instancing of terrain patches.
- Pixel Error: 5-10 (lower = more triangles = better quality = slower).
- Base Map Distance: distance at which terrain switches to low-res base map.
- Tree Distance & Billboard Start: tune per-platform.
- Speed Tree integration: LOD transitions are automatic.

## Pitfalls
- Terrain at origin (0,0,0) is corner, not center. Plan level layout accordingly.
- Multiple terrains for large worlds: create neighbors, stitch edges with same height.
- Terrain collider is auto-generated from heightmap. No manual collider needed.
- Painting near terrain edges = texture bleeding to neighbor. Fix with matching layers.`)
  );

  server.resource(
    "guide-input",
    "unity://guides/input-system",
    makeGuideResource("Unity Input System Workflow", `## Setup
1. Install Input System package.
2. Create Input Actions asset: right-click > Create > Input Actions.
3. Define Action Maps: "Player", "UI", "Vehicle".
4. Define Actions per map: "Move", "Jump", "Attack", "Look".
5. Add Bindings per action: WASD, gamepad stick, touch.

## Action Types
- Value: continuous input (movement axis, mouse position). Fires continuously.
- Button: discrete press (jump, attack). Fires on press/release.
- Pass-Through: raw input, no conflict resolution. For multi-device.

## Player Input Component
- Add PlayerInput to player GameObject.
- Set Actions asset, Default Map.
- Behavior modes: Send Messages, Invoke Unity Events, Invoke C# Events.
- Invoke Unity Events: most flexible, wire up in Inspector.

## Code Usage
\`\`\`
// Generate C# class from Input Actions asset (tick "Generate C# Class")
var input = new PlayerInputActions();
input.Player.Jump.performed += ctx => Jump();
input.Player.Move.performed += ctx => movement = ctx.ReadValue<Vector2>();
input.Player.Move.canceled += ctx => movement = Vector2.zero;
input.Enable();
\`\`\`

## Control Schemes
- Define schemes: "Keyboard&Mouse", "Gamepad", "Touch".
- Assign devices to schemes.
- Auto-switch based on last used device.
- Use PlayerInput.onControlsChanged for UI prompt switching (show gamepad/keyboard icons).

## Common Patterns
- Rebinding: use Interactive Rebinding to let players customize keys.
- Composite bindings: WASD = 2D Vector composite (up/down/left/right).
- Input buffering: store last input for N frames for responsive combat systems.
- Dead zone: set on Stick binding for consistent gamepad behavior.

## Pitfalls
- Don't mix old Input Manager and new Input System without setting Active Input Handling to "Both".
- UI requires InputSystemUIInputModule (replaces StandaloneInputModule).
- Action must be enabled: actionMap.Enable() or playerInput handles it.`)
  );

  server.resource(
    "guide-timeline",
    "unity://guides/timeline",
    makeGuideResource("Unity Timeline Workflow", `## Setup
1. Create Timeline asset: right-click > Create > Timeline.
2. Add Playable Director component to controller GameObject.
3. Assign Timeline asset to Playable Director.
4. Open Timeline window: Window > Sequencing > Timeline.

## Track Types
- Animation Track: play animation clips on specific objects.
- Activation Track: enable/disable GameObjects at specific times.
- Audio Track: play audio clips with timing.
- Signal Track: fire events at specific points (trigger gameplay, VFX).
- Cinemachine Track: blend between virtual cameras for cutscenes.

## Clips & Blending
- Drag clips onto tracks, position on timeline.
- Overlap clips to blend (crossfade) between them.
- Ease In/Out curves control blend shape.
- Set clip speed multiplier for slow-mo/fast-forward effects.

## Signals
1. Create Signal asset: right-click > Create > Signal.
2. Place Signal Emitter on Signal Track at desired time.
3. Add Signal Receiver component to any GameObject.
4. Wire Signal to UnityEvent in Signal Receiver.
- Use for: trigger dialogue, spawn enemies, play particles.

## Common Patterns
- Cutscene: combine Camera track + Animation + Audio + Activation tracks.
- Intro sequence: activate title text, fade camera, start gameplay.
- Boss entrance: camera dolly + animation + audio + particle activation.
- Wrap Mode: None (play once), Loop, Hold (freeze on last frame).

## Pitfalls
- Timeline overrides object state during playback. Original state restored on stop (if using defaults).
- Binding references: Timeline tracks need object bindings in Playable Director.
- Performance: complex timelines with many tracks = editor slowdown. Keep focused.`)
  );

  server.resource(
    "guide-vfx",
    "unity://guides/vfx-particles",
    makeGuideResource("Unity VFX & Particle Workflow", `## Particle System (Shuriken)
- Good for: most game effects (fire, smoke, sparks, magic, rain).
- Start with defaults, modify one module at a time.
- Play on Awake: true for ambient (rain, fire), false for triggered effects (explosion).

## Key Modules
- Main: duration, start lifetime, speed, size, color, gravity.
- Emission: rate (continuous) or bursts (one-time spawn).
- Shape: Cone (default), Sphere, Box, Edge. Matches emission pattern.
- Color over Lifetime: fade out (alpha 1→0). Essential for smooth effects.
- Size over Lifetime: shrink for fire/smoke, grow for shockwaves.
- Noise: turbulence for organic movement (smoke, magical).

## VFX Graph (GPU Particles)
- Good for: massive particle counts (millions), compute-based effects.
- Requires HDRP or URP with VFX Graph package.
- Visual node-based editor. More powerful but steeper learning curve.
- Use for: volumetric fog, flocking, fluid simulation, dense environments.

## Common Effect Recipes
- Fire: Cone shape, upward velocity, orange→red color over life, noise, shrink.
- Smoke: Sphere shape, slow upward, grey→transparent, grow slightly, high noise.
- Sparks: short lifetime, high speed, gravity, small size, bright yellow.
- Explosion: burst emission, sphere shape, outward velocity, fire + smoke sub-emitters.
- Rain: Box shape (wide, above camera), downward velocity, long streaks (stretched billboard).

## Performance
- Particle budget: 1000-5000 for mobile, 10000-50000 for desktop.
- Use Sub Emitters sparingly (each sub-emitter = separate system).
- Renderer: Billboard (cheapest), Stretched Billboard, Mesh (expensive).
- Culling: Max Particles limit + off-screen culling.

## Pitfalls
- Particle systems sort by distance. Overlapping transparent particles = overdraw.
- Looping particles with Prewarm: first frame renders full lifetime of particles.
- Collision module is expensive. Use trigger zone instead when possible.`)
  );

  server.resource(
    "guide-addressables",
    "unity://guides/addressables",
    makeGuideResource("Unity Addressables Workflow", `## When to Use
- Large projects with many assets (100+ MB).
- Need to reduce initial download/build size.
- Want to load/unload assets on demand.
- DLC or downloadable content packs.

## Setup
1. Install Addressables package.
2. Window > Asset Management > Addressables > Groups.
3. Mark assets as Addressable (checkbox in Inspector or drag to group).
4. Group assets by loading pattern: "Level1Assets", "UIAssets", "AudioPack".

## Loading
\`\`\`
// Load single asset
var handle = Addressables.LoadAssetAsync<GameObject>("player-prefab");
handle.Completed += op => Instantiate(op.Result);

// Load scene
Addressables.LoadSceneAsync("level-01");

// Release when done (critical for memory)
Addressables.Release(handle);
\`\`\`

## Groups & Profiles
- Group = bundle. Assets in same group = packed together.
- Local group: built into app. Remote group: downloaded at runtime.
- Profiles: switch between local/remote paths for dev vs production.
- Build Script: Use "Use Existing Build" for dev, "New Build" for release.

## Labels
- Tag assets with labels: "enemies", "level1", "hd-textures".
- Load all assets with label: Addressables.LoadAssetsAsync<Sprite>("ui-icons").
- Multiple labels on one asset = included in multiple load calls.

## Common Patterns
- Preload: load critical assets during loading screen, hold handles.
- Reference: use AssetReference in Inspector instead of direct reference.
- Catalog update: check for remote updates with Addressables.CheckForCatalogUpdates().
- Memory management: track all handles, release when transitioning scenes.

## Pitfalls
- Forgetting to Release = memory leak. Every LoadAsync needs matching Release.
- Duplicate dependencies: asset in Group A references texture in Group B = texture loaded with both.
- Analyze tool: Window > Asset Management > Addressables > Analyze to detect duplicates.
- Initial build requires "Build Addressables" before "Build Player".`)
  );

  server.resource(
    "guide-csharp",
    "unity://guides/csharp-conventions",
    makeGuideResource("Unity C# Conventions", `## Naming Conventions

### Classes & Files
| Type | Pattern | Example |
|------|---------|---------|
| Manager | \`{System}Manager\` | \`AudioManager\`, \`UIManager\` |
| Service Interface | \`I{System}Service\` | \`IAuthService\`, \`ISaveService\` |
| Settings SO | \`{System}Settings\` | \`AudioSettings\`, \`GameSettings\` |
| Handler | \`{Purpose}Handler\` | \`InputHandler\`, \`DamageHandler\` |
| ViewModel | \`{Screen}ViewModel\` | \`MainMenuViewModel\` |
| View | \`{Screen}View\` | \`MainMenuView\` |
| Data | \`{Name}Data\` | \`PlayerData\`, \`ItemData\` |
| Event Args | \`{Name}EventArgs\` | \`DamageEventArgs\` |

### Members
| Type | Pattern | Example |
|------|---------|---------|
| Private field | \`_camelCase\` | \`_health\`, \`_moveSpeed\` |
| Public property | \`PascalCase\` | \`Health\`, \`MoveSpeed\` |
| Constant | \`PascalCase\` | \`MaxHealth\`, \`DefaultSpeed\` |
| Event callback | \`On{Event}\` | \`OnDeath\`, \`OnLevelUp\` |
| Bool property | \`is/has/can\` prefix | \`IsAlive\`, \`HasKey\`, \`CanJump\` |

## Using Statement Order
\`\`\`csharp
// 1. Unity namespaces first
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// 2. Third-party packages
using DG.Tweening;
using R3;
// 3. System namespaces last
using System;
using System.Collections.Generic;
using System.Linq;
\`\`\`

## Coding Style Rules

### Prefer Modern C# Syntax
\`\`\`csharp
// switch expression over switch statement
string label = state switch
{
    GameState.Menu => "Main Menu",
    GameState.Playing => "In Game",
    GameState.Paused => "Paused",
    _ => "Unknown",
};

// null-conditional + null-coalescing
int count = _pool?.Count ?? 0;
_service?.Initialize();
string name = player?.Name ?? "Unknown";

// guard clause (return early)
public void TakeDamage(float amount)
{
    if (!IsAlive) return;
    if (amount <= 0) return;
    _health -= amount;
}

// pattern matching
if (collision.gameObject.TryGetComponent<IDamageable>(out var target))
{
    target.TakeDamage(damage);
}
\`\`\`

### SerializeField Pattern
\`\`\`csharp
[Header("=== Movement ===")]
[Tooltip("Walk speed in units/sec")]
[SerializeField] private float _walkSpeed = 5f;

[Tooltip("Run speed multiplier")]
[SerializeField] private float _runMultiplier = 1.5f;

[Header("=== References ===")]
[SerializeField] private Transform _groundCheck;
[SerializeField] private LayerMask _groundLayer;

// Expose via read-only property
public float WalkSpeed => _walkSpeed;
\`\`\`

### Debug Logging Format
\`\`\`csharp
// Always include class name for easy filtering
Debug.Log($"[{GetType().Name}] Initialized with {_itemCount} items");
Debug.LogWarning($"[{GetType().Name}] Pool exhausted, creating new instance");
Debug.LogError($"[{GetType().Name}] Failed to load: {assetPath}");
\`\`\`

## ScriptableObject Settings Pattern
\`\`\`csharp
[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
public class GameSettings : ScriptableObject
{
    [Header("=== Gameplay ===")]
    [Tooltip("Player max HP")]
    [SerializeField] private int _maxHealth = 100;

    [Tooltip("Respawn delay in seconds")]
    [SerializeField] private float _respawnDelay = 3f;

    [Header("=== Debug ===")]
    [SerializeField] private bool _enableDebugLog = false;

    public int MaxHealth => _maxHealth;
    public float RespawnDelay => _respawnDelay;
    public bool EnableDebugLog => _enableDebugLog;
}
\`\`\`

## MonoBehaviour Lifecycle Order
\`\`\`csharp
public class PlayerController : MonoBehaviour
{
    // 1. Serialized fields at top
    [SerializeField] private float _speed = 5f;

    // 2. Private fields
    private Rigidbody _rb;
    private bool _isGrounded;

    // 3. Unity lifecycle in execution order
    private void Awake()    { _rb = GetComponent<Rigidbody>(); }
    private void OnEnable() { InputManager.OnJump += HandleJump; }
    private void Start()    { Initialize(); }
    private void Update()   { ReadInput(); }
    private void FixedUpdate() { ApplyMovement(); }
    private void OnDisable() { InputManager.OnJump -= HandleJump; }
    private void OnDestroy() { Cleanup(); }

    // 4. Public methods
    public void TakeDamage(float amount) { /* ... */ }

    // 5. Private methods
    private void Initialize() { /* ... */ }
    private void HandleJump() { /* ... */ }
}
\`\`\`

## Common Anti-Patterns to Avoid
\`\`\`csharp
// BAD: GetComponent in Update
void Update() { var rb = GetComponent<Rigidbody>(); } // Every frame!

// GOOD: Cache in Awake
Rigidbody _rb;
void Awake() { _rb = GetComponent<Rigidbody>(); }

// BAD: Find in runtime
void Start() { var player = GameObject.Find("Player"); }

// GOOD: SerializeField or cached reference
[SerializeField] private GameObject _player;

// BAD: String comparison for tags
if (other.gameObject.tag == "Enemy") { }

// GOOD: CompareTag (no GC allocation)
if (other.gameObject.CompareTag("Enemy")) { }

// BAD: Instantiate/Destroy frequently
void Shoot() { var b = Instantiate(bulletPrefab); Destroy(b, 2f); }

// GOOD: Object pooling
void Shoot() { var b = _bulletPool.Get(); b.ReturnAfter(2f); }

// BAD: new List in Update (GC pressure)
void Update() { var nearby = new List<Enemy>(); }

// GOOD: Reuse pre-allocated list
private readonly List<Enemy> _nearby = new();
void Update() { _nearby.Clear(); /* reuse */ }
\`\`\`

## URP Shader Rules
- Use \`Universal Render Pipeline/Lit\` or URP-compatible shaders only.
- Built-in shaders (\`Standard\`, \`Legacy\`) = pink material in URP.
- After importing external assets, always verify material shaders.
- ShaderGraph must target URP pipeline.
- For transparent: prefer alpha cutout over true transparency when possible.`)
  );
}
