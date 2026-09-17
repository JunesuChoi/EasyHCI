# Third-party notices

This repository is a maintained fork of [Manbocoon/EasyHCI](https://github.com/Manbocoon/EasyHCI).
The upstream project is MIT licensed and that notice is kept in [LICENSE](LICENSE) unchanged.

## What this fork changed about bundled software

| Component | Upstream | This fork |
|---|---|---|
| HCI MemTest (`memtest.exe`) | Bundled as a loose file **and** embedded into `EasyHCI.exe` through `Properties/Resources.resx` | **Not bundled and not embedded.** The launcher locates a copy the operator already has. |
| Costura.Fody / Fody | Vendored under `packages/` and woven into the binary | Removed. The build has no IL weaving. |
| MaterialSkin | Vendored at `Resources/MaterialSkin.dll` | Kept as-is, because the UI code is written against this exact build. |

### Why HCI MemTest is not bundled

HCI MemTest is proprietary freeware owned by HCI Design. Its free edition is intended for
personal, non-commercial use, and distributing it inside another program requires the author's
permission. Upstream EasyHCI embeds the executable in its own binary and extracts it on first run,
which is exactly that pattern. This fork therefore ships no copy: you install HCI MemTest yourself
from the vendor and point the launcher at it. See [README](README.md) for the search order.

- Vendor: <https://hcidesign.com/memtest/>
- Licensing questions: HCI Design asks to be contacted through the address published on that page.

## MaterialSkin

- Project: <https://github.com/IgnaceMaes/MaterialSkin>
- License: MIT
- Copyright (c) Ignace Maes

MaterialSkin is redistributed here as an unmodified DLL because the form code targets that API.
It is the only third-party binary in this repository.

## Removed build-time packages

`Costura.Fody` and `Fody` were previously committed under `packages/`. Both are MIT licensed, and
both were removed rather than upgraded: embedding referenced assemblies into the executable adds
no capability here, makes the build harder to reproduce, and is a common source of antivirus false
positives for a tool that is already flagged by heuristics.

