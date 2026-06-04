# Extensions

Extension helpers live under `SaltboxGames.Core.Extensions`.

## ListExtensions.cs

`ListExtensions` contains list-specific helpers.

Key members:

- `RemoveAtSwapBack<T>(this List<T> list, int index)`: removes an item in O(1) by replacing it with the last element. Order is not preserved.
- `RemoveSwapBack<T>(this List<T> list, T item)`: finds an item and removes it with swap-back semantics. Order is not preserved.
- `AsSpan<T>(this List<T> list)`: exposes the active list storage as a span where supported.

`AsSpan<T>()` target availability:

- Unity 6 or newer, non-IL2CPP, and Unity Editor: uses cached reflection to access the backing array.
- Standalone .NET `net8.0` or newer: uses `CollectionsMarshal.AsSpan(list)`.
- Unity IL2CPP: not available.

Example:

```csharp
using SaltboxGames.Core.Extensions;

var values = new List<int> { 1, 2, 3, 4 };
values.RemoveAtSwapBack(1); // values now contains 1, 4, 3
```

## ShuffleExtensions.cs

`ShuffleExtensions` provides deterministic Fisher-Yates shuffle helpers backed by [`DeterministicRandom`](./utilities.md#deterministicrandomcs).

Key members:

- `Shuffle<T>(this Span<T> span)`: shuffles a span in place.
- `Shuffle<T>(this List<T> list)`: shuffles a list in place.

On targets where list [`AsSpan()`](#listextensionscs) is available, the list overload shuffles via span access. On other targets, it swaps through the list indexer.

Example:

```csharp
using SaltboxGames.Core.Extensions;
using SaltboxGames.Core.Utilities;

DeterministicRandom.Seed(1234);
var values = new List<int> { 1, 2, 3, 4 };
values.Shuffle();
```
