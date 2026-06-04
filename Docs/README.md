# SaltboxGames.Core Documentation

Documentation for the runtime and package files in `SaltboxGames.Core`.

## Runtime Areas

- [Collections](./collections.md): collection types and pooling helpers.
- [Extensions](./extensions.md): extension methods.
- [Utilities](./utilities.md): general-purpose utility helpers.
- [Shims](./shims.md): cross-target compatibility types.
- [Reflection](./reflection.md): reflection-backed access helpers.

## Runtime File Coverage

- [`Runtime/Collections/CircularBuffer.cs`](./collections.md#circularbuffercs): pooled double-ended circular buffer.
- [`Runtime/Collections/CircularBuffer.ZLinq.cs`](./collections.md#circularbufferzlinqcs): optional ZLinq value enumerable integration.
- [`Runtime/Collections/ListPool.cs`](./collections.md#listpoolcs): reusable `List<T>` pool.
- [`Runtime/Collections/ObjectPool.cs`](./collections.md#objectpoolcs): reusable object pool for `new()` types.
- [`Runtime/Collections/QueuePool.cs`](./collections.md#queuepoolcs): reusable `Queue<T>` pool.
- [`Runtime/Extensions/ListExtensions.cs`](./extensions.md#listextensionscs): swap-back removal and target-specific `List<T>.AsSpan()` support.
- [`Runtime/Extensions/ShuffleExtensions.cs`](./extensions.md#shuffleextensionscs): deterministic Fisher-Yates shuffle helpers.
- [`Runtime/Shims/SafeGuid.cs`](./shims.md#safeguidcs): serializable and Unity-friendly `Guid` wrapper.
- [`Runtime/Utilities/DeterministicRandom.cs`](./utilities.md#deterministicrandomcs): deterministic XorShift-style random source.
- [`Runtime/Utilities/Reflection.Fields.cs`](./utilities.md#reflectionfieldscs): cached compiled field getters and setters.
- [`Runtime/Utilities/Reflection.Properties.cs`](./utilities.md#reflectionpropertiescs): cached compiled property getters.
- [`Runtime/Utilities/SpanUtilities.cs`](./utilities.md#spanutilitiescs): span/reference swap helpers.
- [`Runtime/Utilities/StringUtilities.cs`](./utilities.md#stringutilitiescs): longest common prefix helpers.

## Installation

See the package [README](../README.md) for Unity and .NET installation notes.
