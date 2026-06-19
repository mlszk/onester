# onester v0.1.0 experimental

## Added

- Object nesting for rectangular sheets.
- Configurable sheet width and height.
- Configurable object/circle diameter.
- Configurable gap between objects.
- Configurable edge clearance / border gap.
- Visual preview.
- Closest-pair minimum-distance check.
- Selectable dim dash-dot guide circles for full object gap envelopes.
- Selectable dim dash-dot edge-clearance rectangle.
- DXF export using AutoCAD-safe R12 format.

## Notes

- Experimental row-based optimizer.
- Intended for quick workshop/layout use.
- Not yet a mathematically guaranteed global optimizer.

- Gap envelope and edge clearance logic corrected: edge clearance now keeps the full object+gap envelope inside the sheet.
