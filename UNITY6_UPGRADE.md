# Unity 6 upgrade notes

This repository shipped with Unity 2018.2 artifacts and legacy Oculus Integration scripts that depended on the removed `UnityEngine.VR` namespace. The following changes were made so that the project can compile inside Unity 6 (6000.x) without requiring you to touch the third-party sources:

1. Added `Assets/Scripts/Compatibility/UnityVRCompatibility.cs`, which recreates the old `UnityEngine.VR` API surface on top of `UnityEngine.XR`. Every pre-existing script that still calls `UnityEngine.VR.*` continues to work, while new Unity editors and runtimes call into the supported XR stack.
2. Replaced the deprecated Oculus Integration input layer with XR Interaction Toolkit compatible shims:
   - Removed the legacy `Assets/OVR`, `Assets/Oculus`, and `Assets/OculusPlatform` folders that were blocked on Unity 6.
   - Introduced `Assets/Scripts/Compatibility/OVRInputCompatibility.cs`, a lightweight `OVRInput` reimplementation that reads controller state from `UnityEngine.XR.InputDevice`. Your gameplay scripts still call `OVRInput.*`, but the data now flows through Unity's XR provider chain (OpenXR/Oculus via XR Plug-in Management).
   - Added `com.unity.inputsystem`, `com.unity.xr.interaction.toolkit`, `com.unity.xr.management`, `com.unity.xr.oculus`, and `com.unity.xr.openxr` to `Packages/manifest.json` so Unity installs the first-party XR stack on open.
3. Automated the migration to Universal Render Pipeline (URP):
   - Added `com.unity.render-pipelines.universal` to the manifest.
   - `Assets/Editor/URPSetup/EnsureURPSetup.cs` generates a default URP renderer, pipeline asset, and global settings on the next Unity launch, and wires them into `GraphicsSettings`/`QualitySettings` so every quality tier uses URP automatically.

## Manual steps you still need to perform

- Remove the legacy `Library/` folder before opening the project in Unity 6 so the asset database can be rebuilt with the new serialization format.
- Install Unity 6 (6000.x) via Unity Hub and open the project; Unity will update `ProjectSettings/ProjectVersion.txt` automatically.
- In `Project Settings > XR Plug-in Management`, install **XR Plug-in Management**, enable it for every target platform you need, and toggle the Oculus/OpenXR providers that you actually ship with.
- Switch `Edit > Project Settings > Player > Active Input Handling` to **Both** (or **Input System Package (New)**) so the Input System backend that XR Interaction Toolkit depends on is active.
- In `Project Settings > XR Interaction Toolkit`, create (or import) an input action asset and assign bindings for primary/secondary buttons, triggers, grips, thumbsticks, and menu so the `OVRInput` compatibility layer can read meaningful values from `UnityEngine.XR.InputDevice`.
- After Unity finishes importing URP, optionally run `Window > Rendering > Render Pipeline Converter` to convert legacy built-in materials/shaders to URP-compatible variants if you see pink surfaces.
- Review `Packages/manifest.json` in the Unity Package Manager UI and let Unity update first-party packages (Ads, Analytics, Purchasing, TextMeshPro, etc.) to the versions that are verified for Unity 6; the lockfile is intentionally absent so Hub/Editor can write fresh versions.
- Run the project in Play Mode (PCVR and Quest/Android if applicable) and validate rendering, input, and scene streaming paths because Unity 6 tightens XR rendering defaults (render scale, viewport scale).

Keeping these actions manual avoids committing Unity-generated files that are specific to the editor build you use, while making sure the repository itself already contains the code changes needed for Unity 6 compatibility.
