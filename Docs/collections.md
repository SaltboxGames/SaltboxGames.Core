# Collections

Collection helpers live under `SaltboxGames.Core.Collections`.

## CircularBuffer.cs

`CircularBuffer<T>` is a pooled, double-ended buffer backed by `ArrayPool<T>`. It supports efficient insertion and removal at both ends.

Key members:

- `Count`, `Capacity`, `IsEmpty`: current buffer state.
- `Push(T item)`: append at the tail.
- `Shift(T item)`: insert at the head.
- `Pop()`: remove from the tail.
- `Unshift()`: remove from the head.
- `PushRange(ReadOnlySpan<T> items)`: append multiple items.
- `ShiftRange(ReadOnlySpan<T> items)`: insert multiple items at the head while preserving source order.
- `PopRange(Span<T> destination)`: remove up to `destination.Length` items from the tail.
- `UnshiftRange(Span<T> destination)`: remove up to `destination.Length` items from the head.
- `PeekHead()`, `PeekTail()`: inspect ends without removal.
- `this[int index]`, `this[Index index]`: logical indexing from the head.
- `Justify()`: make the active range contiguous within the backing array.
- `AsSpan()`: return a contiguous span over the active range after justifying.
- `Dispose()`: return the backing array to `ArrayPool<T>`.

Example:

```csharp
using SaltboxGames.Core.Collections;

using var buffer = new CircularBuffer<int>(4);
buffer.Push(1);
buffer.Push(2);
buffer.Shift(0);

int first = buffer.PeekHead(); // 0
int last = buffer.Pop();      // 2
```

`AsSpan()` returns a view into the internal buffer. Treat that span as invalid after mutating the buffer.

## CircularBuffer.ZLinq.cs

When the `ZLINQ` compilation symbol is defined, [`CircularBuffer<T>`](#circularbuffercs) implements ZLinq's `IValueEnumerable<CircularBufferEnumerator<T>, T>`.

Provided members:

- `AsValueEnumerable()`: creates a ZLinq `ValueEnumerable` over the buffer.
- `CircularBufferEnumerator<T>`: value enumerator with count, span, and copy support.

This file is excluded unless `ZLINQ` is defined.

## ListPool.cs

`ListPool<T>` is a simple stack-backed pool for `List<T>` instances.

Key members:

- `ListPool<T>.Shared`: shared pool instance.
- `Rent()`: returns a pooled list or creates a new one.
- `Return(List<T> list)`: clears and stores the list for reuse.

Example:

```csharp
var list = ListPool<int>.Shared.Rent();
list.Add(10);
ListPool<int>.Shared.Return(list);
```

## ObjectPool.cs

`ObjectPool<T>` is a stack-backed pool for types with a public parameterless constructor.

Key members:

- `ObjectPool<T>.Shared`: shared pool instance.
- `ObjectPool(int initialCapacity = 0)`: preallocates items.
- `Rent()`: returns a pooled object or creates a new one.
- `Return(T item)`: stores an object for reuse.

The pool does not reset object state. Callers must reset returned objects before reuse if needed.

## QueuePool.cs

`QueuePool<T>` is a simple stack-backed pool for `Queue<T>` instances.

Key members:

- `QueuePool<T>.Shared`: shared pool instance.
- `Rent()`: returns a pooled queue or creates a new one.
- `Return(Queue<T> queue)`: clears and stores the queue for reuse.
