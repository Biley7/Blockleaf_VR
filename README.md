# Blockleaf

*A seated spatial VR task-management workspace for Meta Quest.*

[![Unity 6](https://img.shields.io/badge/Unity-6-000000?logo=unity&logoColor=white)](https://unity.com/releases/unity-6)
[![C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![OpenXR](https://img.shields.io/badge/XR-OpenXR-4B9CD3)](https://www.khronos.org/openxr/)
[![Meta XR SDK](https://img.shields.io/badge/Meta%20XR%20SDK-209.0.0-0467DF)](https://developer.oculus.com/documentation/unity/)
[![Platform: Meta Quest](https://img.shields.io/badge/Platform-Meta%20Quest-1C1E20?logo=meta&logoColor=white)](https://www.meta.com/quest/)

## Overview

Blockleaf moves task prioritization out of flat 2D lists and into a small, tactile 3D workspace positioned in front of a seated user. Grab task cubes and place them in spatial trays to make urgency and progress visible at a glance. Focus Mode reduces the visible task set to urgent and in-progress work.

## Demo

- **YouTube demo:** [Add demo video URL](YOUTUBE_VIDEO_URL)
- **Devpost submission — Meta VR Start Developer Competition 2026:** [![Devpost submission](https://img.shields.io/badge/Devpost-Submission-003E54?logo=devpost&logoColor=white)](DEVPOST_SUBMISSION_URL)

### Screenshots

| View | Screenshot |
|---|---|
| Seated workspace | `docs/screenshots/workspace.png` — add screenshot |
| Task priority trays | `docs/screenshots/priority-trays.png` — add screenshot |
| Focus Mode | `docs/screenshots/focus-mode.png` — add screenshot |

## Core spatial features

- **Procedural seated desk:** `BlockleafWorkspace` builds the desk, labeled priority trays, Focus Mode button, and starter tasks at runtime. It places the workspace in front of the headset view when available, with a fixed-position fallback.
- **Grabbable task cubes:** `TaskBlock` holds a task title and priority, updates its material color and optional TextMeshPro label, and settles toward a zone drop position after release. The starter task in `SampleScene` has a Meta XR Interaction SDK `Grabbable` component.
- **Trigger-based priority trays:** `PriorityZone` detects task colliders entering and leaving its trigger. On entry, it assigns **Urgent**, **In Progress**, or **Done** and asks the task to snap toward the tray. Task material colors are updated through the renderer material, including URP's `_BaseColor` property when present. Backlog tasks use blue; urgent tasks use coral red; in-progress tasks use amber; done tasks use green. Done labels are struck through when a TextMeshPro label is assigned.
- **One-tap Focus Mode:** `FocusTrigger` toggles the workspace filter when a non-task collider enters its trigger. Focus Mode keeps urgent and in-progress task blocks visible and hides backlog and done blocks; toggling it off restores all tracked tasks. The Focus Mode button changes color to indicate the active state.
- **Workspace recentering:** Repositions the workspace in front of the current view while preserving task positions relative to the workspace.

## Architecture

### Directory tree

```text
Assets/
├── BlockleafWorkspace.cs
├── PriorityZone.cs
├── TaskBlock.cs
├── Scenes/
│   └── SampleScene.unity
├── Settings/
│   └── Build Profiles/
│       └── Meta Quest.asset
└── TextMesh Pro/
    ├── Fonts/
    └── Resources/
Packages/
├── manifest.json
└── packages-lock.json
ProjectSettings/
├── EditorBuildSettings.asset
└── ProjectVersion.txt
```

### Core scripts

| Script | Responsibilities | Key components and interaction |
|---|---|---|
| `BlockleafWorkspace.cs` | Creates the desk, status trays, world labels, Focus Mode button, and three sample task cubes; positions and recenters the workspace; toggles focus filtering. | Uses primitive cubes, colliders, `PriorityZone`, `FocusTrigger`, and TextMeshPro. `Update` reads `OVRInput.Button.One` / `Button.Two` and keyboard `Alpha1` / `Alpha2`. |
| `PriorityZone.cs` | Maps a trigger volume to a `TaskPriority`; assigns tasks on trigger entry and clears the active zone on exit. | Requires a collider. On entry, calls `TaskBlock.AssignToZone` with the zone position plus `dropOffset`; on exit, calls `ClearZone`. |
| `TaskBlock.cs` | Stores the task title and priority, updates the task color and optional label, and smoothly settles a released task toward its zone target. | Requires a `Rigidbody`; reads its kinematic state to detect grab/release behavior. A `TextMeshPro` label is optional. The sample scene supplies the Meta XR `Grabbable`. |

`FocusTrigger` is defined in `BlockleafWorkspace.cs`; it debounces trigger-entry toggles and ignores task-block colliders.

## Controls and interaction

| Input | Action |
|---|---|
| Meta Quest controller — `OVRInput.Button.One` | Toggle Focus Mode |
| Meta Quest controller — `OVRInput.Button.Two` | Recenter the workspace |
| Hand tracking / physical triggers | `SampleScene` attaches the Meta XR Interaction SDK `Grabbable` to the starter task. Actual grab input depends on the active Meta XR interaction setup; the gameplay scripts define no hand-gesture binding. Unity trigger callbacks assign task priority when a block enters a tray and toggle Focus Mode when a non-task collider enters its button trigger. |
| Meta XR Simulator / keyboard — `Alpha1` | Toggle Focus Mode |
| Meta XR Simulator / keyboard — `Alpha2` | Recenter the workspace |

The keyboard bindings are `KeyCode.Alpha1` and `KeyCode.Alpha2`. No other keyboard shortcuts are defined in the workspace script.

## Getting started

### Prerequisites

- **Unity Editor:** `6000.6.9f1` (Unity 6), as recorded in `ProjectSettings/ProjectVersion.txt`.
- **Meta XR All-in-One SDK:** `209.0.0` (`com.meta.xr.sdk.all`).
- **OpenXR plugin:** `1.19.0` (`com.unity.xr.openxr`).
- **TextMesh Pro:** used for world-space labels; TextMesh Pro assets are included under `Assets/TextMesh Pro/`.
- For headset builds: Android Build Support modules for the installed Unity Editor and a Meta Quest headset for device testing.
- For simulator testing: Meta XR Simulator tooling available and configured in the Editor.

### Run in the Editor or Meta XR Simulator

1. Open the repository in Unity `6000.6.9f1` and allow Package Manager to resolve the manifest dependencies.
2. Open `Assets/Scenes/SampleScene.unity`.
3. If testing without a headset, start/configure Meta XR Simulator for the project. With a headset, connect the device and use the project's OpenXR runtime configuration.
4. Enter **Play Mode**. The workspace is generated after a short initialization delay; use the controls above to try Focus Mode, recentering, and task placement.

### Build an APK for Meta Quest

1. In Unity 6, open **File → Build Profiles** and select the existing **Meta Quest** profile. Confirm the Android target is installed and that `Assets/Scenes/SampleScene.unity` is included in the build scene list.
2. In the profile's Player settings, set the Android **Application Identifier** to `com.biley7.blockleaf`. Confirm the OpenXR Android configuration and Meta Quest settings for the intended devices.
3. For a release build, configure a keystore and key under **Player Settings → Publishing Settings**. Store the keystore and passwords securely; do not commit them to the repository.
4. Build the project to produce an `.apk`. For local device testing, enable USB debugging on the Quest, connect it, and install with `adb install -r path/to/Blockleaf.apk`.
5. For managed testing or distribution, upload the signed build through the Meta Horizon developer tools and assign it to the appropriate Release Channel.

## Roadmap

- Mixed Reality Passthrough with desk anchoring.
- Voice-to-block task creation.
- Persistent task and workspace storage across sessions.

## License

MIT License. Replace the copyright placeholders below with the applicable year and copyright holder before distribution.

```text
MIT License

Copyright (c) [YEAR] [COPYRIGHT HOLDER]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```