# CalculationManager.cs Explainer

## Purpose
`CalculationManager` validates arithmetic operations, records successful steps, and sends successful results into the cache.

## Key Responsibilities
- Tracks two selected numbers for the current operation.
- Provides `TryCompute` as pure validation logic with no side effects.
- Supports addition, positive subtraction, multiplication, exact division, and non-zero modulo.
- Records successful calculations as readable step strings.
- Adds successful results to `CacheManager`.
- Clears selection state when an operation completes or is cancelled.

## Inspector Setup
- Assign the scene's `CacheManager` to `cacheManager`.

## Mechanic Role
This script enforces the arithmetic constraints that make the puzzle intentional: invalid subtraction, division, and modulo attempts produce no cached result. `GameManager` also reuses `TryCompute` for reachability checks before declaring a loss.
