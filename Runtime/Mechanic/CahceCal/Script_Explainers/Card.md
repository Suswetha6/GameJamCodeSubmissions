# Card.cs Explainer

## Purpose
`Card` is the clickable UI component for both original number cards and cache slots. It stores the displayed integer value, forwards clicks to `GameManager`, and updates the card visuals when the card is selected, consumed, reset, or cleared.

## Key Responsibilities
- Requires a Unity `Button` component and subscribes to clicks in `Awake`.
- Displays the current number through `TextMeshProUGUI`.
- Tracks whether the card is a reusable cache card or a consumable original card.
- Exposes `IsAvailable` so `GameManager` can search only unconsumed original cards.
- Applies visual state colors for normal, selected, and consumed cards.

## Inspector Setup
- Assign `valueText` to the card label.
- Assign `background` to the image that should change color.
- Assign `gameManager`, or let the script find it at runtime.
- Enable `isCache` only for cache slot cards.

## Mechanic Role
Original cards are consumed after a valid operation, while cache cards remain reusable. This difference makes CacheCal behave like a small FIFO cache rather than a normal calculator.
