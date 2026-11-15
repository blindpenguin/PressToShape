# SVG conformity (W3C): note for later

Status: **idea, not decided** (2026-11-15). No code was written. This note keeps the thought, so it can be picked up when SVG import or export is designed.

## Question

Do other vector drawing apps conform to the W3C SVG specification, and should PressToShape?

## What other apps do

- SVG 1.1 is the baseline almost everywhere. SVG 2 is only partly supported.
- **Inkscape**: SVG is the native format, so the fit is closest. It adds its own `inkscape:` and `sodipodi:` metadata, and offers "Plain SVG" to strip it. Mesh gradients and hatches (SVG 2) are only partly supported.
- **Adobe Illustrator**: imports and exports SVG, but its model is not SVG-native. Output is often verbose, and filters, masks and CSS are handled inconsistently.
- **Affinity Designer, Sketch, Figma**: clean export for common cases. Blend modes, advanced filters and text on a path can be lost on a round trip.
- **Browsers**: the closest thing to a reference renderer, but they differ on SVG 2, filters and `foreignObject`.
- **Library renderers** (resvg, librsvg, Batik): vary widely. resvg publishes a conformance test suite. Batik targets SVG 1.1.

Areas where conformance usually breaks: filters, text layout, CSS in `<style>`, markers, patterns, masks and clipping with nested transforms, SVG 2 additions, `foreignObject`.

## Idea to consider

- Target the SVG 1.1 core: paths, shapes, groups, transforms, gradients, basic text.
- Test round trips with Inkscape and a browser.
- Use the W3C SVG test suite or the resvg tests as a check.
- Anything beyond the core will behave differently in other tools.

## Open questions

- Is SVG import, export, or both in scope? Check `.spec-workflow/steering/product.md` first.
- Which SVG version is the target?
- What happens to document features that SVG cannot express?
