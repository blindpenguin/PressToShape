# Device check: ring context menu (task 12)

Run on 2026-11-16 on the owner's tablet (TB336FU, Android 16, 2560 × 1600 px landscape, 320 dpi). A Debug build was installed with `dotnet build src/PressToShape.Android -t:Install -c Debug` and driven with `adb` (one finger only, no stylus). The app was stopped afterwards, nothing was saved to the tablet, no file was written. Screenshots are kept in the scratchpad folder only. The desktop was checked on this machine by running the app and driving the mouse with `xdotool`.

Result values: **passed** (seen working), **passed by tests** (covered by automated tests, not seen on a device), **owner** (needs you), **not checked**.

## Requirements

| Req | Result | Evidence |
|---|---|---|
| R1.1 Right click opens the ring (desktop) | passed | Right click on a rectangle opened the ring at the pointer, with the shape selected. |
| R1.2 Long press opens the ring (finger) | passed | A 900 ms hold on a rectangle (`input swipe x y x y 900`) opened the ring at the press, with touch sizes. The finger's lift on the center did not close it. |
| R1.2 Long press with a pen | owner | `adb` cannot inject a stylus. Covered by tests (`CanvasAreaViewModelLongPressTests`). |
| R1.3 Pen barrel button | owner | Needs the pen. The code opens the ring on `IsBarrelButtonPressed` or the right button. Whether Android reports the barrel button this way on the TB336FU is not known. |
| R1.4 Not during a drag, pan or gesture | passed by tests | A second finger, a move past the slop and an early lift cancel the long press (tests). Two fingers cannot be injected. |
| R1.5 Other tools unchanged | passed by tests | With the Rectangle tool, nothing opens. On the device, drawing a rectangle with a finger did not open a ring. |
| R1.6 Ring stays inside the canvas area | passed (left and top edge) | A long press 70 dp from the left tool column and the top of the canvas moved the ring into the area. Right and bottom edges were not tried on the device (tests cover all four). |
| R1.7 Canvas does not react while open | passed | Desktop: a press outside the ring closed it and did not start anything (also covered by a headless test). |
| R2 Selection rules | passed | Right click and long press on an unselected rectangle selected it. On empty canvas the selection was cleared and the empty canvas ring (Paste, Select All) was shown. |
| R3 Slots for a selection | passed | Seven wedges in the order Group, Ungroup, Duplicate, Delete, Lock, Hide, Arrange. On the desktop, two shapes selected with a marquee and Group from the ring grouped them (one group, Ungroup then enabled). On the tablet, Delete from the ring removed the rectangle. Duplicate, Lock and Hide were not used on a device (tests compare them with the menu commands). |
| R3.3 Unavailable wedge | passed | Group and Ungroup were dimmed with one shape selected, Group was enabled with two. |
| R4 Empty canvas slots | passed (tablet) | Paste (dimmed, nothing copied) and Select All were shown. |
| R5 Submenu | passed | On both platforms Arrange swapped to four wedges and a back arrow in the center, the center went back. On the desktop Escape closed the ring from the top level. |
| R5.4 Arrange wedges run the commands | passed by tests | Not used on a device. |
| R6 Closing | passed | Escape (desktop), a tap outside (tablet) and a press outside (desktop) closed the ring and changed nothing. |
| R7.1, R7.2 Menu key and arrow keys | passed by tests | Headless test with the Menu key, arrows and Enter. Not tried by hand with a real keyboard. |
| R7.3 Accessible names | passed by tests | Names and disabled state of the wedges are checked headless. Not checked with TalkBack. |
| R7.4 Name in the center | passed | Desktop: hovering the Delete wedge showed "Löschen" in the center. |
| R8 Look and size | passed | Dark theme on both platforms. The tablet ring is 300 dp across with wedges of about 106 dp. The light theme was not looked at; its icon contrast was calculated (11.6 to 15.5 to 1 on the wedges, see the log of task 9). |
| R9 Translation | passed | German texts on both platforms (Löschen, Gruppieren …). |
| No clash with double-tap | passed | A quick double-tap on a rectangle opened no ring. A short tap selects as before. |

## For the owner

1. **Pen.** Long press with the pen, and the barrel button on a shape and on empty canvas.
2. **Hold, then slide.** Hold a finger until the ring opens, then slide to a wedge and lift: the wedge should run. (`adb` cannot hold and then move.)
3. **Right and bottom edges.** Long press close to the right and bottom edge of the canvas.
4. **Feel.** Whether the wedge size, the 500 ms delay and the dim of unavailable wedges feel right. The numbers are in `Styles/Theme.axaml` (`RingOuterRadius`, `RingInnerRadius`, `RingIconSize`, with the touch values in `TouchSizes`) and `LongPressTracker.DelayMs`.
5. **Light theme** on the tablet.
6. **Long press on a tool group button** in the tool column still opens its flyout (only the canvas gained a long press). It was not tried again here.

## Not checked

Pen, barrel button, two fingers, TalkBack, light theme, right and bottom edges on a device, the Menu key by hand, Lock, Hide and Duplicate from the ring on a device.
