# 🔍 Reflection Utilities

The `SaltboxGames.Core.Utilities.Reflection` class provides thread-local, cached accessors for getting and setting private or public instance fields via compiled expression trees.

This is particularly useful for:

- High-performance access to fields without using `FieldInfo.SetValue()`
- Accessing private fields for testing or runtime patching
- Avoiding repeated reflection overhead with delegate caching

---

## Features

- Strongly-typed accessors: `Action<T1>` and `Func<T1>`
- Zero-allocation once cached
- Works with private & public instance fields

## Table of Contents

- [🔍 Reflection Utilities](#-reflection-utilities)
  - [Features](#features)
  - [Table of Contents](#table-of-contents)
  - [Fields](#fields)
    - [📦 Example Usage](#-example-usage)
    - [Advanced Usage: Direct Getter/Setter Creation](#advanced-usage-direct-gettersetter-creation)
  - [Properties](#properties)
    - [📦 Example Usage](#-example-usage-1)
    - [Advanced Usage: Direct Getter Creation](#advanced-usage-direct-getter-creation)


## Fields

### 📦 Example Usage

```csharp
using SaltboxGames.Core.Utilities;

class MyType
{
    private int _value = 42;
}

var myInstance = new MyType();

// Create a getter for "_value"
Func<int> getter = Reflection.GetFieldGetter<MyType, int>(myInstance, "_value");
int val = getter(); // 42

// Create a setter for "_value"
Action<int> setter = Reflection.GetFieldSetter<MyType, int>(myInstance, "_value");
setter(100);
```

### Advanced Usage: Direct Getter/Setter Creation

If you want the raw compiled delegate instead of a per-instance version:
```csharp
// Static delegate for any instance
Action<MyType, int> staticGetter = Reflection.GetFieldSetter<MyType, int>("_value");
int result = staticGetter(myInstance);

Func<MyType, int> staticSetter = Reflection.GetFieldSetter<MyType, int>("_value");
int result = staticSetter(myInstance, 100);
```

## Properties
### 📦 Example Usage
```csharp
class MyType
{
    private int MyProperty { get; set; } = 123;
}

var instance = new MyType();

var propGetter = Reflection.GetPropertyGetter<MyType, int>(instance, "MyProperty");
int val = propGetter(); // 123
```


### Advanced Usage: Direct Getter Creation

You can also use the raw delegate version if you're not targeting a single instance:
```csharp
var rawGetter = Reflection.GetPropertyGetter<MyType, int>("_value");
int val = rawGetter(instance);
```


