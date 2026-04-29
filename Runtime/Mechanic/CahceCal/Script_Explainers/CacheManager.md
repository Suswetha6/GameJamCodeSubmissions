# CacheManager.cs Explainer

## Purpose
`CacheManager` owns the runtime cache values and refreshes the cache slot UI. It implements FIFO eviction when the cache is full.

## Key Responsibilities
- Stores cached results in insertion order.
- Keeps the cache size at two slots by default.
- Supports a temporary three-slot cache through `SetMaxSize`.
- Evicts the oldest cached value before adding a new value when the cache is full.
- Returns copied cache values so other scripts cannot mutate the internal list directly.
- Clears and redraws all cache UI slots on level reset.

## Inspector Setup
- Assign the cache slot `Card` objects to `cacheSlots` in display order.
- Mark each assigned cache slot card's `isCache` field as true.

## Mechanic Role
Every valid calculation result enters this manager. Because older values are evicted first, players must plan operation order around what will remain available for later calculations.
