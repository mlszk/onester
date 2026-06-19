# onester

**onester** is a small Windows tool for quick object nesting on rectangular sheets.

Current experimental version:

```text
onester v0.1.0 experimental
```

The first version is focused on circular objects because this is the most common quick layout task for milling.

## Features

- rectangular sheet size input
- object/circle diameter input
- minimum gap between objects
- edge clearance / border gap
- visual preview
- closest-pair check
- selectable dim dash-dot guide for full object gap envelopes
- selectable dim dash-dot guide for the edge clearance rectangle
- AutoCAD-safe R12 DXF export

## Preview legend

```text
white rectangle = real sheet border
red circles = actual objects / cut circles
yellow line = closest pair / minimum-distance check
dim dash-dot circles = full object gap envelope
dim dash-dot rectangle = edge clearance from sheet border
```

The dim dash-dot rectangle is not a milling path. If edge clearance is set to 7 mm, the rectangle is drawn exactly 7 mm from each sheet border.

The dim dash-dot circles show the full object gap envelope:

```text
gap envelope radius = object radius + gap
```

Example:

```text
object diameter = 64 mm
object radius = 32 mm
gap = 4.5 mm
edge clearance = 7 mm
```

Then the object center must be at least:

```text
32 + 4.5 + 7 = 43.5 mm
```

from each sheet border.

For a 600 x 300 mm sheet, the allowed centers are inside:

```text
x = 43.5 ... 556.5
y = 43.5 ... 256.5
```

The visible edge-clearance rectangle itself is still drawn at:

```text
x = 7 ... 593
y = 7 ... 293
```

## Build

From the source folder:

```bat
dotnet build -c Release
```

Publish small framework-dependent build:

```bat
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true /p:DebugType=None /p:DebugSymbols=false -o publish\onester-v0.1.0
```

Or run:

```bat
build-release.bat
```

## Notes

This is an experimental row-based optimizer. It checks all distances exactly after generating candidate layouts.

It is intended as a practical workshop tool, not yet a mathematically guaranteed global optimizer.
