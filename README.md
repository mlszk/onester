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
- selectable dim dash-dot guide for object + gap zones
- selectable dim dash-dot guide for the center field after edge clearance
- AutoCAD-safe R12 DXF export
- coordinate copy

## Preview legend

```text
white rectangle = real sheet border
red circles = actual objects / cut circles
yellow line = closest pair / minimum-distance check
dim dash-dot circles = object + required gap zone
dim dash-dot rectangle = allowed center field after edge clearance
```

The dim dash-dot rectangle is not a milling path. It only shows where object centers are allowed.

Example:

```text
object diameter = 64 mm
edge clearance = 7 mm
center margin = 32 + 7 = 39 mm
```

So object centers must stay inside:

```text
x = 39 ... 561
y = 39 ... 261
```

for a 600 x 300 mm sheet.

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
