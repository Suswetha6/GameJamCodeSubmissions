# GameManager.cs Explainer

## Purpose
`GameManager` coordinates the full CacheCal flow: level loading, card selection, operator input, result checking, scoring, bonus cache size, and loss detection.

## Key Responsibilities
- Defines the five target levels, starting cards, and par values.
- Loads card values and resets cache, score, selections, and UI panels.
- Controls the flow `first card -> operator -> second card -> compute`.
- Consumes original cards after valid operations while keeping cache cards reusable.
- Checks whether the target has been reached by the latest result or any cached value.
- Runs a three-move lookahead to detect when the target can no longer be reached.
- Awards stars based on par and unlocks a three-slot cache for the next level after a perfect score.
- Handles restart, next level, retry, clear selection, and how-to-play UI actions.

## Inspector Setup
- Assign `CalculationManager` and `CacheManager`.
- Assign target, selected value, steps, score, and panel UI references.
- Assign the original number card array in board order.
- Wire operator buttons to `OnOperatorPressed` with `+`, `-`, `*`, `/`, or `%`.
- Wire Clear, Restart, Next Level, and Start Game buttons to their matching public methods.

## Mechanic Role
This is the gameplay coordinator. It turns the cache rules into a playable puzzle loop by deciding when cards are consumed, when cache values remain reusable, when levels end, and when the bonus cache slot is granted.
