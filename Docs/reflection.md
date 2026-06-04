# Reflection Utilities

`SaltboxGames.Core.Utilities.Reflection` provides cached, strongly typed delegates for accessing instance fields and properties through compiled expression trees.

Reflection helpers are only compiled when `!ENABLE_IL2CPP && (UNITY_6000_0_OR_NEWER || UNITY_EDITOR)` is true. They are intentionally unavailable on IL2CPP targets because they depend on runtime expression compilation.

## Runtime Files

- `Runtime/Utilities/Reflection.Fields.cs`
- `Runtime/Utilities/Reflection.Properties.cs`

## Fields

Use field helpers when you need repeated access to a public or private instance field without repeatedly calling `FieldInfo.GetValue()` or `FieldInfo.SetValue()`.

```csharp
using System;
using SaltboxGames.Core.Utilities;

class MyType
{
    private int _value = 42;
}

var instance = new MyType();

Func<int> getter = Reflection.GetFieldGetter<MyType, int>(instance, "_value");
Action<int> setter = Reflection.GetFieldSetter<MyType, int>(instance, "_value");

int before = getter();
setter(100);
int after = getter();
```

For reusable delegates that take the target instance as a parameter:

```csharp
Func<MyType, int> getter = Reflection.GetFieldGetter<MyType, int>("_value");
Action<MyType, int> setter = Reflection.GetFieldSetter<MyType, int>("_value");

int value = getter(instance);
setter(instance, 100);
```

## Properties

Property helpers currently support getters only.

```csharp
using System;
using SaltboxGames.Core.Utilities;

class MyType
{
    private int Value { get; set; } = 123;
}

var instance = new MyType();

Func<int> getter = Reflection.GetPropertyGetter<MyType, int>(instance, "Value");
int value = getter();
```

For a reusable property getter:

```csharp
Func<MyType, int> getter = Reflection.GetPropertyGetter<MyType, int>("Value");
int value = getter(instance);
```

## Notes

- Delegates are cached in thread-static dictionaries.
- Only instance members are supported.
- Missing fields or unreadable properties throw `ArgumentException`.
- These APIs should be guarded by the same preprocessor condition if referenced from code that also builds for IL2CPP.
