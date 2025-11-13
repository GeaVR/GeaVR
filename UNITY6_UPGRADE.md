# Unity 6 upgrade notes

This repository shipped with Unity 2018.2 artifacts and legacy Oculus Integration scripts that depended on the removed `UnityEngine.VR` namespace. The following changes were made so that the project can compile inside Unity 6 (6000.x) without requiring you to touch the third-party sources:

1. Added `Assets/Scripts/Compatibility/UnityVRCompatibility.cs`, which recreates the old `UnityEngine.VR` API surface on top of `UnityEngine.XR`. Every pre-existing script that still calls `UnityEngine.VR.*` continues to work, while new Unity editors and runtimes call into the supported XR stack.
2. Included `.meta` files for the new compatibility shim so Unity registers it deterministically across machines.

## Manual steps you still need to perform

- Remove the legacy `Library/` folder before opening the project in Unity 6 so the asset database can be rebuilt with the new serialization format.
- Install Unity 6 (6000.x) via Unity Hub and open the project; Unity will update `ProjectSettings/ProjectVersion.txt` automatically.
- In `Project Settings > XR Plug-in Management`, install **XR Plug-in Management**, enable it for every target platform you need, and toggle the Oculus/OpenXR providers that you actually ship with.
- Reimport/update the Oculus Integration package to the latest release that officially supports Unity 6 (from the Unity Asset Store or Oculus Package Manager). The compatibility shim keeps older scripts compiling until you can take the vendor update.
- Review `Packages/manifest.json` in the Unity Package Manager UI and let Unity update first-party packages (Ads, Analytics, Purchasing, TextMeshPro, etc.) to the versions that are verified for Unity 6; the lockfile is intentionally absent so Hub/Editor can write fresh versions.
- Run the project in Play Mode (PCVR and Quest/Android if applicable) and validate rendering, input, and scene streaming paths because Unity 6 tightens XR rendering defaults (render scale, viewport scale).

Keeping these actions manual avoids committing Unity-generated files that are specific to the editor build you use, while making sure the repository itself already contains the code changes needed for Unity 6 compatibility.
