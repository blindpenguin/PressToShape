# Palm detection on touch devices: research and device findings

Status: **shelved** (2026-11-15). No code was written. This note keeps what was learned, so the work can restart from here.

## Goal and constraint

Reject accidental palm contacts on touch devices. Blocking all touch while the pen is near is not wanted, because a finger held on the canvas should still work as a modifier (for example proportional editing while drawing with the pen).

## How other apps and platforms do it

- **Rejection by the OS or digitizer, then a cancel event.** Android sends `ACTION_CANCEL`, or `FLAG_CANCELED` on API 33 and higher, for a rejected palm. Apps should undo what that pointer started. See Android's [Reject stylus palm touches](https://developer.android.com/quick-guides/content/reject-stylus-palm-touches) and [Advanced stylus features](https://developer.android.com/guide/topics/large-screens/support-advanced-stylus-features).
- **Contact size.** Procreate treats large touches as palms. Users report that one fixed threshold can break two-finger gestures, so the threshold should be tunable ([Procreate help](https://folio.procreate.com/help-support/17172)).
- **Pen proximity.** Hardware and patents discard touches that arrive while the pen is in range ([patent](https://patents.justia.com/patent/10642419), [XP-Pen](https://www.xp-pen.com/blog/touch-function-drawing-tablet.html)). Used alone, it rules out finger modifiers.
- **Timing and trace heuristics.** A touch that appears around the pen and does not move like a finger is classified as a palm afterwards and cancelled.
- **User switches.** Procreate has "Disable Touch Actions". Some apps have a pen-only mode.

## Design that was chosen (not built)

Judge each touch separately:
- Reject by contact size (if available) and by the OS cancel flag.
- Apply pen proximity only to new touches that start while the pen is down or hovering. A finger that went down earlier stays valid.
- A settings toggle, on by default.

## What Avalonia and Android report (tested on the owner's tablet)

Device: Lenovo Idea Tab (model TB336FU), Android 16. Method: a temporary logging build (native `MotionEvent` and Avalonia pointer events), removed afterwards.

- **Contact size:** Avalonia's `PointerPointProperties.ContactRect` is 0×0 for finger and pen. The native `MotionEvent.GetTouchMajor` works: fingers measured 1.5 to 11.8, the pen 0. Using it needs a hook in the Android head project.
- **Pen proximity:** Avalonia has no `IsInRange` property. A hovering pen shows up as `Pen` pointer moves without a press, and there are no Entered, Exited or CaptureLost events when it leaves, so leaving range needs a timeout. Natively, hover arrives as `HoverMove` with tool type `Stylus` through `DispatchGenericMotionEvent`.
- **Tool types:** native `Stylus` and `Finger` map to Avalonia `Pen` and `Touch` correctly.
- **Cancel events:** `ACTION_CANCEL` and `FLAG_CANCELED` never appeared. Every flag was `None`.

## The tablet already rejects touch near the pen

Two probe runs covered: a finger held while the pen draws, a palm resting before the pen, and a palm landing during a pen stroke.

- A held, moving finger ended with a plain `Up` (no cancel, no flag) 13 ms before the pen's `Down`. The system ended the finger as the pen arrived.
- No palm or finger contact reached the app during any pen stroke.

So on this tablet, touch cannot be used as a modifier while the pen draws, and palm-versus-pen needs no app code. Whether this is firmware or Android was not determined.

## Not tested

- A palm resting on the canvas with no pen near.
- Any device other than this tablet. Behavior with firmware that does not reject touch is unknown.
- Palm contact sizes. No palm reached the app, so there are no samples to set a threshold from.

## If this is picked up again

1. Test a palm alone (no pen near) on the tablet, to see whether any palm contact reaches the app.
2. If other devices matter, build the per-touch filter above through the spec workflow (see `.claude/rules/spec-workflow.md`), with a settings toggle and translations.
3. The touch input code is in `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml.cs` and `src/PressToShape.App/Workspace/` (`TouchGestureStep`, `TouchGestureStepType`, `InputPointerType`).
